using AhuErp.Core.Models;
using AhuErp.Core.Services;
using Xunit;

namespace AhuErp.Tests
{
    /// <summary>
    /// Granular permissions matrix должен:
    /// - давать Admin полный набор;
    /// - не давать TechSupport право регистрировать или удалять документы;
    /// - явно запрещать «не указанной» комбинации (роль/permission).
    /// </summary>
    public class PermissionsTests
    {
        [Fact]
        public void Admin_has_every_permission()
        {
            Assert.True(Permissions.Has(EmployeeRole.Admin, Permissions.DocumentRegister));
            Assert.True(Permissions.Has(EmployeeRole.Admin, Permissions.DocumentDelete));
            Assert.True(Permissions.Has(EmployeeRole.Admin, Permissions.WorkflowApprove));
            Assert.True(Permissions.Has(EmployeeRole.Admin, Permissions.AuditView));
            Assert.True(Permissions.Has(EmployeeRole.Admin, Permissions.AuditVerify));
            Assert.True(Permissions.Has(EmployeeRole.Admin, Permissions.NsiManage));
            Assert.True(Permissions.Has(EmployeeRole.Admin, Permissions.ReportExport));
        }

        [Fact]
        public void TechSupport_cannot_register_or_delete_documents()
        {
            Assert.False(Permissions.Has(EmployeeRole.TechSupport, Permissions.DocumentRegister));
            Assert.False(Permissions.Has(EmployeeRole.TechSupport, Permissions.DocumentDelete));
            Assert.False(Permissions.Has(EmployeeRole.TechSupport, Permissions.WorkflowApprove));
        }

        [Fact]
        public void TechSupport_can_read_documents_and_close_assigned_tasks()
        {
            Assert.True(Permissions.Has(EmployeeRole.TechSupport, Permissions.DocumentRead));
            Assert.True(Permissions.Has(EmployeeRole.TechSupport, Permissions.TaskComplete));
        }

        [Fact]
        public void Archivist_can_manage_NSI_but_warehouse_cannot()
        {
            Assert.True(Permissions.Has(EmployeeRole.Archivist, Permissions.NsiManage));
            Assert.False(Permissions.Has(EmployeeRole.WarehouseManager, Permissions.NsiManage));
        }

        [Fact]
        public void WarehouseManager_can_writeoff_inventory_and_book_fleet()
        {
            Assert.True(Permissions.Has(EmployeeRole.WarehouseManager, Permissions.InventoryWriteOff));
            Assert.True(Permissions.Has(EmployeeRole.WarehouseManager, Permissions.FleetBook));
            // ...но не имеет аудита и удаления документов
            Assert.False(Permissions.Has(EmployeeRole.WarehouseManager, Permissions.AuditView));
            Assert.False(Permissions.Has(EmployeeRole.WarehouseManager, Permissions.DocumentDelete));
        }

        [Fact]
        public void Unknown_permission_string_is_denied()
        {
            Assert.False(Permissions.Has(EmployeeRole.Admin, "Bogus.Permission"));
        }

        [Fact]
        public void ListFor_returns_defensive_copy_so_callers_cannot_mutate_global_state()
        {
            var snapshot1 = Permissions.ListFor(EmployeeRole.TechSupport);

            // Попытка через downcast к ICollection<string> добавить TechSupport
            // запрещённое право: должно либо бросить, либо НЕ повлиять на
            // следующий ListFor / Has — доступа к внутренней коллекции нет.
            try
            {
                ((System.Collections.Generic.ICollection<string>)snapshot1).Add(Permissions.DocumentDelete);
            }
            catch (System.NotSupportedException) { /* ok — read-only обёртка */ }

            Assert.False(Permissions.Has(EmployeeRole.TechSupport, Permissions.DocumentDelete));
            Assert.DoesNotContain(Permissions.DocumentDelete, Permissions.ListFor(EmployeeRole.TechSupport));
        }

        [Fact]
        public void ListFor_returns_consistent_set_with_Has()
        {
            foreach (EmployeeRole role in System.Enum.GetValues(typeof(EmployeeRole)))
            {
                var list = Permissions.ListFor(role);
                foreach (var p in list)
                {
                    Assert.True(Permissions.Has(role, p),
                        $"Has({role}, {p}) должно быть true, раз permission присутствует в ListFor.");
                }
            }
        }
    }
}
