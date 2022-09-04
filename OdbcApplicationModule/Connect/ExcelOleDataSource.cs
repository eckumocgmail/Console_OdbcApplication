
using miac.ServerApp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace miac.ServerApp.Daa
{
    public class ExcelOleDataSource:OleDataSource
    {
        public ExcelOleDataSource(string filename):base("Excel", filename)
        {
            if( !System.IO.File.Exists(filename))
            {
                throw new Exception("file:"+filename+" not exist");
            }
        }
    }
}
