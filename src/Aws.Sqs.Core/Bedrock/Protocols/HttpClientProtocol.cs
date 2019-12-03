using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HighPerfCloud.Aws.Sqs.Core.Bedrock.Infrastructure;
using HighPerfCloud.Aws.Sqs.Core.Primitives;
using Microsoft.AspNetCore.Connections;

namespace HighPerfCloud.Aws.Sqs.Core.Bedrock.Protocols
{
    public class HttpClientProtocol
    {
        private readonly ConnectionContext _connection;
        private State _state;

        private ReadOnlySpan<byte> Http11 => new byte[] { (byte)'H', (byte)'T', (byte)'T', (byte)'P', (byte)'/', (byte)'1', (byte)'.', (byte)'1' };
        private ReadOnlySpan<byte> NewLine => new byte[] { (byte)'\r', (byte)'\n' };
        private ReadOnlySpan<byte> Space => new byte[] { (byte)' ' };
        private ReadOnlySpan<byte> TrimChars => new byte[] { (byte)' ', (byte)'\t' };

        private HttpClientProtocol(ConnectionContext connection)
        {
            _connection = connection;
        }

        public async ValueTask<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, HttpCompletionOption completionOption = HttpCompletionOption.ResponseHeadersRead)
        {
            WriteHttpRequestMessage(requestMessage);

            if (requestMessage.Content != null)
            {
                await requestMessage.Content.CopyToAsync(_connection.Transport.Output.AsStream()).ConfigureAwait(false);
            }

            await _connection.Transport.Output.FlushAsync();

            var response = new HttpResponseMessage();

            while (true)
            {
                var result = await _connection.Transport.Input.ReadAsync().ConfigureAwait(false);
                var buffer = result.Buffer;

                ParseHttpResponse(ref buffer, response, out var examined);

                _connection.Transport.Input.AdvanceTo(buffer.Start, examined);

                if (_state == State.Body)
                {
                    var r = new ReceiveMessageResponseReader();

                    var count = r.CountMessages(buffer.First.Span);

                    break;
                }

                if (result.IsCompleted)
                {
                    if (_state != State.Body)
                    {
                        // Incomplete request, close the connection with an error
                    }
                    break;
                }
            }

            return response;
        }

        public async Task SendTestRequestSqs(QueueName queueName, AwsRegion region, AccountId accountId) // temporarily located - can't use in params
        {
            WriteRequest(queueName, region, accountId);

            // cheating for now

            using var response = new HttpResponseMessage();

            while (true)
            {
                var result = await _connection.Transport.Input.ReadAsync().ConfigureAwait(false);
                var buffer = result.Buffer;

                ParseHttpResponse(ref buffer, response, out var examined);

                _connection.Transport.Input.AdvanceTo(buffer.Start, examined);

                if (_state == State.Body)
                {
                    var r = new ReceiveMessageResponseReader();

                    var count = r.CountMessages(buffer.First.Span);

                    break;
                }

                if (result.IsCompleted)
                {
                    if (_state != State.Body)
                    {
                        // Incomplete request, close the connection with an error
                    }
                    break;
                }
            }

            // todo - return
        }

