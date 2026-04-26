using System.Collections.Generic;
using System.Linq;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>In-memory репозиторий вложений для тестов.</summary>
    public sealed class InMemoryAttachmentRepository : IAttachmentRepository
    {
        private readonly List<DocumentAttachment> _attachments = new List<DocumentAttachment>();
        private int _nextId = 1;

        public DocumentAttachment GetById(int id) => _attachments.FirstOrDefault(a => a.Id == id);

        public IReadOnlyList<DocumentAttachment> ListByDocument(int documentId)
            => _attachments.Where(a => a.DocumentId == documentId)
                           .OrderBy(a => a.AttachmentGroupId)
                           .ThenByDescending(a => a.VersionNumber)
                           .ToList()
                           .AsReadOnly();

        public IReadOnlyList<DocumentAttachment> ListByGroup(int attachmentGroupId)
            => _attachments.Where(a => a.AttachmentGroupId == attachmentGroupId)
                           .OrderBy(a => a.VersionNumber)
                           .ToList()
                           .AsReadOnly();

        public DocumentAttachment GetCurrentByGroup(int attachmentGroupId)
            => _attachments.FirstOrDefault(a => a.AttachmentGroupId == attachmentGroupId && a.IsCurrentVersion);

        public int GetMaxVersionInGroup(int attachmentGroupId)
        {
            var versions = _attachments.Where(a => a.AttachmentGroupId == attachmentGroupId);
            return versions.Any() ? versions.Max(a => a.VersionNumber) : 0;
        }

        public DocumentAttachment Add(DocumentAttachment attachment)
        {
            if (attachment.Id == 0) attachment.Id = _nextId++;
            else _nextId = System.Math.Max(_nextId, attachment.Id + 1);
            _attachments.Add(attachment);
            return attachment;
        }

        public void Update(DocumentAttachment attachment)
        {
            var idx = _attachments.FindIndex(a => a.Id == attachment.Id);
            if (idx < 0) return;
            _attachments[idx] = attachment;
        }
    }
}
