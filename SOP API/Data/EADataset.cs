using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SOPAPI.Dataset;

namespace SOPAPI.Data {
    /// <summary>
    /// Class for interfacing with Energy Accounting datasets more efficiently and without requiring adding the web service every time.
    /// </summary>
    public class EADataset {
        /// <summary>
        /// The credentials necessary to view this dataset.
        /// </summary>
        public NetworkCredential Credentials { get => credentials; }
        /// <summary>
        /// The name of this dataset as defined in EA.
        /// </summary>
        public string QueryName { get => queryName; }
        /// <summary>
        /// The required arguments to view this dataset.
        /// </summary>
        public List<object> Arguments { get => arguments; set => arguments = value; }
        /// <summary>
        /// The headers of this dataset. Empty if the query has not yet been run.
        /// </summary>
        public List<string> Headers { get => headers; }
        /// <summary>
        /// The data from this dataset, in rows. Each row is a list of objects that represent the fields of that row. Empty if the query has not yet been run.
        /// </summary>
        public List<List<object>> Rows { get => rows; }
        
        private readonly NetworkCredential credentials;
        private readonly string queryName;
        private List<object> arguments;
        private List<List<object>> rows;
        private List<string> headers;

        /// <summary>
        /// Initialize a new EADataset object.
        /// </summary>
        /// <param name="queryName">The name of this dataset as defined in EA.</param>
        /// <param name="credentials">The credentials necessary to view this dataset.</param>
        /// <param name="arguments">The required arguments to view this dataset. Automatically converts DateTime objects to GTTimestamp objects.</param>
        public EADataset(string queryName, NetworkCredential credentials, IEnumerable<object> arguments) {
            this.queryName = queryName;
            this.credentials = credentials;

            this.arguments = new List<object>();
            for (int i = 0; i < arguments.Count(); i++) {
                if (arguments.ElementAt(i) is DateTime) {
                    GTTimestamp gt = new GTTimestamp {
                        calendar = (DateTime)arguments.ElementAt(i)
                    };
                    this.arguments.Add(gt);
                } else this.arguments.Add(arguments.ElementAt(i));
            }
            headers = new List<string>();
            rows = new List<List<object>>();
        }
        /// <summary>
        /// Initialize a new EADataset object.
        /// </summary>
        /// <param name="queryName">The name of this dataset as defined in EA.</param>
        /// <param name="username">The user account through which to view this dataset.</param>
        /// <param name="password">The password for the associated user account.</param>
        /// <param name="arguments">The required arguments to view this dataset. Automatically converts DateTime objects to GTTimestamp objects.</param>
        public EADataset(string queryName,string username, string password, IEnumerable<object> arguments) : this(queryName, new NetworkCredential(username,password),arguments) { }

        /// <summary>
        /// Run this query using the provided parameters. All GTTimestamp objects are converted to DateTime objects.
        /// </summary>
        public void Run() {
            using (DatasetQueryService dqs = new DatasetQueryService()) {
                dqs.Credentials = Credentials;

                DatasetResultView[] drv;
                int attempts = 0;
                while (true) {
                    try {
                        drv = dqs.RunQuery(6, QueryName, Arguments.ToArray());
                        break;
                    } catch (WebException e) {
                        if (attempts < 10) continue;
                        throw new EAException("Could not connect to EA.",e);
                    }
                }

                if (drv.Length == 0) throw new EAException("No data was returned by the dataset.");
                headers = drv[0].columns.ToList();
                rows.Clear();

                foreach (DatasetResultView row in drv) {
                    rows.Add(row.values.ToList());
                    List<object> last = rows.Last();
                    bool found = false;
                    for (int i=0;i<last.Count;i++) {
                        object o = last[i];
                        if (o is GTTimestamp) {
                            last[i] = ((DateTime)((GTTimestamp)o).calendar).ToLocalTime();
                            found = true;
                        }
                    }

                    if (found) rows[rows.Count - 1] = last;
                }
            }
        }

        /// <summary>
        /// Gets a value from the returned data.
        /// </summary>
        /// <param name="row">The specified row number.</param>
        /// <param name="column">The specified column number.</param>
        /// <returns>The value and the specified location.</returns>
        public object Get(int row, int column) {
            if (Rows.Count == 0) throw new InvalidOperationException("Query must be run before values can be retrieved.");
            if (Rows.Count <= row) throw new ArgumentOutOfRangeException("Row " + row + " does not exist. " + Rows.Count + " row(s) found.");
            if (Headers.Count <= column) throw new ArgumentOutOfRangeException("Column " + column + " does not exist. " + Headers.Count + " column(s) found.");

            return rows[row][column];
        }

        /// <summary>
        /// Gets a value from the returned data.
        /// </summary>
        /// <param name="row">The specified row number.</param>
        /// <param name="field">The specified field name.</param>
        /// <returns>The value and the specified location.</returns>
        public object Get(int row, string field) {
            if (Headers.IndexOf(field) < 0) throw new ArgumentOutOfRangeException("Field " + field + " does not exist.");
            return Get(row, Headers.IndexOf(field));
        }
    }
}
