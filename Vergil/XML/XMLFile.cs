using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace Vergil.XML {
    /// <summary>
    /// Class for reading XML files into dynamic objects. Can be used to read data or configurations. Best used for shorter files.
    /// </summary>
    public class XMLFile {
        /// <summary>
        /// The filepath to this document
        /// </summary>
        public string Location;

        /// <summary>
        /// All XMLNodes contained in the top level of this document
        /// </summary>
        public List<XMLNode> Children;

        /// <summary>
        /// Initializes a new XMLFile object with no inherent file.
        /// </summary>
        public XMLFile() {
            Children = new List<XMLNode>();
        }
        /// <summary>
        /// Create a new XMLFile object with the specified XML file. If the file does not already exist, it will be created.
        /// </summary>
        /// <param name="location">The location of this XML file</param>
        public XMLFile(string location) {
            if (!location.Split('.')[1].Equals(".xml")) location = location.Split('.')[0] + ".xml";
            if (!File.Exists(location)) File.Create(location);
            Location = location;
            Children = new List<XMLNode>();
            Load(XmlReader.Create(location));
        }

        /// <summary>
        /// Load an XMLFile from a string of XML.
        /// </summary>
        /// <param name="xmlText">The text containing the XML. Must be valid XML.</param>
        /// <returns>An XMLFile containing a document with the XML you passed to it.</returns>
        public static XMLFile FromText(string xmlText) {
            XmlDocument d = new XmlDocument();
            try {
                d.LoadXml(xmlText);
            } catch (XmlException) {
                throw new ArgumentException("Invalid XML passed.");
            }
            /*string tempPath = Util.GetFirstFreePath(Directory.GetCurrentDirectory() + "\\tempXML.xml");
            File.WriteAllText(tempPath,xmlText);
            XMLFile f = new XMLFile(tempPath);
            File.Delete(tempPath);
            return f;*/
            XMLFile f = new XMLFile();
            f.Load(XmlReader.Create(new StringReader(xmlText)));
            return f;
        }

        private void Load(XmlReader reader) {
            using (reader) {
                while (reader.Read()) {
                    if (reader.NodeType == XmlNodeType.Element) {
                        XmlReader subtree = reader.ReadSubtree();
                        subtree.ReadToFollowing(reader.Name, reader.NamespaceURI);
                        XMLNode node = ReadNode(subtree);
                        if (node != null) Children.Add(node);
                    }
                }
            }
        }

        /// <summary>
        /// Gets all children of this node that are XML sections
        /// </summary>
        /// <returns>A List containing all XML sections nested inside this section</returns>
        public List<XMLSection> GetSections() {
            List<XMLSection> sections = new List<XMLSection>();
            foreach (XMLNode child in Children) {
                if (child is XMLSection) sections.Add((XMLSection)child);
            }
            return sections;
        }

        /// <summary>
        /// Gets all children of this node that are XML sections and match the specified key
        /// </summary>
        /// <param name="key">The key to look up</param>
        /// <returns>A List containing all XML sections nested inside this section that match the specified key</returns>
        public List<XMLSection> GetSections(string key) {
            List<XMLSection> sections = new List<XMLSection>();
            foreach (XMLNode child in Children) {
                if (child is XMLSection && child.Key.ToUpper().Equals(key.ToUpper())) sections.Add((XMLSection)child);
            }
            return sections;
        }

        /// <summary>
        /// Gets all children of this node that are XML sections and contain a node with the corresponding key and value
        /// </summary>
        /// <param name="nodeKey">The key of a node inside a child of this node</param>
        /// <param name="value">The value of the key of a node inside a child of this node</param>
        /// <returns>A List containing all XML sections nested inside this section that contain a node with the specified key and value</returns>
        public List<XMLSection> GetSections(string nodeKey, string value) {
            List<XMLSection> sections = new List<XMLSection>();
            foreach (XMLNode child in Children) {
                if (child is XMLSection && ((XMLSection)child).Get(nodeKey).ToUpper().Equals(value.ToUpper())) sections.Add((XMLSection)child);
            }
            return sections;
        }

        /// <summary>
        /// Recursively finds the first-occurring, highest-level XML section with the specified key. Should be used to find unique container sections.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public XMLSection FindSection(string key) {
            if (GetSections(key).Count > 0) return GetSections(key)[0];
            foreach (XMLSection section in GetSections()) {
                XMLSection found = section.FindSection(key);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// Finds the first occurrence of a node with the specified name in the file
        /// </summary>
        /// <param name="key">The name of the node to find</param>
        /// <returns>An XMLNode representing the node if found, else null</returns>
        public XMLNode FindNode(string key) {
            foreach (XMLNode node in Children) {
                if (node.Key.ToLower().Equals(key.ToLower())) return node;
                else {
                    if (node is XMLSection) {
                        XMLNode found = ((XMLSection)node).FindNode(key);
                        if (found != null) return found;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Adds a top-level child node.
        /// </summary>
        /// <param name="node">The XMLNode to add.</param>
        public void AddChild(XMLNode node) {
            if (Children == null) Children = new List<XMLNode>();
            Children.Add(node);
        }
        /// <summary>
        /// Adds a top-level child node.
        /// </summary>
        /// <param name="key">The name of the node to add.</param>
        /// <param name="value">The value of the node to add.</param>
        public void AddChild(string key, string value) {
            if (Children == null) Children = new List<XMLNode>();
            Children.Add(new XMLNode(key, value));
        }

        /// <summary>
        /// Adds a top-level section.
        /// </summary>
        /// <param name="section">The XMLSection to add.</param>
        public void AddSection(XMLSection section) {
            if (Children == null) Children = new List<XMLNode>();
            Children.Add(section);
        }
        /// <summary>
        /// Adds a top-level section.
        /// </summary>
        /// <param name="key">The name of the section to add.</param>
        public void AddSection(string key) {
            if (Children == null) Children = new List<XMLNode>();
            Children.Add(new XMLSection(key));
        }

        /// <summary>
        /// Saves this XMLFile back to its location
        /// </summary>
        public void Save() { Save(Location); }
        /// <summary>
        /// Saves a copy of this XMLFile to the specified location
        /// </summary>
        /// <param name="path">The location to which this file should be written</param>
        public void Save(string path) {
            File.WriteAllText(path, ToString());
        }

        /// <summary>
        /// Returns a string representation of this document.
        /// </summary>
        /// <returns>The text in this document.</returns>
        public override string ToString() {
            return ToString(true);
        }
        /// <summary>
        /// Returns a string representation of this document with option to remove whitespace.
        /// </summary>
        /// <param name="whitespace">If true, whitespace characters will be added.</param>
        /// <returns>The text in this document.</returns>
        public string ToString(bool whitespace) {
            StringBuilder lines = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            foreach (XMLNode child in Children) lines.Append(WriteNode(child, 0, whitespace));
            return lines.ToString();
        }

        private string WriteNode(XMLNode node, int indent, bool whitespace = true) {
            StringBuilder text = new StringBuilder();
            if (whitespace) {
                text.Append(Environment.NewLine);
                for (int i = 0; i < indent; i++) text.Append("\t");
            }
            text.Append("<" + node.Key);
            foreach (string attribute in node.GetAttributes().Keys) text.Append(" " + attribute + "=\"" + node.GetAttributes()[attribute] + "\"");
            text.Append(">");
            if (node.HasValue()) text.Append(node.Get().Replace("&", "&amp;"));
            else if (node is XMLSection) foreach (XMLNode n in ((XMLSection)node).Children) text.Append(WriteNode(n, indent + 1, whitespace));
            if (whitespace && node is XMLSection) {
                text.Append(Environment.NewLine);
                for (int i = 0; i < indent; i++) text.Append("\t");
            }
            text.Append("</" + node.Key + ">");
            return text.ToString();
        }

        private XMLNode ReadNode(XmlReader reader) {
            using (reader) {
                string key = reader.Name;
                Dictionary<string, string> attributes = ReadAttributes(reader);

                if (reader.IsEmptyElement) return new XMLNode(key, "", attributes);

                reader.Read();
                string value = "";
                List<XMLNode> children = new List<XMLNode>();
                while (reader.NodeType != XmlNodeType.EndElement && reader.Name != key) {
                    if (reader.NodeType == XmlNodeType.Text) value = reader.Value;
                    if (reader.NodeType == XmlNodeType.Element) {
                        XmlReader subtree = reader.ReadSubtree();
                        subtree.ReadToFollowing(reader.Name, reader.NamespaceURI);
                        XMLNode node = ReadNode(subtree);
                        if (node != null) children.Add(node);
                    }
                    reader.Read();
                }
                if (children.Count > 0) return new XMLSection(key, attributes, children);
                else if (value.Length > 0) return new XMLNode(key, value, attributes);
                else return null;
            }
        }

        private Dictionary<string, string> ReadAttributes(XmlReader reader) {
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            if (reader.HasAttributes) {
                while (reader.MoveToNextAttribute()) {
                    attributes.Add(reader.Name.ToLower(), reader.Value);
                }
            }
            return attributes;
        }
    }
}
