using System;
using Orleans.Storage;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans;

namespace OrleansBasics.GrainStorage.FileStorage
{
    public static class FileSiloBuilderExtensions
    {
        public static ISiloHostBuilder AddFileGrainStorage(
            this ISiloHostBuilder builder,
            string providerName,
            Action<FileGrainStorageOptions> options)
        {
            return builder.ConfigureServices(services => services.AddFileGrainStorage(providerName, options));
        }

        public static IServiceCollection AddFileGrainStorage(this IServiceCollection services, string providerName, Action<FileGrainStorageOptions> options)
        {
            services
                .AddOptions<FileGrainStorageOptions>(providerName)
                .Configure(options);

            /*
             * Our FileGrainStorage implements two interfaces, IGrainStorage and ILifecycleParticipant<ISiloLifecycle>
             * therefore we need to register two named services for each interfaces
             */

            services
                .AddSingletonNamedService(
                    providerName,
                    FileGrainStorageFactory.Create);

            services
                .AddSingletonNamedService(
                    providerName,
                    (serviceProvider, name) => (ILifecycleParticipant<ISiloLifecycle>)serviceProvider.GetRequiredServiceByName<IGrainStorage>(name));

            /*
             var silo = new SiloHostBuilder()
                .UseLocalhostClustering()
                .AddFileGrainStorage("File", opts =>
                {
                    opts.RootDirectory = "C:/TestFiles";
                })
                .Build();
            */

            return services;
        }
    }
}
