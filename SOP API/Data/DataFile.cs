using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SOPAPI.XML;
using System.Globalization;

namespace SOPAPI.Data {
    /// <summary>
    /// A class for flexibly interpreting files containing rows of data.
    /// </summary>
    public class DataFile {
        /// <summary>
        /// The file path of this DataFile.
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// The database table associated with this DataFile
        /// </summary>
        public string Table { get; set; }
        /// <summary>
        /// The Format type associated with this DataFile
        /// </summary>
        public DataFileFormat Format { get; set; }
        /// <summary>
        /// A list of keys for this DataFile that translate between file headers and their respective field names in the Table
        /// </summary>
        public List<Key> Keys { get; set; }
        /// <summary>
        /// The data stored in this file. Each key in the Dictionary represents a header, and each List represents the values under that header, in order. Objects in the lists are converted to thier respective types based on the value of the Format property of the respective Key in the Keys list.
        /// </summary>
        public Dictionary<string, List<object>> Data;

        /// <summary>
        /// Initialize a new DataFile that points to a specific file.
        /// </summary>
        /// <param name="path">The file path of this file.</param>
        /// <param name="table">The associated table in the database.</param>
        /// <param name="format">The associated Format type</param>
        /// <param name="keys">The associated Keys</param>
        /// <param name="skipFormatErrors">Whether or not to skip any </param>
        public DataFile(string path, string table, DataFileFormat format, List<Key> keys, bool skipFormatErrors=false) {
            string name = path.Split('\\')[path.Split('\\').Length - 1];
            string dir = path.Substring(0, path.Length - name.Length-1);
            string[] files = Directory.GetFiles(dir,name);
            if (files.Length == 0) throw new ArgumentException("Can't find any file at path " + path);
            else if (files.Length > 0) {
                string mostRecentPath = "";
                foreach (string file in files) if (mostRecentPath == "" || File.GetLastWriteTime(file) > File.GetLastWriteTime(mostRecentPath)) mostRecentPath = file;
                path = mostRecentPath;
            }
            Path = path;
            Table = table;
            Format = format;
            Keys = keys;
            Data = new Dictionary<string, List<object>>();
            if (Format.Type == DataFileFormatType.CSV) {
                string[] lines = File.ReadAllLines(Path);
                List<string> h = new List<string>();
                foreach (Key k in Keys) Data.Add(k.Field.Trim(), new List<object>());
                
                for (int i = Format.DataStart; i < lines.Length; i++) {
                    if (lines[i].Trim().Length == 0 || !lines[i].Contains(',')) continue;
                    string[] line = lines[i].Split(',');
                    for (int j = 0; j < Data.Keys.Count; j++) {
                        if (j >= line.Length) break;
                        object value = null;
                        try {
                            value = ConvertToValueType(line[j], Keys[j]);
                        } catch (FormatException e) {
                            if (skipFormatErrors) continue;
                            throw new FormatException("Invalid data: \"" + line[j] + "\" on line " + i + " for key " + Keys[j].Header + " in " + Path + ".",e);
                        }
                        Data[Data.Keys.ElementAt(j)].Add(value);
                    }
                }
            } else if (Format.Type == DataFileFormatType.XML) {
                Data = new Dictionary<string, List<object>>();
                foreach (Key key in keys) {
                    if (key.Field != null) Data.Add(key.Field, new List<object>());
                    else if (key.Header != null) Data.Add(key.Header, new List<object>());
                }

                XmlFormat f = (XmlFormat) format;
                XMLFile xfile = new XMLFile(Path);
                XMLSection parent = xfile.FindSection(f.ParentNode);
                if (parent == null) throw new ArgumentException("Could not find specified parent node.");
                foreach (XMLSection section in parent.GetSections(f.ChildNode)) {
                    foreach (Key k in Keys) {
                        object value = null;
                        try {
                            value = ConvertToValueType(section.Get(k.Header), k);
                        } catch(FormatException e) {
                            throw new FormatException("Could not parse value " + section.Get(k.Header) + " in " + Path + ".", e);
                        }
                        Data[k.Field].Add(value);
                    }
                }
            }
        }

        /// <summary>
        /// Get the Key that corresponds to the given header name.
        /// </summary>
        /// <param name="header">The name of one of this file's headers. Case-insensitive.</param>
        /// <returns>A Key whose header matches the given string. Returns null if no matching Key found.</returns>
        public Key GetKey(string header) {
            foreach (Key k in Keys) if (k.Header.ToLower().Equals(header.ToLower())) return k;
            return null;
        }

