using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Data;

namespace SOPAPI.Data.DB {
    /// <summary>
    /// Wrapper class for OdbcConnection. Simplifies creating and executing simple queries.
    /// </summary>
    public class DBConnection : IDisposable {

        private readonly string connectionstring;
        private OdbcConnection connection;

        private bool isOpen;
        /// <summary>
        /// True if the connection has been opened.
        /// </summary>
        public bool IsOpen { get => isOpen;}

        /// <summary>
        /// Initialize a new connection to the Access database at the specified file location
        /// </summary>
        /// <param name="location">Filepath to the Access database</param>
        public DBConnection(string location) {
            connectionstring = "Driver={Microsoft Access Driver (*.mdb, *.accdb)};"
                    + "DBQ=" + location + ";";
            connection = new OdbcConnection(connectionstring);
            isOpen = false;
        }
        /// <summary>
        /// Initialize a new connection to the Access database at the specified file location with the specified credentials.
        /// </summary>
        /// <param name="location">Filepath to the Access database</param>
        /// <param name="user">The username to use (default: admin)</param>
        /// <param name="password">The password to use.</param>
        public DBConnection(string location, string user, string password) {
            connectionstring = "Driver={Microsoft Access Driver (*.mdb, *.accdb)};"
                    + "DBQ=" + location + ";"
                    + "Uid=" + user + ";"
                    + "Pwd=" + password + ";";
            connection = new OdbcConnection(connectionstring);
            isOpen = false;
        }

        /// <summary>
        /// Open connection to the database
        /// </summary>
        public void Open() {
            connection.Open();
            isOpen = true;
        }

        /// <summary>
        /// Close connection to the database
        /// </summary>
        public void Close() {
            connection.Close();
            isOpen = false;
        }

        /// <summary>
        /// Close this connection.
        /// </summary>
        public void Dispose() {
            Close();
        }

        /// <summary>
        /// Checks to see if a table with the specified name exists in the database.
        /// </summary>
        /// <param name="table">The name of a table to find, case insensitive.</param>
        /// <returns>True if the database contains a table matching the specified name.</returns>
        public bool TableExists(string table) {
            foreach (DataRow row in connection.GetSchema("Tables" /*, new string[] {null,null,null, "TABLE" }*/).Rows) {
                if (row.ItemArray[2].ToString().ToLower().Equals(table.ToLower())) return true;
            } return false;
        }

        /// <summary>
        /// Returns a list of names of data sources in this database.
        /// </summary>
        /// <returns>Returns a list of names of data-containing objects in this database. First are the tables, then the queries.</returns>
        public Dictionary<string,string> GetDataViewNames() {
            Dictionary<string, string> datasets = new Dictionary<string, string>();
            foreach (DataRow row in connection.GetSchema("Tables").Rows) {
                string name = row.ItemArray[2].ToString();
                if (!name.Contains("MSys")) datasets.Add(name,"Table");
            }
            foreach (DataRow row in connection.GetSchema("Views").Rows) datasets.Add(row.ItemArray[2].ToString(),"Query");
            return datasets;
        }

        /// <summary>
        /// Adds data to the database. Attempts to UPDATE records, then if no records are updated, it performs an INSERT query. Assumes all fields will be updated.
        /// </summary>
        /// <param name="table">The name of the table that will be affected</param>
        /// <param name="values">An array of values to add to the specified fields. Indicies must match up with their respective fields.</param>
        /// <param name="updateCondition">A WHERE condition to add to the UPDATE query. Should be formatted before being passed to this method and not include "WHERE" keyword.</param>
        /// <returns></returns>
        public int AddRecord(string table, IEnumerable<string> values, string updateCondition) {
            return AddRecord(table, GetFields(table).ToArray(), values, updateCondition);
        }
        /// <summary>
        /// Adds data to the database. Attempts to UPDATE records, then if no records are updated, it performs an INSERT query.
        /// </summary>
        /// <param name="table">The name of the table that will be affected</param>
        /// <param name="fields">An array of field names in the database</param>
        /// <param name="values">An array of values to add to the specified fields. Indicies must match up with their respective fields.</param>
        /// <param name="updateCondition">A WHERE condition to add to the UPDATE query. Should be formatted before being passed to this method and not include "WHERE" keyword.</param>
        public int AddRecord(string table, IEnumerable<string> fields, IEnumerable<string> values, string updateCondition) {
            OdbcCommand updateCommand = GenerateUpdateQuery(table, fields, values, updateCondition);
            int recordsAffected = updateCommand.ExecuteNonQuery();
            updateCommand.Dispose();
            if (recordsAffected < 1) {
                OdbcCommand insertCommand = GenerateInsertQuery(table, fields, values);
                recordsAffected = insertCommand.ExecuteNonQuery();
                insertCommand.Dispose();
            }
            return recordsAffected;
        }

