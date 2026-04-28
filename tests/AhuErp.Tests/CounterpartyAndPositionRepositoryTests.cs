using System;
using System.Linq;
using AhuErp.Core.Models;
using AhuErp.Core.Services;
using Xunit;

namespace AhuErp.Tests
{
    /// <summary>
    /// Контрактные тесты для in-memory справочников НСИ
    /// (<see cref="ICounterpartyRepository"/>, <see cref="IPositionRepository"/>).
    /// </summary>
    public class CounterpartyAndPositionRepositoryTests
    {
        // ── Counterparty ─────────────────────────────────────────────────

        [Fact]
        public void Counterparty_Add_assigns_id_and_lists_alphabetically()
        {
            var repo = new InMemoryCounterpartyRepository();
            repo.Add(new Counterparty { ShortName = "Бета", IsActive = true });
            repo.Add(new Counterparty { ShortName = "Альфа", IsActive = true });

            var list = repo.List(activeOnly: true);
            Assert.Equal(new[] { "Альфа", "Бета" }, list.Select(c => c.ShortName).ToArray());
            Assert.All(list, c => Assert.True(c.Id > 0));
        }

        [Fact]
        public void Counterparty_List_filters_inactive_when_activeOnly_true()
        {
            var repo = new InMemoryCounterpartyRepository();
            repo.Add(new Counterparty { ShortName = "Активный", IsActive = true });
            repo.Add(new Counterparty { ShortName = "Архивный", IsActive = false });

            Assert.Single(repo.List(activeOnly: true));
            Assert.Equal(2, repo.List(activeOnly: false).Count);
        }

        [Fact]
        public void Counterparty_Add_rejects_duplicate_inn()
        {
            var repo = new InMemoryCounterpartyRepository();
            repo.Add(new Counterparty { ShortName = "Один", Inn = "7701234567", IsActive = true });

            Assert.Throws<InvalidOperationException>(() =>
                repo.Add(new Counterparty { ShortName = "Другой", Inn = "7701234567", IsActive = true }));
        }

        [Fact]
        public void Counterparty_Add_allows_empty_inn_for_multiple_entities()
        {
            // ИНН необязателен (например, для физлиц без ИНН).
            var repo = new InMemoryCounterpartyRepository();
            repo.Add(new Counterparty { ShortName = "Без ИНН 1", IsActive = true });
            repo.Add(new Counterparty { ShortName = "Без ИНН 2", IsActive = true });
            Assert.Equal(2, repo.List(activeOnly: false).Count);
        }

        [Fact]
        public void Counterparty_FindByInn_returns_null_for_blank_or_unknown()
        {
            var repo = new InMemoryCounterpartyRepository();
            repo.Add(new Counterparty { ShortName = "X", Inn = "1234567890", IsActive = true });
            Assert.Null(repo.FindByInn(null));
            Assert.Null(repo.FindByInn(""));
            Assert.Null(repo.FindByInn("9999999999"));
            Assert.NotNull(repo.FindByInn("1234567890"));
        }

        [Fact]
        public void Counterparty_Update_changes_existing_record()
        {
            var repo = new InMemoryCounterpartyRepository();
            var c = repo.Add(new Counterparty { ShortName = "Старое", IsActive = true });
            var updated = new Counterparty
            {
                Id = c.Id,
                ShortName = "Новое",
                FullName = "Полное наименование",
                Address = "Москва, Тверская, 1",
                IsActive = true
            };

            repo.Update(updated);
            var got = repo.Get(c.Id);
            Assert.Equal("Новое", got.ShortName);
            Assert.Equal("Полное наименование", got.FullName);
            Assert.Equal("Москва, Тверская, 1", got.Address);
        }

        [Fact]
        public void Counterparty_Add_rejects_blank_short_name()
        {
            var repo = new InMemoryCounterpartyRepository();
            Assert.Throws<ArgumentException>(() =>
                repo.Add(new Counterparty { ShortName = "  ", IsActive = true }));
        }

        // ── Position ─────────────────────────────────────────────────────

        [Fact]
        public void Position_Add_and_List_orderedByName()
        {
            var repo = new InMemoryPositionRepository();
            repo.Add(new Position { Name = "Специалист", Category = "Специалист", IsActive = true });
            repo.Add(new Position { Name = "Главный специалист", Category = "Специалист", IsActive = true });
            repo.Add(new Position { Name = "Архивариус", Category = "Специалист", IsActive = false });

            var active = repo.List(activeOnly: true);
            Assert.Equal(new[] { "Главный специалист", "Специалист" },
                active.Select(p => p.Name).ToArray());
        }

        [Fact]
        public void Position_Update_changes_existing()
        {
            var repo = new InMemoryPositionRepository();
            var p = repo.Add(new Position { Name = "Старое", IsActive = true });
            repo.Update(new Position { Id = p.Id, Name = "Новое", Category = "Руководитель", IsActive = false });

            var got = repo.Get(p.Id);
            Assert.Equal("Новое", got.Name);
            Assert.Equal("Руководитель", got.Category);
            Assert.False(got.IsActive);
        }

        [Fact]
        public void Position_Update_throws_when_not_found()
        {
            var repo = new InMemoryPositionRepository();
            Assert.Throws<InvalidOperationException>(() =>
                repo.Update(new Position { Id = 999, Name = "X" }));
        }

        [Fact]
        public void Position_Add_rejects_blank_name()
        {
            var repo = new InMemoryPositionRepository();
            Assert.Throws<ArgumentException>(() =>
                repo.Add(new Position { Name = "", IsActive = true }));
        }
    }
}