        /// <summary>
        /// Gets an element from the data..
        /// </summary>
        /// <param name="field">The name of the field in which to look.</param>
        /// <param name="row">The row on which to look.</param>
        /// <returns>The value at the specified location.</returns>
        public object Get(string field, int row) {
            if (!Data.ContainsKey(field)) throw new ArgumentException("Field " + field + " not found.");
            if (row < 0 || row >= Data[field].Count) throw new ArgumentException("Row index " + row + " out of bounds. Row count: " + Data[field].Count + ".");
            return Data[field][row];
        }
        /// <summary>
        /// Gets an element from the data.
        /// </summary>
        /// <param name="column">The column index on which to look.</param>
        /// <param name="row">The row on which to look.</param>
        /// <returns>The value at the specified location.</returns>
        public object Get(int column, int row) {
            if (column < 0 || column >= Data.Keys.Count) throw new ArgumentException("Column index " + column + " out of bounds. Column count: " + Data.Keys.Count + ".");
            return Get(Data.Keys.ElementAt(column), row);
        }
        /// <summary>
        /// Gets an element from the data and attempts to convert it to the specified type.
        /// </summary>
        /// <typeparam name="T">The expected type.</typeparam>
        /// <param name="field">The name of the field in which to look.</param>
        /// <param name="row">The row on which to look.</param>
        /// <returns>The value at the specified location, converted to the specified type, if possible.</returns>
        public T Get<T>(string field, int row) {
            object value = Get(field, row);
            if (!(value is T)) throw new ArgumentException("Value " + value.ToString() + " is not of type " + typeof(T).ToString());
            return Util.Convert<T>(value);
        }
        /// <summary>
        /// Gets an element from the data and attempts to convert it to the specified type.
        /// </summary>
        /// <typeparam name="T">The expected type.</typeparam>
        /// <param name="column">The column index on which to look.</param>
        /// <param name="row">The row on which to look.</param>
        /// <returns>The value at the specified location, converted to the specified type, if possible.</returns>
        public T Get<T>(int column, int row) {
            object value = Get(column, row);
            if (!(value is T)) throw new ArgumentException("Value " + value.ToString() + " is not of type " + typeof(T).ToString());
            return Util.Convert<T>(value);
        }

        /// <summary>
        /// Converts the value at the specified location to a string, with its respective formatting characters.
        /// </summary>
        /// <param name="field">The field in which to look.</param>
        /// <param name="row">The row in which to look.</param>
        /// <returns>The value at the specified location as a string, enclosed by its respective formatting characters, if any.</returns>
        public string ToString(string field, int row) {
            object value = Get(field, row);
            if (value == null) return "";
            Key key = null;
            foreach (Key k in Keys) if (k.Field.ToUpper() == field.ToUpper()) key = k;
            if (key == null) throw new ArgumentException("Field " + field + " not found.");
            if (value is DateTime) value = ((DateTime)value).ToString("MM/dd/yyyy");
            if (key.Format != '\0') return key.Format + value.ToString() + key.Format;
            return value.ToString();
        }
        /// <summary>
        /// Converts the value at the specified location to a string, with its respective formatting characters.
        /// </summary>
        /// <param name="column">The column in which to look.</param>
        /// <param name="row">The row in which to look.</param>
        /// <returns>The value at the specified location as a string, enclosed by its respective formatting characters, if any.</returns>
        public string ToString(int column, int row) {
            if (column < 0 || column >= Data.Keys.Count) throw new ArgumentException("Column index out of bounds.");
            return ToString(Data.Keys.ElementAt(column), row);
        }

        private object ConvertToValueType(string value, Key key) {
            value = value.Trim();
            //try {
                switch (key.Format) {
                    case '#':
                        DateTime date;
                        try {
                            date = Convert.ToDateTime(value);
                        } catch(FormatException e) {
                            if (key.DateFormat == null || key.DateFormat.Length == 0)
                                throw e;
                            date = DateTime.ParseExact(value, key.DateFormat, CultureInfo.CurrentCulture);
                        } return date;
                        
                    case '\'':
                        return value;
                    default:
                        if (value.Length == 0) return 0D;
                        string val = value.Split(':')[0];
                        if (!Util.IsNumber(val)) return value;
                        double v = Convert.ToDouble(val);
                        if (key.Calculation != null) return key.Calculation.Calculate(v);
                        return v;
                }
            //} catch(FormatException) { return null; }
        }
    }
}
