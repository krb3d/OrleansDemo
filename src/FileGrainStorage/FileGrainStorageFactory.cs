using System;
using Orleans.Storage;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration.Overrides;

namespace GrainStorage
{
    public static class FileGrainStorageFactory
    {
        internal static IGrainStorage Create(IServiceProvider services, string name)
        {
            var optionsSnapshot = services.GetRequiredService<IOptionsSnapshot<FileGrainStorageOptions>>();
            return ActivatorUtilities.CreateInstance<FileGrainStorage>(
                    provider: services,
                    name,
                    optionsSnapshot.Get(name),
                    services.GetProviderClusterOptions(name));
        }
    }
}
