using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Vergil {
    /// <summary>
    /// Variable levels of severity for messages written to the log
    /// </summary>
    public enum Severity {
        /// <summary>
        /// Standard severity level, used for regular runtime information
        /// </summary>
        INFO,
        /// <summary>
        /// Low severity level, used for messages shown only during program debug
        /// </summary>
        DEBUG,
        /// <summary>
        /// Elevated severity level, used for irregular conditions that do not compromise function of the program
        /// </summary>
        WARNING,
        /// <summary>
        /// Maximum severity level, used for critical conditions that compromise the function of the program
        /// </summary>
        SEVERE
    };

    /// <summary>
    /// Class used for managing information logging for programs. Auto-formats log messages and supports writing information at different serverity levels.
    /// </summary>
    public class Log : IDisposable {
        private string logName;
        private string filepath;
        private string fullpath;
        private StreamWriter writer;

        /// <summary>
        /// Creates a log file with the name "log.txt" in the current directory.
        /// </summary>
        public Log() : this(Directory.GetCurrentDirectory() + "\\Log.txt") { }
        /// <summary>
        /// Creates a log file with the name "log.txt" at the specified location. If the file does not exist, it will be created.
        /// </summary>
        /// <param name="pathToFile">The path of the folder containing the log file</param>
        public Log(string pathToFile) {
            if (pathToFile.Contains('.')) {
                string[] dirTree = string.Join("\\", pathToFile.Split('/')).Split('\\');
                logName = dirTree[dirTree.Length - 1];
                filepath = pathToFile.Remove(pathToFile.Length - logName.Length);
            } else {
                logName = "Log.txt";
                filepath = pathToFile;
            }
            fullpath = filepath + logName;
            if (File.Exists(fullpath)) {
                if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\Logs")) Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Logs");
                string archivepath = Util.GetFirstFreePath(Directory.GetCurrentDirectory() + "\\Logs\\" + File.GetLastWriteTime(fullpath).ToString("yyyy-MM-dd") + ".txt");
                File.WriteAllLines(archivepath,File.ReadAllLines(fullpath));
            } else File.Create(fullpath);
            writer = new StreamWriter(fullpath);
            writer.AutoFlush = true;

            Write("Starting " + Assembly.GetEntryAssembly().GetName().Name + ".");
        }

        /// <summary>
        /// Writes a blank line to the log
        /// </summary>
        public void Write() {
            writer.WriteLine();
        }
        /// <summary>
        /// Writes a single line to the log file at INFO-level severity.
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        public void Write(string message) {
            writer.WriteLine(DateTime.Now.ToString("(yyyy-MM-dd HH:mm:ss)") + " [INFO] " + message); 
        }
        /// <summary>
        /// Writes a single line to the log file at the specified level of severity.
        /// </summary>
        /// <param name="level">The level of severity of the message (INFO, WARNING, SEVERE)</param>
        /// <param name="message">The message to write to the log</param>
        public void Write(Severity level, string message) {
            writer.WriteLine(DateTime.Now.ToString("(yyyy-MM-dd HH:mm:ss)") + " [" + level.ToString() + "] " + message); 
        }
        /// <summary>
        /// Writes a single line to the log file at DEBUG-level severity if debug is true.
        /// </summary>
        /// <param name="debug">If true the message will be written</param>
        /// <param name="message">The message to write to the log</param>
        public void Write(bool debug, string message) {
            if (debug) writer.WriteLine(DateTime.Now.ToString("(yyyy-MM-dd HH:mm:ss)") + " [DEBUG] " + message);
        }
        /// <summary>
        /// Writes an INFO-level line to the log containing the string representation of an arbitrary object
        /// </summary>
        /// <param name="obj">Arbitrary object</param>
        public void Write(Object obj) {
            writer.WriteLine(DateTime.Now.ToString("(yyyy-MM-dd HH:mm:ss)") + " [INFO] " + obj.ToString());
        }
        /// <summary>
        /// Writes a single line to the log at the specified severity level containing the string representation of an arbitrary object
        /// </summary>
        /// <param name="level">Severity level</param>
        /// <param name="obj">Arbitrary object</param>
        public void Write(Severity level,Object obj) {
            writer.WriteLine(DateTime.Now.ToString("(yyyy-MM-dd HH:mm:ss)") + " [" + level.ToString() + "] " + obj.ToString());
        }
        /// <summary>
        /// If debug is true, writes a single line to the log at DEBUG-level containing the string representation of an arbitrary object
        /// </summary>
        /// <param name="debug">If true, the message will be written</param>
        /// <param name="obj">Arbitrary object</param>
        public void Write(bool debug, Object obj) {
            if (debug) writer.WriteLine(DateTime.Now.ToString("(yyyy-MM-dd HH:mm:ss)") + " [DEBUG] " + obj.ToString());
        }
        /// <summary>
        /// Writes a Problem object to the log. If the Problem contains an Exception, the stack trace for the Exception is printed as well.
        /// </summary>
        /// <param name="p">A Problem object to write to the log.</param>
        public void Write(Problem p) {
            writer.WriteLine(DateTime.Now.ToString("(yyyy-MM-dd HH:mm:ss)") + " [" + p.Severity.ToString() + "] " + p.Message);
            if (p.Exception != null) writer.WriteLine(p.Exception.ToString());
        }

        /// <summary>
        /// Logs an exception with a custom message.
        /// </summary>
        /// <param name="message">The message to add to this exception</param>
        /// <param name="e">The exception to log</param>
        public void Exception(string message, Exception e) {
            Exception(message, e, Severity.SEVERE);
        }
        /// <summary>
        /// Logs an exception with a custom message at the specified severity level.
        /// </summary>
        /// <param name="message">The message to add to this exception</param>
        /// <param name="e">The exception to log</param>
        /// <param name="level">The Severity level to use</param>
        public void Exception(string message, Exception e, Severity level) {
            Write(level, message + '\n' + e.ToString());
        }

        /// <summary>
        /// Saves a copy of this Log to \Logs\YYYY-MM-DD.txt.
        /// </summary>
        public void Archive() {Archive(Directory.GetCurrentDirectory() + "\\Logs");}
        /// <summary>
        /// Saves a copy of this Log as YYYY-MM-DD.txt in the specified directory.
        /// </summary>
        /// <param name="archiveDirectory">The directory in which to save a copy of this Log.</param>
        public void Archive(string archiveDirectory) {
            if (!Directory.Exists(archiveDirectory)) Directory.CreateDirectory(archiveDirectory);
            File.WriteAllLines(Util.GetFirstFreePath(archiveDirectory + DateTime.Now.ToString("yyyy-MM-dd") + ".txt"), GetLines());
        }

        /// <summary>
        /// Returns a process-safe copy of the lines in this Log file. May not be fully complete at time of call.
        /// </summary>
        /// <returns>An array containing each line that has been written to this Log at the time of call</returns>
        public string[] GetLines() {
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(File.Open(fullpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))) {
                while (!reader.EndOfStream) {
                    lines.Add(reader.ReadLine());
                }
            } return lines.ToArray();
        }

        /// <summary>
        /// Gets this Log's file path. Does not include the filename.
        /// </summary>
        /// <returns>This Log's file path</returns>
        public string GetPath() {
            return filepath;
        }

        /// <summary>
        /// Closes the output stream and archives this Log.
        /// </summary>
        public void Dispose() {
            try {
                Write(Assembly.GetEntryAssembly().GetName().Name + " stopped.");
            } catch (Exception) { }
            writer.Close();
            Archive();
        }
    }
}
