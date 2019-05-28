using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOPAPI.Data {
    /// <summary>
    /// Class that represents a translation between a file's header and its respective database Field.
    /// </summary>
    public class Key {
        /// <summary>
        /// The name of the header in the DataFile.
        /// </summary>
        public string Header { get; set; }
        /// <summary>
        /// The name of the field in the DataFile's Table.
        /// </summary>
        public string Field { get; set; }
        /// <summary>
        /// Formatting character used to wrap this Key's data before insertion to the database. Not to be confused with the Format type.
        /// </summary>
        public char Format { get; set; }
        /// <summary>
        /// The Calculation object associated with this Key.
        /// </summary>
        public Calculation Calculation { get; set; }
        /// <summary>
        /// A string containing a date format used if this Key contains date data. Can be null.
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        /// Initialize a new Key object.
        /// </summary>
        /// <param name="header">The name of the header in the DataFile.</param>
        /// <param name="field">The name of the field in the DataFile's Table.</param>
        /// <param name="format">Formatting character used to wrap this Key's data before insertion to the database. Not to be confused with the Format type.</param>
        /// <param name="calculation">The Calculation object associated with this Key.</param>
        public Key(string header, string field, char format, Calculation calculation) : this(header,field,format,calculation,null) { }
        /// <summary>
        /// Initialize a new Key object.
        /// </summary>
        /// <param name="header">The name of the header in the DataFile.</param>
        /// <param name="field">The name of the field in the DataFile's Table.</param>
        /// <param name="format">Formatting character used to wrap this Key's data before insertion to the database. Not to be confused with the Format type.</param>
        /// <param name="calculation">The Calculation object associated with this Key.</param>
        /// <param name="dateformat">A string containing a date format used if this Key contains date data.</param>
        public Key(string header, string field, char format, Calculation calculation, string dateformat) {
            Header = header;
            Field = field;
            Format = format;
            Calculation = calculation;
            DateFormat = dateformat;
        }
    }
}
