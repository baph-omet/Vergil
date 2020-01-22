using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Vergil.Utilities {
    /// <summary>
    /// Class containing string extension methods
    /// </summary>
    public static class StringUtil {
        private static char[] commentCharacters {
            get {
                return new[]{
                    ' ',
                    '#',
                    '\t',
                    '\n'
                };
            }
        }
        /// <summary>
        /// Wildcard characters for file names.
        /// </summary>
        public static char[] FilenameWildcards {
            get {
                return new[] { '*', '?' };
            }
        }

        /// <summary>
        /// Gets the number of times target appears in source.
        /// </summary>
        /// <param name="source">The source string to search</param>
        /// <param name="target">The target character to search for</param>
        /// <returns>The number of times target is found in source. Returns 0 if not found.</returns>
        public static int CountOf(this string source, char target) {
            int total = 0;
            foreach (char c in source) if (c == target) total++;
            return total;
        }
        /// <summary>
        /// Gets the number of times target appears in source.
        /// </summary>
        /// <param name="source">The source string to search</param>
        /// <param name="target">The target string to search for</param>
        /// <returns>The number of times target is found in source. Returns 0 if not found.</returns>
        public static int CountOf(this string source, string target) {
            int total = 0;
            int i = 0;
            while (i + target.Length <= source.Length) {
                if (source.Substring(i, target.Length).Equals(target)) total++;
                i++;
            }
            return total;
        }
        /// <summary>
        /// Checks a string for inclusion of any of the target characters.
        /// </summary>
        /// <param name="source">String to check.</param>
        /// <param name="targets">Characters to find.</param>
        /// <returns>True if the source contains any of the targets.</returns>
        public static bool ContainsAny(this string source, char[] targets) {
            foreach (char target in targets) if (source.Contains(target)) return true;
            return false;
        }
        /// <summary>
        /// Checks a string for inclusion of any of the target characters.
        /// </summary>
        /// <param name="source">String to check.</param>
        /// <param name="targets">Characters to find.</param>
        /// <returns>True, if the source contains any of the targets.</returns>
        public static bool ContainsAny(this string source, string[] targets) {
            foreach (string target in targets) if (source.Contains(target)) return true;
            return false;
        }

        /// <summary>
        /// Checks a string for filename wildcard characters.
        /// </summary>
        /// <param name="source">String to check.</param>
        /// <returns>True if the source contains any filename wildcards.</returns>
        public static bool ContainsWildcards(this string source) {
            return ContainsAny(source, FilenameWildcards);
        }
        /// <summary>
        /// Checks to see if a line of a Configuration or samesuch text file contains meaningful data.
        /// </summary>
        /// <param name="line">The string to check</param>
        /// <returns>False if the line is empty, or starts with a whitespace character or comment character. Else true.</returns>
        public static bool IsSignificant(this string line) {
            return line.Length > 0 && !commentCharacters.Contains(line[0]);
        }

        /// <summary>
        /// Checks a wildcard pattern string to see if it matches another string.
        /// </summary>
        /// <param name="pattern">The wildcard pattern.</param>
        /// <param name="target">The string to check.</param>
        /// <returns>True if target matches pattern.</returns>
        public static bool MatchesWildcardPattern(this string pattern, string target) {
            return new Regex(pattern.Replace('?', '.').Replace("*", ".*")).IsMatch(target);
        }

        /// <summary>
        /// Returns a string where the first letter of each word is capitalized.
        /// </summary>
        /// <param name="str">The string to capitalize.</param>
        /// <returns>A string where each word of str is capitalized.</returns>
        public static string Capitalize(this string str) {
            return Capitalize(str, new string[] { });
        }
        /// <summary>
        /// Returns a string where the first letter of each word is capitalized. Excludes specified words.
        /// </summary>
        /// <param name="str">The string to capitalize.</param>
        /// <param name="excluded">Any words in this list will be excluded from capitalization. (case-insensitive).</param>
        /// <returns>A string where each word of str is capitalized.</returns>
        public static string Capitalize(this string str, string[] excluded) {
            for (int i = 0; i < excluded.Length; i++) excluded[i] = excluded[i].ToLower();
            StringBuilder builder = new StringBuilder();
            string[] words = str.Split(' ');
            for (int i = 0; i < words.Length; i++) {
                string s = words[i];
                if (s.Length > 0) {
                    if (excluded.Contains(s.ToLower())) builder.Append(s.ToLower());
                    else {
                        builder.Append(s.Substring(0, 1).ToUpper());
                        if (s.Length > 1) builder.Append(s.Substring(1).ToLower());
                    }
                    if (i < words.Length - 1) builder.Append(' ');
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Helper method for determining if a string is a literal representation of a number.
        /// </summary>
        /// <param name="str">A string to check</param>
        /// <returns>True if the string is at least one character long, does not contain a dash other than at the beginning, and contains only digits and up to one period.</returns>
        public static bool IsNumber(this string str) {
            if (str == null) return false;
            str = str.Trim();
            try {
                Convert.ToDouble(str);
                return true;
            } catch (Exception) { }
            if (str == null) return false;
            if (str.Length == 0) return false;
            if (str.Split('.').Length > 2) return false;
            for (int i = 0; i < str.Length; i++) {
                char c = str[i];
                if (!char.IsDigit(c) && c != '.' || (c == '-' && i > 0)) return false;
            }
            return true;
        }

        /// <summary>
        /// Finds any string representations of whitespace characters and converts them into actual whitespace characters. If none are found, str is unchanged.
        /// </summary>
        /// <param name="str">Source string.</param>
        /// <returns>str with all whitespace representations changed to whitespace characters.</returns>
        public static string ConvertWhitespaceCharacters(this string str) {
            string[] targets = new[] { @"\a", @"\b", @"\f", @"\n", @"\r", @"\t", @"\v" };
            char[] replacements = new[] { '\a', '\b', '\f', '\n', '\r', '\t', '\v' };
            for (int i = 0; i < targets.Length; i++) str = str.Replace(targets[i], replacements[i].ToString());
            return str;
        }

        /// <summary>
        /// Finds any whitespace characters and converts them into their string representations. If none are found, str is unchanged.
        /// </summary>
        /// <param name="str">Source string.</param>
        /// <returns>str with all whitespace characters converted to strings.</returns>
        public static string ConvertWhitespaceToCharacters(this string str) {
            string[] replacements = new[] { @"\a", @"\b", @"\f", @"\n", @"\r", @"\t", @"\v" };
            char[] targets = new[] { '\a', '\b', '\f', '\n', '\r', '\t', '\v' };
            for (int i = 0; i < targets.Length; i++) str = str.Replace(targets[i].ToString(), replacements[i]);
            return str;
        }

        /// <summary>
        /// Removes whitespace characters from a string.
        /// </summary>
        /// <param name="str">String to parse.</param>
        /// <returns>Copy of str with whitespace characters removed.</returns>
        public static string RemoveWhitespace(this string str) {
            char[] targets = new[] { '\a', '\b', '\f', '\n', '\r', '\t', '\v', ' ' };
            StringBuilder builder = new StringBuilder();
            foreach (char c in str) {
                if (targets.Contains(c)) continue;
                builder.Append(c);
            } return builder.ToString();
        }

        /// <summary>
        /// Extension for string Split method that takes a string as delimiter
        /// </summary>
        /// <param name="str">The source string</param>
        /// <param name="delimiter">The delimiter on which to split</param>
        /// <returns>Array of strings containing source string split on delimiter</returns>
        public static string[] Split(this string str, string delimiter) {
            List<string> splits = new List<string>();
            StringBuilder buffer = new StringBuilder();
            for (int i = 0; i < str.Length; i++) {
                if (i < str.Length - delimiter.Length && str.Substring(i, delimiter.Length) == delimiter) {
                    splits.Add(buffer.ToString());
                    buffer.Clear();
                    i += delimiter.Length - 1;
                    continue;
                }
                buffer.Append(str[i]);
            }
            splits.Add(buffer.ToString());
            return splits.ToArray();
        }

        /// <summary>
        /// Checks string equality, regardless of letter case
        /// </summary>
        /// <param name="str">String to check</param>
        /// <param name="target">String to check against</param>
        /// <returns>True if strings are the same, barring case</returns>
        public static bool EqualsIgnoreCase(this string str, string target) {
            return str.ToLower().Equals(target.ToLower());
        }


    }
}
