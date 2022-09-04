using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace syntez.ServerApp.Data
{
    public class DatabaseSnapshot
    {
        public IDatabaseMetadata metadata;
        public Dictionary<string, object> datasets=new Dictionary<string, object>();

        public DatabaseSnapshot() { }
    }
}
