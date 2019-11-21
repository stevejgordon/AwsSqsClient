using Microsoft.AspNetCore.Connections;

namespace HighPerfCloud.Aws.Sqs.Core.Bedrock.Protocols
{
    public static class Protocol
    {
        public static ProtocolWriter<TWriteMessage> CreateWriter<TWriteMessage>(this ConnectionContext connection, IProtocolWriter<TWriteMessage> writer)
            => new ProtocolWriter<TWriteMessage>(connection, writer);

        public static ProtocolReader<TReadMessage> CreateReader<TReadMessage>(this ConnectionContext connection, IProtocolReader<TReadMessage> reader, int? maximumMessageSize = null)
            => new ProtocolReader<TReadMessage>(connection, reader, maximumMessageSize);
    }
}
