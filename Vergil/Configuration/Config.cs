using System;
using Vergil.Utilities;

namespace Vergil.Configuration {
    /// <summary>
    /// Interface for different configuration types
    /// </summary>
    public abstract class Config {
        private bool debug;
        /// <summary>
        /// The Debug state of this program.
        /// </summary>
        public bool Debug { get { return debug; } set { Set("debug", value); debug = value; } }

        /// <summary>
        /// Get the value of one of this node's children. 
        /// </summary>
        /// <param name="property">The name of the property to return</param>
        /// <returns>The value of the requested child, or the default value if not found.</returns>
        public abstract string Get(string property);
        /// <summary>
        /// Get the value of the specified property with a generic type, if found.
        /// Attempts to convert the value from a string to the specified type. All conversion errors are thrown.
        /// </summary>
        /// <typeparam name="T">The type to which this value will be converted.</typeparam>
        /// <param name="property">The name of the property to return</param>
        /// <returns>The value stored in the specified key, if it exists.</returns>
        public T Get<T>(string property) {
            string value = Get(property);
            if (value == null) throw new ArgumentException("Property " + property + " not found.");
            return Util.Convert<T>(Get(property));
        }
        /// <summary>
        /// Get the value of the specified property with a generic type, if found, else returns the default value.
        /// Attempts to convert the value from a string to the specified type. All conversion errors are thrown.
        /// </summary>
        /// <typeparam name="T">The type to which this value will be converted.</typeparam>
        /// <param name="property">The name of the property to return</param>
        /// <param name="defaultValue">The default value for this property</param>
        /// <returns>The value of the specified property if found, else defaultValue</returns>
        public T Get<T>(string property, T defaultValue) {
            string value = Get(property);
            if (value == null) return defaultValue;
            return Util.Convert<T>(Get(property));
        }

        /// <summary>
        /// Get a Enum value from the specified key and Enum type.
        /// </summary>
        /// <typeparam name="T">The type of the enum to pass.</typeparam>
        /// <param name="property">The property whose value will be checked. Throws an <code>ArgumentException</code> if the key does not exist in this section. All conversion exceptions will be thrown.</param>
        /// <param name="ignoreCase">Whether or not to ignore case when attempting to parse to the enum. Default: true</param>
        /// <returns>An <code>object</code> of the enum constant that is represented by the value of the specified key.</returns>
        public T GetEnum<T>(string property, bool ignoreCase = true) where T : Enum {
            string value = Get(property);
            if (value != null) return EnumUtil.ParseEnum<T>(value, ignoreCase);
            throw new ArgumentException("Property " + property + " not found.");
        }
        /// <summary>
        /// Get a Enum value from the specified key and Enum type.
        /// </summary>
        /// <typeparam name="T">The type of the enum to pass.</typeparam>
        /// <param name="property">The property whose value will be checked. All conversion exceptions will be thrown.</param>
        /// <param name="defaultValue">A default value to pass. If the key is not found, this value will be returned instead.</param>
        /// <param name="ignoreCase">Whether or not to ignore case when attempting to parse to the enum. Default: true</param>
        /// <returns>An <code>object</code> of the enum constant that is represented by the value of the specified key, or the default value if the key is not found.</returns>
        public T GetEnum<T>(string property, T defaultValue, bool ignoreCase = true) where T : Enum {
            string value = Get(property);
            if (value != null) return EnumUtil.ParseEnum<T>(value, ignoreCase);
            if (defaultValue != null) return defaultValue;
            throw new ArgumentException("Property " + property + " not found.");
        }
        
        /// <summary>
        /// Overwrites the config to include a new value for the specified key. If the specified key is not found, it will be appended to the config.
        /// </summary>
        /// <param name="property">The property whose value will be overwritten</param>
        /// <param name="value">The value to assign to the specified key</param>
        public abstract void Set(string property, object value);

        /// <summary>
        /// Delete the specified property.
        /// </summary>
        /// <param name="property">The property to delete.</param>
        public abstract void DeleteProperty(string property);
    }
}
