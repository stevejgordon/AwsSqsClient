using System.Buffers;
using System.IO.Pipelines;
using System.Text.Http.Parser;
using System.Threading.Tasks;

namespace HighPerfCloud.Aws.Sqs.Core.Client
{
    public readonly struct PipeHttpClient
    {
        static readonly HttpParser _headersParser = new HttpParser();

        readonly IDuplexPipe _pipe;

        public PipeHttpClient(IDuplexPipe pipe)
        {
            _pipe = pipe;
        }

        public async ValueTask<TResponse> SendRequest<TRequest, TResponse>(TRequest request)
            where TRequest : IPipeWritable
            where TResponse : IHttpResponseHandler, new()
        {
            await request.WriteAsync(_pipe.Output).ConfigureAwait(false);

            PipeReader reader = _pipe.Input;
            TResponse response = await ParseResponseAsync<TResponse>(reader).ConfigureAwait(false);
            await response.OnBody(reader);
            return response;
        }

        static async ValueTask<T> ParseResponseAsync<T>(PipeReader reader)
            where T : IHttpResponseHandler, new()
        {
            var handler = new T();

            while (true)
            {
                ReadResult result = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;
                
                if (_headersParser.ParseResponseLine(ref handler, ref buffer, out int rlConsumed))
                {
                    reader.AdvanceTo(buffer.GetPosition(rlConsumed));
                    break;
                }
                reader.AdvanceTo(buffer.Start, buffer.End);
            }

            while (true)
            {
                ReadResult result = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;

                if (_headersParser.ParseHeaders(ref handler, buffer, out int hdConsumed))
                {
                    reader.AdvanceTo(buffer.GetPosition(hdConsumed));
                    break;
                }
                reader.AdvanceTo(buffer.Start, buffer.End);
            }

            await handler.OnBody(reader);
            return handler;
        }

        public bool IsConnected => _pipe is object;
    }
}