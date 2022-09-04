using miac.ServerApp.Daa;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace eckumoc.Data
{
    class ReadExcel
    {
        public static void Main2(string[] args)
        {
            args = new string[] { @"H:\medorganizations.xlsx" };
            foreach (string arg in args)
            {
                Dictionary<string, object> db = new Dictionary<string, object>();
                ExcelOleDataSource ds = new ExcelOleDataSource(arg);
                foreach (JObject table in ds.GetTablesMetadata())
                {
                    db[table["TABLE_NAME"].Value<string>()] = ds.Execute("select * from [" + table["TABLE_NAME"].Value<string>() + "]");
                }
                Console.WriteLine(JObject.FromObject(db).ToString());
                //System.IO.File.WriteAllText(@"H:\\mo.json", JObject.FromObject(db).ToString());
            }
        }

    }
}
