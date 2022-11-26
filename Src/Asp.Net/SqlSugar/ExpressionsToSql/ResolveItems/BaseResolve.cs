﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
namespace SqlSugar
{
    /// <summary>
    ///BaseResolve New Expression
    /// </summary>
    public partial class BaseResolve
    {

        public string GetNewExpressionValue(Expression item)
        {
            var newContext = this.Context.GetCopyContextWithMapping();
            newContext.SugarContext = this.Context.SugarContext;
            newContext.Resolve(item, this.Context.IsJoin ? ResolveExpressType.WhereMultiple : ResolveExpressType.WhereSingle);
            this.Context.Index = newContext.Index;
            this.Context.ParameterIndex = newContext.ParameterIndex;
            if (newContext.Parameters.HasValue())
            {
                this.Context.Parameters.AddRange(newContext.Parameters);
            }
            if (this.Context.SingleTableNameSubqueryShortName == "Subqueryable()")
            {
                this.Context.SingleTableNameSubqueryShortName = newContext.SingleTableNameSubqueryShortName;
            }
            else if (newContext.SingleTableNameSubqueryShortName!=null&& newContext.Result !=null && newContext.Result.Contains(this.Context.SqlTranslationLeft+ newContext.SingleTableNameSubqueryShortName+ this.Context.SqlTranslationRight))
            {
                this.Context.SingleTableNameSubqueryShortName = newContext.SingleTableNameSubqueryShortName;
            }
            return newContext.Result.GetResultString();
        }

        public string GetNewExpressionValue(Expression item, ResolveExpressType type)
        {
            var newContext = this.Context.GetCopyContextWithMapping();
            newContext.SugarContext = this.Context.SugarContext;
            newContext.Resolve(item, type);
            this.Context.Index = newContext.Index;
            this.Context.ParameterIndex = newContext.ParameterIndex;
            if (newContext.Parameters.HasValue())
            {
                this.Context.Parameters.AddRange(newContext.Parameters);
            }
            return newContext.Result.GetResultString();
        }


        protected void ResolveNewExpressions(ExpressionParameter parameter, Expression item, string asName)
        {
            if (item is ConstantExpression)
            {
                ResolveConst(parameter, item, asName);
            }
            else if ((item is MemberExpression) && ((MemberExpression)item).Expression == null)
            {
                ResolveMember(parameter, item, asName);
            }
            else if ((item is MemberExpression) && ((MemberExpression)item).Expression.NodeType == ExpressionType.Constant)
            {
                ResolveMemberConst(parameter, item, asName);
            }
            else if (item is MemberExpression)
            {
                ResolveMemberOther(parameter, item, asName);
            }
            else if (item is UnaryExpression && ((UnaryExpression)item).Operand is MemberExpression)
            {
                ResolveUnaryExpMem(parameter, item, asName);
            }
            else if (item is UnaryExpression && ((UnaryExpression)item).Operand is ConstantExpression)
            {
                ResolveUnaryExpConst(parameter, item, asName);
            }
            else if (item is BinaryExpression)
            {
                ResolveBinary(item, asName);
            }
            else if (item.Type.IsClass())
            {
                asName = ResolveClass(parameter, item, asName);
            }
            else if (item.Type == UtilConstants.BoolType && item is MethodCallExpression && IsNotCaseExpression(item))
            {
                ResloveBoolMethod(parameter, item, asName);
            }
            else if (item.NodeType == ExpressionType.Not
                && (item as UnaryExpression).Operand is MethodCallExpression
                && ((item as UnaryExpression).Operand as MethodCallExpression).Method.Name.IsIn("IsNullOrEmpty", "IsNullOrWhiteSpace"))
            {
                ResloveNot(parameter, item, asName);
            }
            else if (item is MethodCallExpression && (item as MethodCallExpression).Method.Name.IsIn("Count", "Any") && !item.ToString().StartsWith("Subqueryable"))
            {
                ResloveCountAny(parameter, item, asName);
            }
            else if (item is MethodCallExpression || item is UnaryExpression || item is ConditionalExpression || item.NodeType == ExpressionType.Coalesce)
            {
                ResloveOtherMUC(parameter, item, asName);
            }
            else
            {
                Check.ThrowNotSupportedException(item.GetType().Name);
            }
        }

