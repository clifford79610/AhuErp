using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AhuErp.Core.Data;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// EF6-реализация <see cref="IPositionRepository"/>. Использует
    /// singleton <see cref="AhuDbContext"/>.
    /// </summary>
    public sealed class EfPositionRepository : IPositionRepository
    {
        private readonly AhuDbContext _ctx;

        public EfPositionRepository(AhuDbContext ctx)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        }

        public IReadOnlyList<Position> List(bool activeOnly)
        {
            IQueryable<Position> q = _ctx.Positions.AsNoTracking();
            if (activeOnly) q = q.Where(x => x.IsActive);
            return q.OrderBy(x => x.Name).ToList().AsReadOnly();
        }

        public Position Get(int id) =>
            _ctx.Positions.AsNoTracking().FirstOrDefault(x => x.Id == id);

        public Position Add(Position position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            _ctx.Positions.Add(position);
            _ctx.SaveChanges();
            return position;
        }

        public Position Update(Position position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            var existing = _ctx.Positions.FirstOrDefault(x => x.Id == position.Id);
            if (existing == null)
                throw new InvalidOperationException($"Должность id={position.Id} не найдена.");

            existing.Name = position.Name;
            existing.Category = position.Category;
            existing.IsActive = position.IsActive;
            _ctx.SaveChanges();
            return existing;
        }
    }
}
