using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vergil.Data {
    /// <summary>
    /// Which type of format this is.
    /// </summary>
    public enum DataFileFormatType {
        /// <summary>
        /// Designates .csv files
        /// </summary>
        CSV,
        /// <summary>
        /// Designates .xml files
        /// </summary>
        XML
    }

    /// <summary>
    /// A class for specifying file format. This base class should be used for CSV-like files, which is the default.
    /// </summary>
    public class DataFileFormat {
        /// <summary>
        /// The ID number associated with this Format. Is arbitrary, so it depends on the specific implementation.
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// The FormatType associated with this Format. If not CSV, a subclass should be used.
        /// </summary>
        public DataFileFormatType Type { get; set; }
        /// <summary>
        /// The line of the file on which the file's headers are located. If less than 0, the file's headers will need to be assumed from its Keys.
        /// </summary>
        public int HeaderLine { get; set; }
        /// <summary>
        /// The line of the file on which the rows of data start.
        /// </summary>
        public int DataStart { get; set; }

        /// <summary>
        /// Initialize a new Format object.
        /// </summary>
        /// <param name="ID">The associated ID number.</param>
        /// <param name="headerline">The line on which the headers are located.</param>
        /// <param name="datastart">The line on which the data rows begin.</param>
        /// <param name="type">The associated FormatType.</param>
        public DataFileFormat(int ID, int headerline, int datastart, DataFileFormatType type) {
            this.ID = ID;
            this.HeaderLine = headerline;
            this.DataStart = datastart;
            this.Type = type;
        }

        /// <summary>
        /// Gets the FormatType constant from a given string.
        /// </summary>
        /// <param name="typeName">The name of the desired type. Case-insensitive.</param>
        /// <returns>The specified FormatType if it matches an existing type. If none is found, returns FormatType.CSV.</returns>
        public static DataFileFormatType GetTypeFromString(string typeName) {
            switch (typeName.ToUpper()) {
                case "CSV":
                    return DataFileFormatType.CSV;
                case "XML":
                    return DataFileFormatType.XML;
                default:
                    return DataFileFormatType.CSV;
            }
        }
    }
}
