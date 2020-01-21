using System;
using System.Linq;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;

namespace Vergil.Data.DB {
    public abstract class DBConnection : IDisposable {
        protected DbConnection connectionObject;

        private readonly string connectionString;
        public string ConnectionString => connectionString;
        protected bool isOpen;
        /// <summary>
        /// True if the connection has been opened.
        /// </summary>
        public bool IsOpen { get => isOpen; }

        public DBConnection(string connString) {
            connectionString = connString;
            isOpen = false;
        }


        /// <summary>
        /// Open connection to the database
        /// </summary>
        public void Open() {
            connectionObject.Open();
            isOpen = true;
        }

        /// <summary>
        /// Close connection to the database
        /// </summary>
        public void Close() {
            connectionObject.Close();
            isOpen = false;
        }
        /// <summary>
        /// Checks to see if a table with the specified name exists in the database.
        /// </summary>
        /// <param name="table">The name of a table to find, case insensitive.</param>
        /// <returns>True if the database contains a table matching the specified name.</returns>
        public bool TableExists(string table) {
            foreach (DataRow row in connectionObject.GetSchema("Tables").Rows) {
                if (row.ItemArray[2].ToString().ToLower().Equals(table.ToLower())) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a list of names of data sources in this database.
        /// </summary>
        /// <returns>Returns a list of names of data-containing objects in this database. First are the tables, then the queries.</returns>
        public Dictionary<string, string> GetDataViewNames() {
            Dictionary<string, string> datasets = new Dictionary<string, string>();
            foreach (DataRow row in connectionObject.GetSchema("Tables").Rows) {
                string name = row.ItemArray[2].ToString();
                if (!name.Contains("MSys")) datasets.Add(name, "Table");
            }
            foreach (DataRow row in connectionObject.GetSchema("Views").Rows) datasets.Add(row.ItemArray[2].ToString(), "View");
            return datasets;
        }

        /// <summary>
        /// Adds data to the database. Attempts to UPDATE records, then if no records are updated, it performs an INSERT query. Assumes all fields will be updated.
        /// </summary>
        /// <param name="table">The name of the table that will be affected</param>
        /// <param name="values">An array of values to add to the specified fields. Indicies must match up with their respective fields.</param>
        /// <param name="updateCondition">A WHERE condition to add to the UPDATE query. Should be formatted before being passed to this method and not include "WHERE" keyword.</param>
        /// <returns></returns>
        public abstract int AddRecord(string dataView, IEnumerable<string> values, string updateCondition);
        /// <summary>
        /// Adds data to the database. Attempts to UPDATE records, then if no records are updated, it performs an INSERT query.
        /// </summary>
        /// <param name="table">The name of the table that will be affected</param>
        /// <param name="fields">An array of field names in the database</param>
        /// <param name="values">An array of values to add to the specified fields. Indicies must match up with their respective fields.</param>
        /// <param name="updateCondition">A WHERE condition to add to the UPDATE query. Should be formatted before being passed to this method and not include "WHERE" keyword.</param>
        public abstract int AddRecord(string table, IEnumerable<string> fields, IEnumerable<string> values, string updateCondition);

        /// <summary>
        /// Executes an UPDATE query on the database.
        /// </summary>
        /// <param name="table">The name of the table that will be affected</param>
        /// <param name="fields">An array of field names in the database</param>
        /// <param name="values">An array of values to add to the specified fields. Indicies must match up with their respective fields.</param>
        /// <param name="updateCondition">A WHERE condition to add to the UPDATE query. Should be formatted before being passed to this method and not include "WHERE" keyword.</param>
        /// <returns>The number of records affected by this query.</returns>
        public abstract int Update(string table, IEnumerable<string> fields, IEnumerable<string> values, string updateCondition);

        /// <summary>
        /// Executes an INSERT query on the database
        /// </summary>
        /// <param name="table">The name of the table that will be affected</param>
        /// <param name="fields">An array of field names in the database</param>
        /// <param name="values">An array of values to add to the specified fields. Indicies must match up with their respective fields.</param>
        /// <returns>The number of records affected by this query.</returns>
        public abstract int Insert(string table, IEnumerable<string> fields, IEnumerable<string> values);

        /// <summary>
        /// Execute a SELECT query on the database. Selects data from the specified field that meets the specified condition. Will only Select DISTINCT records if distinct is true.
        /// </summary>
        /// <param name="table">The table from which to Select</param>
        /// <param name="field">The field from which to Select</param>
        /// <param name="SelectCondition">A WHERE condition to filter records. Should be formatted before being passed to this method and not include "WHERE" keyword.</param>
        /// <param name="distinct">If true, only distinct records will be Selected</param>
        /// <param name="parameters">Parameters to pass into this query</param>
        /// <returns>OdbcDataReader containing Selected records</returns>
        public abstract DbDataReader Select(string table, string field = "*", string SelectCondition = "", bool distinct = false, Dictionary<string, object> parameters = null);
        /// <summary>
        /// Execute a SELECT query on the database. Selects data from the specified fields that meets the specified condition. Will only Select DISTINCT records if distinct is true.
        /// </summary>
        /// <param name="table">The table from which to Select</param>
        /// <param name="fields">The fields from which to Select</param>
        /// <param name="SelectCondition">A WHERE condition to filter records. Should be formatted before being passed to this method and not include "WHERE" keyword.</param>
        /// <param name="distinct">If true, only distinct records will be Selected</param>
        /// <param name="parameters">Parameters to pass into this query</param>
        /// <returns>OdbcDataReader containing Selected records</returns>
        public abstract DbDataReader Select(string table, IEnumerable<string> fields, string SelectCondition = "", bool distinct = false, Dictionary<string, object> parameters = null);

        /// <summary>
        /// Execute an arbitrary query on the database.
        /// Useful for more obscure query types.
        /// </summary>
        /// <param name="query">A string containing the query definition</param>
        /// <returns>An OdbcDataReader containing any records returned by this query. Not guaranteed to actually contain records.</returns>
        public abstract DbDataReader Query(string query);

        /// <summary>
        /// Returns a List of field names for the specified table (or query).
        /// </summary>
        /// <param name="table">The name of the table (or query) to check.</param>
        /// <returns>A List containing the names of each of the specified table's fields.</returns>
        public List<string> GetFields(string table) {
            List<string> fields = new List<string>();
            DbDataReader reader = Select(table, SelectCondition: "null");
            for (int i = 0; i < reader.FieldCount; i++) fields.Add(reader.GetName(i));
            return fields;
        }

        /// <summary>
        /// Reads out a DbDataReader object into an array containing all its values.
        /// Obviously slower than reading the reader directly, but more straightforward to work with.
        /// </summary>
        /// <param name="reader">The OdbcDataReader object to read</param>
        /// <returns>A two-dimensional string array where each row contains an array containing each field's value.
        /// The first row of the array is the data set's field names.</returns>
        public static List<string[]> ReadRows(DbDataReader reader) {
            List<string[]> rows = new List<string[]>();
            List<string> headers = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++) headers.Add(reader.GetName(i));
            rows.Add(headers.ToArray());

            while (reader.Read()) {
                List<string> row = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++) {
                    try {
                        row.Add((reader.GetValue(i) ?? "#####").ToString());
                    } catch (DbException) {
                        row.Add("#####");
                    }
                }
                rows.Add(row.ToArray());
            }

            return rows;
        }

        protected abstract DbCommand GenerateUpdateQuery(string table, IEnumerable<string> fields, IEnumerable<string> values, string updateCondition);

        protected abstract DbCommand GenerateInsertQuery(string table, IEnumerable<string> fields, IEnumerable<string> values);

        protected abstract DbCommand GenerateSelectQuery(string table, IEnumerable<string> fields, string selectCondition, bool distinct);

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    Close();
                    connectionObject.Dispose();
                } disposedValue = true;
            }
        }
        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
