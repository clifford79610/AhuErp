using System.Collections.Generic;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>Доступ к данным вложений документов.</summary>
    public interface IAttachmentRepository
    {
        DocumentAttachment GetById(int id);
        IReadOnlyList<DocumentAttachment> ListByDocument(int documentId);
        IReadOnlyList<DocumentAttachment> ListByGroup(int attachmentGroupId);
        DocumentAttachment GetCurrentByGroup(int attachmentGroupId);
        int GetMaxVersionInGroup(int attachmentGroupId);
        DocumentAttachment Add(DocumentAttachment attachment);
        void Update(DocumentAttachment attachment);
    }
}
