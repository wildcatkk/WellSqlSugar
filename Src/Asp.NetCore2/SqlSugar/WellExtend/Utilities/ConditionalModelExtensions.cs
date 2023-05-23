using System.Collections.Generic;

namespace SqlSugar
{
    public static class ConditionalModelExtensions
    {
        public static void Add(this List<IConditionalModel> conditions, string fieldName, object fieldValue, ConditionalType conditionalType = ConditionalType.Equal)
        {
            if (conditions is null)
            {
                conditions = new List<IConditionalModel>();
            }

            conditions.Add(SugarConditional.CreateOne(fieldName, fieldValue, conditionalType));
        }

        public static void Add(this List<ConditionalModel> conditions, string fieldName, object fieldValue, ConditionalType conditionalType = ConditionalType.Equal)
        {
            if (conditions is null)
            {
                conditions = new List<ConditionalModel>();
            }

            conditions.Add(SugarConditional.CreateOne(fieldName, fieldValue, conditionalType));
        }

        public static void Add(this List<KeyValuePair<WhereType, ConditionalModel>> conditions, WhereType whereType, string fieldName, object fieldValue, ConditionalType conditionalType = ConditionalType.Equal)
        {
            if (conditions is null)
            {
                conditions = new List<KeyValuePair<WhereType, ConditionalModel>>();
            }

            conditions.Add(SugarConditional.CreateOne(whereType, fieldName, fieldValue, conditionalType));
        }
    }

    public static class SugarConditional
    {
        private static ConditionalModel CreateConditionalModel(string fieldName, object fieldValue, ConditionalType conditionalType = ConditionalType.Equal)
        {
            ConditionalModel condi = new ConditionalModel();

            if (fieldValue is bool)
            {
                if (fieldValue is null)
                {
                    fieldValue = false;
                }

                condi.FieldName = fieldName;
                condi.FieldValue = fieldValue.ToString()?.ToLower();
                condi.CSharpTypeName = "bool";
                condi.ConditionalType = conditionalType;
            }
            else
            {
                if (fieldValue is null)
                {
                    fieldValue = "";
                }

                condi.FieldName = fieldName;
                condi.FieldValue = fieldValue.ToString();
                condi.ConditionalType = conditionalType;
            }

            return condi;
        }

        public static List<IConditionalModel> Create(string fieldName, object fieldValue, ConditionalType conditionalType = ConditionalType.Equal)
        {
            List<IConditionalModel> conditions = new List<IConditionalModel>
            {
                { fieldName, fieldValue, conditionalType }
            };

            return conditions;
        }

        public static List<KeyValuePair<WhereType, ConditionalModel>> Create(WhereType whereType, string fieldName, object fieldValue, ConditionalType conditionalType = ConditionalType.Equal)
        {
            List<KeyValuePair<WhereType, ConditionalModel>> conditions = new List<KeyValuePair<WhereType, ConditionalModel>>
            {
                { whereType, fieldName, fieldValue, conditionalType }
            };

            return conditions;
        }

        public static ConditionalModel CreateOne(string fieldName, object fieldValue, ConditionalType conditionalType = ConditionalType.Equal)
            => CreateConditionalModel(fieldName, fieldValue, conditionalType);

        public static KeyValuePair<WhereType, ConditionalModel> CreateOne(WhereType whereType, string fieldName, object fieldValue, ConditionalType conditionalType = ConditionalType.Equal)
            => new KeyValuePair<WhereType, ConditionalModel>(whereType, CreateOne(fieldName, fieldValue, conditionalType));

        public static ConditionalCollections CreateWhere(List<KeyValuePair<WhereType, ConditionalModel>> list)
            => new ConditionalCollections { ConditionalList = list } ;

        public static List<IConditionalModel> CreateList(List<KeyValuePair<WhereType, ConditionalModel>> list)
            => new List<IConditionalModel> { new ConditionalCollections { ConditionalList = list } };

    }
}
