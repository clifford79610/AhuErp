using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AhuErp.Core.Data;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// EF6-реализация <see cref="ICounterpartyRepository"/>. Использует
    /// singleton <see cref="AhuDbContext"/>, как и другие Ef-репозитории.
    /// </summary>
    public sealed class EfCounterpartyRepository : ICounterpartyRepository
    {
        private readonly AhuDbContext _ctx;

        public EfCounterpartyRepository(AhuDbContext ctx)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        }

        public IReadOnlyList<Counterparty> List(bool activeOnly)
        {
            IQueryable<Counterparty> q = _ctx.Counterparties.AsNoTracking();
            if (activeOnly) q = q.Where(x => x.IsActive);
            return q.OrderBy(x => x.ShortName).ToList().AsReadOnly();
        }

        public Counterparty Get(int id) =>
            _ctx.Counterparties.AsNoTracking().FirstOrDefault(x => x.Id == id);

        public Counterparty Add(Counterparty counterparty)
        {
            if (counterparty == null) throw new ArgumentNullException(nameof(counterparty));
            // Нормализуем пустой ИНН → null, иначе поведение расходится с
            // filtered unique-индексом в БД (WHERE Inn IS NOT NULL): пустые строки
            // проходят нашу C#-проверку (IsNullOrWhiteSpace), но вылетают
            // на SaveChanges как DbUpdateException на второй вставке.
            if (string.IsNullOrWhiteSpace(counterparty.Inn)) counterparty.Inn = null;
            if (counterparty.Inn != null &&
                _ctx.Counterparties.Any(x => x.Inn == counterparty.Inn))
            {
                throw new InvalidOperationException(
                    $"Контрагент с ИНН '{counterparty.Inn}' уже существует.");
            }
            _ctx.Counterparties.Add(counterparty);
            _ctx.SaveChanges();
            return counterparty;
        }

        public Counterparty Update(Counterparty counterparty)
        {
            if (counterparty == null) throw new ArgumentNullException(nameof(counterparty));
            var existing = _ctx.Counterparties.FirstOrDefault(x => x.Id == counterparty.Id);
            if (existing == null)
                throw new InvalidOperationException($"Контрагент id={counterparty.Id} не найден.");

            // Так же нормализуем пустой ИНН → null для согласованности с filtered
            // unique-индексом в БД. После этого проверяем уникальность только
            // для непустого (и изменившегося) ИНН.
            if (string.IsNullOrWhiteSpace(counterparty.Inn)) counterparty.Inn = null;
            if (counterparty.Inn != null &&
                !string.Equals(existing.Inn, counterparty.Inn, StringComparison.Ordinal) &&
                _ctx.Counterparties.Any(x => x.Id != counterparty.Id && x.Inn == counterparty.Inn))
            {
                throw new InvalidOperationException(
                    $"Контрагент с ИНН '{counterparty.Inn}' уже существует.");
            }

            existing.ShortName = counterparty.ShortName;
            existing.FullName = counterparty.FullName;
            existing.Inn = counterparty.Inn;
            existing.Kpp = counterparty.Kpp;
            existing.Ogrn = counterparty.Ogrn;
            existing.Address = counterparty.Address;
            existing.Phone = counterparty.Phone;
            existing.Email = counterparty.Email;
            existing.Kind = counterparty.Kind;
            existing.IsActive = counterparty.IsActive;
            _ctx.SaveChanges();
            return existing;
        }

        public Counterparty FindByInn(string inn)
        {
            if (string.IsNullOrWhiteSpace(inn)) return null;
            return _ctx.Counterparties.AsNoTracking().FirstOrDefault(x => x.Inn == inn);
        }
    }
}