        /// <summary>
        /// Executes an UPDATE query on the database.
        /// </summary>
        /// <param name="table">The name of the table that will be affected</param>
        /// <param name="fields">An array of field names in the database</param>
        /// <param name="values">An array of values to add to the specified fields. Indicies must match up with their respective fields.</param>
        /// <param name="updateCondition">A WHERE condition to add to the UPDATE query. Should be formatted before being passed to this method and not include "WHERE" keyword.</param>
        /// <returns>The number of records affected by this query.</returns>
        public int Update(string table, IEnumerable<string> fields, IEnumerable<string> values, string updateCondition) {
            return GenerateUpdateQuery(table,fields,values,updateCondition).ExecuteNonQuery();
        }

        /// <summary>
        /// Executes an INSERT query on the database
        /// </summary>
        /// <param name="table">The name of the table that will be affected</param>
        /// <param name="fields">An array of field names in the database</param>
        /// <param name="values">An array of values to add to the specified fields. Indicies must match up with their respective fields.</param>
        /// <returns>The number of records affected by this query.</returns>
        public int Insert(string table, IEnumerable<string> fields, IEnumerable<string> values) {
            return GenerateInsertQuery(table, fields, values).ExecuteNonQuery();
        }

        /// <summary>
        /// Execute a SELECT query on the database. Selects data from the specified field that meets the specified condition. Will only Select DISTINCT records if distinct is true.
        /// </summary>
        /// <param name="table">The table from which to Select</param>
        /// <param name="field">The field from which to Select</param>
        /// <param name="SelectCondition">A WHERE condition to filter records. Should be formatted before being passed to this method and not include "WHERE" keyword.</param>
        /// <param name="distinct">If true, only distinct records will be Selected</param>
        /// <param name="parameters">Parameters to pass into this query</param>
        /// <returns>OdbcDataReader containing Selected records</returns>
        public OdbcDataReader Select(string table, string field = "*", string SelectCondition = "", Boolean distinct = false, Dictionary<string, object> parameters = null) {
            return Select(table, new string[] { field }, SelectCondition, distinct, parameters);
        }
        /// <summary>
        /// Execute a SELECT query on the database. Selects data from the specified fields that meets the specified condition. Will only Select DISTINCT records if distinct is true.
        /// </summary>
        /// <param name="table">The table from which to Select</param>
        /// <param name="fields">The fields from which to Select</param>
        /// <param name="SelectCondition">A WHERE condition to filter records. Should be formatted before being passed to this method and not include "WHERE" keyword.</param>
        /// <param name="distinct">If true, only distinct records will be Selected</param>
        /// <param name="parameters">Parameters to pass into this query</param>
        /// <returns>OdbcDataReader containing Selected records</returns>
        public OdbcDataReader Select(string table, IEnumerable<string> fields, string SelectCondition = "", Boolean distinct = false, Dictionary<string, object> parameters = null) {
            OdbcCommand SelectCommand = GenerateSelectQuery(table, fields, SelectCondition, distinct);
            if (parameters != null) foreach (string param in parameters.Keys) SelectCommand.Parameters.Add(new OdbcParameter(param, parameters[param]));
            return SelectCommand.ExecuteReader();
        }

        /// <summary>
        /// Execute an arbitrary query on the database.
        /// Useful for more obscure query types.
        /// </summary>
        /// <param name="query">A string containing the query definition</param>
        /// <returns>An OdbcDataReader containing any records returned by this query. Not guaranteed to actually contain records.</returns>
        public OdbcDataReader Query(string query) {
            return new OdbcCommand(query, connection).ExecuteReader();
        }

