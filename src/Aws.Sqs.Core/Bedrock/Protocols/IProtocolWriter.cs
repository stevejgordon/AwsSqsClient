using System.Buffers;

namespace HighPerfCloud.Aws.Sqs.Core.Bedrock.Protocols
{
    public interface IProtocolWriter<TMessage>
    {
        void WriteMessage(TMessage message, IBufferWriter<byte> output);
    }
}
