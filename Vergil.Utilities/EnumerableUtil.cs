using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vergil.Utilities {
    public static class EnumerableUtil {

        /// <summary>
        /// Shortcut for String.Join(...) to make things a little more graceful
        /// </summary>
        /// <param name="en">Enumerable to join</param>
        /// <param name="delimiter">String to insert between items</param>
        /// <returns>Joined string</returns>
        public static string Join<T>(this IEnumerable<T> en, string delimiter) {
            return string.Join(delimiter, en);
        }
        /// <summary>
        /// Shortcut for String.Join(...) to make things a little more graceful
        /// </summary>
        /// <param name="en">Enumerable to join</param>
        /// <param name="delimiter">Character to insert between items</param>
        /// <returns>Joined string</returns>
        public static string Join<T>(this IEnumerable<T> en, char delimiter = ',') {
            return string.Join(delimiter.ToString(), en);
        }

        /// <summary>
        /// Inverse of Any().
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="en"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static bool None<T>(this IEnumerable<T> en, Func<T, bool> predicate) {
            return !en.Any(predicate);
        }

        /// <summary>
        /// Inverse of Any().
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="en"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static bool None<T>(this IQueryable<T> en, Func<T, bool> predicate) {
            return !en.Any(predicate);
        }

        /// <summary>
        /// Inverse of Any().
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="en"></param>
        /// <returns></returns>
        public static bool None<T>(this IQueryable<T> en) {
            return !en.Any();
        }

        /// <summary>
        /// Checks to see if a collection of strings contains a target string, regardless of case.
        /// </summary>
        /// <param name="en">Source collection</param>
        /// <param name="target">Target string</param>
        /// <returns>True if source collection contains any instance of target string</returns>
        public static bool ContainsIgnoreCase(this IEnumerable<string> en, string target) {
            return en.FirstOrDefault(x => x.EqualsIgnoreCase(target)) is not null;
        }

        public static T RandomElement<T>(this IEnumerable<T> en, Random? rng = null) {
            if (en.Count() == 0) throw new ArgumentOutOfRangeException(nameof(en));
            rng ??= new Random();

            return en.ElementAt(rng.Next(en.Count()));
        }
    }
}
