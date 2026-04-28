using System.Collections.Generic;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// Доступ к справочнику контрагентов. Минимальный CRUD для НСИ — UI и
    /// бизнес-логика связывания с документами добавляются последующими PR.
    /// </summary>
    public interface ICounterpartyRepository
    {
        IReadOnlyList<Counterparty> List(bool activeOnly);
        Counterparty Get(int id);
        Counterparty Add(Counterparty counterparty);
        Counterparty Update(Counterparty counterparty);

        /// <summary>
        /// Найти контрагента по ИНН. Возвращает <c>null</c>, если не найден или
        /// ИНН не задан. Полезно при импорте/дедуп проверке.
        /// </summary>
        Counterparty FindByInn(string inn);
    }
}
