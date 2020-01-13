using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vergil.XML;
using System.IO;
using Vergil.Web;
using System.Net;
using Vergil.Data.DB;
using System.Threading;

namespace Vergil.Data {
    /// <summary>
    /// Intervals for running queries
    /// </summary>
    public enum QueryInterval {
        /// <summary>
        /// Query once each hour
        /// </summary>
        Hourly = 0,
        /// <summary>
        /// Query once each day
        /// </summary>
        Daily = 1,
        /// <summary>
        /// Query once each month
        /// </summary>
        Monthly = 2
    }

    /// <summary>
    /// Any type of Query output behavior.
    /// </summary>
    public interface QueryOutput {
        /// <summary>
        /// Output the query.
        /// </summary>
        /// <param name="data">The data to output.</param>
        /// <returns>A problem that occurred, else null.</returns>
        void Output(string data);

        /// <summary>
        /// Outputs the data for this output object as an XMLSection.
        /// </summary>
        /// <returns>An XML representation of this object.</returns>
        XMLSection ToXML();
    }

    /// <summary>
    /// Output behavior for writing a query to a file.
    /// </summary>
    public class QueryOutputFile : QueryOutput {
        /// <summary>
        /// The path of the file to which data will be written.
        /// </summary>
        public string OutputLocation;

        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        /// <param name="outputlocation">The path of the file to which data will be written.</param>
        public QueryOutputFile(string outputlocation) {
            OutputLocation = outputlocation;
        }

