using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public class DatabaseMetadata
{
    public string driver;
    public string database;
    public string serverVersion;
    public string connectionString;

    public Dictionary<string, TableMetaData> Tables = new Dictionary<string, TableMetaData>();
    public Dictionary<string, object> Metadata = new Dictionary<string, object>();

    public DatabaseMetadata() { }
}
