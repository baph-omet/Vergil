using System;
using System.Configuration;
using Vergil.Utilities;

namespace Vergil.Configuration {
    /// <summary>
    /// A configuration where properties are stored in the program's internal Properties.
    /// </summary>
    public class SettingsConfig : Config {

        /// <summary>
        /// Delete a property from the settings.
        /// </summary>
        /// <param name="property"></param>
        public override void DeleteProperty(string property) {
            Properties.Settings.Default.PropertyValues.Remove(property);
        }

        /// <summary>
        /// Get the value of a property.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <returns>The value of the property as a string, or null if property is not found.</returns>
        public override string Get(string property) {
            if (!HasProperty(property)) return null;
            return Properties.Settings.Default.PropertyValues[property].PropertyValue.ToString();
        }

        /// <summary>
        /// Get the value of a property.
        /// </summary>
        /// <typeparam name="T">The type to attempt conversion to.</typeparam>
        /// <param name="property">The name of the property.</param>
        /// <returns>The value of the property, or null if property is not found.</returns>
        public new T Get<T>(string property) {
            if (!HasProperty(property)) return default(T);
            return Util.Convert<T>(Properties.Settings.Default.PropertyValues[property].PropertyValue);
        }

        /// <summary>
        /// Get the value of a property.
        /// </summary>
        /// <typeparam name="T">The type to attempt conversion to.</typeparam>
        /// <param name="property">The name of the property.</param>
        /// <param name="defaultValue">A default value to return if property cannot be found.</param>
        /// <returns>The value of the property, or defaultValue if property cannot be found.</returns>
        public new T Get<T>(string property, T defaultValue) {
            try {
                object o = Get<T>(property);
                if (o == null) return defaultValue;
                else return (T)o;
            } catch (Exception) {
                return defaultValue;
            }
        }

        /// <summary>
        /// Set the value of a property.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public override void Set(string property, object value) {
            SettingsPropertyValueCollection coll = Properties.Settings.Default.PropertyValues;
            if (HasProperty(property)) coll[property].PropertyValue = value;
            else {
                Properties.Settings.Default.Properties.Add(new SettingsProperty(property));
                coll[property].PropertyValue = value;
            }
        }

        private bool HasProperty(string property) {
            foreach (SettingsPropertyValue pv in Properties.Settings.Default.PropertyValues) if (pv.Name.ToLower() == property.ToLower()) return true;
            return false;
        }
    }
}
