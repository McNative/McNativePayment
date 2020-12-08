using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace McNativePayment
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseIISIntegration()
                .UseUrls("https://0.0.0.0:443")
                .UseSentry("https://e07a49149f144ef9a8877f12338664ef@o428820.ingest.sentry.io/5549143")
                .UseStartup<Startup>();
    }
}
