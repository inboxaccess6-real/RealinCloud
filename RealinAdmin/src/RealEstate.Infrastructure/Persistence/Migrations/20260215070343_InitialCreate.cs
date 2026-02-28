using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RealEstate.Infrastructure.Persistence.Migrations
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
                    can_export = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    provider = table.Column<string>(type: "text", nullable: false),
                    oauth_provider_id = table.Column<string>(type: "text", nullable: true),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_blocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_blacklisted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    blacklist_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    blocked_by = table.Column<Guid>(type: "uuid", nullable: true),
                    blocked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.CheckConstraint("CK_User_ContactMethod", "email IS NOT NULL OR phone_number IS NOT NULL");
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
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    verification_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    verified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_proof_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    company_details = table.Column<string>(type: "jsonb", nullable: true, defaultValueSql: "'{}'"),
                    is_blocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_blacklisted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    blacklist_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    blocked_by = table.Column<Guid>(type: "uuid", nullable: true),
                    blocked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                name: "audit_logs",
                schema: "realin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    performed_by = table.Column<Guid>(type: "uuid", nullable: false),
                    details = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'"),
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

            migrationBuilder.CreateTable(
                name: "otp_sessions",
                schema: "realin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    otp_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    delivery_method = table.Column<string>(type: "text", nullable: false),
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
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    is_blocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_blacklisted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    blacklist_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    blocked_by = table.Column<Guid>(type: "uuid", nullable: true),
                    blocked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
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
                    is_blocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_blacklisted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    blacklist_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    blocked_by = table.Column<Guid>(type: "uuid", nullable: true),
                    blocked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
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
                    title = table.Column<string>(type: "text", nullable: false),
                    listing_type = table.Column<string>(type: "text", nullable: false),
                    listing_category = table.Column<string>(type: "text", nullable: false),
                    property_type = table.Column<string>(type: "text", nullable: false),
                    construction_status = table.Column<string>(type: "text", nullable: false),
                    rera_id = table.Column<string>(type: "text", nullable: true),
                    available_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    locality = table.Column<string>(type: "text", nullable: true),
                    city = table.Column<string>(type: "text", nullable: true),
                    pin_code = table.Column<string>(type: "text", nullable: true),
                    landmark = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    floor_number = table.Column<int>(type: "integer", nullable: true),
                    total_floors = table.Column<int>(type: "integer", nullable: true),
                    facing = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "text", nullable: false),
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
                    furnishing_status = table.Column<string>(type: "text", nullable: true),
                    property_age = table.Column<int>(type: "integer", nullable: true),
                    ownership_type = table.Column<string>(type: "text", nullable: true),
                    loan_available = table.Column<bool>(type: "boolean", nullable: true),
                    video_url = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "text", nullable: true),
                    amenities = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'"),
                    interior_features = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'"),
                    utilities = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'"),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    is_featured = table.Column<bool>(type: "boolean", nullable: false),
                    approval_status = table.Column<string>(type: "text", nullable: false),
                    rejection_reason = table.Column<string>(type: "text", nullable: true),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_flagged = table.Column<bool>(type: "boolean", nullable: false),
                    flag_reason = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
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
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
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
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "New"),
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
                    { new Guid("10000000-0000-0000-0000-000000000001"), "USER_MGMT", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Manage users, roles, and permissions", true, "User Management", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "PROPERTY_BROWSE", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Browse and search properties", true, "Property Browse", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "PROPERTY_MANAGE", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Create, update, and manage property listings", true, "Property Management", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "BOOKMARKS", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Save and manage favorite properties", true, "Bookmarks", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000005"), "ANALYTICS", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "View analytics and reports", true, "Analytics", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000006"), "SETTINGS", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Manage system settings and configurations", true, "Settings", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000007"), "BLACKLIST", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Manage blacklisted users and entities", true, "Blacklist", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000008"), "AUDIT_LOGS", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "View audit trail and activity logs", true, "Audit Logs", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000009"), "REPORTS", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Generate and view reports", true, "Reports", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-00000000000a"), "APPROVALS", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Manage approval workflows", true, "Approvals", new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                schema: "realin",
                table: "roles",
                columns: new[] { "id", "created_at", "description", "is_active", "name", "role_type", "updated_at" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Guest user with limited access", true, "Guest", 0, new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000002"), new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Registered user with standard access", true, "User", 1, new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000003"), new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Real estate agent with property management access", true, "Agent", 5, new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000004"), new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Administrator with elevated access", true, "Admin", 10, new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0000-000000000005"), new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Super administrator with full system access", true, "SuperAdmin", 100, new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_agents_license_number",
                schema: "realin",
                table: "agents",
                column: "license_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agents_status",
                schema: "realin",
                table: "agents",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_agents_user_id",
                schema: "realin",
                table: "agents",
                column: "user_id",
                unique: true);

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
                name: "IX_properties_approval_status",
                schema: "realin",
                table: "properties",
                column: "approval_status");

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
                name: "audit_logs",
                schema: "realin");

            migrationBuilder.DropTable(
                name: "favorites",
                schema: "realin");

            migrationBuilder.DropTable(
                name: "inquiries",
                schema: "realin");

            migrationBuilder.DropTable(
                name: "leads",
                schema: "realin");

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
