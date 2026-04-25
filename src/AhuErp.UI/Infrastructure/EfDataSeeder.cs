using System;
using System.Linq;
using AhuErp.Core.Data;
using AhuErp.Core.Models;
using AhuErp.Core.Services;

namespace AhuErp.UI.Infrastructure
{
    /// <summary>
    /// Минимальный сидинг для EF6-бэкенда: при пустой таблице <c>Employees</c>
    /// создаёт одного администратора с паролем <c>password</c> (PBKDF2-хэш),
    /// чтобы пользователь смог войти в систему сразу после первого запуска.
    /// Демонстрационный контент (документы / ТС / ТМЦ) сюда не входит — пусть
    /// реальная БД остаётся «чистой», а пользователь либо раскомментирует
    /// блок seed в <c>scripts/create-db.sql</c>, либо заведёт данные через UI.
    /// </summary>
    public static class EfDataSeeder
    {
        public const string DefaultPassword = "password";
        public const string AdminFullName = "Иванов Иван Иванович";

        public static void EnsureSeeded(AhuDbContext ctx, IPasswordHasher hasher)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (hasher == null) throw new ArgumentNullException(nameof(hasher));

            // Логин невозможен без PasswordHash, поэтому проверяем наличие
            // именно «логинабельного» сотрудника, а не просто любых строк
            // (например, в scripts/create-db.sql сид-блок может быть раскомментирован
            // и создать сотрудников без паролей).
            var hasLoginable = ctx.Employees.Any(e => e.PasswordHash != null && e.PasswordHash != "");
            if (hasLoginable) return;

            // Если уже есть сотрудник с тем же ФИО (без пароля) — обновляем ему
            // хэш, чтобы не плодить дубликаты в будущих демо-сценариях.
            var existing = ctx.Employees.FirstOrDefault(e => e.FullName == AdminFullName);
            if (existing != null)
            {
                existing.Role = EmployeeRole.Admin;
                existing.PasswordHash = hasher.Hash(DefaultPassword);
                ctx.SaveChanges();
                return;
            }

            ctx.Employees.Add(new Employee
            {
                FullName = AdminFullName,
                Position = "Администратор",
                Role = EmployeeRole.Admin,
                PasswordHash = hasher.Hash(DefaultPassword)
            });
            ctx.SaveChanges();
        }
    }
}
