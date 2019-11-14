using HighPerfCloud.Aws.Sqs.Core.Primitives;
using System;
using FluentAssertions;
using Xunit;

namespace Aws.Sqs.Core.Tests.Primitives
{
    public class AccountIdTests
    {
        [Theory]
        [InlineData(123456789012)]
        [InlineData(1)]
        [InlineData(0012)]
        public void Ctor_DoesNotThrow_ForValidAccountIds(long value)
        {
            _ = new AccountId(value);
        }

        [Fact]
        public void Ctor_Throws_ForNegativeAccountId()
        {
            _ = Assert.Throws<ArgumentException>(() => new AccountId(-1));
        }

        [Fact]
        public void Ctor_Throws_ForAccountId_GreaterThanTwelveDigits()
        {
            _ = Assert.Throws<ArgumentException>(() => new AccountId(1_000_000_000_000));
        }

        [Theory]
        [InlineData(123456789012, new[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '1', '2' })]
        [InlineData(12, new[] { '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '1', '2' })]
        public void TryFormat_Succeeds_AndProducesExpectedCharacters(long acctId, char[] expected)
        {
            var accountId = new AccountId(acctId);

            var destination = new char[12].AsSpan();

            var result = accountId.TryFormat(destination);

            result.Should().BeTrue();

            destination.ToArray().Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void TryFormat_Fails_WhenDestinationIsTooSmall()
        {
            var accountId = new AccountId(123456789012);

            var destination = new char[11].AsSpan();

            var expected = destination.ToArray();

            var result = accountId.TryFormat(destination);

            result.Should().BeFalse();

            destination.ToArray().Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void TwoAccountIds_WithTheSameInternalValue_AreEqual_UsingOperator()
        {
            var accountId1 = new AccountId(123456789012);
            var accountId2 = new AccountId(123456789012);

            var result = accountId1 == accountId2;

            result.Should().BeTrue();
        }

        [Fact]
        public void TwoAccountIds_WithTheSameInternalValue_AreEqual()
        {
            var accountId1 = new AccountId(123456789012);
            var accountId2 = new AccountId(123456789012);

            accountId1.Equals(accountId2).Should().BeTrue();
        }

        [Fact]
        public void ImplicitConversionToLong_Succeeds()
        {
            var accountId = new AccountId(123456789012);

            long value = accountId;

            value.Should().Be(123456789012);
        }

        [Fact]
        public void ImplicitConversionFromLong_Succeeds()
        {
            const long accountId = 123456789012;

            AccountId value = accountId;

            value.Should().Be(new AccountId(123456789012));
        }
    }
}
