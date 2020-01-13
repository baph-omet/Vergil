using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace Vergil.XML {
    /// <summary>
    /// Static class containing methods for serializing and deserializing objects to XML.
    /// </summary>
    public static class XMLSerialization {
        /// <summary>
        /// Serialize an object into XML at the specified location.
        /// </summary>
        /// <param name="file">The location of the XML file where the serialization will be dumped.</param>
        /// <param name="data">The object to serialize.</param>
        public static void Serialize(string file, object data) {
            if (!File.Exists(file)) throw new ArgumentException(file + " does not exist or is not accessible.");
            XmlSerializer serializer = new XmlSerializer(data.GetType());
            using (FileStream stream = new FileStream(file, FileMode.Create)) serializer.Serialize(stream, data);
        }

        /// <summary>
        /// Deserialize an object from XML at the specified location.
        /// </summary>
        /// <typeparam name="T">The type to which the file will deserialize.</typeparam>
        /// <param name="file">The XML file to parse.</param>
        /// <returns>A object that has been deserialized from XML. It will need to be casted down to its actual type after being returned.</returns>
        public static T Deserialize<T>(string file) {
            if (!File.Exists(file)) throw new ArgumentException(file + " does not exist or is not accessible.");
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (XmlReader reader = new XmlTextReader(file)) return (T)serializer.Deserialize(reader);
        }
    }
}
