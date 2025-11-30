using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealinApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEngagementEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "favorites",
                schema: "realin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_favorites", x => x.id);
                    table.ForeignKey(
                        name: "FK_favorites_properties_property_id",
                        column: x => x.property_id,
                        principalSchema: "realin",
                        principalTable: "properties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_favorites_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "realin",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inquiries",
                schema: "realin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    contact_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    contact_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    preferred_contact_time = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    response = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    responded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inquiries", x => x.id);
                    table.ForeignKey(
                        name: "FK_inquiries_properties_property_id",
                        column: x => x.property_id,
                        principalSchema: "realin",
                        principalTable: "properties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_inquiries_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "realin",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "leads",
                schema: "realin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_agent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    contacted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    converted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leads", x => x.id);
                    table.ForeignKey(
                        name: "FK_leads_agents_assigned_agent_id",
                        column: x => x.assigned_agent_id,
                        principalSchema: "realin",
                        principalTable: "agents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_leads_properties_property_id",
                        column: x => x.property_id,
                        principalSchema: "realin",
                        principalTable: "properties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_leads_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "realin",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_favorites_property_id",
                schema: "realin",
                table: "favorites",
                column: "property_id");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_user_id",
                schema: "realin",
                table: "favorites",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_user_id_property_id",
                schema: "realin",
                table: "favorites",
                columns: new[] { "user_id", "property_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inquiries_created_at",
                schema: "realin",
                table: "inquiries",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_inquiries_property_id",
                schema: "realin",
                table: "inquiries",
                column: "property_id");

            migrationBuilder.CreateIndex(
                name: "IX_inquiries_status",
                schema: "realin",
                table: "inquiries",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_inquiries_user_id",
                schema: "realin",
                table: "inquiries",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_leads_assigned_agent_id",
                schema: "realin",
                table: "leads",
                column: "assigned_agent_id");

            migrationBuilder.CreateIndex(
                name: "IX_leads_created_at",
                schema: "realin",
                table: "leads",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_leads_property_id",
                schema: "realin",
                table: "leads",
                column: "property_id");

            migrationBuilder.CreateIndex(
                name: "IX_leads_status",
                schema: "realin",
                table: "leads",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_leads_user_id",
                schema: "realin",
                table: "leads",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "favorites",
                schema: "realin");

            migrationBuilder.DropTable(
                name: "inquiries",
                schema: "realin");

            migrationBuilder.DropTable(
                name: "leads",
                schema: "realin");
        }
    }
}
