using Orleans;
using System;
using Orleans.Storage;
using Orleans.Runtime;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Newtonsoft.Json;
using System.Threading;
using Orleans.Serialization;
using System.IO;

namespace OrleansBasics.GrainStorage.FileStorage
{
    /// <remarks>
    /// https://dotnet.github.io/orleans/docs/tutorials_and_samples/custom_grain_storage.html
    /// </remarks>
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
            var fName = GetKeyString(grainType, grainReference);
            var path = Path.Combine(_options.RootDirectory, fName);

            var fileInfo = new FileInfo(path);
            if (fileInfo.Exists)
            {
                if (fileInfo.LastWriteTimeUtc.ToString() != grainState.ETag)
                {
                    throw new InconsistentStateException(
                        $"Version conflict (ClearState): ServiceId={_clusterOptions.ServiceId} ProviderName={_storageName} GrainType={grainType} GrainReference={grainReference.ToKeyString()}.");
                }

                grainState.ETag = null;
                grainState.State = Activator.CreateInstance(grainState.State.GetType());
                fileInfo.Delete();
            }

            return Task.CompletedTask;
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var fName = GetKeyString(grainType, grainReference);
            var path = Path.Combine(_options.RootDirectory, fName);

            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                grainState.State = Activator.CreateInstance(grainState.State.GetType());
                return;
            }

            using (var stream = fileInfo.OpenText())
            {
                var storedData = await stream.ReadToEndAsync();
                grainState.State = JsonConvert.DeserializeObject(storedData, _jsonSettings);
            }

            // We use the fileInfo.LastWriteTimeUtc as a ETag which will be used by other functions for inconsistency checks to prevent data loss.
            grainState.ETag = fileInfo.LastWriteTimeUtc.ToString();
        }

        public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            var storedData = JsonConvert.SerializeObject(grainState.State, _jsonSettings);

            var fName = GetKeyString(grainType, grainReference);
            var path = Path.Combine(_options.RootDirectory, fName);

            var fileInfo = new FileInfo(path);

            if (fileInfo.Exists && fileInfo.LastWriteTimeUtc.ToString() != grainState.ETag)
            {
                throw new InconsistentStateException(
                    $"Version conflict (WriteState): ServiceId={_clusterOptions.ServiceId} ProviderName={_storageName} GrainType={grainType} GrainReference={grainReference.ToKeyString()}.");
            }

            using (var stream = new StreamWriter(fileInfo.Open(FileMode.Create, FileAccess.Write)))
            {
                await stream.WriteAsync(storedData);
            }

            fileInfo.Refresh();
            grainState.ETag = fileInfo.LastWriteTimeUtc.ToString();
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            var toDispose = lifecycle.Subscribe(
                                observerName: OptionFormattingUtilities.Name<FileGrainStorage>(_storageName),
                                stage: ServiceLifecycleStage.ApplicationServices,
                                onStart: Init);
        }

        private Task Init(CancellationToken ct)
        {
            // Settings could be made configurable from Options.
            _jsonSettings = OrleansJsonSerializer.UpdateSerializerSettings(
                                settings: OrleansJsonSerializer.GetDefaultSerializerSettings(_typeResolver, _grainFactory),
                                useFullAssemblyNames: false,
                                indentJson: false,
                                typeNameHandling: null);

            var directory = new System.IO.DirectoryInfo(_options.RootDirectory);
            if (!directory.Exists)
                directory.Create();

            return Task.CompletedTask;
        }

        private string GetKeyString(string grainType, GrainReference grainReference)
        {
            return $"{_clusterOptions.ServiceId}.{grainReference.ToKeyString()}.{grainType}";
        }
    }
}
