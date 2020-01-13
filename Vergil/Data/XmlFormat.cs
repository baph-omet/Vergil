using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vergil.Data {
    /// <summary>
    /// A variant class of Format, specifically represents .xml files.
    /// </summary>
    public class XmlFormat : DataFileFormat {
        /// <summary>
        /// The name of the XML section that contains each of the data sections.
        /// </summary>
        public string ParentNode { get; set; }
        /// <summary>
        /// The name of each of the XML sections that contain the data nodes.
        /// </summary>
        public string ChildNode { get; set; }

        /// <summary>
        /// Initialize a new XMLFormat object.
        /// </summary>
        /// <param name="id">The ID number of this Format.</param>
        /// <param name="parentnode">The name of the XML section that contains each of the data sections.</param>
        /// <param name="childnode">The name of each of the XML sections that contain the data nodes.</param>
        public XmlFormat(int id, string parentnode, string childnode) : base(id,-1,0,DataFileFormatType.XML) {
            ParentNode = parentnode;
            ChildNode = childnode;
        }
    }
}
