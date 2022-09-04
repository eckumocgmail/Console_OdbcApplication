
using Newtonsoft.Json.Linq;
using ServerApp;
using syntez.ServerApp.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace miac.ServerApp
{
    /**
     * "DRIVER={SQL SERVER};SERVER=(LocalDB)\\v11.0;AttachDbFileName=G:\projects\eckumoc\AppData\persistance.mdf;"
            string sql = "Select * from messages";
            System.Data.Odbc.OdbcConnection connection = new System.Data.Odbc.OdbcConnection("Driver={Microsoft Access Driver (*.mdb, *.accdb)};DBQ=a:\\master.mdb");
            System.Data.Odbc.OdbcCommand command = new System.Data.Odbc.OdbcCommand(sql, connection);
            connection.Open();
            Console.WriteLine("Cfonnected");
            connection.Close(); 

       public static string DEFAULT_PROVIDER = "Microsoft.ACE.OLEDB.12.0";

        //System.Data.Odbc   @"Driver={MySQL ODBC 5.3 ANSI Driver};DATA SOURCE=mysql_app;Uid=root;Pwd=root;
        //System.Data.Odbc   @"Driver={Microsoft Access Driver (*.mdb)};Dbq=C:\mydatabase.mdb;Uid=Admin;Pwd=;
        //System.Data.OleDb //"Provider=Microsoft.Jet.OLEDB.12.0;Data Source=a:\\master.mdb;";
     */
    public class OdbcDataSource
    {

        public DatabaseMetadata metadata;
 
        public string connectionString = null;


        public OdbcDataSource() { }
        public OdbcDataSource(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public OdbcDataSource(string dns, string login, string password)
        {
            this.connectionString = "dsn=" + dns + ";UID=" + login + ";PWD=" + password + ";";

        }

        public System.Data.Odbc.OdbcConnection GetConnection()
        {
            //Console.WriteLine(this.connectionString);
            return new System.Data.Odbc.OdbcConnection(this.connectionString);
        }

        public byte[]  ReadBlob( string sqlCommand )
        {
            using ( System.Data.Odbc.OdbcConnection connection = GetConnection() )
            {
                connection.ChangeDatabase("FRMO");
                connection.Open();
                OdbcCommand command = new OdbcCommand( sqlCommand, connection );
                OdbcDataReader reader = command.ExecuteReader();
                if ( reader.Read() )
                {
                    // matching record found, read first column as string instance
                    byte[] value = ( byte[] ) reader.GetValue( 0 );
                    reader.Close();
                    command.ExecuteNonQuery();
                    return value;
                }
                return null;
            }
        }
 

        public int InsertBlob( string sqlCommand, string blobColumn, byte[] data )
        {
            using ( System.Data.Odbc.OdbcConnection connection = GetConnection() )
            {
                connection.Open();
                OdbcCommand command = new OdbcCommand( sqlCommand, connection );
                command.Parameters.Add( blobColumn, OdbcType.Binary );
                command.Parameters[blobColumn].Value = data;


                return command.ExecuteNonQuery();
            }
        }

  
         



        public JObject GetSchema()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            using (System.Data.Odbc.OdbcConnection connection = GetConnection())
            {
                connection.Open();

                DataTable catalogs = connection.GetSchema();
                JArray jcatalogs = this.convert(catalogs);
                foreach (JObject catalogInfo in jcatalogs)
                {
                    string collectionName = catalogInfo["CollectionName"].Value<string>();
                    if(collectionName == "Indexes")
                    {
                        Dictionary<string, object> indexes = new Dictionary<string, object>();
                        foreach ( string table in GetTables())
                        {

                            JArray catalog = this.convert(connection.GetSchema(collectionName,new string[]{ null,null,table }));
                            indexes[table] = catalog;
                        }
                        result[collectionName] = indexes;
                    }
                    else
                    {                        
                        JArray catalog = this.convert(connection.GetSchema(collectionName));
                        result[collectionName] = catalog;                         
                    }                                              
                }
                result["catalogs"] = jcatalogs;


            }
            return JObject.FromObject(result);
        }

        

       
    

        public JArray convert(DataTable dataTable)
        {
            Dictionary<string, object> resultSet = new Dictionary<string, object>();
            List<Dictionary<string, object>> listRow = new List<Dictionary<string, object>>();
            foreach (DataRow row in dataTable.Rows)
            {
                Dictionary<string, object> rowSet = new Dictionary<string, object>();
                foreach (DataColumn column in dataTable.Columns)
                {
                    rowSet[column.Caption] = row[column.Caption];
                    //Console.Write(row[column.Caption]+"\t");
                }
                //Console.WriteLine();
                listRow.Add(rowSet);
            }
            resultSet["rows"] = listRow;

            JObject jrs = JObject.FromObject(resultSet);
            return (JArray)jrs["rows"];

        }

        public List<string> GetTables()
        {
            List<string> tableNames = new List<string>();
            using (System.Data.Odbc.OdbcConnection connection = GetConnection())
            {
                connection.Open();
                DataTable tables = connection.GetSchema("Tables");

                foreach(JObject next in this.convert(tables))
                {
                    tableNames.Add(next["TABLE_NAME"].Value<string>());
                }
            }
            return tableNames;
        }


        public virtual DatabaseMetadata GetDatabaseMetadata( )
        {
            if( metadata != null )
            {
                return metadata;
            }


            
          
            metadata = new DatabaseMetadata();      
            using ( System.Data.Odbc.OdbcConnection connection = GetConnection() )
            {
                connection.Open();

                metadata.driver = connection.Driver;
                metadata.database = connection.Database;
                object site = connection.Site;
            
                metadata.serverVersion = connection.ServerVersion;
                metadata.connectionString = connection.ConnectionString;
 

                DataTable columns = connection.GetSchema( "Columns" );              
                foreach ( DataRow row in columns.Rows )
                {                    
                    string table = row["TABLE_NAME"].ToString();                   
                    string column = row["COLUMN_NAME"].ToString();
                    string type = row["TYPE_NAME"].ToString();
                    string catalog = row["TABLE_CAT"].ToString();
                    string schema = row["TABLE_SCHEM"].ToString();
                    string description = row["COLUMN_DEF"].ToString();
                    string nullable = row["NULLABLE"].ToString();

                    //исколючаем системные таблицы и служебные
                    if (schema == "sys" || schema == "INFORMATION_SCHEMA" || table.ToLower().IndexOf("migration")!=-1 )
                    {
                        continue;
                    }
                  

                    
                    if ( !metadata.Tables.ContainsKey( table ) )
                    {
                        metadata.Tables[table] = new TableMetaData();
                        metadata.Tables[table].name = table;
                        metadata.Tables[table].description = "";

                        //определение наименования в множественном числе и единственном                        
                        string tableName = table.ToLower();
                        if ( tableName.EndsWith( "s" ) )
                        {
                            if( tableName.EndsWith( "ies" ) )
                            {
                                metadata.Tables[table].multicount_name = tableName.ToLower();
                                metadata.Tables[table].singlecount_name = tableName.Substring( 0, tableName.Length - 3 ).ToLower()+"y";
                            }
                            else
                            {
                                metadata.Tables[table].multicount_name = tableName.ToLower();
                                metadata.Tables[table].singlecount_name = tableName.Substring( 0, tableName.Length - 1 ).ToLower();
                            }
                        }
                        else
                        {
                            if( tableName.EndsWith("y") )
                            {
                                metadata.Tables[table].multicount_name = tableName.Substring(0,tableName.Length-1) + "ies";
                                metadata.Tables[table].singlecount_name = tableName.ToLower();

                            }
                            else
                            {
                                metadata.Tables[table].multicount_name = tableName.ToLower() + "s";
                                metadata.Tables[table].singlecount_name = tableName.ToLower();
                            }
                        }
                    }
                    metadata.Tables[table].columns[column] = new ColumnMetaData();
                    metadata.Tables[table].columns[column].name = column;
                    metadata.Tables[table].columns[column].type = type;
                    metadata.Tables[table].columns[column].nullable = (nullable == "1") ? true : false;
                    metadata.Tables[table].columns[column].description = description;                    
                }


                //определение внешних ключей по правилам наименования
                List<TableMetaData> tables = ( from table in metadata.Tables.Values select table ).ToList<TableMetaData>();
                foreach ( var ptable in metadata.Tables )
                {

                    HashSet<string> associations = new HashSet<string>() { ptable.Value.multicount_name, ptable.Value.singlecount_name };
                    foreach ( var pcolumn in ptable.Value.columns )
                    {
                        //дополнительный анализ наименований колоной
                        string[] ids = pcolumn.Key.ToLower().Split( "_" );
                        HashSet<string> idsSet = new HashSet<string>( ids );
                        List<string> lids = ( from id in idsSet select id.ToLower() ).ToList<string>();
                        if ( idsSet.Contains("id") )
                        {
                            int count = ( from s in idsSet where associations.Contains( s ) select s ).Count();
                            if( count == 0 )
                            {
                                /*TableMetaData foreignKeyTable = ( from table in tables where lids.Contains( table.singlecount_name ) || lids.Contains( table.multicount_name ) select table ).SingleOrDefault<TableMetaData>();
                                if( foreignKeyTable == null )
                                {
                                    //throw new Exception("внешний ключ не найден для поля "+ ptable.Key+"."+pcolumn.Key );
                                }
                                else
                                {
                                    //ptable.Value.fk[pcolumn.Key] = foreignKeyTable;
                                }*/
                            }
                            else
                            {
                                pcolumn.Value.primary = true;
                                ptable.Value.pk = metadata.Tables[ptable.Key].pk = pcolumn.Key;

                            }                            
                        }
                    }
                }


                return metadata;
            }      
        }

        public JObject ExecuteSingle( string sql )
        {
            using ( System.Data.Odbc.OdbcConnection connection = GetConnection() )
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                OdbcDataAdapter adapter = new OdbcDataAdapter( sql, connection );
                adapter.Fill( dataTable );

                /*foreach (DataColumn column in dataTable.Columns)
                {
                    if (column.Table != null)
                    {
                        Console.WriteLine(column.ColumnName + " => " + column.Table.TableName);
                    }
                }*/
                JArray rs = this.convert( dataTable );
                foreach ( JObject next in rs )
                {
                    return next;
                }
                return null;
            }
        }

        public JArray Execute( string sql )
        {
            using ( System.Data.Odbc.OdbcConnection connection = GetConnection() )
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                OdbcDataAdapter adapter = new OdbcDataAdapter( sql, connection );
                adapter.Fill( dataTable );

                /*foreach (DataColumn column in dataTable.Columns)
                {
                    if (column.Table != null)
                    {
                        Console.WriteLine(column.ColumnName + " => " + column.Table.TableName);
                    }
                }*/
                return this.convert( dataTable );
            }
        }
    }
}
