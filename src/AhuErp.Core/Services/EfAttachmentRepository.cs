using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AhuErp.Core.Data;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>EF6-репозиторий вложений.</summary>
    public sealed class EfAttachmentRepository : IAttachmentRepository
    {
        private readonly AhuDbContext _ctx;

        public EfAttachmentRepository(AhuDbContext ctx)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        }

        public DocumentAttachment GetById(int id) => _ctx.DocumentAttachments.Find(id);

        public IReadOnlyList<DocumentAttachment> ListByDocument(int documentId)
            => _ctx.DocumentAttachments
                .Where(a => a.DocumentId == documentId)
                .OrderBy(a => a.AttachmentGroupId)
                .ThenByDescending(a => a.VersionNumber)
                .ToList()
                .AsReadOnly();

        public IReadOnlyList<DocumentAttachment> ListByGroup(int attachmentGroupId)
            => _ctx.DocumentAttachments
                .Where(a => a.AttachmentGroupId == attachmentGroupId)
                .OrderBy(a => a.VersionNumber)
                .ToList()
                .AsReadOnly();

        public DocumentAttachment GetCurrentByGroup(int attachmentGroupId)
            => _ctx.DocumentAttachments
                .FirstOrDefault(a => a.AttachmentGroupId == attachmentGroupId && a.IsCurrentVersion);

        public int GetMaxVersionInGroup(int attachmentGroupId)
        {
            var versions = _ctx.DocumentAttachments.Where(a => a.AttachmentGroupId == attachmentGroupId);
            return versions.Any() ? versions.Max(a => a.VersionNumber) : 0;
        }

        public DocumentAttachment Add(DocumentAttachment attachment)
        {
            _ctx.DocumentAttachments.Add(attachment);
            _ctx.SaveChanges();
            return attachment;
        }

        public void Update(DocumentAttachment attachment)
        {
            if (_ctx.Entry(attachment).State == EntityState.Detached)
            {
                _ctx.DocumentAttachments.Attach(attachment);
                _ctx.Entry(attachment).State = EntityState.Modified;
            }
            _ctx.SaveChanges();
        }
    }
}
