using System;
using System.Collections.Generic;
using System.Linq;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// In-memory реализация <see cref="IPositionRepository"/> для тестов и
    /// демо-режима.
    /// </summary>
    public sealed class InMemoryPositionRepository : IPositionRepository
    {
        private readonly List<Position> _items = new List<Position>();
        private int _nextId = 1;

        public IReadOnlyList<Position> List(bool activeOnly)
        {
            IEnumerable<Position> q = _items;
            if (activeOnly) q = q.Where(x => x.IsActive);
            return q.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList().AsReadOnly();
        }

        public Position Get(int id) => _items.FirstOrDefault(x => x.Id == id);

        public Position Add(Position position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            if (string.IsNullOrWhiteSpace(position.Name))
                throw new ArgumentException("Name обязателен.", nameof(position));

            position.Id = _nextId++;
            _items.Add(position);
            return position;
        }

        public Position Update(Position position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            var existing = Get(position.Id);
            if (existing == null)
                throw new InvalidOperationException($"Должность id={position.Id} не найдена.");

            existing.Name = position.Name;
            existing.Category = position.Category;
            existing.IsActive = position.IsActive;
            return existing;
        }
    }
}
