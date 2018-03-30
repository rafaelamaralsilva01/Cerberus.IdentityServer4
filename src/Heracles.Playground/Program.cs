using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Heracles.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(2000);
            Ask().Wait();
        }

        static async Task Ask()
        {
            var disco = await DiscoveryClient.GetAsync("http://localhost:5000");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            var tokenResponseClient = await AskToken(disco, "client", "secret");

            Console.WriteLine(tokenResponseClient.Json);
            Console.ReadLine();

            using (var client = new HttpClient())
            {
                client.SetBearerToken(tokenResponseClient.AccessToken);

                var response = await client.GetAsync("http://localhost:5001/identity");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.StatusCode);
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(JArray.Parse(content));
                }
            }

            Console.ReadLine();

            var tokenClient = new TokenClient(disco.TokenEndpoint, "ro.client", "secret");
            var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("alice", "password", "api1");

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            Console.ReadLine();
        }

        static async Task<TokenResponse> AskToken(DiscoveryResponse disco, string clientId, string clientSecret)
        {
            var tokenClient = new TokenClient(disco.TokenEndpoint, clientId, clientSecret);
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1");

            if (tokenResponse.IsError)
                Console.WriteLine(tokenResponse.Error);

            return tokenResponse;
        }
    }
}
