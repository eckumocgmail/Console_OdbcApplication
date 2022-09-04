using miac.ServerApp;
using Newtonsoft.Json.Linq;
using syntez.ServerApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace messager.ServerApp.Data
{
    public class MySqlOdbcDataSource: OdbcDataSource
    {
        public MySqlOdbcDataSource() : base("mysql_app", "root", "sgdf") { }
        public MySqlOdbcDataSource(string connectionString) : base(connectionString) { }
        public MySqlOdbcDataSource(string datasource, string login, string password):base(datasource,login,password) 
        { 

        }

        private JArray GetForeightKeys()
        {
            string sql = "SELECT "+
                         "TABLE_NAME, "+
                         "COLUMN_NAME, "+
                         "CONSTRAINT_NAME,  "+
                         "REFERENCED_TABLE_NAME, "+
                         "REFERENCED_COLUMN_NAME " +
                         "FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE ";
             
            return this.Execute(sql);
        }

        public override IDatabaseMetadata GetDatabaseMetadata()
        {
            IDatabaseMetadata dbm = base.GetDatabaseMetadata();
            foreach(JObject next in this.GetForeightKeys())
            {
                string table = next["TABLE_NAME"].Value<string>();
                string column = next["COLUMN_NAME"].Value<string>();
                string refTable = next["REFERENCED_TABLE_NAME"].Value<string>();
                string refColumn = next["REFERENCED_COLUMN_NAME"].Value<string>();
                if( refTable != null)
                {
                    dbm.Tables[table].fk[column] = refTable;                    
                }
                else
                {
                    //dbm.Tables[table].pk = column;
                }
            }
            foreach (var p in dbm.Tables)
            {
                p.Value.references = new List<string>();
            }
            foreach ( var p in dbm.Tables)
            {
                string table = p.Key;                
                foreach ( var nextKey in p.Value.fk)
                {
                    string column = nextKey.Key;
                    string refTable = nextKey.Value;
                    dbm.Tables[refTable].references.Add(table);
                }
            }
                
            return dbm;
        }

    }
}
