using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HighPerfCloud.Aws.Sqs.Core.Bedrock.Transports.Sockets;
using Microsoft.AspNetCore.Connections;


namespace HighPerfCloud.Aws.Sqs.Core.Bedrock.Transports
{
    public static partial class ServerBuilderExtensions
    {
        public static ClientBuilder UseSockets(this ClientBuilder clientBuilder)
        {
            return clientBuilder.UseConnectionFactory(new SocketConnectionFactory());
        }
    }

    public class SocketConnectionFactory : IConnectionFactory
    {
        public ValueTask<ConnectionContext> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
        {
            return new SocketConnection(endpoint).StartAsync();
        }
    }
}
