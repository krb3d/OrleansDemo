using Grains.Tests.Cluster;
using Orleans.TestingHost;
using OrleansBasics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Grains.Tests
{
    [Collection(ClusterCollection.Name)]
    public class HelloGrainTests
    {
        private readonly TestCluster _cluster;

        public HelloGrainTests(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
        }

        [Fact]
        public async Task SaysHelloCorrectly()
        {
            var hello = _cluster.GrainFactory.GetGrain<IHello>(0);
            var greeting = await hello.SayHello("TEST");

            Assert.Equal("\n Client said: 'TEST', so HelloGrain says: Hello!", greeting);
        }
    }
}
