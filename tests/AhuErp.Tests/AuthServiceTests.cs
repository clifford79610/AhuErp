using AhuErp.Core.Models;
using AhuErp.Core.Services;
using Xunit;

namespace AhuErp.Tests
{
    public class AuthServiceTests
    {
        private readonly IPasswordHasher _hasher = new Pbkdf2PasswordHasher(iterations: 1000);

        private (AuthService auth, InMemoryEmployeeRepository repo, Employee alice) BuildService()
        {
            var repo = new InMemoryEmployeeRepository();
            var alice = new Employee
            {
                Id = 1,
                FullName = "Иванова Алиса",
                Role = EmployeeRole.Manager,
                PasswordHash = _hasher.Hash("s3cret")
            };
            repo.Add(alice);
            var service = new AuthService(repo, _hasher);
            return (service, repo, alice);
        }

        [Fact]
        public void TryLogin_succeeds_with_correct_credentials_and_sets_current_employee()
        {
            var (auth, _, alice) = BuildService();

            Assert.True(auth.TryLogin("Иванова Алиса", "s3cret"));
            Assert.True(auth.IsAuthenticated);
            Assert.Equal(alice.Id, auth.CurrentEmployee.Id);
            Assert.Equal(EmployeeRole.Manager, auth.CurrentEmployee.Role);
        }

        [Fact]
        public void TryLogin_fails_with_wrong_password_and_clears_current_employee()
        {
            var (auth, _, _) = BuildService();

            Assert.False(auth.TryLogin("Иванова Алиса", "wrong"));
            Assert.False(auth.IsAuthenticated);
            Assert.Null(auth.CurrentEmployee);
        }

        [Fact]
        public void TryLogin_fails_for_unknown_user()
        {
            var (auth, _, _) = BuildService();
            Assert.False(auth.TryLogin("Несуществующий Пользователь", "s3cret"));
            Assert.Null(auth.CurrentEmployee);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void TryLogin_fails_for_blank_login(string login)
        {
            var (auth, _, _) = BuildService();
            Assert.False(auth.TryLogin(login, "s3cret"));
        }

        [Fact]
        public void TryLogin_fails_for_employee_without_password_hash()
        {
            var repo = new InMemoryEmployeeRepository();
            repo.Add(new Employee
            {
                Id = 99,
                FullName = "Anonymous Account",
                Role = EmployeeRole.TechSupport,
                PasswordHash = null
            });
            var auth = new AuthService(repo, _hasher);

            Assert.False(auth.TryLogin("Anonymous Account", "any"));
            Assert.Null(auth.CurrentEmployee);
        }

        [Fact]
        public void Logout_clears_current_employee()
        {
            var (auth, _, _) = BuildService();
            auth.TryLogin("Иванова Алиса", "s3cret");

            auth.Logout();

            Assert.False(auth.IsAuthenticated);
            Assert.Null(auth.CurrentEmployee);
        }
    }
}
