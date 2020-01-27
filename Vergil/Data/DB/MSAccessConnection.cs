using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Data.Common;

namespace Vergil.Data.DB {
    /// <summary>
    /// Wrapper class for OdbcConnection to MS Access databases. Simplifies creating and executing simple queries.
    /// </summary>
    public class MSAccessConnection : DBConnection {
        /// <summary>
        /// Initialize a new connection to the Access database at the specified file location
        /// </summary>
        /// <param name="location">Filepath to the Access database</param>
        public MSAccessConnection(string location) : base("Driver={Microsoft Access Driver (*.mdb, *.accdb)};"
                    + "DBQ=" + location + ";") {
            connectionObject = new OdbcConnection(ConnectionString);
        }
        /// <summary>
        /// Initialize a new connection to the Access database at the specified file location with the specified credentials.
        /// </summary>
        /// <param name="location">Filepath to the Access database</param>
        /// <param name="user">The username to use (default: admin)</param>
        /// <param name="password">The password to use.</param>
        public MSAccessConnection(string location, string user, string password) : base("Driver={Microsoft Access Driver (*.mdb, *.accdb)};"
                    + "DBQ=" + location + ";"
                    + "Uid=" + user + ";"
                    + "Pwd=" + password + ";") {
            connectionObject = new OdbcConnection(ConnectionString);
        }
        /// <summary>
        /// Implementation for generating a query command for the database type to update records.
        /// </summary>
        /// <param name="dataView">The table, view, etc. to update</param>
        /// <param name="fields">The field to update</param>
        /// <param name="values">The values to update in above fields</param>
        /// <param name="updateCondition">The condition, as a string of SQL, that determines if a row is updated</param>
        /// <returns>Created query command</returns>
        protected override DbCommand GenerateUpdateQuery(string dataView, IEnumerable<string> fields, IEnumerable<string> values, string updateCondition) {
            string[] f = fields.ToArray();
            string[] v = values.ToArray();
            if ((dataView.Contains(' ') || dataView.Contains('-')) && !dataView.Contains('[')) dataView = "[" + dataView + "]";
            StringBuilder updateQuery = new StringBuilder("UPDATE " + dataView + " SET ");
            for (int i = 0; i < f.Length; i++) {
                if ((f[i][0] != '[' && f[i][f[i].Length - 1] != ']') || f[i].ToUpper().Equals("DATE")) f[i] = "[" + f[i] + "]";
                if (char.IsLetter(v[i][0])) v[i] = "'" + v[i] + "'";
                updateQuery.Append(f[i] + "=" + v[i]);
                if (i < f.Length - 1) {
                    updateQuery.Append(",");
                }
            }
            updateQuery.Append(" WHERE " + updateCondition + ";");
            return new OdbcCommand(updateQuery.ToString(),(OdbcConnection)connectionObject);
        }

        /// <summary>
        /// Implementation for generating a query command for the database type to insert records.
        /// </summary>
        /// <param name="dataView">The table, view, etc. to update</param>
        /// <param name="fields">The field to update</param>
        /// <param name="values">The values to update in above fields</param>
        /// <returns>Created query command</returns>
        protected override DbCommand GenerateInsertQuery(string dataView, IEnumerable<string> fields, IEnumerable<string> values) {
            string[] f = fields.ToArray();
            string[] v = values.ToArray();
            if ((dataView.Contains(' ') || dataView.Contains('-')) && !dataView.Contains('[')) dataView = "[" + dataView + "]";
            StringBuilder insertQuery = new StringBuilder("INSERT INTO " + dataView + " (");
            for (int i = 0; i < f.Length; i++) {
                if ((f[i][0] != '[' && f[i][f[i].Length - 1] != ']') || f[i].ToUpper().Equals("DATE")) f[i] = "[" + f[i] + "]";
                if (Char.IsLetter(v[i][0])) v[i] = "'" + v[i] + "'";
                insertQuery.Append(f[i]);
                if (i < f.Length - 1) {
                    insertQuery.Append(",");
                }
            }
            insertQuery.Append(") VALUES (" + string.Join(",", values) + ");");
            return new OdbcCommand(insertQuery.ToString(),(OdbcConnection)connectionObject);
        }

        /// <summary>
        /// Implementation for generating a query command for the database type to select records.
        /// </summary>
        /// <param name="dataView">The table, view, etc. to update</param>
        /// <param name="fields">The field to update</param>
        /// <param name="selectCondition">The condition, as a string of SQL, that determines if a row is selected</param>
        /// <param name="distinct">If true, only returns distinct results</param>
        /// <returns>Created query command</returns>
        protected override DbCommand GenerateSelectQuery(string dataView, IEnumerable<string> fields, string selectCondition, bool distinct) {
            string[] f = fields.ToArray();
            if ((dataView.Contains(' ') || dataView.Contains('-')) && !dataView.Contains('[')) dataView = "[" + dataView + "]";
            StringBuilder selectQuery = new StringBuilder("SELECT " + (distinct ? "DISTINCT " : ""));
            for (int i = 0; i < f.Length; i++) {
                if ((f[i][0] != '[' && f[i][f[i].Length - 1] != ']' && f.Length >= 1 && f[0] != "*") || f[i].ToUpper().Equals("DATE")) f[i] = "[" + f[i] + "]";
                selectQuery.Append(f[i]);
                if (i < f.Length - 1) {
                    selectQuery.Append(",");
                }
            }
            selectQuery.Append(" FROM " + dataView);
            if (selectCondition != null && selectCondition.Length > 0) selectQuery.Append(" WHERE " + selectCondition);
            selectQuery.Append(";");

            return new OdbcCommand(selectQuery.ToString(),(OdbcConnection)connectionObject);
        }

    }
}
