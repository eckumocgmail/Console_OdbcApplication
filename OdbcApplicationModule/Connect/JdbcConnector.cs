using Utils;
using JdbcConnectionTool;
using miac.ServerApp;
using Newtonsoft.Json; 
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
     
 

/** 
    * 
    * 
        static string JDBC_CONNECTION_STRING = "jdbc:postgresql://localhost:4200/postgres";
        this.jdbc = new JdbcConnector( JDBC_CONNECTION_STRING, "mister_kest", "Kest1423" );
        private JdbcConnector jdbc;
        JArray resultset =
                this.jdbc.executeCommand("select id from message"  


    * Запускает java на выполнения скомпилированного файла JAR, реализующего JDBC_технологии
    ***/
public class JdbcConnector : JavaExe  {


    public class SqlCommand
    {
        public string connection { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public string method { get; set; }
        public string query { get; set; }
    }

    public JdbcConnector(): base(Cmd.Exec("where java.exe")) { }

    public JdbcConnector( string path ) : base(path)
    {
    }

    string connection;
    string username;
    string password;

    public JdbcConnector(string connection, string username, string password): base(System.IO.Directory.GetCurrentDirectory()+@"\AppData\utils\jdbc-odbc.jar"  )
    {
        this.connection = connection;
        this.username = username;
        this.password = password;


        Console.WriteLine(System.IO.Directory.GetCurrentDirectory());
        if (!System.IO.File.Exists(jarFullPath))
        {
            throw new Exception("не найден файл: "+jarFullPath);
        }
    }


    /**
        * Выполнение запроса выора данных SQL,
         
    public JArray executeQuery(string connection, string login, string password, string sql)
    {
        JObject result = this.runtimeExecute(connection+" "+login+" "+ password + " 0 " + sql);
        switch (result["status"].ToString())
        {
            case "200":
                JObject resultSet = (JObject)result["resultset"];
                JArray dataset = (JArray)resultSet["dataset"];
                return dataset;
            default: throw new Exception("неизвестная ошибка");
        }
    }*/

 
    /**
        * Выполняет команду SQL
        */
    JArray executeCommand(SqlCommand command)
    {
        string message = JsonConvert.SerializeObject(command);
            
        JObject result = this.runtimeExecute(message);
        if (result == null)
        {
            throw new Exception("нет ответа");
        }
        switch (result["status"].Value <string>())
        {
            case "200":
                JArray resultSet = (JArray)result["resultset"];                
                return resultSet;
            default: throw new Exception("неизвестная ошибка");
        }
    }



    public Dictionary<string, object> GetMetadata()
    {
        throw new NotImplementedException();
    }

    public JArray ExecuteQuery( string sql )
    {
        return this.executeCommand( new SqlCommand()
        {
            connection = this.connection,
            login = this.username,
            password = this.password,
            query = sql
        } );
    }

    JObject ExecuteSingle( string sql )
    {
        JArray resultSet = this.executeCommand( new SqlCommand()
        {
            connection = this.connection,
            login = this.username,
            password = this.password,
            query = sql
        } );
        return null; 
    }

    JArray  Execute( string sql )
    {
        return this.executeCommand( new SqlCommand()
        {
            connection = this.connection,
            login = this.username,
            password = this.password,
            query = sql
        } );
    }

    int InsertBlob( string sqlCommand, string blobColumn, byte[] data )
    {
        throw new NotImplementedException();
    }

    byte[] ReadBlob( string sqlCommand )
    {
        throw new NotImplementedException();
    }

    Dictionary<string, object> GetTables( )
    {
        throw new NotImplementedException();
    }
} 