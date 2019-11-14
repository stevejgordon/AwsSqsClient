using FluentAssertions;
using HighPerfCloud.Aws.Sqs.Core;
using HighPerfCloud.Aws.Sqs.Core.Primitives;
using System;
using Xunit;

namespace Aws.Sqs.Core.Tests
{
    public class QueueUrlBuilderTests
    {
        [Fact]
        public void TryBuild_ReturnsExpectedUrl()
        {
            const string expected = "https://sqs.eu-west-1.amazonaws.com/123456789012/MyQueue";

            var region = AwsRegion.EuWest1;
            var accountId = new AccountId(123456789012);
            var queueName = new QueueName("MyQueue");

            var builder = new QueueUrlBuilder();

            var destination = new char[100].AsSpan(); // larger than we need

            var result = builder.TryBuild(destination, queueName, region, accountId, out var bytesWritten);

            result.Should().BeTrue();
            bytesWritten.Should().Be(expected.Length);
            destination.Slice(0, bytesWritten).ToString().Should().Be(expected);
        }
    }
}
