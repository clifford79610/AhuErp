using System;
using System.IO;
using System.Linq;
using System.Text;
using AhuErp.Core.Models;
using AhuErp.Core.Services;
using Xunit;

namespace AhuErp.Tests
{
    /// <summary>
    /// <see cref="AttachmentService"/>: версионирование, корректное снятие
    /// флага <see cref="DocumentAttachment.IsCurrentVersion"/>, аудит.
    /// </summary>
    public class AttachmentServiceTests
    {
        private readonly InMemoryDocumentRepository _docs = new InMemoryDocumentRepository();
        private readonly InMemoryAttachmentRepository _attRepo = new InMemoryAttachmentRepository();
        private readonly InMemoryFileStorageService _storage = new InMemoryFileStorageService();
        private readonly InMemoryAuditLogRepository _auditRepo = new InMemoryAuditLogRepository();
        private readonly AttachmentService _service;
        private readonly Document _doc;

        public AttachmentServiceTests()
        {
            var audit = new AuditService(_auditRepo);
            _service = new AttachmentService(_attRepo, _docs, _storage, audit);
            _doc = new Document
            {
                Title = "Письмо",
                Type = DocumentType.Office,
                CreationDate = DateTime.Now,
                Deadline = DateTime.Now.AddDays(7),
                RegistrationNumber = "ИСХ-01-01/2026-00001"
            };
            _docs.Add(_doc);
        }

        private static MemoryStream MakeStream(string text) =>
            new MemoryStream(Encoding.UTF8.GetBytes(text));

        [Fact]
        public void Upload_creates_first_version_with_group_equal_to_id()
        {
            var att = _service.Upload(_doc.Id, MakeStream("hello"), "draft.docx",
                uploadedById: 1);

            Assert.Equal(1, att.VersionNumber);
            Assert.True(att.IsCurrentVersion);
            Assert.Equal(att.Id, att.AttachmentGroupId);
            Assert.NotNull(att.Hash);
            Assert.True(att.SizeBytes > 0);
        }

        [Fact]
        public void AddVersion_marks_previous_as_not_current_and_increments_version()
        {
            var v1 = _service.Upload(_doc.Id, MakeStream("v1"), "letter.docx", uploadedById: 1);
            var v2 = _service.AddVersion(v1.AttachmentGroupId, MakeStream("v2-content"),
                "letter.docx", uploadedById: 1, comment: "Учли замечания");

            Assert.Equal(2, v2.VersionNumber);
            Assert.True(v2.IsCurrentVersion);
            var prev = _attRepo.GetById(v1.Id);
            Assert.False(prev.IsCurrentVersion);

            var versions = _service.ListVersions(v1.AttachmentGroupId);
            Assert.Equal(2, versions.Count);
        }

        [Fact]
        public void Open_logs_view_event()
        {
            var att = _service.Upload(_doc.Id, MakeStream("data"), "f.txt", 1);
            using (var s = _service.Open(att.Id, viewedById: 7))
            {
                Assert.NotNull(s);
            }

            var logs = _auditRepo.Query(new AuditQueryFilter { ActionType = AuditActionType.AttachmentViewed });
            Assert.Single(logs);
        }

        [Fact]
        public void Upload_throws_when_document_missing()
        {
            Assert.Throws<InvalidOperationException>(() =>
                _service.Upload(999, MakeStream("x"), "f.txt", 1));
        }

        [Fact]
        public void AddVersion_computes_distinct_hash_per_content()
        {
            var v1 = _service.Upload(_doc.Id, MakeStream("hello"), "f.txt", 1);
            var v2 = _service.AddVersion(v1.AttachmentGroupId, MakeStream("hello-changed"),
                "f.txt", 1);
            Assert.NotEqual(v1.Hash, v2.Hash);
        }
    }
}
