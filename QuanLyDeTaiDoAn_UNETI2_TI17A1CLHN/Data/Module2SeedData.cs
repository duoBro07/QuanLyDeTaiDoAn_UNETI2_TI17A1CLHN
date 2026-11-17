
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Models;

namespace QuanLyDeTaiDoAn_UNETI2_TI17A1CLHN.Data
{
    public static class Module2SeedData
    {
        private const string NamHocMau = "2026-2027";
        private const string MatKhauMau = "Sv@123456";
        private const int SoDangKyMucTieu = 28;

        public static async Task SeedAsync(
            ApplicationDbContext db)
        {
            await TaoLinhVuc(db);
            await TaoGiangVien(db);
            await TaoDeTai(db);
            await TaoSinhVien(db);
            await TaoDangKyVaPhanCong(db);
        }

        // =========================================
        // 1. TẠO 5 LĨNH VỰC
        // =========================================
        private static async Task TaoLinhVuc(
            ApplicationDbContext db)
        {
            string[] tenLinhVucs =
            {
                "Công nghệ phần mềm",
                "Trí tuệ nhân tạo",
                "An toàn thông tin",
                "Mạng máy tính",
                "Hệ thống thông tin"
            };

            foreach (string ten in tenLinhVucs)
            {
                bool daCo = await db.LinhVucs
                    .AnyAsync(x => x.TenLinhVuc == ten);

                if (daCo)
                    continue;

                db.LinhVucs.Add(new LinhVuc
                {
                    TenLinhVuc = ten,
                    MoTa = "Lĩnh vực phục vụ quản lý đồ án.",
                    TrangThai = true
                });
            }

            await db.SaveChangesAsync();
        }

        // =========================================
        // 2. TẠO 12 GIẢNG VIÊN
        // =========================================
        private static async Task TaoGiangVien(
            ApplicationDbContext db)
        {
            var linhVucs = await LayLinhVucMau(db);

            if (linhVucs.Count != 5)
                return;

            for (int i = 1; i <= 12; i++)
            {
                string email =
                    $"gvmodule2_{i}@example.com";

                bool daCo = await db.GiangViens
                    .AnyAsync(x => x.Email == email);

                if (daCo)
                    continue;

                db.GiangViens.Add(new GiangVien
                {
                    HoTen = $"Giảng viên mẫu {i:00}",

                    HocVi = i % 3 == 0
                        ? "Tiến sĩ"
                        : "Thạc sĩ",

                    Email = email,

                    SoDienThoai = $"090000{i:0000}",

                    MaLinhVuc =
                        linhVucs[(i - 1) % 5].MaLinhVuc,

                    SoSinhVienHuongDanToiDa =
                        i == 1 ? 2 : 5,

                    TrangThai = "Đang công tác"
                });
            }

            await db.SaveChangesAsync();
        }

        // =========================================
        // 3. TẠO 20 ĐỀ TÀI
        // =========================================
        private static async Task TaoDeTai(
            ApplicationDbContext db)
        {
            var linhVucs = await LayLinhVucMau(db);

            if (linhVucs.Count != 5)
                return;

            string[] tenDeTais =
            {
                "Xây dựng website quản lý bán hàng",
                "Hệ thống quản lý thư viện",
                "Website quản lý đặt lịch khám",
                "Ứng dụng quản lý sinh viên",
                "Hệ thống quản lý kho hàng",

                "Nhận diện khuôn mặt bằng AI",
                "Phân loại văn bản tiếng Việt",
                "Dự đoán kết quả học tập",
                "Chatbot hỗ trợ sinh viên",
                "Phân tích cảm xúc bình luận",

                "Phát hiện email lừa đảo",
                "Hệ thống xác thực đa yếu tố",
                "Phát hiện truy cập bất thường",
                "Giám sát an toàn mạng",
                "Ứng dụng mã hóa dữ liệu",

                "Mô phỏng định tuyến mạng",
                "Giám sát lưu lượng mạng",
                "Quản lý thiết bị mạng",
                "Hệ thống báo cáo doanh thu",
                "Phân tích dữ liệu khách hàng"
            };

            for (int i = 0; i < tenDeTais.Length; i++)
            {
                string ten = tenDeTais[i];
                int hocKy = i < 15 ? 1 : 2;

                bool daCo = await db.DeTais
                    .AnyAsync(x =>
                        x.TenDeTai == ten &&
                        x.NamHoc == NamHocMau &&
                        x.HocKy == hocKy);

                if (daCo)
                    continue;

                string trangThai = i switch
                {
                    18 => "Tạm khóa",
                    19 => "Kết thúc",
                    _ => "Mở đăng ký"
                };

                db.DeTais.Add(new DeTai
                {
                    TenDeTai = ten,

                    MaLinhVuc =
                        linhVucs[i % 5].MaLinhVuc,

                    MoTa =
                        "Phân tích, thiết kế và xây dựng " +
                        $"giải pháp cho đề tài {ten}.",

                    YeuCau =
                        "Có kiến thức lập trình, " +
                        "cơ sở dữ liệu, phân tích hệ thống " +
                        "và làm việc nhóm.",

                    SoSinhVienToiDa = 3,
                    NamHoc = NamHocMau,
                    HocKy = hocKy,
                    TrangThai = trangThai
                });
            }

            await db.SaveChangesAsync();
        }

