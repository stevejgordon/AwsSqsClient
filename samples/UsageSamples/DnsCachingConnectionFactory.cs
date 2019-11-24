using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;
using System.Threading;

namespace UsageSamples
{
    public class DnsCachingConnectionFactory : IConnectionFactory
    {
        private readonly TimeSpan _timeout;
        private readonly MemoryCache _memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        public DnsCachingConnectionFactory(TimeSpan timeout)
        {
            _timeout = timeout;
        }

        public IConnectionFactory ConnectionFactory { get; set; }

        public async ValueTask<ConnectionContext> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
        {
            if (endpoint is DnsEndPoint dnsEndPoint)
            {
                // TODO: Lock etc

                // See if we have an IPEndPoint cached
                var resolvedEndPoint = _memoryCache.Get<IPEndPoint>(dnsEndPoint.Host);

                if (resolvedEndPoint != null)
                {
                    // If it's cached, try to connect
                    try
                    {
                        return await ConnectionFactory.ConnectAsync(resolvedEndPoint);
                    }
                    catch (Exception)
                    {
                        // TODO: Evict from the cache?
                    }
                }

                ConnectionContext connectionContext = null;

                // Resolve the DNS entry
                var entry = await Dns.GetHostEntryAsync(dnsEndPoint.Host);


                foreach (var address in entry.AddressList)
                {
                    resolvedEndPoint = new IPEndPoint(address, dnsEndPoint.Port);

                    try
                    {
                        connectionContext = await ConnectionFactory.ConnectAsync(resolvedEndPoint);
                        break;
                    }
                    catch (Exception)
                    {

                    }
                }

                if (connectionContext != null)
                {
                    _memoryCache.Set(dnsEndPoint.Host, resolvedEndPoint, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _timeout
                    });

                    return connectionContext;
                }

                throw new InvalidOperationException($"Unable to resolve {dnsEndPoint.Host} on port {dnsEndPoint.Port}");

            }
            else
            {
                return await ConnectionFactory.ConnectAsync(endpoint);
            }
        }
    }
}
