using System;
using System.Collections;
using System.Collections.Generic;

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
        /// Shortcut for String.Join(...) to make things a little more graceful
        /// </summary>
        /// <param name="en">Enumerable to join</param>
        /// <param name="delimiter">String to insert between items</param>
        /// <returns>Joined string</returns>
        public static string Join<T>(this T en, string delimiter) where T : IEnumerable {
            return string.Join(delimiter, en);
        }
        /// <summary>
        /// Shortcut for String.Join(...) to make things a little more graceful
        /// </summary>
        /// <param name="en">Enumerable to join</param>
        /// <param name="delimiter">Character to insert between items</param>
        /// <returns>Joined string</returns>
        public static string Join<T>(this T en, char delimiter) where T : IEnumerable {
            return string.Join(delimiter.ToString(), en);
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
        public static bool IsInRange<T>(this T number, T min, T max, bool inclusive = false) where T : IComparable<T> {
            if (inclusive) return number.CompareTo(min) >= 0 && number.CompareTo(max) <= 0;
            return number.CompareTo(min) > 0 && number.CompareTo(max) < 0;
        }
    }
}
