﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace SOPAPI.Configuration {
    /// <summary>
    /// Type for managing SOP config files. Supports reading and writing config files.
    /// </summary>
    public class TextConfig : Config {
        private readonly string Path;
        private readonly char delimiter;
        /// <summary>
        /// The delimiting character used in this config. Uses "=" by default.
        /// </summary>
        public char Delimiter { get { return delimiter; } }

        /// <summary>
        /// Initializes a new config file called "config.txt" in the current directory. File will be created if it does not exist.
        /// </summary>
        public TextConfig() : this(Directory.GetCurrentDirectory() + "\\Config.txt",'=') { }
        /// <summary>
        /// Initializes a new config file. If the file does not exist on disk, it will be created.
        /// </summary>
        /// <param name="path">The file path for the config file</param>
        /// <param name="delimiter">The desired delimiting character. Defaults to '='</param>
        public TextConfig(string path,char delimiter = '=') {
            this.delimiter = delimiter;
            this.Path = path;
            Debug = Get("debug", false);
            Validate();
        }

        /// <summary>
        /// Get a property of this config as a string.
        /// </summary>
        /// <param name="key">The property name to find.</param>
        /// <returns>The value of the specified property as a string if the property exists. Else, null.</returns>
        public override string Get(string key) {
            string[] config = File.ReadAllLines(Path);
            foreach (string line in config) if (Util.IsSignificant(line) && line.Split(delimiter)[0].ToUpper().Trim() == key.ToUpper().Trim() && line.Split(delimiter)[1].Length > 0) return line.Split(delimiter)[1].Trim();
            return null;
        }

        /// <summary>
        /// Gets all properties contained in this Config
        /// </summary>
        /// <returns>A Dictionary containing the key/value pairs contained in this Config</returns>
        public Dictionary<string,string> GetProperties() {
            Dictionary<string,string> props = new Dictionary<string,string>();
            foreach (string line in File.ReadAllLines(Path)) {
                if (IsValidPair(line)) {
                    string[] pair = line.Split(delimiter);
                    props.Add(pair[0], pair[1]);
                }
            }
            return props;
        }

        /// <summary>
        /// Overwrites the config to include a new value for the specified key. If the specified key is not found, it will be appended to the config.
        /// </summary>
        /// <param name="key">The key whose value will be overwritten</param>
        /// <param name="value">The value to assign to the specified key</param>
        public override void Set(string key, object value) {
            key = key.Trim();
            bool ovr = false;
            string[] config = File.ReadAllLines(Path);
            for (int i = 0; i < config.GetLength(0); i++) {
                string line = config[i];
                string k = line.Split(delimiter)[0].ToUpper().Trim();
                if (k == key.ToUpper()) {
                    config[i] = key + delimiter + value;
                    ovr = true;
                    break;
                }
            }
            if (!ovr) {
                string[] newconfig = new string[config.GetLength(0) + 1];
                for (int i = 0; i < config.GetLength(0); i++) newconfig[i] = config[i];
                newconfig[config.GetLength(0)] = key + delimiter + value;
                config = newconfig;
            }
            File.WriteAllLines(Path, config);
        }

        private bool Validate() {
            string problem = "";
            string[] config = File.ReadAllLines(Path);
            for (int i = 0; i < config.GetLength(0); i++) {
                if (config[i].Length > 0 && !(new char[] { ' ', '\t', '\n', '#' }).Contains(config[i][0])) {
                    if (config[i].Contains(delimiter)) {
                        string[] line = config[i].Split(delimiter);
                        if (line.Length != 2) problem = "Config files should be written with key/value pairs.";
                    } else problem = "Key/value pairs should be delimited with '" + delimiter + "'.";
                }
                if (problem.Length > 0) {
                    problem = "Incorrect format at line " + i + " in " + Path + ": " + problem;
                    throw new InvalidConfigException(problem);
                }
            }
            return true;
        }

        private bool IsValidPair(string line) {
            return Util.IsSignificant(line) && line.Contains(delimiter) && line.Split(delimiter)[1].Length > 0;
        }

        /// <summary>
        /// Delete the specified property.
        /// </summary>
        /// <param name="property">The property to delete.</param>
        public override void DeleteProperty(string property) {
            if (Get(property) != null) {
                List<string> lines = File.ReadAllLines(Path).ToList();
                foreach (string line in lines) {
                    if (line.Split(delimiter)[0].ToLower().Equals(property)) {
                        lines.Remove(line);
                        File.WriteAllLines(Path, lines.ToArray());
                        return;
                    }
                }
            }
        }
    }
}