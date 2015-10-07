using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LinqExpressionsMapper.Extensions
{
    public static class EnumExtensions
    {
        private static readonly Dictionary<Guid, List<Enum>> AllValues = new Dictionary<Guid, List<Enum>>();

        public static List<Enum>  GetAllMembers(Type enumType)
        {
            var notNullableEnum = Nullable.GetUnderlyingType(enumType);
            if(notNullableEnum!=null)
            {
                enumType = notNullableEnum;
            }
            List<Enum> enumValues;
            if (!AllValues.TryGetValue(enumType.GUID, out enumValues))
            {
                enumValues = Enum.GetValues(enumType).Cast<Enum>().ToList();

                AllValues.Add(enumType.GUID, enumValues);
            }

            return enumValues;
        }
    }

    public static class EnumExtensions<TEnum>
        where TEnum: struct 
    {
        public static readonly ReadOnlyCollection<TEnum> AllMembers;

        static EnumExtensions()
        {
            AllMembers = new ReadOnlyCollection<TEnum>(EnumExtensions.GetAllMembers(typeof (TEnum)).Cast<TEnum>().ToList());
        }
    }
}
