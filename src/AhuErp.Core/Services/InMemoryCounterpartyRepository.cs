using System;
using System.Collections.Generic;
using System.Linq;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// In-memory реализация <see cref="ICounterpartyRepository"/> для тестов и
    /// демо-режима. ИНН считается уникальным, если он непустой — повторное
    /// добавление контрагента с тем же ИНН выбрасывает <see cref="InvalidOperationException"/>.
    /// </summary>
    public sealed class InMemoryCounterpartyRepository : ICounterpartyRepository
    {
        private readonly List<Counterparty> _items = new List<Counterparty>();
        private int _nextId = 1;

        public IReadOnlyList<Counterparty> List(bool activeOnly)
        {
            IEnumerable<Counterparty> q = _items;
            if (activeOnly) q = q.Where(x => x.IsActive);
            return q.OrderBy(x => x.ShortName, StringComparer.OrdinalIgnoreCase)
                    .ToList().AsReadOnly();
        }

        public Counterparty Get(int id) => _items.FirstOrDefault(x => x.Id == id);

        public Counterparty Add(Counterparty counterparty)
        {
            if (counterparty == null) throw new ArgumentNullException(nameof(counterparty));
            if (string.IsNullOrWhiteSpace(counterparty.ShortName))
                throw new ArgumentException("ShortName обязателен.", nameof(counterparty));

            var existingByInn = FindByInn(counterparty.Inn);
            if (existingByInn != null)
                throw new InvalidOperationException(
                    $"Контрагент с ИНН '{counterparty.Inn}' уже существует (id={existingByInn.Id}).");

            counterparty.Id = _nextId++;
            _items.Add(counterparty);
            return counterparty;
        }

        public Counterparty Update(Counterparty counterparty)
        {
            if (counterparty == null) throw new ArgumentNullException(nameof(counterparty));
            var existing = Get(counterparty.Id);
            if (existing == null)
                throw new InvalidOperationException($"Контрагент id={counterparty.Id} не найден.");

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
            return existing;
        }

        public Counterparty FindByInn(string inn)
        {
            if (string.IsNullOrWhiteSpace(inn)) return null;
            return _items.FirstOrDefault(x => string.Equals(x.Inn, inn, StringComparison.Ordinal));
        }
    }
}
