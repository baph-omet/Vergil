using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.Common;

namespace Vergil.Data.DB {
    public class SQLiteConnection : DBConnection {

        public SQLiteConnection(string connString) : base (connString) {

        }

        public override int AddRecord(string dataView, IEnumerable<string> values, string updateCondition) {
            throw new NotImplementedException();
        }

        public override int AddRecord(string table, IEnumerable<string> fields, IEnumerable<string> values, string updateCondition) {
            throw new NotImplementedException();
        }

        public override int Insert(string table, IEnumerable<string> fields, IEnumerable<string> values) {
            throw new NotImplementedException();
        }

        public override DbDataReader Query(string query) {
            throw new NotImplementedException();
        }

        public override DbDataReader Select(string table, string field = "*", string SelectCondition = "", bool distinct = false, Dictionary<string, object> parameters = null) {
            throw new NotImplementedException();
        }

        public override DbDataReader Select(string table, IEnumerable<string> fields, string SelectCondition = "", bool distinct = false, Dictionary<string, object> parameters = null) {
            throw new NotImplementedException();
        }

        public override int Update(string table, IEnumerable<string> fields, IEnumerable<string> values, string updateCondition) {
            throw new NotImplementedException();
        }

        protected override DbCommand GenerateInsertQuery(string table, IEnumerable<string> fields, IEnumerable<string> values) {
            throw new NotImplementedException();
        }

        protected override DbCommand GenerateSelectQuery(string table, IEnumerable<string> fields, string selectCondition, bool distinct) {
            throw new NotImplementedException();
        }

        protected override DbCommand GenerateUpdateQuery(string table, IEnumerable<string> fields, IEnumerable<string> values, string updateCondition) {
            throw new NotImplementedException();
        }
    }
}
