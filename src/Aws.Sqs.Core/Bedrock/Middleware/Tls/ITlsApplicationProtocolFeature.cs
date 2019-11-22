using System;

namespace HighPerfCloud.Aws.Sqs.Core.Bedrock.Middleware.Tls
{
    public interface ITlsApplicationProtocolFeature
    {
        ReadOnlyMemory<byte> ApplicationProtocol { get; }
    }
}