        private void WriteRequest(QueueName queueName, in AwsRegion region, in AccountId accountId) // temporarily locate
        {
            // this is all really ugly and temporary to prove sending a request can work
            // we should work at the byte level and avoid characters where possible

            Span<char> buffer = stackalloc char[128]; // long enough for our testing needs

            var outputSpan = buffer;

            var urlBuilder = new QueueUrlBuilder();

            var position = 0;

            if (urlBuilder.TryBuild(outputSpan, queueName, region, accountId, out var bytesWritten, skipLengthCheck: true))
            {
                position += bytesWritten;
            }

            var qs = "?Action=ReceiveMessage&Version=2012-11-05";

            qs.AsSpan().CopyTo(outputSpan.Slice(position));

            position += qs.Length;

            var urlBytes = ArrayPool<byte>.Shared.Rent(2048);

            var b = Encoding.ASCII.GetBytes(outputSpan.Slice(0, position), urlBytes);

            var writer = new BufferWriter<PipeWriter>(_connection.Transport.Output);
            writer.WriteAsciiNoValidation("GET");
            writer.Write(Space);
            writer.Write(urlBytes.AsSpan().Slice(0, b));
            writer.Write(Space);
            writer.Write(Http11);
            writer.Write(NewLine);

            // calculate canonical URL for signing

            var canonicalUrlBuffer = ArrayPool<char>.Shared.Rent(2048);

            var canonicalSpan = canonicalUrlBuffer.AsSpan();

            var canonicalPosition = 0;

            // 1. Start with the HTTP request method (GET, PUT, POST, etc.), followed by a newline character.

            "GET".AsSpan().CopyTo(canonicalSpan);
            canonicalPosition += 3;

            canonicalSpan[canonicalPosition++] = '\n';

            // 2. Add the canonical URI parameter, followed by a newline character. 

            canonicalSpan[canonicalPosition++] = '/';

            if (accountId.TryFormat(canonicalSpan.Slice(canonicalPosition), out var cw))
            {
                canonicalPosition += cw;
            }

            canonicalSpan[canonicalPosition++] = '/';

            queueName.Value.AsSpan().CopyTo(canonicalSpan.Slice(canonicalPosition));
            canonicalPosition += queueName.Value.Length;

            canonicalSpan[canonicalPosition++] = '/';

            canonicalSpan[canonicalPosition++] = '\n';

            // 3. Add the canonical query string, followed by a newline character.

            var tempQs = "Action=ListUsers&Version=2010-05-08";

            tempQs.AsSpan().CopyTo(canonicalSpan.Slice(canonicalPosition));
            canonicalPosition += tempQs.Length;

            canonicalSpan[canonicalPosition++] = '\n';

            // 4. Add the canonical headers, followed by a newline character. 

            var tempHeader1 = "host:sqs.eu-west-2.amazonaws.com";
            var tempHeader2 = "x-amz-date:20191203T1900000Z";

            tempHeader1.AsSpan().CopyTo(canonicalSpan.Slice(canonicalPosition));
            canonicalPosition += tempHeader1.Length;

            canonicalSpan[canonicalPosition++] = '\n';

            tempHeader2.AsSpan().CopyTo(canonicalSpan.Slice(canonicalPosition));
            canonicalPosition += tempHeader2.Length;

            canonicalSpan[canonicalPosition++] = '\n';

            // 5. Add the signed headers, followed by a newline character. 

            var tempSignedHeaders = "host;x-amz-date";

            tempSignedHeaders.AsSpan().CopyTo(canonicalSpan.Slice(canonicalPosition));
            canonicalPosition += tempSignedHeaders.Length;

            canonicalSpan[canonicalPosition++] = '\n';

            var emptyPayloadHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

            emptyPayloadHash.AsSpan().CopyTo(canonicalSpan.Slice(canonicalPosition));
            canonicalPosition += emptyPayloadHash.Length;

            var canonicalUrlBytes = ArrayPool<byte>.Shared.Rent(2048);

            var b2 = Encoding.UTF8.GetBytes(canonicalSpan.Slice(0, canonicalPosition), canonicalUrlBytes);

            var hashUrlBytes = ArrayPool<byte>.Shared.Rent(2048);

            using var sha256 = SHA256.Create();

            sha256.TryComputeHash(canonicalUrlBytes.AsSpan().Slice(b2), hashUrlBytes, out var hashedBytes);

            var thing = Convert.ToBase64String(hashUrlBytes.AsSpan().Slice(0, hashedBytes));

            // badly inefficient base-16 string
            StringBuilder hex = new StringBuilder(hashedBytes * 2);
            foreach (byte bb in hashUrlBytes.AsSpan().Slice(0, hashedBytes))
                hex.AppendFormat("{0:x2}", bb);
            var result = hex.ToString();

            // headers

            //var colon = (byte)':';

            //writer.WriteAsciiNoValidation("X-Amz-Date");
            //writer.Write(MemoryMarshal.CreateReadOnlySpan(ref colon, 1));
            //writer.Write(Space);
            //writer.WriteAsciiNoValidation("20191203T1900000Z");
            //writer.Write(NewLine);


            writer.Commit();
        }

