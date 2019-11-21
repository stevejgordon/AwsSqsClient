using System;
using System.Buffers;

namespace HighPerfCloud.Aws.Sqs.Core.Bedrock.Protocols
{
    public interface IProtocolReader<TMessage>
    {
        bool TryParseMessage(in ReadOnlySequence<byte> input, out SequencePosition consumed, out SequencePosition examined, out TMessage message);
    }
}
