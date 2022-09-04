using JdbcConnectionTool;
using miac.ServerApp;
using Newtonsoft.Json.Linq;
using ServerApp;
using syntez.ServerApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace syntez.ServerApp.Common
{
    public class DatabaseManager 
    {
        public Dictionary<string, object> metadata;
        public Dictionary<string, object> fasade = new Dictionary<string, object>();

        public DatabaseManager() { }
        OdbcDataSource GetDataSource()
        {
            return this.ds = new OdbcDataSource(datasource, username, password);
        }


        public void DumpDatabase()
        {
          
        }

        private DatabaseSnapshot CreateDump()
        {
            DatabaseSnapshot dump = new DatabaseSnapshot();
            IDatabaseMetadata dbm = GetDataSource().GetDatabaseMetadata();
            dump.metadata = dbm;
            foreach (string tm in dbm.Tables.Keys)
            {
                dump.datasets[tm] = GetDataSource().Execute("select * from " + tm);
            }
            return dump;
        }
        /*
        public DataModel( ) : base()
        {
            Database.EnsureCreated();
        }

        public DataModel( DbContextOptions<DataModel> options ) : base( options )
        {
            Database.EnsureCreated();
        }


        protected override void OnConfiguring( DbContextOptionsBuilder optionsBuilder )
        {"jdbc:postgresql://localhost:4200/postgres", "mister_kest", "Kest1423"
            //optionsBuilder.UseSqlServer(@"Server=511-5A;Database=webapp;Trusted_Connection=True;");
            //optionsBuilder.UseInMemoryDatabase();// UseMySql("server=localhost;UserId=root;Password=password;database=usersdb3;");
            //optionsBuilder.UseMySQL( "server=localhost;database=library;user=root;password=root" );
        }*/

        //new JdbcConnector("jdbc:postgresql://localhost:4200/postgres","mister_kest", "Kest1423");

        private OdbcDataSource ds;
        string datasource; string username; string password;
        public DatabaseManager( string datasource,string username, string password ): base(  )
        {
            this.datasource = datasource;
            this.username = username;
            this.password = password;
            this.ds = new OdbcDataSource(datasource,username,password);
            /*foreach (var prop in GetMetaData().Tables )
            {               
                TableManager manager = new TableManager( prop.Key, GetDataSource(), prop.Value );
                fasade[prop.Key] = new TableManagerStatefull( this, manager );
            }*/
        }

        public IDatabaseMetadata GetMetaData( )
        {         
            return GetDataSource().GetDatabaseMetadata();
        }

        public Dictionary<string, object> ValidateDatabaseMetadata( Dictionary<string, object> tables )
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach ( var p in tables )
            {
                //TODO:
            }
            return result;
        }

        public object Execute( string sql )
        {
            return GetDataSource().Execute(sql);
        }


        public object RequestData( string beginDate, string endDate, Int64 granularity, JArray  indicators, JArray locations ) {
            if (indicators == null)
            {
                throw new ArgumentNullException("indicators argument references to null pointer");
            }
            if (beginDate == null)
            {
                throw new ArgumentNullException("beginDate argument references to null pointer");
            }
            if (endDate == null)
            {
                throw new ArgumentNullException("endDate argument references to null pointer");
            }
            if (locations == null)
            {
                throw new ArgumentNullException("locations argument references to null pointer");
            }
            string sindicators = indicators.ToString().Replace( "{", "" ).Replace( "}", "" ).Replace( "[", "" ).Replace( "]", "" );
            string slocations = locations.ToString().Replace( "{", "" ).Replace( "}", "" ).Replace( "[", "" ).Replace( "]", "" );
            string sqlQuery = "select * from datainput where indicator_id in (" + sindicators + ") and subject_id in(" + slocations + ") and begin_date between '" + beginDate + "' and '" + endDate + "' and granularity_id = " + granularity + " order by begin_date,subject_id,indicator_id";
            return GetDataSource().Execute( sqlQuery );
        }
 
    }
}