        private void WriteHttpRequestMessage(HttpRequestMessage requestMessage)
        {
            var writer = new BufferWriter<PipeWriter>(_connection.Transport.Output);
            writer.WriteAsciiNoValidation(requestMessage.Method.Method);
            writer.Write(Space);
            writer.WriteAsciiNoValidation(requestMessage.RequestUri.ToString());
            writer.Write(Space);
            writer.Write(Http11);
            writer.Write(NewLine);

            var colon = (byte)':';

            foreach (var header in requestMessage.Headers)
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

        public static HttpClientProtocol CreateFromConnection(ConnectionContext connection)
        {
            return new HttpClientProtocol(connection);
        }

        private void ParseHttpResponse(ref ReadOnlySequence<byte> buffer, HttpResponseMessage httpResponse, out SequencePosition examined)
        {
            var sequenceReader = new SequenceReader<byte>(buffer);
            examined = buffer.End;

            if (_state == State.StartLine)
            {
                if (!sequenceReader.TryReadTo(out ReadOnlySpan<byte> version, (byte)' '))
                {
                    return;
                }

                if (!sequenceReader.TryReadTo(out ReadOnlySpan<byte> statusCodeText, (byte)' '))
                {
                    return;
                }

                if (!sequenceReader.TryReadTo(out ReadOnlySequence<byte> statusText, NewLine))
                {
                    return;
                }

                Utf8Parser.TryParse(statusCodeText, out int statusCode, out _);

                httpResponse.StatusCode = (HttpStatusCode)statusCode;
                httpResponse.ReasonPhrase = Encoding.ASCII.GetString(statusText.IsSingleSegment ? statusText.FirstSpan : statusText.ToArray());
                httpResponse.Version = new Version(1, 1); // TODO: Check

                _state = State.Headers;
                examined = sequenceReader.Position;
            }
            else if (_state == State.Headers)
            {
                while (sequenceReader.TryReadTo(out var headerLine, NewLine))
                {
                    if (headerLine.Length == 0)
                    {
                        examined = sequenceReader.Position;
                        // End of headers
                        _state = State.Body;
                        break;
                    }

                    // Parse the header
                    ParseHeader(headerLine, out var headerName, out var headerValue);

                    var key = Encoding.ASCII.GetString(headerName.Trim(TrimChars));
                    var value = Encoding.ASCII.GetString(headerValue.Trim(TrimChars));

                    httpResponse.Headers.TryAddWithoutValidation(key, value);
                }
            }

            // Slice whatever we've read so far
            buffer = buffer.Slice(sequenceReader.Position);
        }

        private static void ParseHeader(in ReadOnlySequence<byte> headerLine, out ReadOnlySpan<byte> headerName, out ReadOnlySpan<byte> headerValue)
        {
            if (headerLine.IsSingleSegment)
            {
                var span = headerLine.FirstSpan;
                var colon = span.IndexOf((byte)':');
                headerName = span.Slice(0, colon);
                headerValue = span.Slice(colon + 1);
            }
            else
            {
                var headerReader = new SequenceReader<byte>(headerLine);
                headerReader.TryReadTo(out headerName, (byte)':');
                var remaining = headerReader.Sequence.Slice(headerReader.Position);
                headerValue = remaining.IsSingleSegment ? remaining.FirstSpan : remaining.ToArray();
            }
        }

        private enum State
        {
            StartLine,
            Headers,
            Body
        }
    }
}
