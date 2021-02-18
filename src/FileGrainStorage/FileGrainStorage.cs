using Orleans;
using System;
using Orleans.Storage;
using Orleans.Runtime;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Newtonsoft.Json;

namespace GrainStorage
{
    public class FileGrainStorage: IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        private readonly string _storageName;
        private readonly FileGrainStorageOptions _options;
        private readonly ClusterOptions _clusterOptions;
        private readonly IGrainFactory _grainFactory;
        private readonly ITypeResolver _typeResolver;
        private JsonSerializerSettings _jsonSettings;

        public FileGrainStorage(
            string storageName,
            FileGrainStorageOptions options,
            IOptions<ClusterOptions> clusterOptions,
            IGrainFactory grainFactory,
            ITypeResolver typeResolver)
        {
            _storageName = storageName;
            _options = options;
            _clusterOptions = clusterOptions.Value;
            _grainFactory = grainFactory;
            _typeResolver = typeResolver;
        }

        public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            throw new NotImplementedException();
        }

        public Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            throw new NotImplementedException();
        }

        public Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            throw new NotImplementedException();
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            throw new NotImplementedException();
        }
    }
}
