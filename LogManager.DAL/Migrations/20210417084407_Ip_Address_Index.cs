using Microsoft.EntityFrameworkCore.Migrations;

namespace LogManager.DAL.Migrations
{
    public partial class Ip_Address_Index : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_IpInfos_Address",
                table: "IpInfos",
                column: "Address",
                unique: true,
                filter: "[Address] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IpInfos_Address",
                table: "IpInfos");
        }
    }
}
