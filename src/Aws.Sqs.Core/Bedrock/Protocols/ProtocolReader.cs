using Microsoft.AspNetCore.Connections;
using System.Threading;
using System.Threading.Tasks;

namespace HighPerfCloud.Aws.Sqs.Core.Bedrock.Protocols
{
    public class ProtocolReader<TReadMessage>
    {
        private readonly IProtocolReader<TReadMessage> _reader;
        private readonly int? _maximumMessageSize;

        public ProtocolReader(ConnectionContext connection, IProtocolReader<TReadMessage> reader, int? maximumMessageSize)
        {
            Connection = connection;
            _reader = reader;
            _maximumMessageSize = maximumMessageSize;
        }

        public ConnectionContext Connection { get; }

        public async ValueTask<TReadMessage> ReadAsync(CancellationToken cancellationToken = default)
        {
            var input = Connection.Transport.Input;
            var reader = _reader;

            TReadMessage protocolMessage = default;

            while (true)
            {
                var result = await input.ReadAsync(cancellationToken);
                var buffer = result.Buffer;
                var consumed = buffer.Start;
                var examined = buffer.End;

                try
                {
                    if (result.IsCanceled)
                    {
                        break;
                    }

                    if (!buffer.IsEmpty)
                    {
                        // No message limit, just parse and dispatch
                        if (_maximumMessageSize == null)
                        {
                            if (reader.TryParseMessage(buffer, out consumed, out examined, out protocolMessage))
                            {
                                return protocolMessage;
                            }
                        }
                        else
                        {
                            // We give the parser a sliding window of the default message size
                            var maxMessageSize = _maximumMessageSize.Value;

                            if (!buffer.IsEmpty)
                            {
                                var segment = buffer;
                                var overLength = false;

                                if (segment.Length > maxMessageSize)
                                {
                                    segment = segment.Slice(segment.Start, maxMessageSize);
                                    overLength = true;
                                }

                                if (reader.TryParseMessage(segment, out consumed, out examined, out protocolMessage))
                                {
                                    return protocolMessage;
                                }
                                else if (overLength)
                                {
                                    throw new InvalidDataException($"The maximum message size of {maxMessageSize}B was exceeded. The message size can be configured in AddHubOptions.");
                                }
                                else
                                {
                                    // No need to update the buffer since we didn't parse anything
                                    continue;
                                }
                            }
                        }
                    }

                    if (result.IsCompleted)
                    {
                        if (!buffer.IsEmpty)
                        {
                            throw new InvalidDataException("Connection terminated while reading a message.");
                        }
                        break;
                    }
                }
                finally
                {
                    // The buffer was sliced up to where it was consumed, so we can just advance to the start.
                    // We mark examined as buffer.End so that if we didn't receive a full frame, we'll wait for more data
                    // before yielding the read again.
                    input.AdvanceTo(consumed, examined);
                }
            }

            return protocolMessage;
        }
    }
}
