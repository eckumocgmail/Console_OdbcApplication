 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace syntez.ServerApp
{
    public class CommandService   
    {
        public CommandService( ) 
        {
        }

        public static string Execute( string command )
        {
            ProcessStartInfo info = new ProcessStartInfo( "CMD.exe", "/C "+ command );
            
            info.RedirectStandardError = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            System.Diagnostics.Process process = System.Diagnostics.Process.Start( info );
            string response = process.StandardOutput.ReadToEnd();
            return response;
        }
    }
}
