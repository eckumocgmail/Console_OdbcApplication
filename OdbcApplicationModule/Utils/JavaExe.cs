using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{

    /** 
     * оболочка передает аргументы в команду выполнения java.exe 
     ***/
    public class JavaExe
    {
        protected string jarFullPath;

        public JavaExe()
        {
            
        }

        public JavaExe( string jarFullPath )
        {
            this.jarFullPath = jarFullPath;
        }


        /**
         * создает процесс выполнения байт кода java-программы в отдельном процессе 
         * и передает в нее через аргументы строку соединения и параметры доступа 
         * и команду SQL,
         * DriverManager устанавливает соединение выполняет SQL-команду и выводит результат
         * в стандартный поток ввода-вывода в формате JSON
         * прерывания:
         * -файл приложения недоступен или поврежден
         * -не устаноылена виртеальная Java-машина          *  
         * -не озможно установить соединение с источнико данных
         * -переполнение стэка памяти
         * -кодировка входных-выходных потоков
         */
        public JObject runtimeExecute(string args)
        {
            Console.WriteLine("java.exe -jar " + jarFullPath + " " + args);
            string jsonResult =
                this.run("java.exe", "-jar " + jarFullPath + " " + args);

            Console.WriteLine(jsonResult);
            JObject result = JsonConvert.DeserializeObject<JObject>(jsonResult);
            return result;
        }

        /**
         * запрос версии JRE необходим для проверки установки JDK и настроек переменных среды
         */
        public string getJreVersion()
        {
            return run("java", "-version");
        }



        /** выполнение команды с заданными рагументами в отдельном процессе с ожиданием завершения 
         * -из выходного потока считываются текстовое сообщение **/
        private string run(string command, string args)
        {
            Console.WriteLine(command + " " + args);
            ProcessStartInfo processInfo = new ProcessStartInfo(command, args)
            {
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            Process proc = Process.Start(processInfo);
            if (proc == null)
            {
                throw new InvalidOperationException(
                    "система не может выполнить байт-кода Java, проверьте коррекность установки JDK" +
                    " и корректност конфигурации параметров среды в ОС." +
                    "установите в переменную Java_Home путь к установленному JDK и " +
                    "и добавьте путь к каталогу bin через переменную Java_Home в " +
                    " перменную path:\n" +
                    "\t path %path%;%java_home%\bin"
                    );
            }
            else
            {
                proc.Start();
                proc.WaitForExit();

                string bufer = "";
                while (!proc.StandardOutput.EndOfStream)
                {
                    bufer+=(proc.StandardOutput.ReadLine()); // пишем в буфер
                }

                proc.Close();
                return bufer.ToString();
            }
        }
    }
}
