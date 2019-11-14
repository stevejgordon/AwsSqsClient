using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.IO.Pipelines;
using System.Threading.Tasks;
using System.Text.Unicode;

namespace HighPerfCloud.Aws.Sqs.Core.Client
{
    public class AwsSqsClient
    {
        private SocketPipe _socketPipe;

        public AwsSqsClient()
        {

        }

        public string Host { get; }

        public string Port { get; }

        
    }

    public interface ISqsRequest
    {
        AwsSqsClient Client { get; set; }

        Task WriteAsync(PipeWriter writer);

        string RequestPath { get; }
        
        //string CanonicalizedResource { get; }
        
        long ContentLength { get; }
        
        bool ConsumeBody { get; }
    }
}
