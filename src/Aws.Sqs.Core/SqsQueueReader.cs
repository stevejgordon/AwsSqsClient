using System;
using System.Buffers;
using HighPerfCloud.Aws.Sqs.Core.Primitives;

namespace HighPerfCloud.Aws.Sqs.Core
{
    public class SqsSingleQueueReader : QueueReader
    {
        private readonly AwsRegion _region;
        private readonly AccountId _accountId;
        private readonly string _queueName;
        private readonly AwsApiInvoker _awsApiInvoker;

        public SqsSingleQueueReader(in AwsRegion region, in AccountId accountId, string queueName, AwsApiInvoker awsApiInvoker)
        {
            _region = region;
            _accountId = accountId;
            _queueName = queueName;
            _awsApiInvoker = awsApiInvoker;
        }

        public override void ReceiveSingleMessage()
        {
            throw new System.NotImplementedException();
        }
    }

    public class SqsQueueReader
    {
        private readonly AwsApiInvoker _awsApiInvoker;

        public SqsQueueReader(AwsApiInvoker awsApiInvoker)
        {
            _awsApiInvoker = awsApiInvoker;
        }
               
        public void ReceiveSingleMessage(in SqsQueueData sqsQueueData)
        {
            throw new System.NotImplementedException();
        }
    }

    public readonly struct SqsQueueData
    {
        private readonly AwsRegion _region;
        private readonly AccountId _accountId;
        private readonly string _queueName;

        public SqsQueueData(in AwsRegion region, in AccountId accountId, string queueName)
        {
            _region = region;
            _accountId = accountId;
            _queueName = queueName;
        }
    }
}
