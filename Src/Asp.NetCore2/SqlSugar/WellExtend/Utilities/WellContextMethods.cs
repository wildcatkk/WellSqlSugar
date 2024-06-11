using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSugar
{
    public partial interface IContextMethods
    {
        List<IConditionalModel> JsonToConditionalModels(string json, Type tableType);
    }

    public partial class ContextMethods
    {
        public List<IConditionalModel> JsonToConditionalModels(string json, Type tableType)
        {
            List<IConditionalModel> conditionalModels = new List<IConditionalModel>();
            var jarray = this.Context.Utilities.DeserializeObject<JArray>(json);
            foreach (var item in jarray)
            {
                if (item.Count() > 0)
                {
                    if (item.ToString().Contains("ConditionalList"))
                    {
                        IConditionalModel model = new ConditionalTree()
                        {
                            ConditionalList = GetConditionalList(item, tableType)
                        };
                        conditionalModels.Add(model);
                    }
                    else
                    {
                        var conditionalModel = GetConditionalModel(item, tableType);
                        conditionalModels.Add(conditionalModel);
                    }
                }
            }
            return conditionalModels;
        }
        private static List<KeyValuePair<WhereType, IConditionalModel>> GetConditionalList(JToken item, Type tableType)
        {
            List<KeyValuePair<WhereType, IConditionalModel>> result = new List<KeyValuePair<WhereType, IConditionalModel>>();
            var values = item.Values().First();
            foreach (var jToken in values)
            {
                WhereType type = (WhereType)Convert.ToInt32(jToken["Key"].Value<int>());
                IConditionalModel conditionalModel = null;
                var value = jToken["Value"];
                if (value.ToString().Contains("ConditionalList"))
                {
                    conditionalModel = new ConditionalTree()
                    {
                        ConditionalList = GetConditionalList(value, tableType)
                    };
                }
                else
                {
                    conditionalModel = GetConditionalModel(value, tableType);
                }
                result.Add(new KeyValuePair<WhereType, IConditionalModel>(type, conditionalModel));
            }
            return result;
        }

        private static ConditionalModel GetConditionalModel(JToken value, Type tableType)
        {
            var filedName = value["FieldName"] + "";

            var csharpTypeValue = value["CSharpTypeName"].ObjToString();
            string? csharpTypeName = null;
            if (csharpTypeValue.IsNullOrEmpty())
            {
                var prop = tableType.GetProperty(filedName);
                if (prop != null)
                {
                    csharpTypeName = (Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType).Name;
                }
            }
            else
                csharpTypeName = csharpTypeValue;

            return new ConditionalModel()
            {
                ConditionalType = GetConditionalType(value),
                FieldName = filedName,
                CSharpTypeName = csharpTypeName,
                FieldValue = value["FieldValue"].Value<string>() == null ? null : value["FieldValue"].ToString()
            };
        }
    }
}
