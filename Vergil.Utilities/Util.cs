using System;
using System.Collections.Generic;
using System.Linq;

namespace Vergil.Utilities {
    /// <summary>
    /// Contains static helper methods.
    /// </summary>
    public static class Util {

        /// <summary>
        /// Converts an object to a generic type.
        /// </summary>
        /// <typeparam name="T">The type to which the object will be converted.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <returns>Value, converted to T.</returns>
        public static T Convert<T>(object value) {
            return (T)System.Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// Checks a value to see if it is within a given range.
        /// </summary>
        /// <typeparam name="T">A type that can be compared to itself.</typeparam>
        /// <param name="number">The number to check</param>
        /// <param name="min">The minimum value for checking range</param>
        /// <param name="max">The maximum value for checking range</param>
        /// <param name="inclusive">If true, endpoints will be considered part of the range. Default: false</param>
        /// <returns></returns>
        public static bool IsInRange<T>(this T number, T min, T max, bool inclusive = false) where T : IComparable {
            if (inclusive) return number.CompareTo(min) >= 0 && number.CompareTo(max) <= 0;
            return number.CompareTo(min) > 0 && number.CompareTo(max) < 0;
        }

        /// <summary>
        /// Inverts a boolean value. Shortcut for bool = !bool.
        /// </summary>
        /// <param name="b">Boolean value</param>
        /// <returns>Inverse of input value</returns>
        public static void Invert(ref this bool b) {
            b = !b;
        }
    }
}
