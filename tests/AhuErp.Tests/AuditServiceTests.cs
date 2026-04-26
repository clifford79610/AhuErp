using System.Linq;
using AhuErp.Core.Models;
using AhuErp.Core.Services;
using Xunit;

namespace AhuErp.Tests
{
    /// <summary>
    /// Юнит-тесты <see cref="AuditService"/>: проверяем, что
    /// hash-цепочка корректно строится и что её нарушение детектируется
    /// <see cref="IAuditService.VerifyChain"/>.
    /// </summary>
    public class AuditServiceTests
    {
        private readonly InMemoryAuditLogRepository _repo = new InMemoryAuditLogRepository();
        private readonly AuditService _audit;

        public AuditServiceTests()
        {
            _audit = new AuditService(_repo);
        }

        [Fact]
        public void Record_assigns_hash_and_links_to_previous()
        {
            var first = _audit.Record(AuditActionType.Created, "Document", 1, 10, newValues: "title=A");
            var second = _audit.Record(AuditActionType.Updated, "Document", 1, 10, newValues: "title=B");

            Assert.False(string.IsNullOrEmpty(first.Hash));
            Assert.Null(first.PreviousHash);
            Assert.Equal(first.Hash, second.PreviousHash);
            Assert.NotEqual(first.Hash, second.Hash);
        }

        [Fact]
        public void VerifyChain_returns_null_for_intact_chain()
        {
            _audit.Record(AuditActionType.Created, "Document", 1, 10);
            _audit.Record(AuditActionType.Updated, "Document", 1, 10);
            _audit.Record(AuditActionType.StatusChanged, "Document", 1, 10);

            Assert.Null(_audit.VerifyChain());
        }

        [Fact]
        public void VerifyChain_detects_tampering()
        {
            _audit.Record(AuditActionType.Created, "Document", 1, 10);
            var second = _audit.Record(AuditActionType.Updated, "Document", 1, 10, newValues: "title=B");
            _audit.Record(AuditActionType.StatusChanged, "Document", 1, 10);

            // Имитируем подделку: меняем NewValues задним числом.
            second.NewValues = "title=HACKED";

            var corrupted = _audit.VerifyChain();
            Assert.NotNull(corrupted);
            Assert.Equal(second.Id, corrupted.Id);
        }

        [Fact]
        public void Query_filters_by_user_and_entity()
        {
            _audit.Record(AuditActionType.Created, "Document", 1, 10);
            _audit.Record(AuditActionType.Created, "Document", 2, 11);
            _audit.Record(AuditActionType.Created, "Other", 3, 10);

            var byUser = _audit.Query(new AuditQueryFilter { UserId = 10 });
            Assert.Equal(2, byUser.Count);

            var byEntity = _audit.Query(new AuditQueryFilter { EntityType = "Document" });
            Assert.Equal(2, byEntity.Count);

            var both = _audit.Query(new AuditQueryFilter { UserId = 10, EntityType = "Document" });
            Assert.Single(both);
            Assert.Equal(1, both[0].EntityId);
        }
    }
}
