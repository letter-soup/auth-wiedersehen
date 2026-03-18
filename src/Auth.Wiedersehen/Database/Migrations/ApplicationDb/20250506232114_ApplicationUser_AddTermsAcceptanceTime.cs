#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Auth.Wiedersehen.Database.Migrations.ApplicationDb;

/// <inheritdoc />
public partial class ApplicationUser_AddTermsAcceptanceTime : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<DateTime>(
			"TermsAcceptanceTime",
			"AspNetUsers",
			"timestamp with time zone",
			nullable: false,
			defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
		);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
			"TermsAcceptanceTime",
			"AspNetUsers"
		);
	}
}