        private void ResloveOtherMUC(ExpressionParameter parameter, Expression item, string asName)
        {
            this.Expression = item;
            this.Start();
            parameter.Context.Result.Append(this.Context.GetAsString(asName, parameter.CommonTempData.ObjToString()));
        }

        private void ResloveCountAny(ExpressionParameter parameter, Expression item, string asName)
        {
            if (this.Context.IsSingle && this.Context.SingleTableNameSubqueryShortName == null)
            {
                this.Context.SingleTableNameSubqueryShortName = item.ToString().Split('.').First();
            }
            parameter.Context.Result.Append(this.Context.GetAsString(asName, GetNewExpressionValue(item)));
        }

        private void ResloveNot(ExpressionParameter parameter, Expression item, string asName)
        {
            var asValue = GetAsNamePackIfElse(GetNewExpressionValue(item)).ObjToString();
            parameter.Context.Result.Append(this.Context.GetAsString(asName, asValue));
        }

        private void ResloveBoolMethod(ExpressionParameter parameter, Expression item, string asName)
        {
            this.Expression = item;
            this.Start();
            var sql = this.Context.DbMehtods.IIF(new MethodCallExpressionModel()
            {
                Args = new List<MethodCallExpressionArgs>() {
                          new MethodCallExpressionArgs() {
                               IsMember=true,
                               MemberName=parameter.CommonTempData.ObjToString()
                          },
                             new MethodCallExpressionArgs() {
                                IsMember=true,
                                MemberName=1
                          },
                          new MethodCallExpressionArgs() {
                               IsMember=true,
                               MemberName=0
                          }
                     }
            });
            parameter.Context.Result.Append(this.Context.GetAsString(asName, sql));
        }