        /// <summary>
        /// Output the query to file.
        /// </summary>
        /// <param name="data">The data to write to the file.</param>
        /// <returns>A problem that occurred, else null.</returns>
        public void Output(string data) {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(OutputLocation)));
            File.WriteAllText(OutputLocation, data);
        }

        /// <summary>
        /// Outputs the data for this output object as an XMLSection.
        /// </summary>
        /// <returns>An XML representation of this object.</returns>
        public XMLSection ToXML() {
            XMLSection section = new XMLSection("output");
            section.AddChild("type", "file");
            section.AddChild("location", OutputLocation);
            return section;
        }
    }

    /// <summary>
    /// Output behavior for writing a query to an email.
    /// </summary>
    public class QueryOutputEmail : QueryOutput {
        /// <summary>
        /// The email address from which the email will be sent.
        /// </summary>
        public string From;
        /// <summary>
        /// The email addresses to which the email will be sent.
        /// </summary>
        public IEnumerable<string> To;
        /// <summary>
        /// The subject of the email.
        /// </summary>
        public string Subject;
        /// <summary>
        /// Text to add to the body of the email before the data.
        /// </summary>
        public string Body;
        /// <summary>
        /// Whether or not to use comedic openers in the email.
        /// </summary>
        public bool Openers;
        /// <summary>
        /// If not null, this Query will be output to a file and attached to the email.
        /// </summary>
        public QueryOutputFile AttachmentInfo;

        /// <summary>
        /// Create a new instance of this class.
        /// </summary>
        /// <param name="from">The email address from which the email will be sent.</param>
        /// <param name="to">The email addresses to which the email will be sent.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">Text to add to the body of the email before the data. Default: ""</param>
        /// <param name="openers">Whether or not to use comedic openers in the email. Default: false</param>
        /// <param name="attachmentFilepath">A file to attach to this email. Default: ""</param>
        public QueryOutputEmail(string from, IEnumerable<string> to, string subject, string body = "", bool openers = false, string attachmentFilepath = "") {
            From = Mail.GetEmailAddresses(new[]{from}).First();
            To = to;
            Subject = subject;
            Body = body;
            Openers = openers;
            AttachmentInfo = attachmentFilepath != null && attachmentFilepath.Length > 0 ? new QueryOutputFile(attachmentFilepath) : null;
        }

        /// <summary>
        /// Output the query to an email and send it.
        /// </summary>
        /// <param name="data">The data to write to the file.</param>
        /// <returns>A problem that occurred, else null.</returns>
        public void Output(string data) {
            if (AttachmentInfo != null && AttachmentInfo.OutputLocation.Length > 0) {
                AttachmentInfo.Output(data);
                Mail.SendEmail(From, To, Subject, Body, useOpeners: Openers, attachmentFilepaths: new[] { Path.GetFullPath(AttachmentInfo.OutputLocation) });
                File.Delete(AttachmentInfo.OutputLocation);
            } else {
                Mail.SendEmail(From, To, Subject, Body + '\n' + data, useOpeners: Openers);
            }
        }

        /// <summary>
        /// Outputs the data for this output object as an XMLSection.
        /// </summary>
        /// <returns>An XML representation of this object.</returns>
        public XMLSection ToXML() {
            XMLSection section = new XMLSection("output");
            section.AddChild("type", "email");
            section.AddChild("from", From);
            section.AddChild("to", String.Join(",",To));
            section.AddChild("subject", Subject);
            section.AddChild("body", Body);
            if (AttachmentInfo != null) section.AddChild("attachmentFilepath", AttachmentInfo.OutputLocation);
            return section;
        }
    }

    /// <summary>
    /// Output data to a remote file location.
    /// </summary>
    public class QueryOutputFTP : QueryOutput {
        /// <summary>
        /// The FTP address of the host
        /// </summary>
        public string Address;
        /// <summary>
        /// The username to use for authentication. Leave blank for no authentication.
        /// </summary>
        public string Username;
        /// <summary>
        /// The password to use for authentication. Leave blank for no authentication.
        /// </summary>
        public string Password;
        /// <summary>
        /// The remote folder to which this file will be transferred. Relative to the host address.
        /// </summary>
        public string RemotePath;
        /// <summary>
        /// The name of this file.
        /// </summary>
        public string FileName;

        /// <summary>
        /// Create a new QueryOutputFTP object.
        /// </summary>
        /// <param name="address">The FTP address of the host</param>
        /// <param name="filename">The name of this file.</param>
        /// <param name="remotePath">The remote folder to which this file will be transferred. Relative to the host address.</param>
        /// <param name="username">The username to use for authentication. Leave blank for no authentication. Default: ""</param>
        /// <param name="password">The password to use for authentication. Leave blank for no authentication. Default: ""</param>
        public QueryOutputFTP(string address, string filename, string remotePath = "", string username = "", string password = "") {
            Address = address;
            FileName = filename;
            RemotePath = remotePath;
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Output the query to a file and transfer it to the remote location.
        /// </summary>
        /// <param name="data">The data to write to the file.</param>
        /// <returns>A problem that occurred, else null.</returns>
        public void Output(string data) {
            string localPath = Directory.GetCurrentDirectory() + "\\" + FileName;
            File.WriteAllText(localPath, data);
            FTP ftp = new FTP(Address, Username, Password);
            int attempts = 0;
            while (attempts < 10) {
                try {
                    ftp.Upload(localPath, RemotePath + "\\" + FileName);
                    break;
                } catch (WebException e) {
                    if (attempts >= 10) throw e;
                    attempts++;
                }
            }
            File.Delete(localPath);
        }

        /// <summary>
        /// Outputs the data for this output object as an XMLSection.
        /// </summary>
        /// <returns>An XML representation of this object.</returns>
        public XMLSection ToXML() {
            XMLSection section = new XMLSection("output");
            section.AddChild("type", "ftp");
            section.AddChild("filename", FileName);
            section.AddChild("address", Address);
            section.AddChild("remotepath", RemotePath);
            if (Username.Length > 0) section.AddChild("username", Username);
            if (Password.Length > 0) section.AddChild("password", Password);
            return section;
        }
    }

    /// <summary>
    /// A query in a database for writing to a file
    /// </summary>
    public class Query {
        /// <summary>
        /// A QueryOutput object that defines output behavior for this query.
        /// </summary>
        public QueryOutput Output;
        /// <summary>
        /// Whether or not to transpose the contents of the query into the file
        /// </summary>
        public bool Transpose;
        /// <summary>
        /// Whether or not the headers of this query should be printed at the top of the file.
        /// </summary>
        public bool PrintHeaders;
        /// <summary>
        /// The interval at which to query
        /// </summary>
        public QueryInterval Interval;
        /// <summary>
        /// The name of this query. Must be unique per file.
        /// </summary>
        public string Name;
        /// <summary>
        /// The filepath of the database that contains this query
        /// </summary>
        public string Database;
        /// <summary>
        /// The name of the dataview to reference in the database.
        /// </summary>
        public string DataView;
        /// <summary>
        /// The time in the Interval to write this query. For Hourly, Time represents the minute. For Daily, Time represents the hour.
        /// </summary>
        public int Time;
        /// <summary>
        /// The character(s) used to delimit values in this Query.
        /// </summary>
        public string Delimiter;
        /// <summary>
        /// A SQL command to use in place of a named query.
        /// </summary>
        public string QueryStatement = "";
        /// <summary>
        /// If true, this Query will be skipped when processing outputs.
        /// </summary>
        public bool Paused;

        /// <summary>
        /// Creates a blank query object
        /// </summary>
        public Query() : this(null,QueryInterval.Daily,"","") { }

        /// <summary>
        /// Create a new query object.
        /// </summary>
        /// <param name="output">A QueryOutput object containing information necessary to perform output operations.</param>
        /// <param name="interval">The interval at which to query</param>
        /// <param name="name">The name of the query in the database</param>
        /// <param name="database">The filepath of the database that contains this query</param>
        /// <param name="viewName">The data view (query or table) to reference to point to in the database.</param>
        /// <param name="transpose">Whether or not to transpose the contents of the query into the file. Default: false</param>
        /// <param name="time">The time in the Interval to write this query. For Hourly, Time represents the minute. For Daily, Time represents the hour. Default: -1</param>
        /// <param name="delimiter">The character(s) used to delimit values in this Query. Default: ","</param>
        /// <param name="printHeaders">Whether or not the headers of this query should be printed at the top of the file.</param>
        public Query(QueryOutput output, QueryInterval interval, string name, string database, string viewName = "",bool transpose = false, int time = -1, string delimiter = ",",bool printHeaders = false) {
            Output = output;
            Interval = interval;
            Name = name;
            if (viewName == "") DataView = name;
            else DataView = viewName;
            Database = database;
            Transpose = transpose;
            Time = time;
            Delimiter = delimiter;
            PrintHeaders = printHeaders;
            Paused = false;
        }

        /// <summary>
        /// Gets all queries from a local Queries.xml file
        /// </summary>
        /// <returns>A List of Queries found it the file</returns>
        [Obsolete("Moved to QueryList.FromXML(). Use that implementation instead.")]
        public static QueryList GetQueries(string path = "Queries.xml") {
            QueryList queries = new QueryList(path);

            XMLFile qFile = new XMLFile(path);
            foreach (XMLSection section in qFile.GetSections()[0].GetSections()) {
                string interval = section.Get("interval").ToLower();
                
                if (!section.HasSections("output")) continue;
                XMLSection outputSection = section.GetSections("output")[0];
                QueryOutput output = null;
                switch (outputSection.Get("type").ToLower()) {
                    case "file":
                        string outputLocation = outputSection.Get("location").Replace('/','\\');
                        outputLocation = Path.GetFullPath(outputLocation);
                        if (outputSection.HasValue("dateformat")) {
                            DateTime date = DateTime.Now;
                            //if (interval == "daily") date = date.AddDays(-1);

                            string[] dateformat = outputSection.Get("dateformat").ToLower().Split(',');
                            StringBuilder formattedDate = new StringBuilder();
                            foreach (string f in dateformat) {
                                switch (f) {
                                    case "dayofyear":
                                        formattedDate.Append(date.DayOfYear.ToString("000"));
                                        break;
                                    default:
                                        try {
                                            formattedDate.Append(date.ToString(f));
                                        } catch (FormatException) { continue; }
                                        break;
                                }
                            }

                            outputLocation = outputLocation.Replace("*", formattedDate.ToString());
                        }
                        output = new QueryOutputFile(outputLocation);
                        break;
                    case "email":
                        output = new QueryOutputEmail(
                            outputSection.Get("from", "OperationsPlanning@santeecooper.com"),
                            outputSection.Get("to").Split(','),
                            outputSection.Get("subject"),
                            outputSection.Get("body"),
                            outputSection.Get("openers",false),
                            outputSection.Get("attachmentFilepath")
                        );
                        break;
                    case "ftp":
                        string filename = outputSection.Get("filename");
                        if (outputSection.HasValue("dateformat")) {
                            DateTime date = DateTime.Now.AddDays(outputSection.Get("dateOffset",0));
                            //if (interval == "daily") date = date.AddDays(-1);

                            string[] dateformat = outputSection.Get("dateformat").ToLower().Split(',');
                            StringBuilder formattedDate = new StringBuilder();
                            foreach (string f in dateformat) {
                                switch (f) {
                                    case "dayofyear":
                                        formattedDate.Append(date.DayOfYear.ToString("000"));
                                        break;
                                    default:
                                        try {
                                            formattedDate.Append(date.ToString(f));
                                        } catch (FormatException) { continue; }
                                        break;
                                }
                            }
                            filename = filename.Replace("*", formattedDate.ToString());
                        }
                        output = new QueryOutputFTP(
                            outputSection.Get("address"),
                            filename,
                            outputSection.Get("remotepath"),
                            outputSection.Get("username"),
                            outputSection.Get("password")
                        );
                        break;
                }

                if (output == null) continue;
                Query q = new Query(output, section.GetEnum<QueryInterval>("interval"), section.Get("name"), section.Get("database"), section.Get("dataview",""),section.Get("transpose", false), section.Get("time", -1), Util.ConvertWhitespaceCharacters(section.Get("delimiter", ",")), section.Get("printHeaders", false));
                if (section.HasValue("queryStatement") && section.Get("queryStatement").Length > 0) q.QueryStatement = section.Get("queryStatement");
                if (section.HasValue("paused")) q.Paused = section.Get("paused", true);
                queries.Add(q);
            }
            return queries;        
        }

        /// <summary>
        /// Write out this query to its output file.
        /// </summary>
        /// <returns>A Problem if one was encountered</returns>
        /// <exception cref="QueryException">Throws QueryExceptions for any exceptions encountered.</exception>
        public void Write() {
            List<string> lines = new List<string>();
            DBData data = null;
            using (DBConnection conn = new DBConnection(Database)) {
                try {
                    conn.Open();
                } catch (ThreadAbortException) {
                    throw;
                } catch (Exception e) {
                    throw new QueryException(this, "Exception encountered when attempting to connect to database at " + Database + ": " + e.Message, e);
                }
                try {
                    if (QueryStatement.Length > 0) data = new DBData(conn.Query(QueryStatement));
                    else data = new DBData(conn.Select(DataView.Length > 0 ? DataView : Name));
                } catch (ThreadAbortException) {
                    throw;
                } catch (Exception e) {
                    throw new QueryException(this, "Exception encountered when attempting to access query " + Name + " in " + Database + ": " + e.Message, e);
                }
            }

            try {
                if (Transpose) {
                    for (int i = 0; i < data.Headers.Length; i++) {
                        StringBuilder line = new StringBuilder();
                        if (PrintHeaders) line.Append(data.Headers[i]);
                        for (int j = 0; j < data.Data.Count; j++) {
                            if (PrintHeaders || j > 0) line.Append(Delimiter);
                            line.Append(data.Data[j][data.Headers[i]]);
                        }
                        lines.Add(line.ToString());
                    }
                } else {
                    if (PrintHeaders) lines.Add(String.Join(",", data.Headers));
                    for (int i = 0; i < data.Data.Count; i++) {
                        StringBuilder line = new StringBuilder();
                        for (int j = 0; j < data.Headers.Length; j++) {
                            if (j > 0) line.Append(Delimiter);
                            line.Append(data.Data[i][data.Headers[j]]);
                        }
                        lines.Add(line.ToString());
                    }
                }
            } catch (ThreadAbortException) {
                throw;
            } catch (Exception e) {
                throw new QueryException(this,"Exception encountered when attempting to parse data from query " + Name + " in " + Database + ": " + e.Message, e);
            }

            try {
                Output.Output(String.Join(Environment.NewLine, lines));
            } catch (ThreadAbortException) {
                throw;
            } catch (Exception e) {
                throw new QueryException(this, "Exception encountered when attempting to output data from query " + Name + " in " + Database + ": " + e.Message, e);
            }
        }

        /// <summary>
        /// Outputs the data for this query object as an XMLSection.
        /// </summary>
        /// <returns>An XML representation of this object.</returns>
        public XMLSection ToXML() {
            XMLSection section = new XMLSection("query");
            section.AddChild("name", Name);
            section.AddChild("database", Database);
            section.AddChild("dataview", DataView);
            section.AddChild("interval", Interval.ToString());
            section.AddChild("time", Time.ToString());
            if (Delimiter != ",") section.AddChild("delimiter", Delimiter);
            if (Transpose) section.AddChild("transpose", Transpose);
            if (PrintHeaders) section.AddChild("printHeaders", PrintHeaders);
            if (Paused) section.AddChild("paused", Paused);

            section.AddSection(Output.ToXML());

            return section;
        }
    }

    /// <summary>
    /// Encapsulation for Lists of type Query.
    /// </summary>
    public class QueryList : List<Query> {
        private readonly string path = "Queries.xml";
        /// <summary>
        /// The path to which this list's file is saved. Optional.
        /// </summary>
        public string FilePath => path;

        /// <summary>
        /// Create an empty QueryList.
        /// </summary>
        public QueryList() : base() { }
        /// <summary>
        /// Create an empty QueryList with an assigned file path.
        /// </summary>
        /// <param name="path">A file path where this list can be saved.</param>
        public QueryList(string path) : base() {
            this.path = path;
        }

        /// <summary>
        /// Converts the contents of this list into an XMLFile.
        /// </summary>
        /// <returns>An XMLFile object containing all the queries in this list.</returns>
        public XMLFile ToXML() {
            XMLFile file = new XMLFile(FilePath);
            file.Children.Clear();
            XMLSection top = new XMLSection("queries");
            foreach (Query q in this) top.AddSection(q.ToXML());
            file.AddSection(top);
            return file;
        }

        /// <summary>
        /// Save this list to its file path as an XML file.
        /// </summary>
        public void Save() {
            ToXML().Save();
        }

        /// <summary>
        /// Load a QueryList from a Queries.xml file.
        /// </summary>
        /// <param name="path">The path of the XML file for this list.</param>
        /// <returns>A QueryList containing all queries in the file.</returns>
        public static QueryList FromXML(string path = "Queries.xml") {
            QueryList queries = new QueryList(path);

            XMLFile qFile = new XMLFile(path);
            foreach (XMLSection section in qFile.GetSections()[0].GetSections()) {
                string interval = section.Get("interval").ToLower();

                if (!section.HasSections("output")) continue;
                XMLSection outputSection = section.GetSections("output")[0];
                QueryOutput output = null;
                switch (outputSection.Get("type").ToLower()) {
                    case "file":
                        string outputLocation = outputSection.Get("location").Replace('/', '\\');
                        outputLocation = Path.GetFullPath(outputLocation);
                        if (outputSection.HasValue("dateformat")) {
                            DateTime date = DateTime.Now;
                            //if (interval == "daily") date = date.AddDays(-1);

                            string[] dateformat = outputSection.Get("dateformat").ToLower().Split(',');
                            StringBuilder formattedDate = new StringBuilder();
                            foreach (string f in dateformat) {
                                switch (f) {
                                    case "dayofyear":
                                        formattedDate.Append(date.DayOfYear.ToString("000"));
                                        break;
                                    default:
                                        try {
                                            formattedDate.Append(date.ToString(f));
                                        } catch (FormatException) { continue; }
                                        break;
                                }
                            }

                            outputLocation = outputLocation.Replace("*", formattedDate.ToString());
                        }
                        output = new QueryOutputFile(outputLocation);
                        break;
                    case "email":
                        output = new QueryOutputEmail(
                            outputSection.Get("from", "OperationsPlanning@santeecooper.com"),
                            outputSection.Get("to").Split(','),
                            outputSection.Get("subject"),
                            outputSection.Get("body"),
                            outputSection.Get("openers", false),
                            outputSection.Get("attachmentFilepath")
                        );
                        break;
                    case "ftp":
                        string filename = outputSection.Get("filename");
                        if (outputSection.HasValue("dateformat")) {
                            DateTime date = DateTime.Now.AddDays(outputSection.Get("dateOffset", 0));
                            //if (interval == "daily") date = date.AddDays(-1);

                            string[] dateformat = outputSection.Get("dateformat").ToLower().Split(',');
                            StringBuilder formattedDate = new StringBuilder();
                            foreach (string f in dateformat) {
                                switch (f) {
                                    case "dayofyear":
                                        formattedDate.Append(date.DayOfYear.ToString("000"));
                                        break;
                                    default:
                                        try {
                                            formattedDate.Append(date.ToString(f));
                                        } catch (FormatException) { continue; }
                                        break;
                                }
                            }
                            filename = filename.Replace("*", formattedDate.ToString());
                        }
                        output = new QueryOutputFTP(
                            outputSection.Get("address"),
                            filename,
                            outputSection.Get("remotepath"),
                            outputSection.Get("username"),
                            outputSection.Get("password")
                        );
                        break;
                }

                if (output == null) continue;
                Query q = new Query(output, section.GetEnum<QueryInterval>("interval"), section.Get("name"), section.Get("database"), section.Get("dataview",""), section.Get("transpose", false), section.Get("time", -1), section.Get("delimiter", ","), section.Get("printHeaders", false));
                if (section.HasValue("queryStatement") && section.Get("queryStatement").Length > 0) q.QueryStatement = section.Get("queryStatement");
                if (section.HasValue("paused")) q.Paused = section.Get("paused", true);
                queries.Add(q);
            }
            return queries;
        }

        /// <summary>
        /// Gets a specified query by name. If more than one query has the same name, only the first will be returned.
        /// </summary>
        /// <param name="name">The name to select by.</param>
        /// <param name="path">Optional, path to query file.</param>
        /// <returns>The specified query if found, else null.</returns>
        public static Query GetQuery(string name, string path = "Queries.xml") {
            foreach (Query q in QueryList.FromXML(path)) if (q.Name.ToLower() == name.ToLower()) return q;
            return null;
        }
    }

    /// <summary>
    /// Wrapper for Exceptions encountered when running queries.
    /// </summary>
    public class QueryException : Exception {
        /// <summary>
        /// The query that triggered this exception.
        /// </summary>
        public Query Query;

        /// <summary>
        /// Initialize a new QueryException with specified query.
        /// </summary>
        /// <param name="q">The Query object that threw this exception.</param>
        public QueryException(Query q) : base() {
            Query = q;
        }
        /// <summary>
        /// Initialize a new QueryException with specified query.
        /// </summary>
        /// <param name="q">The Query object that threw this exception.</param>
        /// <param name="message">A message to include.</param>
        public QueryException(Query q, string message) : base(message) {
            Query = q;
        }
        /// <summary>
        /// Initialize a new QueryException with specified query.
        /// </summary>
        /// <param name="q">The Query object that threw this exception.</param>
        /// <param name="message">A message to include.</param>
        /// <param name="innerException">The underlying exception.</param>
        public QueryException(Query q, string message, Exception innerException) : base(message,innerException) {
            Query = q;
        }
    }
}