using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AhuErp.Core.Data;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>EF6-реализация <see cref="INomenclatureRepository"/>.</summary>
    public sealed class EfNomenclatureRepository : INomenclatureRepository
    {
        private readonly AhuDbContext _ctx;

        public EfNomenclatureRepository(AhuDbContext ctx)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        }

        public IReadOnlyList<NomenclatureCase> ListCases(int? year, bool activeOnly)
        {
            IQueryable<NomenclatureCase> q = _ctx.NomenclatureCases;
            if (year.HasValue) q = q.Where(c => c.Year == year.Value);
            if (activeOnly) q = q.Where(c => c.IsActive);
            return q.OrderBy(c => c.Index).ToList().AsReadOnly();
        }

        public NomenclatureCase GetCase(int id) => _ctx.NomenclatureCases.Find(id);

        public NomenclatureCase AddCase(NomenclatureCase @case)
        {
            _ctx.NomenclatureCases.Add(@case);
            _ctx.SaveChanges();
            return @case;
        }

        public NomenclatureCase UpdateCase(NomenclatureCase @case)
        {
            if (_ctx.Entry(@case).State == EntityState.Detached)
            {
                _ctx.NomenclatureCases.Attach(@case);
                _ctx.Entry(@case).State = EntityState.Modified;
            }
            _ctx.SaveChanges();
            return @case;
        }

        public IReadOnlyList<DocumentTypeRef> ListTypes(bool activeOnly)
        {
            IQueryable<DocumentTypeRef> q = _ctx.DocumentTypeRefs;
            if (activeOnly) q = q.Where(t => t.IsActive);
            return q.OrderBy(t => t.Name).ToList().AsReadOnly();
        }

        public DocumentTypeRef GetType(int id) => _ctx.DocumentTypeRefs.Find(id);

        public DocumentTypeRef AddType(DocumentTypeRef typeRef)
        {
            _ctx.DocumentTypeRefs.Add(typeRef);
            _ctx.SaveChanges();
            return typeRef;
        }

        public DocumentTypeRef UpdateType(DocumentTypeRef typeRef)
        {
            if (_ctx.Entry(typeRef).State == EntityState.Detached)
            {
                _ctx.DocumentTypeRefs.Attach(typeRef);
                _ctx.Entry(typeRef).State = EntityState.Modified;
            }
            _ctx.SaveChanges();
            return typeRef;
        }

        public int GetMaxSequence(int documentTypeRefId, int year)
        {
            // Последовательность регистрации хранится не в отдельной таблице,
            // а вычисляется по уже выданным регистрационным номерам данного вида
            // в данном году. Это исключает рассинхронизацию счётчика и реальных
            // номеров при ручных правках в БД.
            var query = _ctx.Documents
                .Where(d => d.DocumentTypeRefId == documentTypeRefId
                            && d.RegistrationDate.HasValue
                            && d.RegistrationDate.Value.Year == year);
            return query.Count();
        }

        public IReadOnlyList<Department> ListDepartments()
            => _ctx.Departments.OrderBy(d => d.Name).ToList().AsReadOnly();

        public Department AddDepartment(Department department)
        {
            _ctx.Departments.Add(department);
            _ctx.SaveChanges();
            return department;
        }
    }
}
