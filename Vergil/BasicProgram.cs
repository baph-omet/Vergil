using Vergil.Configuration;
using System.IO;

namespace Vergil {
    /// <summary>
    /// Basic program structure, with a Log, Config, and ProblemList
    /// </summary>
    public abstract class BasicProgram {
        /// <summary>
        /// This program's log file.
        /// </summary>
        public static Log Log { get; private set; }

        /// <summary>
        /// This program's config file.
        /// </summary>
        public static Config Config { get; private set; }

        /// <summary>
        /// The list of problems this program encounters.
        /// </summary>
        public static ProblemList Problems { get; private set; }

        /// <summary>
        /// Whether or not this program is set to run in Debug mode.
        /// </summary>
        public static bool Debug { get => Config.Debug; }

        /// <summary>
        /// Whether or not this method's Initialize() method has been called.
        /// </summary>
        protected static bool Initialized = false;

        /// <summary>
        /// Set up the fields for this program.
        /// </summary>
        public static void Initialize() {
            Log = new Log();
            Problems = new ProblemList(Log);
            if (File.Exists(Directory.GetCurrentDirectory() + "\\Config.xml")) Config = new XMLConfig();
            else if (File.Exists(Directory.GetCurrentDirectory() + "\\Config.txt")) Config = new TextConfig();
            else Config = new SettingsConfig();
            Initialized = true;
        }
    }
}
