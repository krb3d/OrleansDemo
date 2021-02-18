using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Threading.Tasks;
using OrleansBasics.GrainStorage.FileStorage;

namespace OrleansBasics
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                ISiloHost host = await StartSilo();
                Console.WriteLine("\n\n Press Enter to terminate...\n\n");
                Console.ReadLine();

                await host.StopAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static async Task<ISiloHost> StartSilo()
        {
            var builder = new SiloHostBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "OrleansBasics";
                })
                .AddFileGrainStorage(
                    "File",
                    opts =>
                    {
                        opts.RootDirectory = "C:/TestFiles";
                    }
                )
                .Configure<EndpointOptions>(options =>
                    options.AdvertisedIPAddress = System.Net.IPAddress.Loopback)
                .ConfigureApplicationParts(parts => 
                    parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
                .ConfigureLogging(logging =>
                    logging.AddConsole());

            ISiloHost host = builder.Build();
            await host.StartAsync();

            return host;
        }
    }
}