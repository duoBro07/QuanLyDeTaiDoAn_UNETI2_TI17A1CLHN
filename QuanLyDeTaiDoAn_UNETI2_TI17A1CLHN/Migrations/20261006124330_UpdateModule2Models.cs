using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDeTaiDoAn.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModule2Models : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeTais_GiangViens_GiangVienMaGV",
                table: "DeTais");

            migrationBuilder.DropColumn(
                name: "LinhVuc",
                table: "GiangViens");

            migrationBuilder.DropColumn(
                name: "NgayVaoTruong",
                table: "GiangViens");

            migrationBuilder.DropColumn(
                name: "LinhVuc",
                table: "DeTais");

            migrationBuilder.DropColumn(
                name: "MaGV",
                table: "DeTais");

            migrationBuilder.RenameColumn(
                name: "SoSinhVienToiDa",
                table: "GiangViens",
                newName: "SoSinhVienHuongDanToiDa");

            migrationBuilder.RenameColumn(
                name: "MaGV",
                table: "GiangViens",
                newName: "MaGiangVien");

            migrationBuilder.RenameColumn(
                name: "SoSinhVienDaDangKy",
                table: "DeTais",
                newName: "MaLinhVuc");

            migrationBuilder.RenameColumn(
                name: "GiangVienMaGV",
                table: "DeTais",
                newName: "LinhVucMaLinhVuc");

            migrationBuilder.RenameIndex(
                name: "IX_DeTais_GiangVienMaGV",
                table: "DeTais",
                newName: "IX_DeTais_LinhVucMaLinhVuc");

            migrationBuilder.AlterColumn<string>(
                name: "SoDienThoai",
                table: "GiangViens",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "HocVi",
                table: "GiangViens",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "HoTen",
                table: "GiangViens",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "GiangViens",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "LinhVucMaLinhVuc",
                table: "GiangViens",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaLinhVuc",
                table: "GiangViens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "TenDeTai",
                table: "DeTais",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NamHoc",
                table: "DeTais",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "LinhVucs",
                columns: table => new
                {
                    MaLinhVuc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLinhVuc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinhVucs", x => x.MaLinhVuc);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GiangViens_LinhVucMaLinhVuc",
                table: "GiangViens",
                column: "LinhVucMaLinhVuc");

            migrationBuilder.AddForeignKey(
                name: "FK_DeTais_LinhVucs_LinhVucMaLinhVuc",
                table: "DeTais",
                column: "LinhVucMaLinhVuc",
                principalTable: "LinhVucs",
                principalColumn: "MaLinhVuc");

            migrationBuilder.AddForeignKey(
                name: "FK_GiangViens_LinhVucs_LinhVucMaLinhVuc",
                table: "GiangViens",
                column: "LinhVucMaLinhVuc",
                principalTable: "LinhVucs",
                principalColumn: "MaLinhVuc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeTais_LinhVucs_LinhVucMaLinhVuc",
                table: "DeTais");

            migrationBuilder.DropForeignKey(
                name: "FK_GiangViens_LinhVucs_LinhVucMaLinhVuc",
                table: "GiangViens");

            migrationBuilder.DropTable(
                name: "LinhVucs");

            migrationBuilder.DropIndex(
                name: "IX_GiangViens_LinhVucMaLinhVuc",
                table: "GiangViens");

            migrationBuilder.DropColumn(
                name: "LinhVucMaLinhVuc",
                table: "GiangViens");

            migrationBuilder.DropColumn(
                name: "MaLinhVuc",
                table: "GiangViens");

            migrationBuilder.RenameColumn(
                name: "SoSinhVienHuongDanToiDa",
                table: "GiangViens",
                newName: "SoSinhVienToiDa");

            migrationBuilder.RenameColumn(
                name: "MaGiangVien",
                table: "GiangViens",
                newName: "MaGV");

            migrationBuilder.RenameColumn(
                name: "MaLinhVuc",
                table: "DeTais",
                newName: "SoSinhVienDaDangKy");

            migrationBuilder.RenameColumn(
                name: "LinhVucMaLinhVuc",
                table: "DeTais",
                newName: "GiangVienMaGV");

            migrationBuilder.RenameIndex(
                name: "IX_DeTais_LinhVucMaLinhVuc",
                table: "DeTais",
                newName: "IX_DeTais_GiangVienMaGV");

            migrationBuilder.AlterColumn<string>(
                name: "SoDienThoai",
                table: "GiangViens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HocVi",
                table: "GiangViens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "HoTen",
                table: "GiangViens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "GiangViens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "LinhVuc",
                table: "GiangViens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayVaoTruong",
                table: "GiangViens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenDeTai",
                table: "DeTais",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "NamHoc",
                table: "DeTais",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "LinhVuc",
                table: "DeTais",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaGV",
                table: "DeTais",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_DeTais_GiangViens_GiangVienMaGV",
                table: "DeTais",
                column: "GiangVienMaGV",
                principalTable: "GiangViens",
                principalColumn: "MaGV");
        }
    }
}
