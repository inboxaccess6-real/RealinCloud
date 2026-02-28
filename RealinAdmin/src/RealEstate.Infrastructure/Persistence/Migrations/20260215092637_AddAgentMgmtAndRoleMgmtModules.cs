using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RealEstate.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentMgmtAndRoleMgmtModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "realin",
                table: "modules",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "code",
                value: "PROPERTY_MGMT");

            migrationBuilder.InsertData(
                schema: "realin",
                table: "modules",
                columns: new[] { "id", "code", "created_at", "description", "is_active", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-00000000000b"), "AGENT_MGMT", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Manage agents, approvals, and agent profiles", true, "Agent Management", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-00000000000c"), "ROLE_MGMT", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Manage roles and permission assignments", true, "Role Management", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "realin",
                table: "modules",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-00000000000b"));

            migrationBuilder.DeleteData(
                schema: "realin",
                table: "modules",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-00000000000c"));

            migrationBuilder.UpdateData(
                schema: "realin",
                table: "modules",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "code",
                value: "PROPERTY_MANAGE");
        }
    }
}
