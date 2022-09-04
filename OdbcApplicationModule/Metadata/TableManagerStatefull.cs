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
    public class TableManagerStatefull
    {

        private TableManager tableManager;
        public JArray dataRecords;
        private DatabaseManager databaseManager;

        public TableManagerStatefull() { }
        /**
         * выбор обьектов ссылающихся на запись в заданной таблице с заданным идентификатором
         */
        public object SelectReferencesFrom(string table, Int64 record_id)
        {
            string fk = (from p in this.tableManager.metadata.fk where p.Value.ToLower() == table.ToLower() select p.Key).SingleOrDefault<string>();
            return (from r in this.dataRecords where r[fk].Value<Int64>() == record_id select r).ToList();
        }

        /**
         * выбор обьектов ссылающихся на запись в заданной таблице с заданным идентификатором
         */
        public object SelectReferencesTo(string table, Int64 record_id)
        {
            TableManagerStatefull tableRef = (TableManagerStatefull)this.databaseManager.fasade[table];
            return new List<object>();
        }

        /**
         * выбор обьектов ссылающихся на запись в заданной таблице с заданным идентификатором
         */
        public object SelectNotReferencesTo(string table, Int64 record_id)
        {
            string fk = (from p in this.tableManager.metadata.fk where p.Value.ToLower() == table.ToLower() select p.Key).SingleOrDefault<string>();
            return (from r in this.dataRecords where r[fk].Value<Int64>() == record_id select r).ToList();
        }

        public TableManagerStatefull( DatabaseManager databaseManager, TableManager tableManager)
        {
            this.databaseManager = databaseManager;
            this.tableManager = tableManager;
            this.dataRecords = this.tableManager.SelectAll();
            //this.tableManager.SelectMaxId();
        }

        public Int64 Count( )
        {
            return this.dataRecords.Count();
        }

        public TableMetaData GetMetadata( )
        {            
            return this.tableManager.metadata;
        }

        public JArray SelectAll( )
        {
            return this.dataRecords;
        }

        public object WhereColumnValueIn( string column, JArray values )
        {
            List<Int64> valuesList = new List<Int64>();
            foreach (JValue val in values)
            {
                valuesList.Add(val.Value<Int64>());
            }
            return (from rec in this.dataRecords where valuesList.Contains(rec[column].Value<Int64>()) select rec).ToList();
        }

        public JToken Select( Int64 id )
        {

            return (from r in this.dataRecords where r[this.GetMetadata().getPrimaryKey()].Value<Int64>() == id select r).SingleOrDefault<JToken>();
        }

        public JArray Join( Int64 id, string table )
        {
            throw new Exception("unsupported");
            //return this.datasource.Execute( "select * from " + metadata.name + " where " + metadata.pk + " = " + id );
        }

        public JArray SelectPage( Int64 page, Int64 size )
        {
            JArray arr = new JArray();
            
            JObject[] records = this.dataRecords.Values<JObject>().ToArray<JObject>();

            for (Int64 i = ((page)*size); i < ((page+1) * size); i++)
            {
                if (i < records.Length)
                {
                    arr.Add(records[i]);
                }
            }
            return arr;
        }

        public Int64 Create( Dictionary<string, object> values )
        {
            this.dataRecords.Add(JObject.FromObject(values));
            new Task(() => { this.tableManager.Create(values); }).Start();
            return 1;
        }

        

        public Int64 Update( Dictionary<string, object> values )
        {
            if(this.GetMetadata().pk == null)
            {
                throw new Exception("primary key not defined");
            }
            if (!values.ContainsKey(this.GetMetadata().pk))
            {
                throw new Exception("values argument has not primary key identifier");
            }
            Int64 objectId = Int64.Parse(values[this.GetMetadata().pk].ToString());

            JToken record = this.Select(objectId);
            JObject jvalues = JObject.FromObject(values);
            foreach(var p in values )
            {
                record[p.Key] = jvalues[p.Key];
            }
            new Task(() => { this.tableManager.Update(values); }).Start();
            return 1;
        }

       
        public Int64 Delete( Int64 id )
        {
            this.dataRecords.Remove(this.Select(id));
            new Task(() => { this.tableManager.Delete(id); }).Start();
            return 1;                    
        }

    }
}
