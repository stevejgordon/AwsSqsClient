using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace HighPerfCloud.Aws.Sqs.Core
{
    internal static class StreamExtensions
    {
        internal static ValueTask<IMemoryOwner<byte>> RentAndPopulateAsync(this Stream stream,
            int contentLength)
        {
            return SqsReceiveResponseMemoryPool.RentAndPopulateFromStreamAsync(stream, contentLength);
        }
    }

    public class SqsClient
    {
        public IAsyncEnumerable<LightweightMessage> PollForMessages(CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<LightweightMessage>(); // unbounded for now

            Poll(channel.Writer, cancellationToken); // not awaited, handle exceptions etc

            return channel.Reader.ReadAllAsync();

            static Task Poll(ChannelWriter<LightweightMessage> channelWriter, CancellationToken cancellationToken)
            {
                while(!cancellationToken.IsCancellationRequested)
                {
                    // send read request

                    // parse any messages to LightweightMessage

                    // write to channel
                }

                return Task.CompletedTask;
            }
        }
    }

    public class PollingSqsReader
    {

    }
}