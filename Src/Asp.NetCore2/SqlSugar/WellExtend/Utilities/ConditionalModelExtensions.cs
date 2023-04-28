using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSugar
{
    public static class ConditionalModelExtensions
    {
        public static void Add(this List<IConditionalModel> conditions, string fieldName, object fieldValue, ConditionalType conditionalType = ConditionalType.Equal)
        {
            if (fieldValue is bool)
            {
                conditions.Add(new ConditionalModel()
                {
                    FieldName = fieldName,
                    FieldValue = fieldValue.ToString()?.ToLower(),
                    CSharpTypeName = "bool",
                    ConditionalType = conditionalType
                });
            }
            else
            {
                conditions.Add(new ConditionalModel()
                {
                    FieldName = fieldName,
                    FieldValue = fieldValue.ToString(),
                    ConditionalType = conditionalType
                });
            }
        }

    }

    public static class SugarConditional
    {
        public static List<IConditionalModel> Create(string fieldName, object fieldValue, ConditionalType conditionalType = ConditionalType.Equal)
        {
            List<IConditionalModel> conditions = new List<IConditionalModel>
            {
                { fieldName, fieldValue, conditionalType }
            };

            return conditions;
        }

    }
}
