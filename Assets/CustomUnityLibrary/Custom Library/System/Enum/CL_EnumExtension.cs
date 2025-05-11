using System;

namespace CustomLibrary.References
{
    public static class EnumExtension
    {
        /// <summary>
        /// Converts enum to int
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static int ToInt<TEnum>(this TEnum enumValue) where TEnum : Enum
        {
            return Convert.ToInt32(enumValue);
        }

    }
}
