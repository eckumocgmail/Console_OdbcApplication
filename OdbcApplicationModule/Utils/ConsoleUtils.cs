using syntez.ServerApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerApp.Utils
{
    public class ConsoleUtils: CommandService
    {

        public ConsoleUtils():base(){ 
        }


        public static bool? Confirm(string question)
        {
            string answer="?";
            bool? result = true;
            bool complete = false;
            do
            {
                Console.WriteLine(question);
                Console.WriteLine(("Y/N"));
                answer = Console.ReadLine().ToLower();
                if( answer == "")
                {
                    result = null;
                    complete = true;
                }
                else if (answer == "y")
                {
                    result =  true;
                    complete = true;
                }
                else if (answer == "n")
                {
                    result = false;
                    complete = true;
                }
                else
                {
                    complete = false;
                }
            } while (!complete);
            return result;
        }
    }
}