        /// <summary>
        /// Returns a List of field names for the specified table (or query).
        /// </summary>
        /// <param name="table">The name of the table (or query) to check.</param>
        /// <returns>A List containing the names of each of the specified table's fields.</returns>
        public List<string> GetFields(string table) {
            List<string> fields = new List<string>();
            OdbcDataReader reader = Select(table, SelectCondition: "null");
            for (int i = 0; i < reader.FieldCount; i++) fields.Add(reader.GetName(i));
            return fields;
        }

        /// <summary>
        /// Reads out an OdbcDataReader object into an array containing all its values.
        /// Obviously slower than reading the reader directly, but more straightforward to work with.
        /// </summary>
        /// <param name="reader">The OdbcDataReader object to read</param>
        /// <returns>A two-dimensional string array where each row contains an array containing each field's value.
        /// The first row of the array is the data set's field names.</returns>
        public static List<string[]> ReadRows(OdbcDataReader reader) {
            List<string[]> rows = new List<string[]>();
            List<string> headers = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++) headers.Add(reader.GetName(i));
            rows.Add(headers.ToArray());
            
            while (reader.Read()) {
                List<string> row = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++) {
                    try {
                        row.Add((reader.GetValue(i) ?? "#####").ToString());
                    } catch (OdbcException) {
                        row.Add("#####");
                    }
                }
                rows.Add(row.ToArray());
            }

            return rows;
        }

        private OdbcCommand GenerateUpdateQuery(string table, IEnumerable<string> fields, IEnumerable<string> values, string updateCondition) {
            string[] f = fields.ToArray();
            string[] v = values.ToArray();
            if ((table.Contains(' ') || table.Contains('-')) && !table.Contains('[')) table = "[" + table + "]";
            StringBuilder updateQuery = new StringBuilder("UPDATE " + table + " SET ");
            for (int i = 0; i < f.Length; i++) {
                if ((f[i][0] != '[' && f[i][f[i].Length - 1] != ']') || f[i].ToUpper().Equals("DATE")) f[i] = "[" + f[i] + "]";
                if (Char.IsLetter(v[i][0])) v[i] = "'" + v[i] + "'";
                updateQuery.Append(f[i] + "=" + v[i]);
                if (i < f.Length - 1) {
                    updateQuery.Append(",");
                }
            }
            updateQuery.Append(" WHERE " + updateCondition + ";");
            return new OdbcCommand(updateQuery.ToString(),connection);
        }

        private OdbcCommand GenerateInsertQuery(string table, IEnumerable<string> fields, IEnumerable<string> values) {
            string[] f = fields.ToArray();
            string[] v = values.ToArray();
            if ((table.Contains(' ') || table.Contains('-')) && !table.Contains('[')) table = "[" + table + "]";
            StringBuilder insertQuery = new StringBuilder("INSERT INTO " + table + " (");
            for (int i = 0; i < f.Length; i++) {
                if ((f[i][0] != '[' && f[i][f[i].Length - 1] != ']') || f[i].ToUpper().Equals("DATE")) f[i] = "[" + f[i] + "]";
                if (Char.IsLetter(v[i][0])) v[i] = "'" + v[i] + "'";
                insertQuery.Append(f[i]);
                if (i < f.Length - 1) {
                    insertQuery.Append(",");
                }
            }
            insertQuery.Append(") VALUES (" + string.Join(",", values) + ");");
            return new OdbcCommand(insertQuery.ToString(),connection);
        }

        private OdbcCommand GenerateSelectQuery(string table, IEnumerable<string> fields, string selectCondition, Boolean distinct) {
            string[] f = fields.ToArray();
            if ((table.Contains(' ') || table.Contains('-')) && !table.Contains('[')) table = "[" + table + "]";
            StringBuilder selectQuery = new StringBuilder("SELECT " + (distinct ? "DISTINCT " : ""));
            for (int i = 0; i < f.Length; i++) {
                if ((f[i][0] != '[' && f[i][f[i].Length - 1] != ']' && f.Length >= 1 && f[0] != "*") || f[i].ToUpper().Equals("DATE")) f[i] = "[" + f[i] + "]";
                selectQuery.Append(f[i]);
                if (i < f.Length - 1) {
                    selectQuery.Append(",");
                }
            }
            selectQuery.Append(" FROM " + table);
            if (selectCondition != null && selectCondition.Length > 0) selectQuery.Append(" WHERE " + selectCondition);
            selectQuery.Append(";");

            return new OdbcCommand(selectQuery.ToString(),connection);
        }

    }
}
