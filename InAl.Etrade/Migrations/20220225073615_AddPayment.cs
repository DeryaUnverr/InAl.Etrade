using Microsoft.EntityFrameworkCore.Migrations;

namespace InAl.Etrade.Migrations
{
    public partial class AddPayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CartName",
                table: "OrderHeader",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CartNumber",
                table: "OrderHeader",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Cvc",
                table: "OrderHeader",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExprationMonth",
                table: "OrderHeader",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExprationYear",
                table: "OrderHeader",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CartName",
                table: "OrderHeader");

            migrationBuilder.DropColumn(
                name: "CartNumber",
                table: "OrderHeader");

            migrationBuilder.DropColumn(
                name: "Cvc",
                table: "OrderHeader");

            migrationBuilder.DropColumn(
                name: "ExprationMonth",
                table: "OrderHeader");

            migrationBuilder.DropColumn(
                name: "ExprationYear",
                table: "OrderHeader");
        }
    }
}
