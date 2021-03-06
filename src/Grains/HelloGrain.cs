﻿using Microsoft.Extensions.Logging;
using Orleans.Providers;
using System.Threading.Tasks;

namespace OrleansBasics
{
    [StorageProvider(ProviderName = "File")]
    public class HelloGrain : Orleans.Grain, IHello
    {
        private readonly ILogger _logger;

        public HelloGrain(ILogger<HelloGrain> logger)
        {
            _logger = logger;
        }

        Task<string> IHello.SayHello(string greeting)
        {
            _logger.LogInformation(
                "\n SayHello message received: greeting = '{greeting}'",
                greeting);

            return Task.FromResult($"\n Client said: '{greeting}', so HelloGrain says: Hello!");
        }
    }
}