using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RealinApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "realin");

            migrationBuilder.CreateTable(
                name: "modules",
                schema: "realin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "realin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_type = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "realin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    module_id = table.Column<Guid>(type: "uuid", nullable: false),
                    can_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    can_create = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    can_update = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    can_delete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    can_manage = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_role_permissions_modules_module_id",
                        column: x => x.module_id,
                        principalSchema: "realin",
                        principalTable: "modules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "realin",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "realin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    provider = table.Column<int>(type: "integer", nullable: false),
                    oauth_provider_id = table.Column<string>(type: "text", nullable: true),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "realin",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "agents",
                schema: "realin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    license_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    agency_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    experience_years = table.Column<int>(type: "integer", nullable: true),
                    rating = table.Column<decimal>(type: "numeric(2,1)", precision: 2, scale: 1, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agents", x => x.id);
                    table.ForeignKey(
                        name: "FK_agents_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "realin",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "otp_sessions",
                schema: "realin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    otp_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    delivery_method = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_otp_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_otp_sessions_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "realin",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "realin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false),
                    device_info = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "realin",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "builders",
                schema: "realin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    established_year = table.Column<int>(type: "integer", nullable: true),
                    registration_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    headquarters_address = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    created_by_agent_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_builders", x => x.id);
                    table.ForeignKey(
                        name: "FK_builders_agents_created_by_agent_id",
                        column: x => x.created_by_agent_id,
                        principalSchema: "realin",
                        principalTable: "agents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                schema: "realin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    rera_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    builder_id = table.Column<Guid>(type: "uuid", nullable: false),
                    address = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    locality = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    pin_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    landmark = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    construction_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    launch_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    possession_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    total_towers = table.Column<int>(type: "integer", nullable: true),
                    total_units = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    created_by_agent_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                    table.ForeignKey(
                        name: "FK_projects_agents_created_by_agent_id",
                        column: x => x.created_by_agent_id,
                        principalSchema: "realin",
                        principalTable: "agents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_projects_builders_builder_id",
                        column: x => x.builder_id,
                        principalSchema: "realin",
                        principalTable: "builders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "properties",
                schema: "realin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    listing_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    listing_category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    property_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    construction_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    rera_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    available_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "active"),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    address = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    locality = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    pin_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    landmark = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    floor_number = table.Column<int>(type: "integer", nullable: true),
                    total_floors = table.Column<int>(type: "integer", nullable: true),
                    facing = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "INR"),
                    monthly_rent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    security_deposit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    maintenance_charges = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    price_negotiable = table.Column<bool>(type: "boolean", nullable: true),
                    carpet_area = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    builtup_area = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    super_builtup_area = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    bedrooms = table.Column<int>(type: "integer", nullable: true),
                    bathrooms = table.Column<int>(type: "integer", nullable: true),
                    balconies = table.Column<int>(type: "integer", nullable: true),
                    furnishing_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    property_age = table.Column<int>(type: "integer", nullable: true),
                    ownership_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    loan_available = table.Column<bool>(type: "boolean", nullable: true),
                    video_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    amenities = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    interior_features = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    utilities = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_featured = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_properties", x => x.id);
                    table.ForeignKey(
                        name: "FK_properties_agents_agent_id",
                        column: x => x.agent_id,
                        principalSchema: "realin",
                        principalTable: "agents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_properties_projects_project_id",
                        column: x => x.project_id,
                        principalSchema: "realin",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "media",
                schema: "realin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    storage_bucket = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    storage_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media", x => x.id);
                    table.ForeignKey(
                        name: "FK_media_properties_property_id",
                        column: x => x.property_id,
                        principalSchema: "realin",
                        principalTable: "properties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "realin",
                table: "modules",
                columns: new[] { "id", "code", "created_at", "description", "is_active", "name", "updated_at" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "USER_MGMT", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Manage user accounts, roles, and permissions", true, "User Management", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "PROPERTY_BROWSE", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "View and search property listings (public access)", true, "Property Browsing", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "PROPERTY_MANAGE", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Create, edit, and manage property listings (agents only)", true, "Property Management", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "BOOKMARKS", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Save and manage favorite properties", true, "Bookmarks & Favorites", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000005"), "ANALYTICS", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "View analytics, reports, and business insights", true, "Analytics & Reporting", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000006"), "SETTINGS", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Configure system-wide settings and preferences", true, "System Settings", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                schema: "realin",
                table: "roles",
                columns: new[] { "id", "created_at", "description", "is_active", "name", "role_type", "updated_at" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Limited access for unverified users", true, "Guest", 0, new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000002"), new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Standard verified user with basic access", true, "User", 1, new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000003"), new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Real estate professional with property management capabilities", true, "Real Estate Agent", 5, new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000004"), new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "System administrator with elevated privileges", true, "Administrator", 10, new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000005"), new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Full system access with all privileges", true, "Super Administrator", 100, new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_agents_license_number",
                schema: "realin",
                table: "agents",
                column: "license_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agents_user_id",
                schema: "realin",
                table: "agents",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_builders_created_by_agent_id",
                schema: "realin",
                table: "builders",
                column: "created_by_agent_id");

            migrationBuilder.CreateIndex(
                name: "IX_builders_email",
                schema: "realin",
                table: "builders",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_builders_name",
                schema: "realin",
                table: "builders",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_media_property_id",
                schema: "realin",
                table: "media",
                column: "property_id");

            migrationBuilder.CreateIndex(
                name: "IX_modules_code",
                schema: "realin",
                table: "modules",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_modules_name",
                schema: "realin",
                table: "modules",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_otp_sessions_created_at",
                schema: "realin",
                table: "otp_sessions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_otp_sessions_email",
                schema: "realin",
                table: "otp_sessions",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_otp_sessions_phone_number",
                schema: "realin",
                table: "otp_sessions",
                column: "phone_number");

            migrationBuilder.CreateIndex(
                name: "IX_otp_sessions_user_id",
                schema: "realin",
                table: "otp_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_builder_id",
                schema: "realin",
                table: "projects",
                column: "builder_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_city",
                schema: "realin",
                table: "projects",
                column: "city");

            migrationBuilder.CreateIndex(
                name: "IX_projects_created_by_agent_id",
                schema: "realin",
                table: "projects",
                column: "created_by_agent_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_rera_id",
                schema: "realin",
                table: "projects",
                column: "rera_id");

            migrationBuilder.CreateIndex(
                name: "IX_properties_agent_id",
                schema: "realin",
                table: "properties",
                column: "agent_id");

            migrationBuilder.CreateIndex(
                name: "IX_properties_city",
                schema: "realin",
                table: "properties",
                column: "city");

            migrationBuilder.CreateIndex(
                name: "IX_properties_is_featured",
                schema: "realin",
                table: "properties",
                column: "is_featured");

            migrationBuilder.CreateIndex(
                name: "IX_properties_is_published",
                schema: "realin",
                table: "properties",
                column: "is_published");

            migrationBuilder.CreateIndex(
                name: "IX_properties_listing_category",
                schema: "realin",
                table: "properties",
                column: "listing_category");

            migrationBuilder.CreateIndex(
                name: "IX_properties_project_id",
                schema: "realin",
                table: "properties",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_properties_property_type",
                schema: "realin",
                table: "properties",
                column: "property_type");

            migrationBuilder.CreateIndex(
                name: "IX_properties_status",
                schema: "realin",
                table: "properties",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_token",
                schema: "realin",
                table: "refresh_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                schema: "realin",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_module_id",
                schema: "realin",
                table: "role_permissions",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_role_id",
                schema: "realin",
                table: "role_permissions",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_role_id_module_id",
                schema: "realin",
                table: "role_permissions",
                columns: new[] { "role_id", "module_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_name",
                schema: "realin",
                table: "roles",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_roles_role_type",
                schema: "realin",
                table: "roles",
                column: "role_type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                schema: "realin",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_phone_number",
                schema: "realin",
                table: "users",
                column: "phone_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_provider_oauth_provider_id",
                schema: "realin",
                table: "users",
                columns: new[] { "provider", "oauth_provider_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_role_id",
                schema: "realin",
                table: "users",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media",
                schema: "realin");

            migrationBuilder.DropTable(
                name: "otp_sessions",
                schema: "realin");

            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "realin");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "realin");

            migrationBuilder.DropTable(
                name: "properties",
                schema: "realin");

            migrationBuilder.DropTable(
                name: "modules",
                schema: "realin");

            migrationBuilder.DropTable(
                name: "projects",
                schema: "realin");

            migrationBuilder.DropTable(
                name: "builders",
                schema: "realin");

            migrationBuilder.DropTable(
                name: "agents",
                schema: "realin");

            migrationBuilder.DropTable(
                name: "users",
                schema: "realin");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "realin");
        }
    }
}
