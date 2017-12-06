using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NosSharp.CLI.Interfaces;

namespace NosSharp.CLI.Proxies
{
    public class HttpCliProxy : ICliProxy
    {
        private static readonly HttpClient Client = new HttpClient();
        private static string _ip;
        private static short _port;

        private static async Task<bool> Connect()
        {
            try
            {
                HttpRequestMessage message = new HttpRequestMessage
                    {RequestUri = new Uri($"http://{_ip}:{_port}")};
                HttpResponseMessage task = await Client.SendAsync(message);
                return !task.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return true;
            }
        }

        /// <inheritdoc />
        public bool Connect(string ip, short port)
        {
            _ip = ip;
            _port = port;
            Task<bool> test = Connect();
            return test.Result;
        }

        public void RegisterSession(long accountId)
        {
        }

        public void UnregisterSession(long accountId)
        {
        }

        public void SendMessage(string message)
        {
        }

        public void UpdateNosbazaar()
        {
        }

        public void UpdateFamily(long familyId, bool isFactionChange)
        {
        }
    }
}