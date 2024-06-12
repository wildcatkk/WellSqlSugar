using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlSugar
{
    public static class ConditionalModelExtensions
    {
        public static void Add(this List<IConditionalModel> conditions, string fieldName, string fieldValue, Type fieldType, ConditionalType conditionalType = ConditionalType.Equal)
        {
            if (conditions is null)
            {
                conditions = new List<IConditionalModel>();
            }

            conditions.Add(SugarConditional.CreateModel(fieldName, fieldValue, fieldType, conditionalType));
        }

        public static void Add(this List<ConditionalModel> conditions, string fieldName, string fieldValue, Type fieldType, ConditionalType conditionalType = ConditionalType.Equal)
        {
            if (conditions is null)
            {
                conditions = new List<ConditionalModel>();
            }

            conditions.Add(SugarConditional.CreateModel(fieldName, fieldValue, fieldType, conditionalType));
        }

        public static void Add(this List<KeyValuePair<WhereType, ConditionalModel>> conditions, WhereType whereType, string fieldName, string fieldValue, Type fieldType, ConditionalType conditionalType = ConditionalType.Equal)
        {
            if (conditions is null)
            {
                conditions = new List<KeyValuePair<WhereType, ConditionalModel>>();
            }

            conditions.Add(SugarConditional.CreateModel(whereType, fieldName, fieldValue, fieldType, conditionalType));
        }
    }

    public static class SugarConditional
    {
        public static List<IConditionalModel> Create(string fieldName, string fieldValue, Type fieldType, ConditionalType conditionalType = ConditionalType.Equal)
        {
            List<IConditionalModel> conditions = new List<IConditionalModel>
            {
                { fieldName, fieldValue, fieldType, conditionalType }
            };

            return conditions;
        }

        public static List<IConditionalModel> Create(List<KeyValuePair<WhereType, ConditionalModel>> list)
            => new List<IConditionalModel> { new ConditionalCollections { ConditionalList = list } };

        public static ConditionalModel CreateModel(string fieldName, string fieldValue, Type fieldType, ConditionalType conditionalType = ConditionalType.Equal)
        {
            ConditionalModel condi = new ConditionalModel
            {
                FieldValue = fieldValue,
                FieldName = fieldName,
                CSharpTypeName = (Nullable.GetUnderlyingType(fieldType) ?? fieldType).Name,
                ConditionalType = conditionalType
            };

            return condi;
        }

        public static KeyValuePair<WhereType, ConditionalModel> CreateModel(WhereType whereType, string fieldName, string fieldValue, Type fieldType, ConditionalType conditionalType = ConditionalType.Equal)
            => new KeyValuePair<WhereType, ConditionalModel>(whereType, CreateModel(fieldName, fieldValue, fieldType, conditionalType));

        public static List<KeyValuePair<WhereType, ConditionalModel>> CreateModels(WhereType whereType, string fieldName, string fieldValue, Type fieldType, ConditionalType conditionalType = ConditionalType.Equal)
        {
            List<KeyValuePair<WhereType, ConditionalModel>> conditions = new List<KeyValuePair<WhereType, ConditionalModel>>
            {
                { whereType, fieldName, fieldValue, fieldType, conditionalType }
            };

            return conditions;
        }

        public static ConditionalCollections CreateList(List<KeyValuePair<WhereType, ConditionalModel>> list)
            => new ConditionalCollections { ConditionalList = list };

    }
}
