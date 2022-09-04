using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Api.Utils;

public class OdbcApplicationProgram
{
    public static void Start(ref string[] args)
    {
        Clear();
        switch (ProgramDialog.SingleSelect("Выберите действие", new string[]{
            "Новый источник",
            "Выбор источника данных",
            "Выход" }, ref args))
        {
            case "Новый источник":
                CreateDataSource(ref args);
                break;
            case "Тестирование приложения":
                TestOdbcApplication(ref args);
                break;
            case "Выбор источника данных":
                SelectDataSource(ref args);        
                break;
            case "Выход":
                Exit();
                break;
            default: break;
        }
    }

    private static void SelectDataSource(ref string[] args)
    {
         
    }

    private static void TestOdbcApplication(ref string[] args)
    {
        //Console.WriteLine(new OdbcDataSource(@"Driver={SQL Server};Server=DESKTOP-66CFM7U\SQLEXPRESS;Database=master;Trusted_Connection=True;MultipleActiveResultSets=true").GetDatabaseMetadata());
        //Console.WriteLine( args[1] );
        args = new string[] { "sqlserver", "dsn=SpbPublicLibs" };
        if (args.Length == 0)
        {
            Console.WriteLine("write to output database schema");
            Console.WriteLine("usage:");
            Console.WriteLine("  odbc [database-vendor] [odbc-connection-string]");
            Console.WriteLine("example:");
            Console.WriteLine("  odbc mysql dsn=mysql_app;username=root;password=sgdf;");
            Console.WriteLine("  odbc sqlserver Driver={SQL Server};Server=AGENT\\KILLER;TrustServerCertificate=False;;Database=SpbPublicLibs;Trusted_Connection=True;MultipleActiveResultSets=true");

        }
        else
        {
            string result = "{ \"message\": \"wrong parameters\" }";
            OdbcDataSource odbc = null;
            switch (args[0].ToLower())
            {
                case "sqlserver":
                    odbc = new SqlServerOdbcDataSource(args[1]);
                    break;
                case "mysql":
                    odbc = new MySqlOdbcDataSource(args[1]);
                    break;
                default: break;

            }


            Console.WriteLine(odbc.connectionString);
            var metadata = odbc.GetDatabaseMetadata();
            Console.WriteLine(JObject.FromObject(metadata));

            //full schema
            Console.WriteLine(JObject.FromObject(odbc.GetDatabaseMetadata()));
        }
    }

    private static void CreateDataSource(ref string[] args)
    {
        throw new NotImplementedException();
    }
}
