using System.IO.Pipelines;
using System.Threading.Tasks;

namespace HighPerfCloud.Aws.Sqs.Core.Client
{
    public interface IPipeWritable
    {
        Task WriteAsync(PipeWriter writer);
    }
}