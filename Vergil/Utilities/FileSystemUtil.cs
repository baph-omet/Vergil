using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Vergil.Utilities {
    /// <summary>
    /// Utilities for file system checks and actions.
    /// </summary>
    public static class FileSystemUtil {
        /// <summary>
        /// Gets the first free alternate filename corresponding to the supplied path.
        /// </summary>
        /// <param name="path">A filepath to check</param>
        /// <returns>A full filepath. If no file exists by the supplied path, it will just return the supplied path, otherwise it will return a path where the filename contains a numbered discriminator in the form "(X)" before the file extension.</returns>
        public static string GetFirstFreePath(string path) {
            if (!File.Exists(path)) return path;
            path = path.Replace("/", Environment.NewLine);
            string filename = Path.GetFileName(path);
            string mainPath = Path.GetDirectoryName(path);
            string extension = Path.GetExtension(path);
            filename = filename.Replace("." + extension, "");

            int discriminator = 0;
            while (File.Exists(Path.Combine(mainPath, filename + (discriminator > 0 ? "(" + discriminator + ")" : "") + "." + extension))) discriminator++;
            return Path.Combine(mainPath, filename + (discriminator > 0 ? "(" + discriminator + ")" : "") + "." + extension);
        }

        /// <summary>
        /// Creates a directory tree from a filepath, ignoring a filename component, if one is included.
        /// </summary>
        /// <param name="path">Any file or directory path. If a this is the path to a file, the filename will be ignored and only directories will be created.</param>
        /// <returns>True if any directories were created, otherwise false.</returns>
        public static bool CreateDirectories(string path) {
            path = path.Replace("/", Environment.NewLine);
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
        /// <param name="overwriteFile">If True, any files in the destination folder will be overwritten, otherwise conflicting source files will be ignored.</param>
        public static void RecursiveCopy(string SourceDirectory, string DestinationDirectory, bool overwriteFile = true) {
            if (!Directory.Exists(DestinationDirectory)) Directory.CreateDirectory(DestinationDirectory);
            foreach (string f in Directory.GetFiles(SourceDirectory)) {
                string filePath = Path.Combine(DestinationDirectory, Path.GetFileName(f));
                if (overwriteFile || !File.Exists(filePath)) File.Copy(f, filePath, overwriteFile);
            }
            foreach (string d in Directory.GetDirectories(SourceDirectory)) RecursiveCopy(d, Path.Combine(DestinationDirectory, Path.GetFileName(d)), overwriteFile);
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