        private string ResolveClass(ExpressionParameter parameter, Expression item, string asName)
        {
            var mappingKeys = GetMappingColumns(parameter.CurrentExpression);
            var isSameType = mappingKeys.Keys.Count > 0;
            CallContextThread<Dictionary<string, string>>.SetData("Exp_Select_Mapping_Key", mappingKeys);
            CallContextAsync<Dictionary<string, string>>.SetData("Exp_Select_Mapping_Key", mappingKeys);
            this.Expression = item;
            if (this.Context.IsJoin && (item is MemberInitExpression || item is NewExpression))
            {
                List<NewExpressionInfo> newExpressionInfos = new List<NewExpressionInfo>();
                if (item is MemberInitExpression)
                {
                    newExpressionInfos = ExpressionTool.GetNewexpressionInfos(item, this.Context, this);
                }
                else
                {
                    newExpressionInfos = ExpressionTool.GetNewDynamicexpressionInfos(item, this.Context, this);
                }
                foreach (NewExpressionInfo newExpressionInfo in newExpressionInfos)
                {
                    //var property=item.Type.GetProperties().Where(it => it.Name == newExpressionInfo.l).First();
                    //asName = GetAsName(item, newExpressionInfo.ShortName, property);
                    if (newExpressionInfo.Type == nameof(ConstantExpression))
                    {
                        parameter.Context.Result.Append(
                             newExpressionInfo.RightDbName + " AS " +
                              this.Context.SqlTranslationLeft + asName + "." + newExpressionInfo.LeftNameName + this.Context.SqlTranslationRight

                          );
                    }
                    else
                    {
                        parameter.Context.Result.Append(this.Context.GetAsString(
                           this.Context.SqlTranslationLeft + asName + "." + newExpressionInfo.LeftNameName + this.Context.SqlTranslationRight,
                        newExpressionInfo.ShortName + "." + newExpressionInfo.RightDbName
                      ));
                    }
                }
            }
            else if (!this.Context.IsJoin && (item is MemberInitExpression || item is NewExpression))
            {
                List<NewExpressionInfo> newExpressionInfos = new List<NewExpressionInfo>();
                if (item is MemberInitExpression)
                {
                    newExpressionInfos = ExpressionTool.GetNewexpressionInfos(item, this.Context, this);
                }
                else
                {
                    newExpressionInfos = ExpressionTool.GetNewDynamicexpressionInfos(item, this.Context, this);
                }
                //mappingKeys = new Dictionary<string, string>(); 
                foreach (NewExpressionInfo newExpressionInfo in newExpressionInfos)
                {
                    //var property=item.Type.GetProperties().Where(it => it.Name == newExpressionInfo.l).First();
                    //asName = GetAsName(item, newExpressionInfo.ShortName, property);
                    mappingKeys.Add("Single_" + newExpressionInfo.LeftNameName, asName + "." + newExpressionInfo.LeftNameName);
                    if (newExpressionInfo.Type == nameof(ConstantExpression))
                    {
                        CallContextThread<Dictionary<string, string>>.SetData("Exp_Select_Mapping_Key", mappingKeys);
                        CallContextAsync<Dictionary<string, string>>.SetData("Exp_Select_Mapping_Key", mappingKeys);
                        parameter.Context.Result.Append($" {newExpressionInfo.RightDbName} AS {this.Context.SqlTranslationLeft}{asName}.{newExpressionInfo.LeftNameName}{this.Context.SqlTranslationRight}  ");
                    }
                    else
                    {
                        CallContextThread<Dictionary<string, string>>.SetData("Exp_Select_Mapping_Key", mappingKeys);
                        CallContextAsync<Dictionary<string, string>>.SetData("Exp_Select_Mapping_Key", mappingKeys);
                        parameter.Context.Result.Append(this.Context.GetAsString(
                               this.Context.SqlTranslationLeft + asName + "." + newExpressionInfo.LeftNameName + this.Context.SqlTranslationRight,
                                newExpressionInfo.RightDbName
                          ));
                    }
                }
            }
            else if (IsExtSqlFuncObj(item))
            {
                var value = GetNewExpressionValue(item);
                parameter.Context.Result.Append($" {value} AS {asName} ");
            }
            else
            {
                asName = GetAsNameResolveAnObject(parameter, item, asName, isSameType);
            }

            return asName;
        }

        private void ResolveBinary(Expression item, string asName)
        {
            if (this.Context.Result.IsLockCurrentParameter == false)
            {
                var newContext = this.Context.GetCopyContextWithMapping();
                var resolveExpressType = this.Context.IsSingle ? ResolveExpressType.WhereSingle : ResolveExpressType.WhereMultiple;
                newContext.Resolve(item, resolveExpressType);
                this.Context.Index = newContext.Index;
                this.Context.ParameterIndex = newContext.ParameterIndex;
                if (newContext.Parameters.HasValue())
                {
                    this.Context.Parameters.AddRange(newContext.Parameters);
                }
                this.Context.Result.Append(this.Context.GetAsString(asName, newContext.Result.GetString()));
                this.Context.Result.CurrentParameter = null;
                if (this.Context.SingleTableNameSubqueryShortName.IsNullOrEmpty() && newContext.SingleTableNameSubqueryShortName.HasValue())
                {
                    this.Context.SingleTableNameSubqueryShortName = newContext.SingleTableNameSubqueryShortName;
                }
            }
        }

        private void ResolveUnaryExpConst(ExpressionParameter parameter, Expression item, string asName)
        {
            if (this.Context.Result.IsLockCurrentParameter == false)
            {
                this.Expression = ((UnaryExpression)item).Operand;
                this.Start();
                string parameterName = this.Context.SqlParameterKeyWord + "constant" + this.Context.ParameterIndex;
                this.Context.ParameterIndex++;
                parameter.Context.Result.Append(this.Context.GetAsString(asName, parameterName));
                this.Context.Parameters.Add(new SugarParameter(parameterName, parameter.CommonTempData));
            }
        }
       
