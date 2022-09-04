using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Utils;

namespace eckumoc_netcore_userprofile.Controllers
{
    public class SearchService
    {
        private static string url = "http://api.themoviedb.org/3";
        public static string apiKey = "72b56103e43843412a992a8d64bf96e9";

        public async Task<string> keywordSearch(string query, int page)
        {
            return await this.searchAsync("keyword", query, page);
        }

        public async Task<string> collectionSearch(string query, int page)
        {
            return await this.searchAsync("collection", query, page);
        }

        
        public async Task<string> movieSearch(string query, int page)
        {
            return await this.searchAsync("movie", query, page);
        }

        public async Task<string> tvSearch(string query, int page)
        {
            return await this.searchAsync("tv", query, page);
        }

        public async Task<string> companySearch(string query, int page)
        {
            return await this.searchAsync("company", query, page);
        }

        public async Task<string> listSearch(string query, int page)
        {
            return await this.searchAsync("list", query, page);
        }

        protected async Task<string> searchAsync(string category, string query, int page)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams["query"] = query;
            queryParams["page"] = page.ToString();
            queryParams["api_key"] = apiKey;
            return await this.request(queryParams, "search", category);
        }

        private async Task<string> request(Dictionary<string, string> queryParams, string action, string category)
        {
            string query = url + "/" + action + "/" + category + "?";

            foreach (var pair in queryParams)
            {
                query += $"{pair.Key}={pair.Value}&";
            }
            query = query.Substring(0, query.Length - 1);

            Console.WriteLine(query);
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            HttpResponseMessage response = await client.GetAsync(query);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

    }
}
