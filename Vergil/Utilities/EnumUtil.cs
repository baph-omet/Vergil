using System;

namespace Vergil.Utilities {
    /// <summary>
    /// Class for Enum extension methods
    /// </summary>
    public static class EnumUtil {
        /// <summary>
        /// Shortcut for Enum.Parse()
        /// </summary>
        /// <typeparam name="T">The Enum type that will be parsed against.</typeparam>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether or not to ignore case in the value. Default: false</param>
        /// <returns>An Enum value of type T corresponding to its string representation.</returns>
        public static T ParseEnum<T>(string value, bool ignoreCase = false) where T : Enum {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }

        /// <summary>
        /// Shortcut for Enum.GetName(Type, object)
        /// </summary>
        /// <typeparam name="T">The enum type. Can be inferred from value.</typeparam>
        /// <param name="value">The enum value to convert to string.</param>
        /// <returns></returns>
        public static string EnumName<T>(this T value) where T : Enum {
            return Enum.GetName(typeof(T), value);
        }
    }
}
