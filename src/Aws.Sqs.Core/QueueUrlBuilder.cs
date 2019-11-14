using System;
using HighPerfCloud.Aws.Sqs.Core.Primitives;

namespace HighPerfCloud.Aws.Sqs.Core
{
    internal readonly struct QueueUrlBuilder
    {
        private static ReadOnlySpan<char> PrefixChars => new[] { 'h', 't', 't', 'p', 's', ':', '/', '/', 's', 'q', 's', '.' };

        private static ReadOnlySpan<char> SuffixChars => new[] { '.', 'a', 'm', 'a', 'z', 'o', 'n', 'a', 'w', 's', '.', 'c', 'o', 'm', '/', };

        private const char SlashChar = '/';

        public bool TryBuild(Span<char> destination, in QueueName queueName, in AwsRegion region, in AccountId accountId, out int bytesWritten)
        {
            bytesWritten = 0;

            var requiredLength = CalculateRequiredLength(queueName, region);

            if (destination.Length < requiredLength)
                
                return false; // unable to write URL

            PrefixChars.CopyTo(destination);
            destination = destination.Slice(PrefixChars.Length);

            region.RegionCode.AsSpan().CopyTo(destination);
            destination = destination.Slice(region.RegionCode.Length);

            SuffixChars.CopyTo(destination);
            destination = destination.Slice(SuffixChars.Length);

            if (!accountId.TryFormat(destination))
            {
                return false;
            }

            destination = destination.Slice(12);

            destination[0] = SlashChar;

            destination = destination.Slice(1);

            queueName.Value.AsSpan().CopyTo(destination);

            bytesWritten = requiredLength;

            return true;
        }

        public int CalculateRequiredLength(in QueueName queueName, in AwsRegion region)
        {
            var length = PrefixChars.Length +
                         region.RegionCode.Length +
                         SuffixChars.Length +
                         13 + // 12 for account ID + 1 extra for slash
                         queueName.Value.Length;

            return length;
        }
        
        public bool TryBuild(Span<byte> destination, in QueueName queueName, in AwsRegion region, in AccountId accountId)
        {
            throw new NotImplementedException();
        }
    }
}