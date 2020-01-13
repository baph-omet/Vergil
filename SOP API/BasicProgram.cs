using Vergil.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Vergil {
    /// <summary>
    /// Basic program structure, with a Log, Config, and ProblemList
    /// </summary>
    public abstract class BasicProgram {
        private static Log log;
        /// <summary>
        /// This program's log file.
        /// </summary>
        public static Log Log { get => log; }

        private static Config config;
        /// <summary>
        /// This program's config file.
        /// </summary>
        public static Config Config { get => config; }

        private static ProblemList problems;
        /// <summary>
        /// The list of problems this program encounters.
        /// </summary>
        public static ProblemList Problems { get => problems; }

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
            log = new Log();
            problems = new ProblemList(log);
            if (File.Exists(Directory.GetCurrentDirectory() + "\\Config.xml")) config = new XMLConfig();
            else if (File.Exists(Directory.GetCurrentDirectory() + "\\Config.txt")) config = new TextConfig();
            else config = new SettingsConfig();
            Initialized = true;
        }
    }
}
