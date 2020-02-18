using System;
using System.Collections.Generic;
using System.Data.Common;
using Vergil.Utilities;

namespace Vergil.Data.DB {
    /// <summary>
    /// Wrapper class for a List of DBDataRows.
    /// </summary>
    public class DBDataSet : List<DBDataRow> { }
    /// <summary>
    /// Wrapper class for a Dictionary of strings and strings.
    /// </summary>
    public class DBDataRow : Dictionary<string, string> { 
        /// <summary>
        /// Get value from row
        /// </summary>
        /// <typeparam name="T">Type of value to return</typeparam>
        /// <param name="name">Name of field</param>
        /// <returns>Value converted to type if it exists, else null</returns>
        public T Get<T>(string name) {
            if (!ContainsKey(name)) return default(T);
            return Util.Convert<T>(this[name]);
        }

        /// <summary>
        /// Get value as string
        /// </summary>
        /// <param name="name">Name of field</param>
        /// <returns>Value in field as string if exists, else null</returns>
        public string Get(string name) {
            if (!ContainsKey(name)) return null;
            return this[name];
        }
    }

    /// <summary>
    /// Class for containing recordset data from DBConnections.
    /// </summary>
    public class DBData {
        /// <summary>
        /// The names of each of this recordset's fields.
        /// </summary>
        public string[] Headers { get; }

        /// <summary>
        /// The raw data from the recordset. Each Dictionary in the array represents a row of data, where the name of the field is the key.
        /// </summary>
        public DBDataSet Data { get; private set; }

        /// <summary>
        /// Initialize a new DBData object using the data from the specified OdbcDataReader
        /// </summary>
        /// <param name="reader">An OdbcDataReader containing the data to store in this object</param>
        public DBData(DbDataReader reader) {
            List<string> h = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++) h.Add(reader.GetName(i));
            Headers = h.ToArray();

            DBDataSet d = new DBDataSet();
            while (reader.Read()) {
                DBDataRow row = new DBDataRow();
                for (int i = 0; i < reader.FieldCount; i++) {
                    try {
                        object value = reader.GetValue(i) ?? "#####";
                        try {
                            value = Convert.ToDateTime(value);
                        } catch (FormatException) { } catch (InvalidCastException) { }
                        row.Add(Headers[i], value is DateTime ? ((DateTime)value).ToString("MM/dd/yyyy") : value.ToString().ToString());
                    } catch (DbException) {
                        row.Add(Headers[i], "#####");
                    }
                }
                d.Add(row);
            }

            Data = d;
        }

        /// <summary>
        /// Merge another DBData object's data into this one.
        /// </summary>
        /// <param name="dataSet">A DBData object whose data will be merged into this one.</param>
        public void Merge(DBData dataSet) {
            DBDataSet d = Data;
            d.AddRange(dataSet.Data);
            Data = d;
        }
    }
}
