using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
            if (path is null) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) return path;
            path = path.Replace("/", Environment.NewLine);
            string filename = Path.GetFileNameWithoutExtension(path) ?? throw new ArgumentException("Not found.", nameof(path));
            string mainPath = Path.GetDirectoryName(path) ?? throw new ArgumentException("Directory not found.", nameof(path));
            string extension = Path.GetExtension(path);

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
            string dirpath = Path.GetDirectoryName(path) ?? throw new ArgumentException("Directory not found.", nameof(path));
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
    }
}
