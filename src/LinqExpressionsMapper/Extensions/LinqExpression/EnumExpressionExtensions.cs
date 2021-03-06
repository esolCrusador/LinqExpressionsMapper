﻿using System.Collections.Generic;
using LinqExpressionsMapper.Extensions;

namespace System.Linq.Expressions
{
    /// <summary>
    /// Class for bilding expressions related to enums.
    /// </summary>
    public static class EnumExpressionExtensions
    {
        private static readonly Type StringType = typeof(string);
        private static readonly ConstantExpression EmptyStringExpression = Expression.Constant("", StringType);
        private static readonly ConstantExpression NullStringExpression = Expression.Constant(null, StringType);

        /// <summary>
        /// Gets Expression for Enum to string conversion.
        /// </summary>
        /// <typeparam name="TEnum">Enum type. May be nullable.</typeparam>
        /// <param name="toString">Enum to string conversion delegate.</param>
        /// <param name="nullEnumString">String for null enum value.</param>
        /// <param name="zeroEnumString">String for 0 enum value.</param>
        /// <param name="delimiter">Delimiter for [Flags] enum.</param>
        /// <returns></returns>
        public static Expression<Func<TEnum, string>> GetEnumToStringExpression<TEnum>(Func<Enum, string> toString, string nullEnumString = null, string zeroEnumString = "", string delimiter = ", ")
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

            if (zeroEnumString == "")
            {
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
            }
            else
            {
                if (allMembers.ContainsKey(zeroEnumVal))
                    allMembers.Remove(zeroEnumVal);

                zeroStringExpression = Expression.Constant(zeroEnumString, StringType);
            }

            Expression zeroConstant = Expression.Constant(zeroEnumVal, enumType);//Expression.Convert(Expression.Constant(zero, enumUnderlyingType), enumType);

            Expression nullStringExpression = nullEnumString == null ? NullStringExpression : Expression.Constant(nullEnumString, StringType);

            //Flag enum.
            if (enumExactType.GetCustomAttributes(typeof (FlagsAttribute), false).Length != 0)
            {
                return GetFlagEnumToStringExpression<TEnum>(allMembers, enumType, enumExactType, enumUnderlyingType, zeroConstant, zeroStringExpression, nullStringExpression, delimiter);
            }
            else
            {
                return GetEnumToStringExpression<TEnum>(allMembers, enumType, enumExactType, zeroConstant, zeroStringExpression, nullStringExpression);
            }
        }

        private static Expression<Func<TEnum, string>> GetFlagEnumToStringExpression<TEnum>(
            Dictionary<Enum, string> allMembers,
            Type enumType, 
            Type enumExactType,
            Type enumUnderlyingType,
            Expression zeroConstant, 
            Expression zeroStringExpression,
            Expression nullStringExpression,
            string delimiter)
        {
            ParameterExpression parameter = Expression.Parameter(enumType, enumExactType.Name.ToLower());
            int delimiterLength = delimiter.Length;

            Expression<Func<string, string>> subStringExpression = enumResult => enumResult.Substring(delimiterLength, int.MaxValue);

            // Ex: gender => ((gender & Gender.Male) == Gender.Male?", Male":"") + ((gender & Gender.Female) == Gender.Female?", Female":"") + ...; Result: ", Male, Female ..."
            Expression<Func<TEnum, string>> allMembersToString = allMembers.Select(kvp =>
            {
                // (int)Gender.Male
                Expression compareVal = Expression.Convert(Expression.Constant(kvp.Key, enumExactType), enumUnderlyingType);
                // ", Male"
                ConstantExpression resultString = Expression.Constant(delimiter + kvp.Value, StringType);

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
                ConditionalExpression nullStringIfNullValue = Expression.Condition(Expression.Equal(parameter, nullEnumVal), nullStringExpression, notNullExpression.Body);

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
            Expression zeroStringExpression,
            Expression nullStringExpression)
        {
            ParameterExpression parameter = Expression.Parameter(enumType, enumExactType.Name.ToLower());

            Expression resultBody = nullStringExpression;

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
                ConditionalExpression nullStringIfNullValue = Expression.Condition(Expression.Equal(parameter, nullEnumVal), nullStringExpression, resultBody);

                return Expression.Lambda<Func<TEnum, string>>(nullStringIfNullValue, parameter);
            }
            else
            {
                return Expression.Lambda<Func<TEnum, string>>(resultBody, parameter);
            }
        }
    }
}
