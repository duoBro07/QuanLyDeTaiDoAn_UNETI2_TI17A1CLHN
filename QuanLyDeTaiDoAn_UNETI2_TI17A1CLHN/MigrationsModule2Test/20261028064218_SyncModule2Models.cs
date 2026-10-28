using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.MigrationsModule2Test
{
    /// <inheritdoc />
    public partial class SyncModule2Models : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DangKyDeTaiModule2s");

            migrationBuilder.AlterColumn<bool>(
                name: "TrangThai",
                table: "LinhVucs",
                type: "bit",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "LinhVucs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "GiangViens",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "DeTais",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "DangKyDeTais",
                columns: table => new
                {
                    MaDangKy = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaSinhVien = table.Column<int>(type: "int", nullable: false),
                    MaDeTai = table.Column<int>(type: "int", nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LyDoDangKy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GhiChuDuyet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThaiThucHien = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyDeTais", x => x.MaDangKy);
                    table.ForeignKey(
                        name: "FK_DangKyDeTais_DeTais_MaDeTai",
                        column: x => x.MaDeTai,
                        principalTable: "DeTais",
                        principalColumn: "MaDeTai",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoans",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VaiTro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoans", x => x.MaTaiKhoan);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LinhVucs_TenLinhVuc",
                table: "LinhVucs",
                column: "TenLinhVuc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeTais_TenDeTai_NamHoc_HocKy",
                table: "DeTais",
                columns: new[] { "TenDeTai", "NamHoc", "HocKy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DangKyDeTais_MaDeTai",
                table: "DangKyDeTais",
                column: "MaDeTai");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DangKyDeTais");

            migrationBuilder.DropTable(
                name: "TaiKhoans");

            migrationBuilder.DropIndex(
                name: "IX_LinhVucs_TenLinhVuc",
                table: "LinhVucs");

            migrationBuilder.DropIndex(
                name: "IX_DeTais_TenDeTai_NamHoc_HocKy",
                table: "DeTais");

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "LinhVucs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "LinhVucs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "GiangViens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "DeTais",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateTable(
                name: "DangKyDeTaiModule2s",
                columns: table => new
                {
                    MaDangKy = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDeTai = table.Column<int>(type: "int", nullable: false),
                    MaSinhVien = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyDeTaiModule2s", x => x.MaDangKy);
                    table.ForeignKey(
                        name: "FK_DangKyDeTaiModule2s_DeTais_MaDeTai",
                        column: x => x.MaDeTai,
                        principalTable: "DeTais",
                        principalColumn: "MaDeTai",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DangKyDeTaiModule2s_MaDeTai",
                table: "DangKyDeTaiModule2s",
                column: "MaDeTai");
        }
    }
}
