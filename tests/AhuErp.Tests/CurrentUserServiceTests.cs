using AhuErp.Core.Models;
using AhuErp.Core.Services;
using Xunit;

namespace AhuErp.Tests
{
    /// <summary>
    /// Контрактные тесты для <see cref="ICurrentUserService"/>: AuthService
    /// должен предоставлять тот же current view, что и через расширенный
    /// IAuthService.
    /// </summary>
    public class CurrentUserServiceTests
    {
        private readonly IPasswordHasher _hasher = new Pbkdf2PasswordHasher(iterations: 1000);

        private (AuthService auth, Employee employee) Build()
        {
            var repo = new InMemoryEmployeeRepository();
            var emp = new Employee
            {
                FullName = "Тестов Тест Тестович",
                Role = EmployeeRole.Manager,
                PasswordHash = _hasher.Hash("p@ss")
            };
            repo.Add(emp);
            return (new AuthService(repo, _hasher), emp);
        }

        [Fact]
        public void Current_is_null_before_login()
        {
            var (auth, _) = Build();
            ICurrentUserService current = auth;

            Assert.Null(current.Current);
            Assert.Null(current.CurrentId);
            Assert.False(current.IsAuthenticated);
        }

        [Fact]
        public void Current_reflects_logged_in_user()
        {
            var (auth, emp) = Build();
            Assert.True(auth.TryLogin(emp.FullName, "p@ss"));

            ICurrentUserService current = auth;
            Assert.NotNull(current.Current);
            Assert.Equal(emp.Id, current.CurrentId);
            Assert.True(current.IsAuthenticated);
            Assert.Same(auth.CurrentEmployee, current.Current);
        }

        [Fact]
        public void Current_clears_after_logout()
        {
            var (auth, emp) = Build();
            auth.TryLogin(emp.FullName, "p@ss");
            ICurrentUserService current = auth;
            Assert.True(current.IsAuthenticated);

            auth.Logout();

            Assert.Null(current.Current);
            Assert.Null(current.CurrentId);
            Assert.False(current.IsAuthenticated);
        }
    }
}