        private void ResolveUnaryExpMem(ExpressionParameter parameter, Expression item, string asName)
        {
            if (this.Context.Result.IsLockCurrentParameter == false)
            {
                var expression = ((UnaryExpression)item).Operand as MemberExpression;
                var isDateTimeNow = ((UnaryExpression)item).Operand.ToString() == "DateTime.Now";
                if (expression.Expression == null && !isDateTimeNow)
                {
                    this.Context.Result.CurrentParameter = parameter;
                    this.Context.Result.IsLockCurrentParameter = true;
                    parameter.IsAppendTempDate();
                    this.Expression = item;
                    this.Start();
                    parameter.IsAppendResult();
                    this.Context.Result.Append(this.Context.GetAsString(asName, parameter.CommonTempData.ObjToString()));
                    this.Context.Result.CurrentParameter = null;
                }
                else if (expression.Expression is ConstantExpression || isDateTimeNow)
                {
                    string parameterName = this.Context.SqlParameterKeyWord + "constant" + this.Context.ParameterIndex;
                    this.Context.ParameterIndex++;
                    parameter.Context.Result.Append(this.Context.GetAsString(asName, parameterName));
                    this.Context.Parameters.Add(new SugarParameter(parameterName, ExpressionTool.GetMemberValue(expression.Member, expression)));
                }
                else
                {
                    this.Context.Result.CurrentParameter = parameter;
                    this.Context.Result.IsLockCurrentParameter = true;
                    parameter.IsAppendTempDate();
                    this.Expression = expression;
                    this.Start();
                    parameter.IsAppendResult();
                    this.Context.Result.Append(this.Context.GetAsString(asName, parameter.CommonTempData.ObjToString()));
                    this.Context.Result.CurrentParameter = null;
                }
            }
        }

        private void ResolveMemberOther(ExpressionParameter parameter, Expression item, string asName)
        {
            if (this.Context.Result.IsLockCurrentParameter == false)
            {
                this.Context.Result.CurrentParameter = parameter;
                this.Context.Result.IsLockCurrentParameter = true;
                parameter.IsAppendTempDate();
                this.Expression = item;
                if (IsBoolValue(item))
                {
                    this.Expression = (item as MemberExpression).Expression;
                }
                this.Start();
                parameter.IsAppendResult();
                this.Context.Result.Append(this.Context.GetAsString(asName, parameter.CommonTempData.ObjToString()));
                this.Context.Result.CurrentParameter = null;
            }
        }

        private void ResolveMemberConst(ExpressionParameter parameter, Expression item, string asName)
        {
            this.Expression = item;
            this.Start();
            string parameterName = this.Context.SqlParameterKeyWord + "constant" + this.Context.ParameterIndex;
            this.Context.ParameterIndex++;
            parameter.Context.Result.Append(this.Context.GetAsString(asName, parameterName));
            this.Context.Parameters.Add(new SugarParameter(parameterName, parameter.CommonTempData));
        }

        private void ResolveMember(ExpressionParameter parameter, Expression item, string asName)
        {
            var paramterValue = ExpressionTool.GetPropertyValue(item as MemberExpression);
            string parameterName = this.Context.SqlParameterKeyWord + "constant" + this.Context.ParameterIndex;
            this.Context.ParameterIndex++;
            parameter.Context.Result.Append(this.Context.GetAsString(asName, parameterName));
            this.Context.Parameters.Add(new SugarParameter(parameterName, paramterValue));
        }

        private void ResolveConst(ExpressionParameter parameter, Expression item, string asName)
        {
            this.Expression = item;
            this.Start();
            string parameterName = this.Context.SqlParameterKeyWord + "constant" + this.Context.ParameterIndex;
            this.Context.ParameterIndex++;
            parameter.Context.Result.Append(this.Context.GetAsString(asName, parameterName));
            this.Context.Parameters.Add(new SugarParameter(parameterName, parameter.CommonTempData));
        }
    }
}