        // =========================================
        // 4. TẠO 30 TÀI KHOẢN VÀ HỒ SƠ SINH VIÊN
        // =========================================
        private static async Task TaoSinhVien(
            ApplicationDbContext db)
        {
            var hasher = new PasswordHasher<TaiKhoan>();

            for (int i = 1; i <= 30; i++)
            {
                string tenDangNhap = $"svdemo{i:000}";
                string email = $"svdemo{i:000}@example.com";
                string hoTen = $"Sinh viên kiểm thử {i:00}";

                var taiKhoan = await db.TaiKhoans
                    .FirstOrDefaultAsync(x =>
                        x.TenDangNhap == tenDangNhap);

                if (taiKhoan == null)
                {
                    taiKhoan = new TaiKhoan
                    {
                        TenDangNhap = tenDangNhap,
                        HoTen = hoTen,
                        Email = email,
                        VaiTro = "SinhVien",
                        TrangThai = true
                    };

                    taiKhoan.MatKhau =
                        hasher.HashPassword(
                            taiKhoan,
                            MatKhauMau);

                    db.TaiKhoans.Add(taiKhoan);
                    await db.SaveChangesAsync();
                }

                if (taiKhoan.VaiTro != "SinhVien")
                    continue;

                bool daCoHoSo = await db.SinhViens
                    .AnyAsync(x =>
                        x.MaTaiKhoan ==
                        taiKhoan.MaTaiKhoan);

                if (daCoHoSo)
                    continue;

                bool trungEmail = await db.SinhViens
                    .AnyAsync(x => x.Email == email);

                if (trungEmail)
                    continue;

                db.SinhViens.Add(new SinhVien
                {
                    MaTaiKhoan = taiKhoan.MaTaiKhoan,
                    HoTen = hoTen,

                    NgaySinh = new DateTime(
                        2004 + i % 3,
                        1 + i % 12,
                        1 + i % 25),

                    Lop = i <= 15
                        ? "DHTI17A1CL"
                        : "DHTI17A2CL",

                    Khoa = "Công nghệ thông tin",
                    ChuyenNganh = "Công nghệ thông tin",
                    Email = email,
                    SoDienThoai = $"0912{i:000000}",
                    TrangThai = true
                });
            }

            await db.SaveChangesAsync();
        }

