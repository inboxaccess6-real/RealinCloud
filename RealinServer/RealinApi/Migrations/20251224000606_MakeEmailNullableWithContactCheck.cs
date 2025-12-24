using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealinApi.Migrations
{
    /// <inheritdoc />
    public partial class MakeEmailNullableWithContactCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "email",
                schema: "realin",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddCheckConstraint(
                name: "CK_User_ContactMethod",
                schema: "realin",
                table: "users",
                sql: "email IS NOT NULL OR phone_number IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_User_ContactMethod",
                schema: "realin",
                table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                schema: "realin",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
