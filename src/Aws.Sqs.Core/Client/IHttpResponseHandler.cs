using System.IO.Pipelines;
using System.Text.Http.Parser;
using System.Threading.Tasks;

namespace HighPerfCloud.Aws.Sqs.Core.Client
{
    public interface IHttpResponseHandler : IHttpHeadersHandler, IHttpResponseLineHandler
    {
        ValueTask OnBody(PipeReader body);
    }
}