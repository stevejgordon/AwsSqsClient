using System;

namespace HighPerfCloud.Aws.Sqs.Core.Primitives
{
    public class QueueName : IEquatable<QueueName>
    {
        // incomplete

        public QueueName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(message: "Queue name value must not be null or empty.", nameof(value));
            }

            if (value.Length > 80)
            {
                throw new ArgumentException(message: "The length of the queue name must be 80 characters or less in length.", nameof(value));
            }

            if (!IsValid(value))
            {
                throw new ArgumentException(message: "The queue name contains invalid characters. The following characters are accepted: alphanumeric characters, hyphens (-), and underscores (_).", nameof(value));
            }

            Value = value;
        }

        private static bool IsValid(ReadOnlySpan<char> value)
        {
            var isValid = true;

            foreach (var c in value)
            {
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
                {
                    isValid = false;
                    break;
                }
            }

            return isValid;
        }

        public string Value { get; }

        public static bool operator ==(QueueName left, QueueName right) => Equals(left, right);

        public static bool operator !=(QueueName left, QueueName right) => !Equals(left, right);

        public override bool Equals(object obj) => (obj is QueueName awsRegion) && Equals(awsRegion);

        public bool Equals(QueueName other) => (Value) == (other.Value);

        public override int GetHashCode() => HashCode.Combine(Value);

        public override string ToString() => Value;

    }
}