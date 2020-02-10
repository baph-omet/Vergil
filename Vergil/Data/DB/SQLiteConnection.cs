using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using Vergil.Utilities;
using SQLiteDataConnection = System.Data.SQLite.SQLiteConnection;

namespace Vergil.Data.DB {
    /// <summary>
    /// Wrapper class for SQLite connections
    /// </summary>
    public class SQLiteConnection : DBConnection {
        /// <summary>
        /// Create a new connection to a SQLite database. Will fail if database does not exist.
        /// </summary>
        /// <param name="location"></param>
        public SQLiteConnection(string location) : base("Data Source=" + location + "; Version=3; FailIfMissing=True;") {
            if (!File.Exists(location)) throw new ArgumentException("Database must already exist.");
            connectionObject = new SQLiteDataConnection(ConnectionString);
        }

        /// <summary>
        /// Create a new SQLite database.
        /// </summary>
        /// <param name="location">File location to create database</param>
        public static void CreateDatabase(string location) {
            SQLiteDataConnection.CreateFile(location);
        }

        /// <summary>
        /// Returns a List of field names for the specified table (or query).
        /// </summary>
        /// <param name="table">The name of the table (or query) to check.</param>
        /// <returns>A List containing the names of each of the specified table's fields.</returns>
        public new List<string> GetFields(string table) {
            List<string> fields = new List<string>();
            using (SQLiteCommand cmd = new SQLiteCommand($"PRAGMA table_info({table})", (SQLiteDataConnection)connectionObject)) {
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) fields.Add(reader.GetString(1));
                reader.Close();
            }
            return fields;
        }

        /// <summary>
        /// Implementation for generating a query command for the database type to insert records.
        /// </summary>
        /// <param name="table">The table to update</param>
        /// <param name="fields">The fields to update</param>
        /// <param name="values">The values to update in above fields</param>
        /// <returns>Created query command</returns>
        protected override DbCommand GenerateInsertQuery(string table, IEnumerable<string> fields, IEnumerable<string> values) {
            if (GetDataViewNames()[table] == DataViewType.View) throw new ArgumentException("'" + table + "' is a view. SQLite only supports write operations on tables.");

            StringBuilder insertQuery = new StringBuilder("INSERT INTO ");
            insertQuery.Append(table);

            if (fields.Count() > 0 && !fields.ElementAt(0).Contains('*')) insertQuery.Append(" (" + fields.Join(',') + ")");
            insertQuery.Append(" VALUES (");
            for (int i = 0; i < values.Count(); i++) {
                if (i > 0) insertQuery.Append(',');
                string v = values.ElementAt(i);
                if (!v.IsNumber()) {
                    insertQuery.Append('\'');
                    insertQuery.Append(v.Replace("'", "''"));
                    insertQuery.Append('\'');
                } else insertQuery.Append(v);
            }

            insertQuery.Append(");");
            return new SQLiteCommand(insertQuery.ToString(), (SQLiteDataConnection)connectionObject);
        }

        /// <summary>
        /// Implementation for generating a query command for the database type to select records.
        /// </summary>
        /// <param name="table">The table to update</param>
        /// <param name="fields">The fields to update</param>
        /// <param name="selectCondition">The condition, as a string of SQL, that determines if a row is selected</param>
        /// <param name="distinct">If true, only returns distinct results</param>
        /// <returns>Created query command</returns>
        protected override DbCommand GenerateSelectQuery(string table, IEnumerable<string> fields, string selectCondition, bool distinct) {
            StringBuilder selectQuery = new StringBuilder("SELECT ");
            if (distinct) selectQuery.Append("DISTINCT ");
            selectQuery.Append(fields.Join());
            selectQuery.Append(" FROM " + table);
            if (!string.IsNullOrEmpty(selectCondition)) selectQuery.Append(" WHERE " + selectCondition);
            selectQuery.Append(";");

            return new SQLiteCommand(selectQuery.ToString(), (SQLiteDataConnection)connectionObject);
        }

        /// <summary>
        /// Implementation for generating a query command for the database type to update records.
        /// </summary>
        /// <param name="table">The table to update</param>
        /// <param name="fields">The field to update</param>
        /// <param name="values">The values to update in above fields</param>
        /// <param name="updateCondition">The condition, as a string of SQL, that determines if a row is updated</param>
        /// <returns>Created query command</returns>
        protected override DbCommand GenerateUpdateQuery(string table, IEnumerable<string> fields, IEnumerable<string> values, string updateCondition) {
            if (GetDataViewNames()[table] == DataViewType.View) throw new ArgumentException("'" + table + "' is a view. SQLite only supports write operations on tables.");
            if (string.IsNullOrEmpty(updateCondition)) throw new ArgumentException("UpdateCondition is mandatory");
            StringBuilder updateQuery = new StringBuilder("UPDATE " + table + " SET ");
            if (fields.Count() == 0) throw new ArgumentException("Must specify at least one field to update");
            if (fields.Count() == 1 && fields.ElementAt(0).Contains("*")) fields = GetFields(table);
            if (values.Count() != fields.Count()) throw new ArgumentException("Must specify the same number of values as fields");

            for (int i = 0; i < fields.Count(); i++) {
                string field = fields.ElementAt(i);
                if (i > 0) updateQuery.Append(',');
                if (!field.IsNumber()) {
                    if (field[0] != '\'') updateQuery.Append('\'');
                    updateQuery.Append(field);
                    if (field.Last() != '\'') updateQuery.Append('\'');
                } else updateQuery.Append(field);

                updateQuery.Append("=");

                string value = values.ElementAt(i);
                if (!value.IsNumber()) {
                    updateQuery.Append(string.Format("'{0}'", value.Replace("'", "''")));
                } else updateQuery.Append(value);
            }

            updateQuery.Append(" WHERE " + updateCondition + ";");
            return new SQLiteCommand(updateQuery.ToString(), (SQLiteDataConnection)connectionObject);
        }
    }
}
