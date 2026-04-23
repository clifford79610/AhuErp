using System;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// Базовая реализация <see cref="IAuthService"/>. Держит текущего
    /// <see cref="Employee"/> в оперативной памяти; при хранении
    /// последней сессии (remember me) и аудите — расширять в Phase 5.
    /// </summary>
    public sealed class AuthService : IAuthService
    {
        private readonly IEmployeeRepository _employees;
        private readonly IPasswordHasher _hasher;

        public Employee CurrentEmployee { get; private set; }

        public bool IsAuthenticated => CurrentEmployee != null;

        public AuthService(IEmployeeRepository employees, IPasswordHasher hasher)
        {
            _employees = employees ?? throw new ArgumentNullException(nameof(employees));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        }

        public bool TryLogin(string fullName, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrEmpty(password))
            {
                CurrentEmployee = null;
                return false;
            }

            var employee = _employees.FindByFullName(fullName);
            if (employee == null || string.IsNullOrEmpty(employee.PasswordHash))
            {
                CurrentEmployee = null;
                return false;
            }

            if (!_hasher.Verify(password, employee.PasswordHash))
            {
                CurrentEmployee = null;
                return false;
            }

            CurrentEmployee = employee;
            return true;
        }

        public void Logout()
        {
            CurrentEmployee = null;
        }
    }
}
