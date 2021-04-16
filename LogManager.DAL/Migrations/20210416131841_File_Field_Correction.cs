using Microsoft.EntityFrameworkCore.Migrations;

namespace LogManager.DAL.Migrations
{
    public partial class File_Field_Correction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "FileInfos");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "FileInfos",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
