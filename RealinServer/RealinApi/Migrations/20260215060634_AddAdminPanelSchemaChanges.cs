using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RealinApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminPanelSchemaChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "blacklist_reason",
                schema: "realin",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "blocked_at",
                schema: "realin",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "blocked_by",
                schema: "realin",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "realin",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by",
                schema: "realin",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_blacklisted",
                schema: "realin",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_blocked",
                schema: "realin",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "realin",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "can_export",
                schema: "realin",
                table: "role_permissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "approval_status",
                schema: "realin",
                table: "properties",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "draft");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "realin",
                table: "properties",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "deleted_by",
                schema: "realin",
                table: "properties",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "flag_reason",
                schema: "realin",
                table: "properties",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "realin",
                table: "properties",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_flagged",
                schema: "realin",
                table: "properties",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "rejection_reason",
                schema: "realin",
                table: "properties",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "reviewed_at",
                schema: "realin",
                table: "properties",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "reviewed_by",
                schema: "realin",
                table: "properties",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "submitted_at",
                schema: "realin",
                table: "properties",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "blacklist_reason",
                schema: "realin",
                table: "projects",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "blocked_at",
                schema: "realin",
                table: "projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "blocked_by",
                schema: "realin",
                table: "projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "realin",
                table: "projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_blacklisted",
                schema: "realin",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_blocked",
                schema: "realin",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "realin",
                table: "projects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "realin",
                table: "projects",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<string>(
                name: "blacklist_reason",
                schema: "realin",
                table: "builders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "blocked_at",
                schema: "realin",
                table: "builders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "blocked_by",
                schema: "realin",
                table: "builders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "realin",
                table: "builders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_blacklisted",
                schema: "realin",
                table: "builders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_blocked",
                schema: "realin",
                table: "builders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "realin",
                table: "builders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                schema: "realin",
                table: "builders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<string>(
                name: "blacklist_reason",
                schema: "realin",
                table: "agents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "blocked_at",
                schema: "realin",
                table: "agents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "blocked_by",
                schema: "realin",
                table: "agents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_details",
                schema: "realin",
                table: "agents",
                type: "jsonb",
                nullable: true,
                defaultValue: "{}");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "realin",
                table: "agents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "id_proof_url",
                schema: "realin",
                table: "agents",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_blacklisted",
                schema: "realin",
                table: "agents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_blocked",
                schema: "realin",
                table: "agents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "realin",
                table: "agents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "status",
                schema: "realin",
                table: "agents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "pending");

            migrationBuilder.AddColumn<string>(
                name: "verification_notes",
                schema: "realin",
                table: "agents",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "verified_at",
                schema: "realin",
                table: "agents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "verified_by",
                schema: "realin",
                table: "agents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "realin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    performed_by = table.Column<Guid>(type: "uuid", nullable: false),
                    details = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_audit_logs_users_performed_by",
                        column: x => x.performed_by,
                        principalSchema: "realin",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "realin",
                table: "modules",
                columns: new[] { "id", "code", "created_at", "description", "is_active", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000007"), "BLACKLIST", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Manage blacklisted entities (users, agents, builders)", true, "Blacklist Management", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000008"), "AUDIT_LOGS", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "View system audit logs and activity trail", true, "Audit Logs", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000009"), "REPORTS", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Generate and export reports", true, "Reports & Export", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-00000000000a"), "APPROVALS", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Manage approval workflows for agents and properties", true, "Approvals", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_properties_approval_status",
                schema: "realin",
                table: "properties",
                column: "approval_status");

            migrationBuilder.CreateIndex(
                name: "IX_agents_status",
                schema: "realin",
                table: "agents",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_action",
                schema: "realin",
                table: "audit_logs",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_created_at",
                schema: "realin",
                table: "audit_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_entity_type_entity_id",
                schema: "realin",
                table: "audit_logs",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_performed_by",
                schema: "realin",
                table: "audit_logs",
                column: "performed_by");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "realin");

            migrationBuilder.DropIndex(
                name: "IX_properties_approval_status",
                schema: "realin",
                table: "properties");

            migrationBuilder.DropIndex(
                name: "IX_agents_status",
                schema: "realin",
                table: "agents");

            migrationBuilder.DeleteData(
                schema: "realin",
                table: "modules",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                schema: "realin",
                table: "modules",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                schema: "realin",
                table: "modules",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                schema: "realin",
                table: "modules",
                keyColumn: "id",
                keyValue: new Guid("10000000-0000-0000-0000-00000000000a"));

            migrationBuilder.DropColumn(
                name: "blacklist_reason",
                schema: "realin",
                table: "users");

            migrationBuilder.DropColumn(
                name: "blocked_at",
                schema: "realin",
                table: "users");

            migrationBuilder.DropColumn(
                name: "blocked_by",
                schema: "realin",
                table: "users");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "realin",
                table: "users");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                schema: "realin",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_blacklisted",
                schema: "realin",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_blocked",
                schema: "realin",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "realin",
                table: "users");

            migrationBuilder.DropColumn(
                name: "can_export",
                schema: "realin",
                table: "role_permissions");

            migrationBuilder.DropColumn(
                name: "approval_status",
                schema: "realin",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "realin",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                schema: "realin",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "flag_reason",
                schema: "realin",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "realin",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "is_flagged",
                schema: "realin",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "rejection_reason",
                schema: "realin",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "reviewed_at",
                schema: "realin",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "reviewed_by",
                schema: "realin",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "submitted_at",
                schema: "realin",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "blacklist_reason",
                schema: "realin",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "blocked_at",
                schema: "realin",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "blocked_by",
                schema: "realin",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "realin",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "is_blacklisted",
                schema: "realin",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "is_blocked",
                schema: "realin",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "realin",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "realin",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "blacklist_reason",
                schema: "realin",
                table: "builders");

            migrationBuilder.DropColumn(
                name: "blocked_at",
                schema: "realin",
                table: "builders");

            migrationBuilder.DropColumn(
                name: "blocked_by",
                schema: "realin",
                table: "builders");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "realin",
                table: "builders");

            migrationBuilder.DropColumn(
                name: "is_blacklisted",
                schema: "realin",
                table: "builders");

            migrationBuilder.DropColumn(
                name: "is_blocked",
                schema: "realin",
                table: "builders");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "realin",
                table: "builders");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "realin",
                table: "builders");

            migrationBuilder.DropColumn(
                name: "blacklist_reason",
                schema: "realin",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "blocked_at",
                schema: "realin",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "blocked_by",
                schema: "realin",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "company_details",
                schema: "realin",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "realin",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "id_proof_url",
                schema: "realin",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "is_blacklisted",
                schema: "realin",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "is_blocked",
                schema: "realin",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "realin",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "realin",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "verification_notes",
                schema: "realin",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "verified_at",
                schema: "realin",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "verified_by",
                schema: "realin",
                table: "agents");
        }
    }
}
