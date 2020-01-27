using System;
using System.Linq;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;
using System.Text;
using Vergil.Utilities;

namespace Vergil.Data.DB {
    /// <summary>
    /// Enum for data view types
    /// </summary>
    public enum DataViewType {
        /// <summary>
        /// Database table
        /// </summary>
        Table = 0,
        /// <summary>
        /// Non-table dataview type
        /// </summary>
        View = 1
    }

    /// <summary>
    /// Generic database connection type
    /// </summary>
    public abstract class DBConnection : IDisposable {
        /// <summary>
        /// The actual database connection object
        /// </summary>
        protected DbConnection connectionObject;

        /// <summary>
        /// The underlying connection string
        /// </summary>
        public string ConnectionString { get; }
        /// <summary>
        /// Internal property to tell if database is open.
        /// </summary>
        protected bool isOpen;
        /// <summary>
        /// True if the connection has been opened.
        /// </summary>
        public bool IsOpen { get => isOpen; }

        /// <summary>
        /// Create a new instance with a connection string
        /// </summary>
        /// <param name="connString">The connection string for this database type</param>
        public DBConnection(string connString) {
            ConnectionString = connString;
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
        public Dictionary<string, DataViewType> GetDataViewNames() {
            Dictionary<string, DataViewType> datasets = new Dictionary<string, DataViewType>();
            foreach (DataRow row in connectionObject.GetSchema("Tables").Rows) {
                string name = row.ItemArray[2].ToString();
                if (!name.Contains("MSys")) datasets.Add(name, DataViewType.Table);
            }
            foreach (DataRow row in connectionObject.GetSchema("Views").Rows) datasets.Add(row.ItemArray[2].ToString(), DataViewType.View);
            return datasets;
        }

        /// <summary>
        /// Add a table to the database
        /// </summary>
        /// <param name="name">Name of table to add</param>
        /// <param name="fields">Fields to add to database. Type names vary by database type.</param>
        /// <param name="overrideExisting">If true, will delete existing object with same name before creating. If false, will stop if existing object is found.</param>
        public void AddTable(string name, List<DBFieldDefinition> fields, bool overrideExisting = false) {
            var viewNames = GetDataViewNames();
            if (viewNames.ContainsKey(name)) {
                if (overrideExisting) {
                    using (DbCommand dt = connectionObject.CreateCommand()) {
                        dt.CommandText = new[] { "DROP", viewNames[name].EnumName().ToUpper(), name }.Join(' ');
                        dt.ExecuteNonQuery();
                    }
                } else return;
            }
            StringBuilder cmdText = new StringBuilder("CREATE TABLE ");
            cmdText.Append(name);
            cmdText.Append(" (");
            cmdText.Append(fields.Join(','));
            List<DBFieldDefinition> primaries = new List<DBFieldDefinition>();
            foreach (DBFieldDefinition f in fields) if (f.Primary) primaries.Add(f);
            if (primaries.Count > 0) {
                cmdText.Append(", PRIMARY KEY ");
                if (primaries.Count > 0) cmdText.Append("(");
                for (int i = 0; i < primaries.Count; i++) {
                    if (i > 0) cmdText.Append(',');
                    DBFieldDefinition p = primaries[i];
                    cmdText.Append(p.Name);
                    if (p.Descending) cmdText.Append(" desc");
                }
                if (primaries.Count > 0) cmdText.Append(")");
            }
            cmdText.Append(");");

            using (DbCommand command = connectionObject.CreateCommand()) {
                command.CommandText = cmdText.ToString();
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Adds data to the database. Attempts to UPDATE records, then if no records are updated, it performs an INSERT query. Assumes all fields will be updated.
        /// </summary>
        /// <param name="dataView">The name of the table that will be affected</param>
        /// <param name="values">An array of values to add to the specified fields. Indicies must match up with their respective fields.</param>
        /// <param name="updateCondition">A WHERE condition to add to the UPDATE query. Should be formatted before being passed to this method and not include "WHERE" keyword.</param>
        /// <returns></returns>
        public int AddRecord(string dataView, IEnumerable<string> values, string updateCondition) {
            return AddRecord(dataView, GetFields(dataView), values, updateCondition);
        }
        /// <summary>
        /// Adds data to the database. Attempts to UPDATE records, then if no records are updated, it performs an INSERT query.
        /// </summary>
        /// <param name="dataView">The name of the table that will be affected</param>
        /// <param name="fields">An array of field names in the database</param>
        /// <param name="values">An array of values to add to the specified fields. Indicies must match up with their respective fields.</param>
        /// <param name="updateCondition">A WHERE condition to add to the UPDATE query. Should be formatted before being passed to this method and not include "WHERE" keyword.</param>
        public int AddRecord(string dataView, IEnumerable<string> fields, IEnumerable<string> values, string updateCondition) {
            int recordsAffected = 0;
            using (DbCommand updateCommand = GenerateUpdateQuery(dataView, fields, values, updateCondition)) {
                recordsAffected = updateCommand.ExecuteNonQuery();
            }
            if (recordsAffected < 1) using (DbCommand insertCommand = GenerateInsertQuery(dataView, fields, values)) recordsAffected = insertCommand.ExecuteNonQuery();
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
            using (DbCommand command = GenerateUpdateQuery(table, fields, values, updateCondition)) return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes an INSERT query on the database
        /// </summary>
        /// <param name="table">The name of the table that will be affected</param>
        /// <param name="fields">An array of field names in the database</param>
        /// <param name="values">An array of values to add to the specified fields. Indicies must match up with their respective fields.</param>
        /// <returns>The number of records affected by this query.</returns>
        public int Insert(string table, IEnumerable<string> fields, IEnumerable<string> values) {
            using (DbCommand command = GenerateInsertQuery(table, fields, values)) return command.ExecuteNonQuery();
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
        public DbDataReader Select(string table, string field = "*", string SelectCondition = "", bool distinct = false, IEnumerable<DbParameter> parameters = null) {
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
        public DbDataReader Select(string table, IEnumerable<string> fields, string SelectCondition = "", bool distinct = false, IEnumerable<DbParameter> parameters = null) {
            using (DbCommand SelectCommand = GenerateSelectQuery(table, fields, SelectCondition, distinct)) {
                if (parameters != null) foreach (DbParameter param in parameters) SelectCommand.Parameters.Add(param);
                return SelectCommand.ExecuteReader();
            }
        }

        /// <summary>
        /// Execute an arbitrary query on the database.
        /// Useful for more obscure query types.
        /// </summary>
        /// <param name="query">A string containing the query definition</param>
        /// <returns>A DbDataReader containing any records returned by this query. Not guaranteed to actually contain records.</returns>
        public DbDataReader Query(string query) {
            if (string.IsNullOrEmpty(query)) throw new ArgumentException("Query string is mandatory");
            using (DbCommand command = connectionObject.CreateCommand()) {
                command.CommandText = query;
                return command.ExecuteReader();
            }
        }

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

        /// <summary>
        /// Implementation for generating a query command for the database type to update records.
        /// </summary>
        /// <param name="dataView">The table, view, etc. to update</param>
        /// <param name="fields">The fields to update</param>
        /// <param name="values">The values to update in above fields</param>
        /// <param name="updateCondition">The condition, as a string of SQL, that determines if a row is updated</param>
        /// <returns>Created query command</returns>
        protected abstract DbCommand GenerateUpdateQuery(string dataView, IEnumerable<string> fields, IEnumerable<string> values, string updateCondition);

        /// <summary>
        /// Implementation for generating a query command for the database type to insert records.
        /// </summary>
        /// <param name="dataView">The table, view, etc. to update</param>
        /// <param name="fields">The fields to insert into</param>
        /// <param name="values">The values to insert into above fields</param>
        /// <returns>Created query command</returns>
        protected abstract DbCommand GenerateInsertQuery(string dataView, IEnumerable<string> fields, IEnumerable<string> values);

        /// <summary>
        /// Implementation for generating a query command for the database type to select records.
        /// </summary>
        /// <param name="dataView">The table, view, etc. to update</param>
        /// <param name="fields">The fields to select</param>
        /// <param name="selectCondition">The condition, as a string of SQL, that determines if a row is selected</param>
        /// <param name="distinct">If true, only returns distinct results</param>
        /// <returns>Created query command</returns>
        protected abstract DbCommand GenerateSelectQuery(string dataView, IEnumerable<string> fields, string selectCondition, bool distinct);

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        /// <summary>
        /// Dispose of this object
        /// </summary>
        /// <param name="disposing">true if disposing</param>
        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    Close();
                    connectionObject.Dispose();
                } disposedValue = true;
            }
        }
        // This code added to correctly implement the disposable pattern.
        /// <summary>
        /// Dispose of this object
        /// </summary>
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
