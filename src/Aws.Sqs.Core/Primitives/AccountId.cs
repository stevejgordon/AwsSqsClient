using System;
using System.Globalization;

namespace HighPerfCloud.Aws.Sqs.Core.Primitives
{
    /// <summary>
    /// Represents an AWS Account Identifier.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The AWS account ID is a 12-digit number, such as 123456789012, that is used to construct Amazon Resource Names (ARNs).
    /// </para>
    /// <para>
    /// Internally, this type uses a long to store the data for the account ID.
    /// This requires only 64-bits (8-bytes) per account ID vs. 192-bits is using a string, 3x smaller.
    /// </para>
    /// </remarks>
    public readonly struct AccountId :  IComparable<AccountId>, IEquatable<AccountId>
    {
        private readonly long _value;

        private static ReadOnlySpan<char> Format => new[] { '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0' };

        public static int CharacterLength = 12;

        public AccountId(long accountId)
        {
            if (accountId < 0)
            {
                throw new ArgumentException(message: "Account ID cannot be created from a negative value", nameof(accountId));
            }

            if (accountId > 999_999_999_999) // max 12 digits allows for account IDs
            {
                throw new ArgumentException(message: "Account ID cannot be created from a value larger than 12 digits in length", nameof(accountId));
            }

            _value = accountId;
        }
        
        public static bool operator ==(AccountId left, AccountId right) => Equals(left, right);

        public static bool operator !=(AccountId left, AccountId right) => !Equals(left, right);

        public override bool Equals(object obj) => (obj is AccountId accountId) && Equals(accountId);

        public bool Equals(AccountId other) => (_value) == (other._value);

        public override int GetHashCode() => HashCode.Combine(_value);

        public static implicit operator long(AccountId accountId) => accountId._value;

        public static implicit operator AccountId(long accountId) => new AccountId(accountId);

        public int CompareTo(AccountId other) => _value.CompareTo(other._value);

        public static AccountId Default => new AccountId(0);

        /// <summary>
        ///   Tries to format the value of the current <see cref="AccountId"/> instance into the provided <see cref="Span{T}"/> of characters.
        /// </summary>
        /// <param name="destination">When this method returns, this instance's value formatted as a <see cref="Span{T}"/> of characters.</param>
        /// <returns>
        ///   <c>true</c> for success.
        ///   <c>false</c> if destination was too short, or the formatting failed.
        /// </returns>
        public bool TryFormat(Span<char> destination)
        {
            if (destination.Length < 12)
                return false; // not enough capacity

            var formatted = _value.TryFormat(destination, out var written, Format, CultureInfo.InvariantCulture);

            return formatted && written == 12;
        }

        /// <summary>
        ///   Converts the numeric value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation of the value of this instance.</returns>
        public override string ToString() => _value.ToString();
    }
}
