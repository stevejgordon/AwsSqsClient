using System;
using HighPerfCloud.Aws.Sqs.Core.Primitives;

namespace HighPerfCloud.Aws.Sqs.Core
{
    internal readonly struct QueueUrlBuilder
    {
        private static ReadOnlySpan<char> PrefixChars => new[] { 'h', 't', 't', 'p', 's', ':', '/', '/', 's', 'q', 's', '.' };

        private static ReadOnlySpan<char> SuffixChars => new[] { '.', 'a', 'm', 'a', 'z', 'o', 'n', 'a', 'w', 's', '.', 'c', 'o', 'm', '/', };

        private const char SlashChar = '/';

        public bool TryBuild(Span<char> destination, in QueueName queueName, in AwsRegion region, in AccountId accountId, out int bytesWritten, bool skipLengthCheck = false)
        {
            bytesWritten = 0;

            if (!skipLengthCheck)
            {
                var requiredLength = CalculateRequiredCharacterLength(queueName, region);

                if (destination.Length < requiredLength)
                {
                    return false; // unable to write URL
                }
            }

            PrefixChars.CopyTo(destination);
            destination = destination.Slice(PrefixChars.Length);
            bytesWritten += PrefixChars.Length;

            region.RegionCode.AsSpan().CopyTo(destination);
            destination = destination.Slice(region.RegionCode.Length);
            bytesWritten += region.RegionCode.Length;

            SuffixChars.CopyTo(destination);
            destination = destination.Slice(SuffixChars.Length);
            bytesWritten += SuffixChars.Length;

            if (!accountId.TryFormat(destination))
            {
                return false;
            }

            destination = destination.Slice(12);
            destination[0] = SlashChar;
            destination = destination.Slice(1);
            bytesWritten += AccountId.CharacterLength + 1;

            queueName.Value.AsSpan().CopyTo(destination);
            bytesWritten += queueName.Value.Length;

            return true;
        }

        /// <summary> 
        /// Calculates the character length required to build a URL for an AWS SQS queue.
        /// complete queue URL for the <paramref name="queueName"/> and <paramref name="region"/>.
        /// </summary>
        /// <param name="queueName">The <see cref="QueueName"/> for the AWS SQS queue.</param>
        /// <param name="region">The <see cref="AwsRegion"/> where the queue resides.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queueName"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="region"/> is equal to default.</exception>
        /// <returns>A 32-bit signed integer indicating the number of characters needed for an AWS SQS queue.</returns>
        public static int CalculateRequiredCharacterLength(in QueueName queueName, in AwsRegion region)
        {
            if (queueName is null)
                throw new ArgumentNullException(nameof(queueName));

            if (region == default)
                throw new ArgumentException("A valid, AwsRegion is required.", nameof(queueName));

            var length = PrefixChars.Length +
                         region.RegionCode.Length +
                         SuffixChars.Length +
                         AccountId.CharacterLength + 
                         1 + // 1 extra for slash
                         queueName.Value.Length;

            return length;
        }

        public bool TryBuild(Span<byte> destination, in QueueName queueName, in AwsRegion region, in AccountId accountId)
        {
            throw new NotImplementedException();
        }
    }
}