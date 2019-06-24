namespace HighPerfCloud.Aws.Sqs.Core
{
    public abstract class QueueReader
    {
        public abstract void ReceiveSingleMessage();
    }
}
