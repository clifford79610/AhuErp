using AhuErp.Core.Services;
using Xunit;

namespace AhuErp.Tests
{
    public class PasswordHasherTests
    {
        private readonly Pbkdf2PasswordHasher _hasher = new Pbkdf2PasswordHasher(iterations: 1000);

        [Fact]
        public void Hash_produces_non_empty_output_and_never_matches_raw_password()
        {
            var hash = _hasher.Hash("CorrectHorseBatteryStaple");
            Assert.False(string.IsNullOrEmpty(hash));
            Assert.DoesNotContain("CorrectHorseBatteryStaple", hash);
            Assert.StartsWith("1000.", hash);
        }

        [Fact]
        public void Hash_uses_unique_salt_per_call()
        {
            var a = _hasher.Hash("same-password");
            var b = _hasher.Hash("same-password");
            Assert.NotEqual(a, b);
        }

        [Fact]
        public void Verify_returns_true_for_correct_password()
        {
            var hash = _hasher.Hash("hunter2");
            Assert.True(_hasher.Verify("hunter2", hash));
        }

        [Fact]
        public void Verify_returns_false_for_incorrect_password()
        {
            var hash = _hasher.Hash("correct");
            Assert.False(_hasher.Verify("wrong", hash));
        }

        [Theory]
        [InlineData("not-a-valid-hash")]
        [InlineData("1000.onlyOnePart")]
        [InlineData("")]
        [InlineData(null)]
        public void Verify_returns_false_for_malformed_hash(string hash)
        {
            Assert.False(_hasher.Verify("whatever", hash));
        }
    }
}
