using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SOPAPI.XML;

namespace SOPAPI.Configuration {
    /// <summary>
    /// A configuration in XML format. All properties should be nodes within a specific section.
    /// </summary>
    public class XMLConfig : Config, IDisposable {
        private readonly string path;
        private XMLFile file;
        private readonly string parentNode;

        /// <summary>
        /// Initialize a new XMLConfig
        /// </summary>
        /// <param name="path">The path to this XML file</param>
        /// <param name="parentNode">The name of the node in which this config's properties are located. Defaults to the root node.</param>
        public XMLConfig(string path="Config.xml",string parentNode = "") {
            this.path = path;
            file = new XMLFile(path);
            if (parentNode.Length == 0) parentNode = file.Children[0].Key;
            else this.parentNode = parentNode;
        }

        /// <summary>
        /// Get the value of a property.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public override string Get(string property) {
            return file.FindSection(parentNode).Get(property);
        }
        /// <summary>
        /// Get the value of one of this node's children with a generic type, if key is found.
        /// Attempts to convert the value from a string to the specified type. All conversion errors are thrown.
        /// </summary>
        /// <typeparam name="T">The type to which this value will be converted.</typeparam>
        /// <param name="property">The name of the property to return</param>
        /// <returns>The value stored in the specified key, if it exists.</returns>
        public new T Get<T>(string property) {
            return file.FindSection(parentNode).Get<T>(property);
        }
        /// <summary>
        /// Get the value of one of this node's children with a generic type, if key is found, else returns the default value.
        /// Attempts to convert the value from a string to the specified type. All conversion errors are thrown.
        /// </summary>
        /// <typeparam name="T">The type to which this value will be converted.</typeparam>
        /// <param name="property">The name of the property to return</param>
        /// <param name="defaultValue">The default value of this property.</param>
        /// <returns>The value stored in the specified key, if it exists, else the default value.</returns>
        public new T Get<T>(string property, T defaultValue) {
            return file.FindSection(parentNode).Get<T>(property, defaultValue);
        }

        /// <summary>
        /// Get a Enum value from the specified key and Enum type.
        /// </summary>
        /// <typeparam name="T">The type of the enum to pass.</typeparam>
        /// <param name="property">The property whose value will be checked. Throws an <code>ArgumentException</code> if the key does not exist in this section. All conversion exceptions will be thrown.</param>
        /// <param name="ignoreCase">Whether or not to ignore case when attempting to parse to the enum. Default: true</param>
        /// <returns>An <code>object</code> of the enum constant that is represented by the value of the specified key.</returns>
        public new T GetEnum<T>(string property, bool ignoreCase = true) {
            string value = Get(property);
            if (value != null) return Util.ParseEnum<T>(value, ignoreCase);
            throw new ArgumentException("Key " + property + " not found.");
        }
        /// <summary>
        /// Get a Enum value from the specified key and Enum type.
        /// </summary>
        /// <typeparam name="T">The type of the enum to pass.</typeparam>
        /// <param name="property">The property whose value will be checked. All conversion exceptions will be thrown.</param>
        /// <param name="defaultValue">A default value to pass. If the key is not found, this value will be returned instead.</param>
        /// <param name="ignoreCase">Whether or not to ignore case when attempting to parse to the enum. Default: true</param>
        /// <returns>An <code>object</code> of the enum constant that is represented by the value of the specified key, or the default value if the key is not found.</returns>
        public new T GetEnum<T>(string property, T defaultValue, bool ignoreCase = true) {
            string value = Get(property);
            if (value != null) return Util.ParseEnum<T>(value, ignoreCase);
            if (defaultValue != null) return defaultValue;
            throw new ArgumentException("Key " + property + " not found.");
        }

        /// <summary>
        /// Overwrites the config to include a new value for the specified key. If the specified key is not found, it will be appended to the config.
        /// </summary>
        /// <param name="property">The property whose value will be overwritten</param>
        /// <param name="value">The value to assign to the specified key</param>
        public override void Set(string property, object value) {
            XMLSection section = file.FindSection(parentNode);
            if (section == null) throw new InvalidConfigException("Parent node \"" + parentNode + "\" not found.");
            XMLNode node = section.FindNode(property);
            if (node == null) section.AddChild(property,value.ToString());
            else node.Value = value.ToString();
        }

        /// <summary>
        /// Saves this config back to its file.
        /// </summary>
        public void Save() {
            file.Save();
        }
        /// <summary>
        /// Saves a copy of this config to a separate file location.
        /// </summary>
        /// <param name="path">The file path where the copy will be saved.</param>
        public void Save(string path) {
            file.Save(path);
        }

        /// <summary>
        /// Reloads this file from file to memory. Any changes made since the last load will be lost.
        /// </summary>
        public void Reload() {
            file = new XMLFile(path);
        }

        /// <summary>
        /// Delete the specified property.
        /// </summary>
        /// <param name="property">The property to delete.</param>
        public override void DeleteProperty(string property) {
            XMLSection section = file.FindSection(parentNode);
            if (section == null) throw new InvalidConfigException("Parent node \"" + parentNode + "\" not found.");
            XMLNode node = section.FindNode(property);
            if (node != null) section.Children.Remove(node);
        }

        /// <summary>
        /// Saves back to file before deleting.
        /// </summary>
        public void Dispose() {
            Save();
        }
    }
}
