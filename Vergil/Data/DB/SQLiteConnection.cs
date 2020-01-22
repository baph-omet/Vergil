using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.Common;
using Vergil.Utilities;
using SQLiteDataConnection = System.Data.SQLite.SQLiteConnection;
using System.IO;

namespace Vergil.Data.DB {
    /// <summary>
    /// Wrapper class for SQLite connections
    /// </summary>
    public class SQLiteConnection : DBConnection {
        /// <summary>
        /// Create a new connection to a SQLite database. Will fail if database does not exist.
        /// </summary>
        /// <param name="location"></param>
        public SQLiteConnection(string location) : base ("Data Source=" + location + "; Version=3; FailIfMissing=True;") {
            if (!File.Exists(ConnectionString)) throw new ArgumentException("Database must already exist.");
            connectionObject = new SQLiteDataConnection();
        }

        /// <summary>
        /// Create a new SQLite database.
        /// </summary>
        /// <param name="location">File location to create database</param>
        public static void CreateDatabase(string location) {
            SQLiteDataConnection.CreateFile(location);
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
            insertQuery.Append(table + " ");

            if (fields.Count() > 0) insertQuery.Append("(" + fields.Join(',') + ")");
            insertQuery.Append(" VALUES ");
            for (int i = 0; i < values.Count(); i++) {
                if (i > 0) insertQuery.Append(',');
                string v = values.ElementAt(i);
                if (!v.IsNumber()) {
                    insertQuery.Append('\'');
                    insertQuery.Append(v);
                    insertQuery.Append('\'');
                } else insertQuery.Append(v);
            }

            insertQuery.Append(";");
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
            for (int i = 0; i < fields.Count(); i++) {
                string field = fields.ElementAt(i);
                if (i > 0) selectQuery.Append(',');
                if (!field.IsNumber()) {
                    if (field[0] != '\'') selectQuery.Append('\'');
                    selectQuery.Append(field);
                    if (field.Last() != '\'') selectQuery.Append('\'');
                } else selectQuery.Append(field);
            }
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
                    if (value[0] != '\'') updateQuery.Append('\'');
                    updateQuery.Append(value);
                    if (value.Last() != '\'') updateQuery.Append('\'');
                } else updateQuery.Append(value);
            }

            updateQuery.Append(" WHERE " + updateCondition + ";");
            return new SQLiteCommand(updateQuery.ToString(), (SQLiteDataConnection)connectionObject);
        }
    }
}
