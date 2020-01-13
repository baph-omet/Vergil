using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Security.AccessControl;
using System.Security.Principal;

namespace SOPAPI {
    /// <summary>
    /// Contains static helper methods.
    /// </summary>
    public static class Util {
        /// <summary>
        /// Wildcard characters for file names.
        /// </summary>
        public static char[] FilenameWildcards {
            get {
                return new[] { '*', '?' };
            }
        }

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
        /// Gets the first free alternate filename corresponding to the supplied path.
        /// </summary>
        /// <param name="path">A filepath to check</param>
        /// <returns>A full filepath. If no file exists by the supplied path, it will just return the supplied path, otherwise it will return a path where the filename contains a numbered discriminator in the form "(X)" before the file extension.</returns>
        public static string GetFirstFreePath(string path) {
            if (!File.Exists(path)) return path;
            path = path.Replace('/','\\');
            string filename = path.Split('\\')[path.Split('\\').Length - 1];
            string mainPath = path.Replace("\\" + filename, "");
            string extension = filename.Split('.')[filename.Split('.').Length-1];
            filename = filename.Replace("." + extension, "");

            int discriminator = 0;
            while (File.Exists(mainPath + "\\" + filename + (discriminator > 0 ? "(" + discriminator + ")" : "") + "." + extension)) discriminator++;
            return mainPath + "\\" + filename + (discriminator > 0 ? "(" + discriminator + ")" : "") + "." + extension;
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
            } return total;
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
        /// Checks a wildcard pattern string to see if it matches another string.
        /// </summary>
        /// <param name="pattern">The wildcard pattern.</param>
        /// <param name="target">The string to check.</param>
        /// <returns>True if target matches pattern.</returns>
        public static bool MatchesWildcardPattern(string pattern, string target) {
            return new Regex(pattern.Replace('?', '.').Replace("*", ".*")).IsMatch(target);
        }

        /// <summary>
        /// Checks to see if a line of a Configuration or samesuch text file contains meaningful data.
        /// </summary>
        /// <param name="line">The string to check</param>
        /// <returns>False if the line is empty, or starts with a whitespace character or comment character. Else true.</returns>
        public static bool IsSignificant(string line) {
            return line.Length > 0 && !commentCharacters.Contains(line[0]);
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
            } return builder.ToString();
        }

        /// <summary>
        /// Creates a directory tree from a filepath, ignoring a filename component, if one is included.
        /// </summary>
        /// <param name="path">Any file or directory path. If a this is the path to a file, the filename will be ignored and only directories will be created.</param>
        /// <returns>True if any directories were created, otherwise false.</returns>
        public static bool CreateDirectories(string path) {
            path = path.Replace('/', '\\');
            string dirpath = Path.GetDirectoryName(path);
            if (Directory.Exists(dirpath)) return false;
            Directory.CreateDirectory(dirpath);
            return true;
        }

        /// <summary>
        /// Copies all files and folders inside SourceDirectory to DestinationDirectory.
        /// </summary>
        /// <param name="SourceDirectory">The folder whose contents will be copied.</param>
        /// <param name="DestinationDirectory">The folder to which files will be copied.</param>
        public static void RecursiveCopy(string SourceDirectory, string DestinationDirectory) {
            RecursiveCopy(SourceDirectory, DestinationDirectory, true);
        }
        /// <summary>
        /// Copies all files and folders inside SourceDirectory to DestinationDirectory.
        /// </summary>
        /// <param name="SourceDirectory">The folder whose contents will be copied.</param>
        /// <param name="DestinationDirectory">The folder to which files will be copied.</param>
        /// <param name="overwriteFile">If True, any files in the destination folder will be overwritten, otherwise conflicting source files will be ignored.</param>
        public static void RecursiveCopy(string SourceDirectory, string DestinationDirectory, bool overwriteFile) {
            if (!Directory.Exists(DestinationDirectory)) Directory.CreateDirectory(DestinationDirectory);
            foreach (string f in Directory.GetFiles(SourceDirectory)) {
                if (overwriteFile || !File.Exists(DestinationDirectory + "\\" + f.Split('\\').Last())) {
                    File.Copy(f, DestinationDirectory + "\\" + f.Split('\\').Last(), overwriteFile);
                }
            }
            foreach (string d in Directory.GetDirectories(SourceDirectory)) RecursiveCopy(d, DestinationDirectory + "\\" + d.Split('\\').Last(), overwriteFile);
        }

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
        public static string EnumName<T>(T value) where T : Enum {
            return Enum.GetName(typeof(T), value);
        }

        /// <summary>
        /// Helper method for determining if a string is a literal representation of a number.
        /// </summary>
        /// <param name="str">A string to check</param>
        /// <returns>True if the string is at least one character long, does not contain a dash other than at the beginning, and contains only digits and up to one period.</returns>
        public static bool IsNumber(this string str) {
            str = str.Trim();
            try {
                System.Convert.ToDouble(str);
                return true;
            } catch (Exception) { }
            if (str == null) return false;
            if (str.Length == 0) return false;
            if (str.Split('.').Length > 2) return false;
            for (int i = 0; i < str.Length; i++) {
                char c = str[i];
                if (!Char.IsDigit(c) && c != '.' || (c == '-' && i > 0)) return false;
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
        /// Checks to see if the user has write access to the specified folder.
        /// </summary>
        /// <param name="path">The directory path to check.</param>
        /// <returns>True if the user has write permission to the directory, else false.</returns>
        public static bool HasWriteAccess(string path) {
            return CheckFolderPermissions(path, FileSystemRights.Write);
        }

        /// <summary>
        /// Checks to see if the user has read access to the specified folder.
        /// </summary>
        /// <param name="path">The directory path to check.</param>
        /// <returns>True if the user has read permission to the directory, else false.</returns>
        public static bool HasReadAccess(string path) {
            return CheckFolderPermissions(path, FileSystemRights.Read);
        }

        /// <summary>
        /// Checks to see that the current user has specified permissions to the specified directory location.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <param name="accessType">The access type to check for.</param>
        /// <returns>True if </returns>
        public static bool CheckFolderPermissions(string path, FileSystemRights accessType) {
            try {
                AuthorizationRuleCollection collection = Directory.GetAccessControl(Path.GetDirectoryName(path)).GetAccessRules(true, true, typeof(NTAccount));
                foreach (FileSystemAccessRule rule in collection) if ((rule.FileSystemRights & accessType) > 0) return true;
                return true;
            } catch (Exception) {
                return false;
            }
        }

    }
}