        // =========================================
        // 5. BỔ SUNG ĐĂNG KÝ VÀ PHÂN CÔNG MẪU
        // =========================================
        private static async Task TaoDangKyVaPhanCong(
            ApplicationDbContext db)
        {
            // Chỉ bổ sung, không xóa đăng ký cũ.
            var dangKyHienCo = await db.DangKyDeTais
                .AsNoTracking()
                .Include(x => x.DeTai)
                .ToListAsync();

            int canThem =
                SoDangKyMucTieu - dangKyHienCo.Count;

            if (canThem <= 0)
                return;

            // Lấy sinh viên mẫu có tài khoản hợp lệ.
            var sinhViens = await db.SinhViens
                .AsNoTracking()
                .Include(x => x.TaiKhoan)
                .Where(x =>
                    x.TrangThai &&
                    x.TaiKhoan != null &&
                    x.TaiKhoan.TrangThai &&
                    x.TaiKhoan.VaiTro == "SinhVien" &&
                    x.TaiKhoan.TenDangNhap.StartsWith(
                        "svdemo"))
                .OrderBy(x => x.TaiKhoan!.TenDangNhap)
                .ToListAsync();

            // Chỉ chọn đề tài mở, học kỳ 1.
            var deTais = await db.DeTais
                .AsNoTracking()
                .Where(x =>
                    x.NamHoc == NamHocMau &&
                    x.HocKy == 1 &&
                    x.TrangThai == "Mở đăng ký" &&
                    x.SoSinhVienToiDa > 0)
                .OrderBy(x => x.MaDeTai)
                .ToListAsync();

            var giangViens = await db.GiangViens
                .AsNoTracking()
                .Where(x =>
                    x.TrangThai == "Đang công tác" &&
                    x.SoSinhVienHuongDanToiDa > 0)
                .OrderBy(x => x.MaGiangVien)
                .ToListAsync();

            if (sinhViens.Count == 0 ||
                deTais.Count == 0)
            {
                return;
            }

            // Đếm số đăng ký đã duyệt trên từng đề tài.
            var soDaDuyet = dangKyHienCo
                .Where(x => x.TrangThai == "Đã duyệt")
                .GroupBy(x => x.MaDeTai)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());

            // Tải giảng viên đang hiệu lực.
            var phanCongHienCo =
                await db.PhanCongHuongDans
                    .AsNoTracking()
                    .Where(x =>
                        x.DangHieuLuc &&
                        x.TrangThaiThucHien != "Hoàn thành")
                    .ToListAsync();

            var taiGiangVien = giangViens.ToDictionary(
                x => x.MaGiangVien,
                x => phanCongHienCo.Count(p =>
                    p.MaGiangVien == x.MaGiangVien));

            // Sinh viên đã có đăng ký thì giữ nguyên.
            var sinhVienDaCoDangKy = dangKyHienCo
                .Select(x => x.MaSinhVien)
                .ToHashSet();

            // Đếm trạng thái hiện tại.
            var soTheoTrangThai = dangKyHienCo
                .GroupBy(x => x.TrangThai)
                .ToDictionary(
                    x => x.Key,
                    x => x.Count());

            int DemTrangThai(string trangThai)
            {
                return soTheoTrangThai.TryGetValue(
                    trangThai,
                    out int soLuong)
                    ? soLuong
                    : 0;
            }

            void TangTrangThai(string trangThai)
            {
                soTheoTrangThai[trangThai] =
                    DemTrangThai(trangThai) + 1;
            }

            int soDaThem = 0;

            // Tạo trong transaction để tránh lưu dở
            // đăng ký và phân công.
            await using var transaction =
                await db.Database.BeginTransactionAsync();

