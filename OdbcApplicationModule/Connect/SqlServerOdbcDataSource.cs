using miac.ServerApp;
using Newtonsoft.Json.Linq;
using syntez.ServerApp.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace eckumoc_netcore_odbc_datamodel.Data
{
    public class SqlServerOdbcDataSource: OdbcDataSource
    {
        /**
         * Строка подключения к SQL Server через ODBC драйвер отличается от ADO-connectionString
         * только параметром, указывающем на драйвер 
         * Driver={SQL Server};
         * 
         * ADO-connection string
         *  Server=DESKTOP-66CFM7U\\SQLEXPRESS;Database=FRMO;Trusted_Connection=True;MultipleActiveResultSets=true
         * ODBC-connection string
         *  Driver={SQL Server};Server=DESKTOP-66CFM7U\\SQLEXPRESS;Database=FRMO;Trusted_Connection=True;MultipleActiveResultSets=true
         */
        public SqlServerOdbcDataSource(string connectionString) : base(connectionString) { }

        public static string FOREIGN_KEYS =
                "SELECT " +
                "     CCU.TABLE_NAME AS SOURCE_TABLE     " +
                "    , CCU.COLUMN_NAME AS SOURCE_COLUMN " +
                "    ,KCU.TABLE_NAME AS TARGET_TABLE " +
                "    ,KCU.COLUMN_NAME AS TARGET_COLUMN " +
                "FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE CCU " +
                "    INNER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC " +
                "        ON CCU.CONSTRAINT_NAME = RC.CONSTRAINT_NAME " +
                "    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU " +
                "        ON KCU.CONSTRAINT_NAME = RC.UNIQUE_CONSTRAINT_NAME " +
                "ORDER BY CCU.TABLE_NAME";

        public SqlServerOdbcDataSource():base("eckumoc-netcore-userprofile")
        {
            
        }

        /**
         * Структура таблиц
         *   SELECT TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS                                       
         */
        public override IDatabaseMetadata GetDatabaseMetadata()
        {
                IDatabaseMetadata dbm = base.GetDatabaseMetadata();
            
                foreach (JObject next in this.Execute(FOREIGN_KEYS))
                {
                    string table = next["SOURCE_TABLE"].Value<string>();
                    string column = next["SOURCE_COLUMN"].Value<string>();
                    string refTable = next["TARGET_TABLE"].Value<string>();
                    string refColumn = next["TARGET_COLUMN"].Value<string>();
                    if (refTable != null)
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
                foreach (var p in dbm.Tables)
                {
                    string table = p.Key;
                    foreach (var nextKey in p.Value.fk)
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
