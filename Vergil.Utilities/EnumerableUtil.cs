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
        /// Checks to see if a collection of strings contains a target string, regardless of case.
        /// </summary>
        /// <param name="en">Source collection</param>
        /// <param name="target">Target string</param>
        /// <returns>True if source collection contains any instance of target string</returns>
        public static bool ContainsIgnoreCase(this IEnumerable<string> en, string target) {
            return en.FirstOrDefault(x => x.EqualsIgnoreCase(target)) is not null;
        }

        /// <summary>
        /// Returns a random element from the enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="en"></param>
        /// <param name="rng">Optional parameter to supply custom random number generator.</param>
        /// <returns>An element of the enumerable at a random index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the enumerable is empty.</exception>
        public static T RandomElement<T>(this IEnumerable<T> en, Random? rng = null) {
            IEnumerable<T> enumerable = en.ToList();
            if (!enumerable.Any()) throw new ArgumentOutOfRangeException(nameof(en));
            rng ??= new Random();

            return enumerable.ElementAt(rng.Next(enumerable.Count()));
        }

        /// <summary>
        /// Checks an enumerable to see if a specified index is contained in the enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="en"></param>
        /// <param name="index"></param>
        /// <returns>True if <code>index</code> corresponds to a valid element in the enumerable, else false.</returns>
        public static bool IsValidIndex<T>(this IEnumerable<T> en, int index) {
            return en.ElementAtOrDefault(index) is not null;
        }

        /// <summary>
        /// Removes all instances of target string, case-insensitive.
        /// </summary>
        /// <param name="en">Source list</param>
        /// <param name="target">Target string to remove</param>
        /// <returns>Whether any instances of target string are removed.</returns>
        public static bool RemoveIgnoreCase(this List<string> en, string target) {
            return en.RemoveAll(x=>x.Equals(target, StringComparison.InvariantCultureIgnoreCase)) > 0;
        }
    }
}
