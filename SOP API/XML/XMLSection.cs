using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOPAPI.XML {
    /// <summary>
    /// Class that represents an XML Node that contains child nodes
    /// </summary>
    public class XMLSection : XMLNode {

        /// <summary>
        /// Each of the direct children of this section
        /// </summary>
        public List<XMLNode> Children;

        /// <summary>
        /// Initializes a new XMLSection object with no attributes or children
        /// </summary>
        /// <param name="key">The name of this section</param>
        public XMLSection(string key) : this(key, new Dictionary<string, string>(), new List<XMLNode>()) { }
        /// <summary>
        /// Initialize a new XMLSection
        /// </summary>
        /// <param name="key">The name of this node's element</param>
        /// <param name="attributes">A dictionary of all of this node's attributes</param>
        public XMLSection(string key, Dictionary<string, string> attributes) : this(key, attributes, new List<XMLNode>()) { }
        /// <summary>
        /// Initialize a new XMLSection
        /// </summary>
        /// <param name="key">The name of this node's element</param>
        /// <param name="attributes">A dictionary of all of this node's attributes</param>
        /// <param name="children">A List of XMLNodes that represent this section's children</param>
        public XMLSection(string key, Dictionary<string,string> attributes, List<XMLNode> children) : base(key, "", attributes) {
            this.Key = key;
            this.Attributes = attributes;
            this.Children = children;
        }

        /// <summary>
        /// Get the value of one of this node's children. 
        /// </summary>
        /// <param name="key">The name of the child node to find</param>
        /// <param name="defaultValue">A default value to return if the specified key is not found.</param>
        /// <returns>The value of the requested child, or the default value if not found.</returns>
        public string Get(string key, string defaultValue = "") {
            foreach (XMLNode child in Children) {
                if (child.GetKey().ToUpper().Equals(key.ToUpper())) {
                    return child.Get();
                }
            }
            return defaultValue;
        }
        /// <summary>
        /// Get the value of one of this node's children with a generic type, if key is found.
        /// Attempts to convert the value from a string to the specified type. All conversion errors are thrown.
        /// </summary>
        /// <typeparam name="T">The type to which this value will be converted.</typeparam>
        /// <param name="key">The key for which to search.</param>
        /// <returns>The value stored in the specified key, if it exists.</returns>
        public T Get<T>(string key) {
            if (HasValue(key)) return Util.Convert<T>(Get(key));
            throw new ArgumentException("Key " + key + " not found.");
        }
        /// <summary>
        /// Get the value of one of this node's children with a generic type. If the key is not found, the default value will be returned.
        /// Attempts to convert the value from a string to the specified type. All conversion errors are thrown.
        /// </summary>
        /// <typeparam name="T">The type to which this value will be converted.</typeparam>
        /// <param name="key">The key for which to search.</param>
        /// <param name="defaultValue">The value that is returned if the specified key is not found.</param>
        /// <returns>The value stored in the specified key, if it exists, else the default value.</returns>
        public T Get<T>(string key, T defaultValue) {
            if (HasValue(key)) return Util.Convert<T>(Get(key));
            return defaultValue;
        }

        /// <summary>
        /// Get a Enum value from the specified key and Enum type.
        /// </summary>
        /// <typeparam name="T">The type of the enum to pass.</typeparam>
        /// <param name="key">The key whose value will be checked. Throws an <code>ArgumentException</code> if the key does not exist in this section. All conversion exceptions will be thrown.</param>
        /// <param name="ignoreCase">Whether or not to ignore case when attempting to parse to the enum. Default: true</param>
        /// <returns>An <code>object</code> of the enum constant that is represented by the value of the specified key.</returns>
        public T GetEnum<T>(string key, bool ignoreCase = true) where T : Enum {
            if (HasValue(key)) return Util.ParseEnum<T>(Get(key), ignoreCase);
            throw new ArgumentException("Key " + key + " not found.");
        }
        /// <summary>
        /// Get a Enum value from the specified key and Enum type.
        /// </summary>
        /// <typeparam name="T">The type of the enum to pass.</typeparam>
        /// <param name="key">The key whose value will be checked. All conversion exceptions will be thrown.</param>
        /// <param name="defaultValue">A default value to pass. If the key is not found, this value will be returned instead.</param>
        /// <param name="ignoreCase">Whether or not to ignore case when attempting to parse to the enum. Default: true</param>
        /// <returns>An <code>object</code> of the enum constant that is represented by the value of the specified key, or the default value if the key is not found.</returns>
        public T GetEnum<T>(string key, T defaultValue, bool ignoreCase = true) where T : Enum {
            if (HasValue(key)) return Util.ParseEnum<T>(Get(key), ignoreCase);
            if (defaultValue != null) return defaultValue;
            throw new ArgumentException("Key " + key + " not found.");
        }

        /// <summary>
        /// Get all children of this node
        /// </summary>
        /// <returns>A List of XMLNodes that are children of this node</returns>
        public List<XMLNode> GetChildren() {
            return Children;
        }
        /// <summary>
        /// Get all children of this node that match the given key.
        /// </summary>
        /// <param name="key">A key to filter this node's children</param>
        /// <returns>All XMLNodes inside this node that match the given key</returns>
        public List<XMLNode> GetChildren(string key) {
            List<XMLNode> chld = new List<XMLNode>();
            foreach (XMLNode child in Children) {
                if (child.GetKey().ToUpper().Equals(key.ToUpper())) chld.Add(child);
            } return chld;
        }

        /// <summary>
        /// Add a new direct child to this section
        /// </summary>
        /// <param name="key">The name of the child node</param>
        /// <param name="value">The value of the child node</param>
        public void AddChild(string key, string value) { AddChild(new XMLNode(key, value)); }
        /// <summary>
        /// Add a new direct child to this section
        /// </summary>
        /// <param name="key">The name of the child node</param>
        /// <param name="value">The value of the child node</param>
        public void AddChild(string key, object value) { AddChild(new XMLNode(key, value.ToString())); }
        /// <summary>
        /// Add a new empty direct child to this section
        /// </summary>
        /// <param name="key">The name of the child node</param>
        public void AddChild(string key) { AddChild(new XMLNode(key)); }
        /// <summary>
        /// Add a new direct child to this section
        /// </summary>
        /// <param name="node">An XMLNode to add</param>
        public void AddChild(XMLNode node) {
            if (Children == null) Children = new List<XMLNode>();
            Children.Add(node);
        }

        /// <summary>
        /// Add a new XMLSection as a direct child to this section
        /// </summary>
        /// <param name="key">The name of the child section</param>
        public void AddSection(string key) { AddSection(new XMLSection(key)); }
        /// <summary>
        /// Add a new XMLSection as a direct child to this section
        /// </summary>
        /// <param name="section">An XMLSection to add</param>
        public void AddSection(XMLSection section) {
            if (Children == null) Children = new List<XMLNode>();
            Children.Add(section);
        }

        /// <summary>
        /// Gets all children of this node that are XML sections
        /// </summary>
        /// <returns>A List containing all XML sections nested inside this section</returns>
        public List<XMLSection> GetSections() {
            List<XMLSection> sections = new List<XMLSection>();
            foreach (XMLNode child in GetChildren()) {
                if (child is XMLSection) sections.Add((XMLSection) child);
            } return sections;
        }

        /// <summary>
        /// Gets all children of this node that are XML sections and match the specified key
        /// </summary>
        /// <param name="key">The key to look up</param>
        /// <returns>A List containing all XML sections nested inside this section that match the specified key</returns>
        public List<XMLSection> GetSections(string key) {
            List<XMLSection> sections = new List<XMLSection>();
            foreach (XMLNode child in GetChildren()) {
                if (child is XMLSection && child.GetKey().ToUpper().Equals(key.ToUpper())) sections.Add((XMLSection) child);
            } return sections;
        }

        /// <summary>
        /// Gets all children of this node that are XML sections and contain a node with the corresponding key and value
        /// </summary>
        /// <param name="nodeKey">The key of a node inside a child of this node</param>
        /// <param name="value">The value of the key of a node inside a child of this node</param>
        /// <returns>A List containing all XML sections nested inside this section that contain a node with the specified key and value</returns>
        public List<XMLSection> GetSections(string nodeKey, string value) {
            List<XMLSection> sections = new List<XMLSection>();
            foreach (XMLNode child in GetChildren()) {
                if (child is XMLSection && ((XMLSection) child).Get(nodeKey).ToUpper().Equals(value.ToUpper())) sections.Add((XMLSection) child);
            } return sections;
        }

        /// <summary>
        /// Finds the first occurrence of a node with the specified name under this section
        /// </summary>
        /// <param name="key">The name of the node to find</param>
        /// <returns>An XMLNode representing the node if found, else null</returns>
        public XMLNode FindNode(string key) {
            foreach (XMLNode node in GetChildren()) {
                if (node.GetKey().ToLower().Equals(key.ToLower())) return node;
                else {
                    if (node is XMLSection) {
                        XMLNode found = ((XMLSection) node).FindNode(key);
                        if (found != null) return found;
                    }
                }
            }
            return null;
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
            } return null;
        }

        /// <summary>
        /// Checks to see that the child node has a value.
        /// </summary>
        /// <param name="key">The key of the child node to check</param>
        /// <returns>True if the child node exists, is not a section, and has a value, else false.</returns>
        public bool HasValue(string key) {
            return Get(key).Length > 0;
        }

        /// <summary>
        /// Checks to see if this section contains any XMLSections.
        /// </summary>
        /// <returns>True if at least one XMLSection is present.</returns>
        public bool HasSections() {
            return GetSections().Count > 0;
        }
        /// <summary>
        /// Checks to see if this section contains any XMLSections with the given key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if at least one XMLSection is present with the given key.</returns>
        public bool HasSections(string key) {
            return GetSections(key).Count > 0;
        }
    }
}
