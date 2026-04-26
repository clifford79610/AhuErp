using System.Collections.Generic;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>Доступ к данным справочников номенклатуры дел и видов документов.</summary>
    public interface INomenclatureRepository
    {
        IReadOnlyList<NomenclatureCase> ListCases(int? year, bool activeOnly);
        NomenclatureCase GetCase(int id);
        NomenclatureCase AddCase(NomenclatureCase @case);
        NomenclatureCase UpdateCase(NomenclatureCase @case);

        IReadOnlyList<DocumentTypeRef> ListTypes(bool activeOnly);
        DocumentTypeRef GetType(int id);
        DocumentTypeRef AddType(DocumentTypeRef typeRef);
        DocumentTypeRef UpdateType(DocumentTypeRef typeRef);

        /// <summary>Получить максимальную последовательность регистрационных номеров для пары вид/год.</summary>
        int GetMaxSequence(int documentTypeRefId, int year);

        /// <summary>
        /// Зафиксировать факт выдачи регистрационного номера. Реализации,
        /// вычисляющие <see cref="GetMaxSequence"/> по реальным документам
        /// (например, EF6), могут оставить метод пустым; реализации со
        /// своим счётчиком (in-memory) обязаны его обновить.
        /// </summary>
        void BumpSequence(int documentTypeRefId, int year, int sequence);

        IReadOnlyList<Department> ListDepartments();
        Department AddDepartment(Department department);
    }
}
