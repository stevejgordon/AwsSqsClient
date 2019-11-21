using Microsoft.AspNetCore.Connections;
using System.Threading;
using System.Threading.Tasks;

namespace HighPerfCloud.Aws.Sqs.Core.Bedrock.Protocols
{
    public class ProtocolWriter<TWriteMessage>
    {
        private readonly IProtocolWriter<TWriteMessage> _writer;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        public ProtocolWriter(ConnectionContext connection, IProtocolWriter<TWriteMessage> writer)
        {
            Connection = connection;
            _writer = writer;
        }

        public ConnectionContext Connection { get; }

        public async ValueTask WriteAsync(TWriteMessage protocolMessage, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                _writer.WriteMessage(protocolMessage, Connection.Transport.Output);
                await Connection.Transport.Output.FlushAsync(cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
