using HighPerfCloud.Aws.Sqs.Core.Bedrock;
using System;

namespace UsageSamples
{
    public static class DnsCachingConnectionExtensions
    {
        public static ClientBuilder UseDnsCaching(this ClientBuilder clientBuilder, TimeSpan timeout)
        {
            return clientBuilder.Use(previous => new DnsCachingConnectionFactory(timeout)
            {
                ConnectionFactory = previous
            });
        }
    }
}
