using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace syntez.ServerApp.Data
{
    public class ColumnMetaData
    {
        public string name;
        public string description;

        public string type;
        public bool primary;
        public bool incremental;
        public bool unique;
        public bool nullable;

        public HashSet<Func<object, bool>> validators = new HashSet<Func<object, bool>>();

        public ColumnMetaData() { }
    }
}
