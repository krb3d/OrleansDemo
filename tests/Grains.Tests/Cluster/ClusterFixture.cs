using Grains.Tests.Hosted.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.TestingHost;
using Orleans.Timers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grains.Tests.Cluster
{
    public class ClusterFixture: IDisposable
    {
        public TestCluster Cluster { get; }

        public List<FakeReminderRegistry> ReminderRegistries { get; } = new List<FakeReminderRegistry>();

        public FakeReminder GetReminder(IGrain grain, string name)
        {
            return ReminderRegistries
                .Select(_ => _.GetReminder((GrainReference)grain, name).Result)
                .Where(_ => _ != null)
                .SingleOrDefault();
        }

        public ClusterFixture()
        {
            var builder = new TestClusterBuilder();

            builder.AddSiloBuilderConfigurator<SiloConfigurator>();

            Cluster = builder.Build();
            Cluster.Deploy();
            var inProcHandles = Cluster.Silos.Cast<InProcessSiloHandle>().ToList();
            ReminderRegistries.AddRange(inProcHandles.Select(x => x.SiloHost.Services.GetService<FakeReminderRegistry>()));
        }

        private class SiloConfigurator: ISiloConfigurator
        {
            public void Configure(ISiloBuilder siloBuilder)
            {
                siloBuilder.ConfigureServices(services =>
                {
                    // add the fake reminder registry in a way that lets us extract it afterwards
                    services.AddSingleton<FakeReminderRegistry>();
                    services.AddSingleton<IReminderRegistry>(_ => _.GetService<FakeReminderRegistry>());
                });
            }
        }

        public void Dispose() => Cluster.StopAllSilos();
    }
}