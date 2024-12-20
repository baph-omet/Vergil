namespace Vergil.Data.DB {
    /// <summary>
    /// Class for a field definition in a database.
    /// </summary>
    public class DBFieldDefinition {
        /// <summary>
        /// The SQL-safe name of this field
        /// </summary>
        public string Name;
        /// <summary>
        /// The SQL-safe name of this field's type parameter. Varies by database implementation.
        /// </summary>
        public string TypeName;
        /// <summary>
        /// Whether or not this field is a primary key
        /// </summary>
        public bool Primary;
        /// <summary>
        /// If true and Primary is true, this field is sorted descending
        /// </summary>
        public bool Descending;

        /// <summary>
        /// Create a new instance of this class
        /// </summary>
        /// <param name="name">The SQL-safe name of this field</param>
        /// <param name="typeName">The SQL-safe name of this field's type parameter. Varies by database implementation.</param>
        /// <param name="primary">Whether or not this field is a primary key</param>
        /// <param name="descending">If true and Primary is true, this field is sorted descending</param>
        public DBFieldDefinition(string name, string typeName, bool primary = false, bool descending = false) {
            Name = name;
            TypeName = typeName;
            Primary = primary;
            Descending = descending;
        }

        /// <summary>
        /// Name + Type
        /// </summary>
        /// <returns>String representation of this object</returns>
        public override string ToString() {
            return Name + " " + TypeName;
        }
    }
}
