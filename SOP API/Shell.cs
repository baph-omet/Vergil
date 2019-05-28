using System;
using System.Diagnostics;
using System.IO;

namespace SOPAPI {
    /// <summary>
    /// Class for automating Windows shell commands
    /// </summary>
    [Obsolete("Shell class is deprecated due to limited feasibility. Please use native .NET code whenever possible.")]
    public class Shell {
        /// <summary>
        /// Silently calls a shell with the given command
        /// </summary>
        /// <param name="command">The command to be passed to the shell</param>
        public static void Call(string command) {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/C " + command
            };
            process.StartInfo = startInfo;
            process.Start();
        }

        /// <summary>
        /// Executes an external batch file
        /// </summary>
        /// <param name="batchFile">The full path to the desired batch file.</param>
        public static void Execute(string batchFile) {
            if (File.Exists(batchFile) && batchFile.Split('.')[1].ToLower().Equals("bat")) System.Diagnostics.Process.Start(batchFile);
            else throw new ArgumentException(batchFile + " is not a valid batch file.");
        }
    }
}
