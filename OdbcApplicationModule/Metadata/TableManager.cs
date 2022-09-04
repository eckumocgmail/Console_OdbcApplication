using miac.ServerApp;
using Newtonsoft.Json.Linq;
using syntez.ServerApp.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace syntez.ServerApp.Common
{
    public class TableManager
    {
        private OdbcDataSource datasource;
        public TableMetaData metadata;

        public TableManager() { }

        public TableManager( string name, OdbcDataSource datasource, TableMetaData metadata ){
            this.datasource = datasource;
            this.metadata = metadata;        
            
        }



        public int SelectMaxId(){
            return this.datasource.ExecuteSingle("select max("+this.metadata.pk+") as MaxId from " + metadata.name)["MaxId"].Value<int>();
        }

        public int Count( )
        {
            JObject record = this.datasource.ExecuteSingle( "select count(*) as RecordsCount from " + metadata.name );
            return record["RecordsCount"].Value<int>();
        }

        public TableMetaData GetMetadata( )
        {            
            return this.metadata;
        }

        public JArray SelectAll( )
        {
            return this.datasource.Execute( "select * from " + metadata.name );
        }

        public JArray Select( Int64 id )
        {
            return this.datasource.Execute( "select * from " + metadata.name + " where " + metadata.pk + " = " + id );
        }

        public JArray Join( Int64 id, string table )
        {
            return this.datasource.Execute( "select * from " + metadata.name + " where " + metadata.pk + " = " + id );
        }

        public JArray SelectPage( Int64 page, Int64 size )
        {
            return this.datasource.Execute( "select * from " + metadata.name + " limit " + page + " , " + size );
        }

        public int Create( Dictionary<string, object> values )
        {
            string keys = "";
            string valuesstr = "";
            if( !this.metadata.ContainsBlob())
            {
                foreach (var p in metadata.columns)
                {
                    if (values.ContainsKey(p.Key))
                    {
                        keys += p.Key + ",";
                        valuesstr += this.toSqlValueStr(p.Key, values[p.Key]) + ",";
                    }
                };
                keys = keys.Substring(0, keys.Length - 1);
                valuesstr = valuesstr.Substring(0, valuesstr.Length - 1);
                string sql = "insert into " + metadata.name + "(" + keys + ") values (" + valuesstr + ")";
                this.datasource.Execute(sql);
                return this.datasource.ExecuteSingle(" SELECT LAST_INSERT_ID( ) as ID;")["ID"].Value<int>();
            }
            else
            {
                byte[] data = null;
                foreach (var p in metadata.columns)
                {
                    if (values.ContainsKey(p.Key))
                    {
                        keys += p.Key + ",";
                        if( this.metadata.columns[p.Key].type.ToLower()!="blob")
                        {
                            valuesstr += this.toSqlValueStr(p.Key, values[p.Key]) + ",";
                        }
                        else
                        {
                            valuesstr += "?,";
                            string binaryString = values[p.Key].ToString();
                            data = new byte[binaryString.Length];
                            for (int i = 0; i < binaryString.Length; i++)
                            {
                                data[i] = (byte)binaryString[i];
                            }
                        }
                        
                    }
                };
                keys = keys.Substring(0, keys.Length - 1);
                valuesstr = valuesstr.Substring(0, valuesstr.Length - 1);
                string sql = "insert into " + metadata.name + "(" + keys + ") values (" + valuesstr + ")";
                this.datasource.InsertBlob(sql, "@bin_data", data);
                return this.datasource.ExecuteSingle(" SELECT LAST_INSERT_ID( ) as ID;")["ID"].Value<int>();

            }
        }

        public int Update( Dictionary<string, object> values )
        {
            string setup = "";
            foreach(var p in values )
            {
                if ( p.Key == metadata.pk ) continue;
                string value = toSqlValueStr( p.Key, p.Value );

                setup += p.Key + "=" + value + ",";
            }
            setup = setup.Substring( 0, setup.Length - 1 );
            this.datasource.Execute( "update "+ metadata.name + " set " + setup + " where "+metadata.pk + " = " + values[metadata.pk]);
            return 1;
        }

        private string toSqlValueStr( string name, object val )
        {
            string value = "";
            string typestr = metadata.columns[name].type.ToLower();
            switch ( typestr )
            {
                case "date":
                    DateTime date = ( DateTime ) val;
                    value = "'" + date.Year + "-" + date.Month + "-" + date.Day + "'";
                    break;
                case "nvarchar":
                case "varchar":

                    value = "'" + val + "'";
                    break;
                default: value = val.ToString(); break;
            }

            return value;
        }

        public int Delete( Int64 id )
        {            
            if( String.IsNullOrEmpty( metadata.pk ) )
            {
                throw new Exception("primary key not defined at table metadata");
            }
            this.datasource.Execute( "delete from " + metadata.name + " where "+metadata.pk + " = " + id );
            return 1;
        }

    }
}
