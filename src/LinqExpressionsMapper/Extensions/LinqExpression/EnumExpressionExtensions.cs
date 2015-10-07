using System.Collections.Generic;
using LinqExpressionsMapper.Extensions;

namespace System.Linq.Expressions
{
    public static class EnumExpressionExtensions
    {
        private static readonly ConstantExpression EmptyStringExpression = Expression.Constant(String.Empty, typeof(string));
        private static readonly ConstantExpression NullStringExpression = Expression.Constant(null, typeof(string));
        private static readonly Type StringType = typeof (String);

        public static Expression<Func<TEnum, string>> GetEnumToStringExpression<TEnum>(Func<Enum, string> toString)
        {
            Type enumType = typeof (TEnum);

            Type enumExactType = enumType;
            Type nullableEnumType = Nullable.GetUnderlyingType(enumExactType);
            if (nullableEnumType != null)
            {
                enumExactType = nullableEnumType;
            }
            Type enumUnderlyingType = Enum.GetUnderlyingType(enumExactType);

            var allMembers = EnumExtensions.GetAllMembers(enumExactType)
                .Select(e => new KeyValuePair<Enum, string>(e, toString(e)))
                .OrderBy(e => e.Value)
                .ToDictionary(e => e.Key, e => e.Value);

            object zero = Activator.CreateInstance(enumUnderlyingType);
            Enum zeroEnumVal = (Enum)Enum.ToObject(enumExactType, zero);

            Expression zeroStringExpression;
            string zeroString;
            if (allMembers.TryGetValue(zeroEnumVal, out zeroString))
            {
                allMembers.Remove(zeroEnumVal);
                zeroStringExpression = Expression.Constant(zeroString, StringType);
            }
            else
            {
                zeroStringExpression = EmptyStringExpression;
            }

            Expression zeroConstant = Expression.Constant(zeroEnumVal, enumType);//Expression.Convert(Expression.Constant(zero, enumUnderlyingType), enumType);

            //Flag enum.
            if (enumExactType.GetCustomAttributes(typeof (FlagsAttribute), false).Any())
            {
                return GetFlagEnumToStringExpression<TEnum>(allMembers, enumType, enumExactType, enumUnderlyingType, zeroConstant, zeroStringExpression);
            }
            else
            {
                return GetEnumToStringExpression<TEnum>(allMembers, enumType, enumExactType, zeroConstant, zeroStringExpression);
            }
        }

        private static Expression<Func<TEnum, string>> GetFlagEnumToStringExpression<TEnum>(
            Dictionary<Enum, string> allMembers,
            Type enumType,
            Type enumExactType,
            Type enumUnderlyingType,
            Expression zeroConstant,
            Expression zeroStringExpression)
        {
            ParameterExpression parameter = Expression.Parameter(enumType, enumExactType.Name.ToLower());

            Expression<Func<string, string>> subStringExpression = enumResult => enumResult.Substring(2, int.MaxValue);

            // Ex: gender => ((gender & Gender.Male) == Gender.Male?", Male":"") + ((gender & Gender.Female) == Gender.Female?", Female":"") + ...; Result: ", Male, Female ..."
            Expression<Func<TEnum, string>> allMembersToString = allMembers.Select(kvp =>
            {
                // (int)Gender.Male
                Expression compareVal = Expression.Convert(Expression.Constant(kvp.Key, enumExactType), enumUnderlyingType);
                // ", Male"
                ConstantExpression resultString = Expression.Constant(", " + kvp.Value, StringType);

                // (gender & Gender.Male) == Gender.Male
                Expression equalsExpression = Expression.Equal(Expression.And(Expression.Convert(parameter, enumUnderlyingType), compareVal), compareVal);
                // (gender & Gender.Male) == Gender.Male?", Male":""
                Expression enumToStringBody = Expression.Condition(equalsExpression, resultString, EmptyStringExpression);

                return Expression.Lambda<Func<TEnum, string>>(enumToStringBody, parameter);
            })
                .Join((l, r) => String.Concat(l, r));

            // Ex: gender => (((gender & Gender.Male) == Gender.Male?", Male":"") + ((gender & Gender.Female) == Gender.Female?", Female":"") + ...).Substring(2); Result: "Male, Female ..."
            Expression<Func<TEnum, string>> notNullExpression = allMembersToString.Continue(subStringExpression);

            //Zero check
            Expression checkZeroBody = Expression.Condition(Expression.Equal(parameter, zeroConstant), zeroStringExpression, notNullExpression.Body);

            // Ex: gender => gender==0 ? "" : (((gender & Gender.Male) == Gender.Male?", Male":"") + ((gender & Gender.Female) == Gender.Female?", Female":"") + ...).Substring(2);
            notNullExpression = Expression.Lambda<Func<TEnum, string>>(checkZeroBody, notNullExpression.Parameters[0]);

            bool isNullable = enumType != enumExactType;
            if (isNullable)
            {
                // Ex: gender => gender==null ? null : (gender==0 ? "" : (((gender & Gender.Male) == Gender.Male?", Male":"") + ((gender & Gender.Female) == Gender.Female?", Female":"") + ...).Substring(2));
                ConstantExpression nullEnumVal = Expression.Constant(null, enumType);
                ConditionalExpression nullStringIfNullValue = Expression.Condition(Expression.Equal(parameter, nullEnumVal), NullStringExpression, notNullExpression.Body);

                return Expression.Lambda<Func<TEnum, string>>(nullStringIfNullValue, parameter);
            }
            else
            {
                // Ex: gender => gender==0 ? "" : (((gender & Gender.Male) == Gender.Male?", Male":"") + ((gender & Gender.Female) == Gender.Female?", Female":"") + ...).Substring(2);
                return notNullExpression;
            }
        }

        private static Expression<Func<TEnum, string>> GetEnumToStringExpression<TEnum>(
            Dictionary<Enum, string> allMembers, 
            Type enumType, 
            Type enumExactType, 
            Expression zeroConstant, 
            Expression zeroStringExpression)
        {
            ParameterExpression parameter = Expression.Parameter(enumType, enumExactType.Name.ToLower());

            Expression resultBody = NullStringExpression;

            foreach (var kvp in allMembers)
            {
                ConstantExpression compareVal = Expression.Constant(kvp.Key, enumType);
                ConstantExpression resultString = Expression.Constant(kvp.Value, StringType);

                // gender => (gender == Gender.Male ? "Male" : (gender == Gender.Female ? "Female" : (...)));
                resultBody = Expression.Condition(Expression.Equal(parameter, compareVal), resultString, resultBody);
            }

            //Zero check
            // gender => gender == 0 ? "" : (gender == Gender.Male ? "Male" : (gender == Gender.Female ? "Female" : (...)));
            resultBody = Expression.Condition(Expression.Equal(parameter, zeroConstant), zeroStringExpression, resultBody);

            bool isNullable = enumType != enumExactType;
            if (isNullable)
            {
                ConstantExpression nullEnumVal = Expression.Constant(null, enumType);
                // gender => gender == null ? null : (gender == 0 ? "" : (gender == Gender.Male ? "Male" : (gender == Gender.Female ? "Female" : (...))));
                ConditionalExpression nullStringIfNullValue = Expression.Condition(Expression.Equal(parameter, nullEnumVal), NullStringExpression, resultBody);

                return Expression.Lambda<Func<TEnum, string>>(nullStringIfNullValue, parameter);
            }
            else
            {
                return Expression.Lambda<Func<TEnum, string>>(resultBody, parameter);
            }
        }
    }
}
