using System;
using System.Buffers;
using System.Net.Http;
using System.Runtime.InteropServices;
using HighPerfCloud.Aws.Sqs.Core.Bedrock.Infrastructure;
using HighPerfCloud.Aws.Sqs.Core.Primitives;

namespace HighPerfCloud.Aws.Sqs.Core.Bedrock.Protocols
{
    public class Http1RequestMessageWriter : IProtocolWriter<HttpRequestMessage>
    {
        private ReadOnlySpan<byte> Get => new [] { (byte)'G', (byte)'E', (byte)'T' };
        private ReadOnlySpan<byte> Http11 => new [] { (byte)'H', (byte)'T', (byte)'T', (byte)'P', (byte)'/', (byte)'1', (byte)'.', (byte)'1' };
        private ReadOnlySpan<byte> NewLine => new [] { (byte)'\r', (byte)'\n' };
        private ReadOnlySpan<byte> Space => new [] { (byte)' ' };

        public void WriteMessage(HttpRequestMessage message, IBufferWriter<byte> output)
        {
            var writer = new BufferWriter<IBufferWriter<byte>>(output);
            writer.WriteAsciiNoValidation(message.Method.Method);
            writer.Write(Space);
            writer.WriteAsciiNoValidation(message.RequestUri.PathAndQuery);
            writer.Write(Space);
            writer.Write(Http11);
            writer.Write(NewLine);

            var colon = (byte)':';

            foreach (var header in message.Headers)
            {
                foreach (var value in header.Value)
                {
                    writer.WriteAsciiNoValidation(header.Key);
                    writer.Write(MemoryMarshal.CreateReadOnlySpan(ref colon, 1));
                    writer.Write(Space);
                    writer.WriteAsciiNoValidation(value);
                    writer.Write(NewLine);
                }
            }

            writer.Write(NewLine);
            writer.Commit();
        }

        // todo - temp location for this, will be in a higher level class later
        public void WriteReceieveSqsRequest(QueueName queueName, in AwsRegion region, in AccountId accountId, IBufferWriter<byte> output)
        {
            var writer = new BufferWriter<IBufferWriter<byte>>(output);

            var urlBuilder = new QueueUrlBuilder();
                       
            writer.Write(Get);
            writer.Write(Space);
        }
    }
}
