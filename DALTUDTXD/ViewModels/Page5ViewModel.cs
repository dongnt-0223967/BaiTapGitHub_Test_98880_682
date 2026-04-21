using DALTUDTXD.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DALTUDTXD.ViewModels
{
    public class Page5ViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private ObservableCollection<FootingResult> _calculationResults;

        public ObservableCollection<FootingResult> CalculationResults
        {
            get => _calculationResults;
            set { _calculationResults = value; OnPropertyChanged(nameof(CalculationResults)); }
        }

        // Lấy danh sách đầu vào từ Page 4
        public ObservableCollection<ForceInputEntry> ForceInputList => _mainViewModel.Page4ViewModel.ForceInputList;

        public Page5ViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            CalculationResults = new ObservableCollection<FootingResult>();

            // Mỗi khi danh sách móng ở Page 4 thay đổi, tự động tính lại bảng này
            ForceInputList.CollectionChanged += (s, e) => CalculateAll();
            CalculateAll();
        }

        public void CalculateAll()
        {
            CalculationResults.Clear();
            var soilList = _mainViewModel.Page1ViewModel.FoundationList;

            foreach (var input in ForceInputList)
            {
                if (input.Mong == null || input.Mong.SoilLayer == null) continue;

                // --- 1. Thông số cơ bản ---
                double N = input.AxialForce;
                double M = input.Moment;
                double b = input.Mong.ChieuRongMong;
                double hm = input.Mong.ChieuSauChonMong;
                double bt = input.Mong.BeDayTuong;
                double hd = input.Mong.ChieuCaoDai;
                double bv = input.Mong.ChieuDayLopBaoVe / 1000.0;
                double rbt = ConcreteProperties.GetRbtInTM2(input.Mong.CapDoBeTong);
                double gamma = input.Mong.SoilLayer.Khoiluongtunhien;
                double E0 = input.Mong.SoilLayer.Modunbiendang;
                double h_lop_hien_tai = input.Mong.SoilLayer.Chieudaylopdat;

                // --- 2. Tính toán các loại áp lực ---
                double ptb = Math.Round(N / (1.15 * b) + 2 * hm, 2);
                double pmax = Math.Round(N / (1.15 * b) + 2 * hm + 6 * M / (1.15 * b * b), 2);
                double pmin = Math.Round(N / (1.15 * b) + 2 * hm - 6 * M / (1.15 * b * b), 2);

                // Áp lực ròng (P0)
                double p0max = Math.Round(N / b + 6 * M / (b * b), 2);
                double p0min = Math.Round(N / b - 6 * M / (b * b), 2);
                double Pgl = ptb - (gamma * hm);

                // --- 3. Logic chiều dày đất (Xử lý lớp 1 + lớp 2 nếu móng sâu) ---
                double h_duoi_day = h_lop_hien_tai - hm;
                if (h_duoi_day < 0)
                {
                    int currentIndex = soilList.IndexOf(input.Mong.SoilLayer);
                    if (currentIndex >= 0 && currentIndex < soilList.Count - 1)
                    {
                        double h_lop_ke_tiep = soilList[currentIndex + 1].Chieudaylopdat;
                        h_duoi_day = (h_lop_hien_tai + h_lop_ke_tiep) - hm;
                    }
                }
                double h_final = Math.Max(0, h_duoi_day);

                // --- 4. Tính độ lún S (Nội suy w) ---
                double tiSo = (b >= 1) ? b : (1 / b);
                double w = GetWInterpolated(tiSo);
                double Bqu = b + 2 * h_final * Math.Tan(30 * Math.PI / 180);

                double doLunS = 0;
                if (E0 > 0)
                {
                    double quy = 0.25;
                    double s_met = (Pgl * Bqu * w * (1 - Math.Pow(quy, 2))) / E0;
                    doLunS = Math.Round(s_met , 2); // cm
                }

                // --- 5. Kiểm tra chọc thủng ---
                double bHieuDung = b - bt - 2 * (hd - bv);
                double pDamThung = Math.Round((p0min + (p0max - p0min) * (b - 0.5 * bHieuDung) / b + p0max) / 2 * (0.5 * bHieuDung), 2);
                double pChongDamThung = Math.Round((hd - bv) * rbt, 2);

                // --- 6. Ghi chú trạng thái ---
                bool isOk = pmin >= 0 && pDamThung <= pChongDamThung && doLunS <= 8.0;
                string status = isOk ? "Thỏa mãn" : (doLunS > 8.0 ? "Lún lớn!" : (pmin < 0 ? "Pmin < 0" : "Check!"));

                // --- 7. Xuất kết quả ra bảng (Đầy đủ P0min) ---
                CalculationResults.Add(new FootingResult
                {
                    TenMong = input.Mong.TenMong,
                    Ptb = ptb,
                    Pmax = pmax,
                    Pmin = pmin,
                    P0max = p0max,
                    P0min = p0min, // Gán giá trị hiển thị ra bảng ở đây
                    PDamThung = pDamThung,
                    PChongDamThung = pChongDamThung,
                    DoLun = doLunS,
                    GhiChu = status
                });
            }
        }

        /// <summary>
        /// Hàm nội suy hệ số w dựa trên tỷ số r
        /// </summary>
        private double GetWInterpolated(double r)
        {
            // Bảng dữ liệu bạn cung cấp
            double[] rValues = { 1, 1.5, 2, 3, 4, 5, 6, 7, 10 };
            double[] wValues = { 0.88, 1.08, 1.22, 1.44, 1.61, 1.72, 1.83, 1.91, 2.12 };

            // Nếu nhỏ hơn giá trị đầu tiên
            if (r <= rValues[0]) return wValues[0];
            // Nếu lớn hơn giá trị cuối cùng
            if (r >= rValues[rValues.Length - 1]) return wValues[wValues.Length - 1];

            // Tìm khoảng để nội suy
            for (int i = 0; i < rValues.Length - 1; i++)
            {
                if (r >= rValues[i] && r <= rValues[i + 1])
                {
                    // Công thức nội suy tuyến tính: y = y0 + (r - r0) * (y1 - y0) / (r1 - r0)
                    return wValues[i] + (r - rValues[i]) * (wValues[i + 1] - wValues[i]) / (rValues[i + 1] - rValues[i]);
                }
            }

            return 0.88; // Mặc định phòng ngừa
        }
    }
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string;
            if (string.IsNullOrEmpty(status)) return Brushes.Black;

            // Nếu ghi chú chứa chữ "Đạt" hoặc "Thỏa mãn" -> Màu xanh
            if (status.Contains("Đạt") || status.Contains("Thỏa mãn") || status.Contains("TỐI ƯU"))
                return Brushes.Green;

            // Nếu ghi chú chứa chữ "Không" hoặc "Lãng phí" -> Màu đỏ
            if (status.Contains("Không") || status.Contains("Lãng phí"))
                return Brushes.Red;

            return Brushes.Orange; // Mặc định (ví dụ: "Chưa tối ưu")
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}