using System;
using System.Linq;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
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

        /// <summary>
        /// Shortcut for String.Join(...) to make things a little more graceful
        /// </summary>
        /// <param name="en">Enumerable to join</param>
        /// <param name="delimiter">String to insert between items</param>
        /// <returns>Joined string</returns>
        public static string Join(this IEnumerable<object> en, string delimiter) {
            return string.Join(delimiter, en);
        }
        /// <summary>
        /// Shortcut for String.Join(...) to make things a little more graceful
        /// </summary>
        /// <param name="en">Enumerable to join</param>
        /// <param name="delimiter">Character to insert between items</param>
        /// <returns>Joined string</returns>
        public static string Join(this IEnumerable<object> en, char delimiter) {
            return string.Join(delimiter.ToString(), en);
        }
    }
}
