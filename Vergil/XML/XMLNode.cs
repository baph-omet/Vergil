using System.Collections.Generic;
using Vergil.Utilities;

namespace Vergil.XML {
    /// <summary>
    /// A class representing a single node in an XML file
    /// </summary>
    public class XMLNode {
        /// <summary>
        /// The name of this node
        /// </summary>
        public string Key;
        /// <summary>
        /// The value of this node
        /// </summary>
        public string Value;
        /// <summary>
        /// Dictionary containing each of this node's attributes
        /// </summary>
        public Dictionary<string, string> Attributes;

        /// <summary>
        /// Initializes an empty node with the given name
        /// </summary>
        /// <param name="key">The name of this node</param>
        public XMLNode(string key) : this(key, "", new Dictionary<string, string>()) { }
        /// <summary>
        /// Initializes a non-empty node with the given name and value. Has no attributes.
        /// </summary>
        /// <param name="key">The name of this node</param>
        /// <param name="value">The value to assign to this node</param>
        public XMLNode(string key, string value) : this(key, value, new Dictionary<string, string>()) { }
        /// <summary>
        /// Initializes a non-empty node with the given name, value, and attributes.
        /// </summary>
        /// <param name="key">The name of this node</param>
        /// <param name="value">The value to assign to this node</param>
        /// <param name="attributes">A Dictionary containing this node's attributes</param>
        public XMLNode(string key, object value, Dictionary<string, string> attributes) {
            Key = key;
            Value = value.ToString();
            Attributes = attributes;
        }

        /// <summary>
        /// Get the value of this node. 
        /// </summary>
        /// <returns>The value of this node.</returns>
        public string Get() {
            return Value;
        }
        /// <summary>
        /// Get the value of one of this node with a generic type.
        /// Attempts to convert the value from a string to the specified type. All conversion errors are thrown.
        /// </summary>
        /// <typeparam name="T">The type to which this value will be converted.</typeparam>
        /// <returns>The value stored in the specified key, if it exists.</returns>
        public T Get<T>() {
            return Util.Convert<T>(Value);
        }

        /// <summary>
        /// Gets this node's attributes
        /// </summary>
        /// <returns>A Dictionary containing this node's attributes</returns>
        public Dictionary<string, string> GetAttributes() {
            return Attributes;
        }

        /// <summary>
        /// Gets the value of a specific attribute of this node
        /// </summary>
        /// <param name="attribute">The name of the attribute to retrieve</param>
        /// <returns>The value of the specified attribute, or null if the attribute cannot be found.</returns>
        public string GetAttribute(string attribute) {
            foreach (string attr in Attributes.Keys) {
                if (attr.ToUpper().Equals(attribute.ToUpper())) return Attributes[attribute.ToLower()];
            }
            return null;
        }

        /// <summary>
        /// Checks to see if this node has a value
        /// </summary>
        /// <returns>True if the value is not an empty string, else false.</returns>
        public bool HasValue() {
            return Value.Length > 0;
        }
    }
}
