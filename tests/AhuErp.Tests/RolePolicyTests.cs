using AhuErp.Core.Models;
using AhuErp.Core.Services;
using Xunit;

namespace AhuErp.Tests
{
    public class RolePolicyTests
    {
        [Theory]
        [InlineData(EmployeeRole.Admin, RolePolicy.Dashboard, true)]
        [InlineData(EmployeeRole.Admin, RolePolicy.Warehouse, true)]
        [InlineData(EmployeeRole.Manager, RolePolicy.ItService, true)]
        [InlineData(EmployeeRole.Manager, RolePolicy.Archive, true)]
        [InlineData(EmployeeRole.Archivist, RolePolicy.Archive, true)]
        [InlineData(EmployeeRole.Archivist, RolePolicy.Dashboard, true)]
        [InlineData(EmployeeRole.Archivist, RolePolicy.Fleet, false)]
        [InlineData(EmployeeRole.Archivist, RolePolicy.ItService, false)]
        [InlineData(EmployeeRole.TechSupport, RolePolicy.ItService, true)]
        [InlineData(EmployeeRole.TechSupport, RolePolicy.Archive, false)]
        [InlineData(EmployeeRole.TechSupport, RolePolicy.Fleet, false)]
        [InlineData(EmployeeRole.WarehouseManager, RolePolicy.Fleet, true)]
        [InlineData(EmployeeRole.WarehouseManager, RolePolicy.Warehouse, true)]
        [InlineData(EmployeeRole.WarehouseManager, RolePolicy.Office, true)]
        [InlineData(EmployeeRole.WarehouseManager, RolePolicy.ItService, false)]
        [InlineData(EmployeeRole.WarehouseManager, RolePolicy.Archive, false)]
        public void IsAllowed_matches_specified_matrix(EmployeeRole role, string module, bool expected)
        {
            Assert.Equal(expected, RolePolicy.IsAllowed(role, module));
        }

        [Fact]
        public void IsAllowed_returns_false_for_unknown_module()
        {
            Assert.False(RolePolicy.IsAllowed(EmployeeRole.Admin, "NotAModule"));
        }
    }
}