            foreach (var sinhVien in sinhViens)
            {
                if (soDaThem >= canThem)
                    break;

                if (sinhVienDaCoDangKy.Contains(
                    sinhVien.MaSinhVien))
                {
                    continue;
                }

                // Phân bố dữ liệu mẫu:
                // 10 Đã duyệt, 10 Chờ duyệt,
                // 5 Từ chối, còn lại Đã hủy.
                string trangThai;

                if (DemTrangThai("Đã duyệt") < 10)
                {
                    trangThai = "Đã duyệt";
                }
                else if (DemTrangThai("Chờ duyệt") < 10)
                {
                    trangThai = "Chờ duyệt";
                }
                else if (DemTrangThai("Từ chối") < 5)
                {
                    trangThai = "Từ chối";
                }
                else
                {
                    trangThai = "Đã hủy";
                }

                DeTai? deTai = null;
                GiangVien? giangVien = null;

                // Đăng ký đã duyệt phải có đề tài
                // còn chỗ và giảng viên đủ điều kiện.
                if (trangThai == "Đã duyệt")
                {
                    foreach (var dt in deTais)
                    {
                        int daDuyet =
                            soDaDuyet.GetValueOrDefault(
                                dt.MaDeTai);

                        if (daDuyet >=
                            dt.SoSinhVienToiDa)
                        {
                            continue;
                        }

                        var gv = giangViens
                            .Where(x =>
                                x.MaLinhVuc ==
                                    dt.MaLinhVuc &&
                                taiGiangVien[
                                    x.MaGiangVien] <
                                    x.SoSinhVienHuongDanToiDa)
                            .OrderBy(x =>
                                taiGiangVien[
                                    x.MaGiangVien])
                            .ThenBy(x => x.MaGiangVien)
                            .FirstOrDefault();

                        if (gv == null)
                            continue;

                        deTai = dt;
                        giangVien = gv;
                        break;
                    }

                    // Không tạo đăng ký đã duyệt
                    // nếu không thể phân công hợp lệ.
                    if (deTai == null)
                    {
                        trangThai = "Chờ duyệt";
                    }
                }

                // Đăng ký chưa duyệt không chiếm chỗ.
                if (deTai == null)
                {
                    deTai = deTais[
                        soDaThem % deTais.Count];
                }

                var dangKy = new DangKyDeTai
                {
                    MaSinhVien =
                        sinhVien.MaSinhVien,

                    MaDeTai =
                        deTai.MaDeTai,

                    NgayDangKy =
                        new DateTime(
                            2026, 10, 1)
                            .AddDays(soDaThem),

                    LyDoDangKy =
                        "Em mong muốn nghiên cứu " +
                        "và thực hiện đề tài này để " +
                        "vận dụng kiến thức đã học.",

                    TrangThai = trangThai,

                    GhiChuDuyet = trangThai switch
                    {
                        "Đã duyệt" =>
                            "Đủ điều kiện thực hiện đề tài.",

                        "Từ chối" =>
                            "Chưa đáp ứng yêu cầu đề tài.",

                        "Đã hủy" =>
                            "Sinh viên chủ động hủy đăng ký.",

                        _ => null
                    }
                };

                db.DangKyDeTais.Add(dangKy);

                // Lưu để lấy khóa chính MaDangKy.
                await db.SaveChangesAsync();

                sinhVienDaCoDangKy.Add(
                    sinhVien.MaSinhVien);

                TangTrangThai(trangThai);
                soDaThem++;

                if (trangThai == "Đã duyệt" &&
                    giangVien != null)
                {
                    soDaDuyet[deTai.MaDeTai] =
                        soDaDuyet.GetValueOrDefault(
                            deTai.MaDeTai) + 1;

                    db.PhanCongHuongDans.Add(
                        new PhanCongHuongDan
                        {
                            MaDangKy =
                                dangKy.MaDangKy,

                            MaGiangVien =
                                giangVien.MaGiangVien,

                            NgayPhanCong =
                                dangKy.NgayDangKy
                                    .AddDays(1),

                            NgayKetThuc = null,

                            TrangThaiThucHien =
                                "Chưa bắt đầu",

                            DangHieuLuc = true,

                            GhiChu =
                                "Phân công hướng dẫn mẫu."
                        });

                    await db.SaveChangesAsync();

                    taiGiangVien[
                        giangVien.MaGiangVien]++;
                }
            }

            await transaction.CommitAsync();
        }

        // =========================================
        // 6. HÀM DÙNG CHUNG: LẤY 5 LĨNH VỰC MẪU
        // =========================================
        private static async Task<List<LinhVuc>>
            LayLinhVucMau(ApplicationDbContext db)
        {
            string[] tenLinhVucs =
            {
                "Công nghệ phần mềm",
                "Trí tuệ nhân tạo",
                "An toàn thông tin",
                "Mạng máy tính",
                "Hệ thống thông tin"
            };

            var tatCa = await db.LinhVucs
                .Where(x => x.TrangThai)
                .ToListAsync();

            var ketQua = new List<LinhVuc>();

            foreach (string ten in tenLinhVucs)
            {
                var linhVuc = tatCa.FirstOrDefault(
                    x => x.TenLinhVuc == ten);

                if (linhVuc == null)
                    return new List<LinhVuc>();

                ketQua.Add(linhVuc);
            }

            return ketQua;
        }
    }
}
