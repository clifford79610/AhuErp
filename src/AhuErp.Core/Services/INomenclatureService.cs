using System.Collections.Generic;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// Управление номенклатурой дел и регистрацией документов.
    /// Соответствует Типовой инструкции по делопроизводству и ГОСТ Р 7.0.8-2013:
    /// каждый документ при регистрации получает регистрационный номер по
    /// шаблону вида документа и привязывается к делу.
    /// </summary>
    public interface INomenclatureService
    {
        IReadOnlyList<NomenclatureCase> ListCases(int? year = null, bool activeOnly = true);
        NomenclatureCase GetCase(int id);
        NomenclatureCase AddCase(NomenclatureCase @case);
        NomenclatureCase UpdateCase(NomenclatureCase @case);
        void DeactivateCase(int id);

        IReadOnlyList<DocumentTypeRef> ListTypes(bool activeOnly = true);
        DocumentTypeRef GetType(int id);
        DocumentTypeRef AddType(DocumentTypeRef typeRef);
        DocumentTypeRef UpdateType(DocumentTypeRef typeRef);

        /// <summary>
        /// Зарегистрировать документ: присвоить регистрационный номер, дату
        /// регистрации и (опционально) основное дело номенклатуры. Если
        /// <paramref name="caseId"/> не передан, сервис подбирает дефолтное
        /// дело по виду документа и году. Возвращает обновлённый документ.
        /// Бросает <see cref="System.InvalidOperationException"/>, если документ
        /// уже зарегистрирован или у вида документа отсутствует шаблон номера.
        /// </summary>
        Document Register(int documentId, int? caseId = null);

        /// <summary>
        /// Сформировать регистрационный номер по шаблону вида документа.
        /// Шаблон поддерживает плейсхолдеры:
        /// <c>{Code}</c> — короткий код вида,
        /// <c>{CaseIndex}</c> — индекс дела,
        /// <c>{Year}</c> — текущий год,
        /// <c>{Sequence:0000}</c> — порядковый номер с заданным форматом.
        /// </summary>
        string BuildRegistrationNumber(DocumentTypeRef typeRef, NomenclatureCase @case, int year, int sequence);
    }
}
