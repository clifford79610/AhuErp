using System;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// Сервис обработки архивных запросов.
    /// </summary>
    public interface IArchiveService
    {
        ArchiveRequest CreateRequest(
            string title,
            DateTime creationDate,
            int? assignedEmployeeId = null,
            ArchiveRequestKind requestKind = ArchiveRequestKind.SocialLegal);

        /// <exception cref="InvalidOperationException">Пакет документов неполон.</exception>
        void CompleteRequest(ArchiveRequest request);
    }
}
