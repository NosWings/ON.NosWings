using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace NosSharp.Web
{
    public class Program
    {
        private static void InitConfiguration()
        {
            
        }


        public static void Main(string[] args)
        {
            InitConfiguration();
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args).UseStartup<Startup>().Build();
        }
    }
}
