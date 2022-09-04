
using Newtonsoft.Json.Linq;


using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace miac.ServerApp
{
    public class OleDataSource 
    {
        private static string DEFAULT_PROVIDER = "Microsoft.ACE.OLEDB.12.0";

        private string provider;
        private string datasource;
        
        public OleDataSource(string provider, string type, string datasource)
        {
            this.provider = provider;
            this.datasource = datasource;
        }

        public OleDataSource(string type, string datasource)
        {
            this.provider = DEFAULT_PROVIDER;
            switch (type)
            {
                case "Access":
                    this.datasource = datasource;
                    break;
                case "Excel":
                    this.datasource= string.Format("{0};Extended Properties=Excel 8.0;", datasource);
                   
                    break;
                default: throw new Exception("Unknown type of OleDb datasources");
            }
             
        }

        private string GetConnectionString()
        {
            return "Provider="+this.provider+";Data Source="+this.datasource;
        }

        private OleDbConnection GetConnection()
        {
            OleDbConnection connection = new OleDbConnection();
            connection.ConnectionString = GetConnectionString();
            connection.Open();
            return connection;
        }
        public JArray GetTablesMetadata()
        {
            using (OleDbConnection connection = GetConnection())
            {
                DataTable dataTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                return this.Convert(dataTable);
            }
        }

        public string[] GetTables()
        {
            using (OleDbConnection connection = GetConnection())
            {                
                DataTable dataTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
               
                string[] tables = new string[dataTable.Rows.Count];     int i = 0;                  
                foreach (DataRow row in dataTable.Rows)
                {                                       
                    tables[i++] = row["TABLE_NAME"].ToString();
                }
                return tables;
            }
        }

        public Dictionary<string, object> GetDataModel()
        {
            Dictionary<string, object> model = new Dictionary<string, object>();
            using (OleDbConnection connection = GetConnection())
            {
                //model["catalogs"] = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Catalogs, null);
                Dictionary<string, object> tablesDictionary = new Dictionary<string, object>();
                DataTable tables = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                foreach(JObject table in this.Convert(tables))
                {
                    string tableName = table["tablE_NAME"].Value<string>();
                    string tableSchema = table["tablE_SCHEMA"].Value<string>();

                    Dictionary<string, object> columns = new Dictionary<string, object>();
                    foreach (JObject column in this.Convert( connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[]{ tableName })))
                    {
                        columns[columns.Count + ""] = column;
                    }

                    Dictionary<string, object> metadata = new Dictionary<string, object>();
                    metadata["name"] = tableName;
                    metadata["schema"] = tableSchema;
                    metadata["columns"] = columns;
                    tablesDictionary[tableName] = metadata;
                }
                model["tables"] = tables;

                
                model["views"] = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Views, null);
                model["foreign"] = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys, null);
                model["primary"] = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, null);
            }
            return model;
        }

         
        public JArray Convert(DataTable table)
        {
            Dictionary<string, object> resultSet = new Dictionary<string, object>();
            List<Dictionary<string, object>> listRow = new List<Dictionary<string, object>>();
            foreach (DataRow row in table.Rows)
            {
                Dictionary<string, object> rowSet = new Dictionary<string, object>();
                foreach (DataColumn column in table.Columns)
                {
                    rowSet[column.Caption] = row[column.Caption];
                }
                listRow.Add(rowSet);
            }
            resultSet["rows"] = listRow;
            return (JArray)(JObject.FromObject(resultSet)["rows"]);
        }

      
        public JArray Execute(string sql)
        {
            using ( OleDbConnection connection = GetConnection() )
            {
                DataTable dataTable = new DataTable();
                OleDbDataAdapter adapter = new OleDbDataAdapter( sql, connection );
                adapter.Fill( dataTable );

                Dictionary<string, object> resultSet = new Dictionary<string, object>();
                List<Dictionary<string, object>> listRow = new List<Dictionary<string, object>>();
                foreach ( DataRow row in dataTable.Rows )
                {
                    Dictionary<string, object> rowSet = new Dictionary<string, object>();
                    foreach ( DataColumn column in dataTable.Columns )
                    {
                        rowSet[column.Caption] = row[column.Caption];
                    }
                    listRow.Add( rowSet );
                }
                resultSet["rows"] = listRow;
                /*
                Dictionary<string, object> metadata = new Dictionary<string, object>();
                foreach (DataColumn column in dataTable.Columns)
                {
                    metadata[column.Caption] = JObject.FromObject(column);
                }
                resultSet["metadata"] = metadata;
                
                return metadata;*/


                JObject jrs = JObject.FromObject( resultSet );

                return ( JArray ) jrs["rows"];
            }
        }

       
 
    }
}
