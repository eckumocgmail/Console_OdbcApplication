using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class HttpClient
    {
        private static List<string> HTTP_METHODS = new List<string>() { "get", "post", "put", "head", "options", "delete" };

        public HttpClient() { }

        public static string Rus( string eng )
        {
            return Request( "get", @"https://translate.google.com/#view=home&op=translate&sl=en&tl=ru&text=" + eng );
        }

        public static string Eng( string rus )
        {
            return Request( "get", @"https://translate.google.com/#view=home&op=translate&sl=ru&tl=en&text=" + rus );
        }

        private static async Task<string> RequestAsync( string method, string url )
        {          
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            HttpResponseMessage response = await client.GetAsync( url );
        
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;             
        }

        public static string Request( string method, string url )
        {
            if ( url == null ) throw new Exception( "не указан обязательный параметр url (параметры чувствительны к регистру)" );
            if ( method == null ) throw new Exception( "не указан обязательный параметр method (параметры чувствительны к регистру)" );
            if ( !HTTP_METHODS.Contains( method ) ) throw new Exception( "не верно задан параметр method (параметры чувствительны к регистру) укажите один из: get, post, put, head, options or delete" );
            Task<string> response = RequestAsync( method, url );
            response.Wait();
            string res = response.Result;
            Debug.WriteLine(res);
            return res;
        }

        private static string Request( string json )
        {
            try
            {
                JObject options = JsonConvert.DeserializeObject<JObject>(json);
                if ( !options.ContainsKey( "url" ) ) throw new Exception( "не указан обязательный параметр url (параметры чувствительны к регистру)" );
                if ( !options.ContainsKey( "method" ) ) throw new Exception( "не указан обязательный параметр method (параметры чувствительны к регистру)" );
                string url = options["url"].Value<string>();
                string method = options["method"].Value<string>();
                if ( !HTTP_METHODS.Contains( method ) ) throw new Exception( "не верно задан параметр method (параметры чувствительны к регистру) укажите один из: get, post, put, head, options or delete" );
                Task<string> response = RequestAsync( method, url );
                response.Wait();
                return response.Result;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            
        }
    }
}
