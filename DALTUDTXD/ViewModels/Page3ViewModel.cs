using DALTUDTXD.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Runtime.CompilerServices;

namespace DALTUDTXD.ViewModels
{
    public class Page3ViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel;

        private ForceInputEntry _selectedForceInput;
        private double _terzaghiP;
        private double _terzaghiP120;
        private double _chieuRongMong;
        private double _chieuSauChonMong;

        private double _chenhLechKinhTe;
        private Brush _dieuKien1Brush = Brushes.Black;
        private Brush _dieuKien2Brush = Brushes.Black;
        private Brush _kinhTeBrush = Brushes.Black;
        private string _ketLuanDieuKien1;
        private string _ketLuanDieuKien2;
        private string _ketLuanKinhTe;
        private string _ketLuanTongThe;

        private double _calculatedSteelArea = 3.85; // Giả định diện tích thép yêu cầu (cm2)
        private int _selectedSteelDiameter;
        private int _calculatedSteelBars;
        private double _actualSteelArea;
        private int _steelSpacing;

        private Geometry _foundationOutline;
        private Geometry _geometryGroup_LongRebar;

        public Page3ViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            if (_mainViewModel?.Page5ViewModel?.CalculationResults != null)
            {
                _mainViewModel.Page5ViewModel.CalculationResults.CollectionChanged += (s, e) =>
                {
                    UpdateCalculationDisplay();
                };
            }
        }

        #region Danh sách và Lựa chọn (ComboBox Binding)

        public ObservableCollection<ForceInputEntry> ForceInputList => _mainViewModel.Page4ViewModel.ForceInputList;

        public ForceInputEntry SelectedForceInput
        {
            get => _selectedForceInput;
            set
            {
                if (_selectedForceInput != value)
                {
                    _selectedForceInput = value;
                    OnPropertyChanged();

                    if (value?.Mong != null)
                    {
                        // Tự động điền kích thước từ móng đã chọn
                        ChieuRongMong = value.Mong.ChieuRongMong;
                        ChieuSauChonMong = value.Mong.ChieuSauChonMong;

                        // Cập nhật đường kính thép mặc định nếu chưa chọn
                        if (SelectedSteelDiameter == 0) SelectedSteelDiameter = 12;
                    }
                    else
                    {
                        ClearCalculatedValues();
                    }

                    UpdateCalculationDisplay();
                }
            }
        }

        #endregion

        #region Thuộc tính hiển thị kết quả từ Page 5

        public double PtbDisplayed
        {
            get
            {
                var result = _mainViewModel?.Page5ViewModel?.CalculationResults?
                    .FirstOrDefault(r => r.TenMong == SelectedForceInput?.Mong?.TenMong);
                return result?.Ptb ?? 0;
            }
        }

        public double PmaxDisplayed
        {
            get
            {
                var result = _mainViewModel?.Page5ViewModel?.CalculationResults?
                    .FirstOrDefault(r => r.TenMong == SelectedForceInput?.Mong?.TenMong);
                return result?.Pmax ?? 0;
            }
        }

        #endregion

        #region Thuộc tính Binding UI

        public double ChieuRongMong
        {
            get => _chieuRongMong;
            set { _chieuRongMong = value; OnPropertyChanged(); RecalculateAll(); }
        }

        public double ChieuSauChonMong
        {
            get => _chieuSauChonMong;
            set { _chieuSauChonMong = value; OnPropertyChanged(); RecalculateAll(); }
        }

        public double TerzaghiP { get => _terzaghiP; set { _terzaghiP = value; OnPropertyChanged(); } }
        public double TerzaghiP120 { get => _terzaghiP120; set { _terzaghiP120 = value; OnPropertyChanged(); } }
        public double ChenhLechKinhTe { get => _chenhLechKinhTe; set { _chenhLechKinhTe = value; OnPropertyChanged(); } }

        public string KetLuanDieuKien1 { get => _ketLuanDieuKien1; set { _ketLuanDieuKien1 = value; OnPropertyChanged(); } }
        public string KetLuanDieuKien2 { get => _ketLuanDieuKien2; set { _ketLuanDieuKien2 = value; OnPropertyChanged(); } }
        public string KetLuanKinhTe { get => _ketLuanKinhTe; set { _ketLuanKinhTe = value; OnPropertyChanged(); } }
        public string KetLuanTongThe { get => _ketLuanTongThe; set { _ketLuanTongThe = value; OnPropertyChanged(); } }

        public Brush DieuKien1Brush { get => _dieuKien1Brush; set { _dieuKien1Brush = value; OnPropertyChanged(); } }
        public Brush DieuKien2Brush { get => _dieuKien2Brush; set { _dieuKien2Brush = value; OnPropertyChanged(); } }
        public Brush KinhTeBrush { get => _kinhTeBrush; set { _kinhTeBrush = value; OnPropertyChanged(); } }

        #endregion

        #region Tính toán Reinforcement (Cốt thép)

        public ObservableCollection<int> SteelDiameters { get; } = new ObservableCollection<int> { 6, 8, 10, 12, 14, 16, 18, 20, 22, 25, 28, 30, 32, 36, 40 };

        public int SelectedSteelDiameter
        {
            get => _selectedSteelDiameter;
            set { if (_selectedSteelDiameter != value) { _selectedSteelDiameter = value; OnPropertyChanged(); CalculateReinforcement(); } }
        }

        public int CalculatedSteelBars { get => _calculatedSteelBars; set { _calculatedSteelBars = value; OnPropertyChanged(); } }
        public string CalculatedSteelArea { get => _actualSteelArea.ToString("F3"); }
        public int CalculatedSteelSpacing { get => _steelSpacing; set { _steelSpacing = value; OnPropertyChanged(); } }

        #endregion

        #region Logic Tính toán và Kiểm tra

        private void RecalculateAll()
        {
            CalculateTerzaghiValues();
            UpdateCalculationDisplay();
        }

        private void UpdateCalculationDisplay()
        {
            OnPropertyChanged(nameof(PtbDisplayed));
            OnPropertyChanged(nameof(PmaxDisplayed));
            CheckConditions();
            CalculateReinforcement();
        }

        private void CalculateTerzaghiValues()
        {
            if (SelectedForceInput?.Mong?.SoilLayer == null || ChieuRongMong <= 0) return;

            var soil = SelectedForceInput.Mong.SoilLayer;
            double phi = soil.Gocmasattrong;
            double gamma = soil.Khoiluongtunhien;
            double c = soil.Lucdinhket;
            double b = ChieuRongMong;
            double hm = ChieuSauChonMong;

            var coeffs = GetTerzaghiCoefficients(phi);
            if (coeffs != null)
            {
                // Công thức Terzaghi (Fs = 2.5)
                double result = (0.5 * coeffs.Ny * gamma * b + coeffs.Nq * (gamma * hm) + coeffs.Nc * c) / 2.5;
                TerzaghiP = Math.Round(result, 2);
                TerzaghiP120 = Math.Round(1.2 * TerzaghiP, 2);
            }
        }

        private void CheckConditions()
        {
            if (SelectedForceInput == null) return;

            double ptb = PtbDisplayed;
            double pmax = PmaxDisplayed;

            // Điều kiện 1: Ptb < P_Terzaghi
            if (ptb < TerzaghiP && TerzaghiP > 0)
            {
                KetLuanDieuKien1 = $"THỎA MÃN (Ptb={ptb} < P={TerzaghiP})";
                DieuKien1Brush = Brushes.Green;
            }
            else
            {
                KetLuanDieuKien1 = $"KHÔNG ĐẠT (Ptb={ptb} ≥ P={TerzaghiP})";
                DieuKien1Brush = Brushes.Red;
            }

            // Điều kiện 2: Pmax < 1.2 * P_Terzaghi
            if (pmax < TerzaghiP120 && TerzaghiP120 > 0)
            {
                KetLuanDieuKien2 = $"THỎA MÃN (Pmax={pmax} < 1.2P={TerzaghiP120})";
                DieuKien2Brush = Brushes.Green;
            }
            else
            {
                KetLuanDieuKien2 = $"KHÔNG ĐẠT (Pmax={pmax} ≥ 1.2P={TerzaghiP120})";
                DieuKien2Brush = Brushes.Red;
            }

            // Tính kinh tế
            if (TerzaghiP120 > 0)
            {
                ChenhLechKinhTe = Math.Round(((TerzaghiP120 - pmax) / TerzaghiP120) * 100, 2);
                if (ChenhLechKinhTe >= 0 && ChenhLechKinhTe <= 15) { KetLuanKinhTe = $"TỐI ƯU ({ChenhLechKinhTe}%)"; KinhTeBrush = Brushes.Green; }
                else if (ChenhLechKinhTe > 15 && ChenhLechKinhTe <= 40) { KetLuanKinhTe = $"CHƯA TỐI ƯU ({ChenhLechKinhTe}%)"; KinhTeBrush = Brushes.Orange; }
                else { KetLuanKinhTe = $"LÃNG PHÍ ({ChenhLechKinhTe}%)"; KinhTeBrush = Brushes.Red; }
            }

            KetLuanTongThe = (DieuKien1Brush == Brushes.Green && DieuKien2Brush == Brushes.Green)
                ? "KẾT LUẬN: MÓNG ĐẢM BẢO CHỊU LỰC."
                : "KẾT LUẬN: KÍCH THƯỚC MÓNG CHƯA HỢP LÝ.";
        }

        private TerzaghiCoefficients GetTerzaghiCoefficients(double phi)
        {
            var table = new List<TerzaghiCoefficients>
            {
                new TerzaghiCoefficients { Phi = 0, Ny = 0, Nq = 1, Nc = 5.14 },
                new TerzaghiCoefficients { Phi = 5, Ny = 0.5, Nq = 1.6, Nc = 6.5 },
                new TerzaghiCoefficients { Phi = 10, Ny = 1.2, Nq = 2.5, Nc = 8.4 },
                new TerzaghiCoefficients { Phi = 15, Ny = 2.5, Nq = 4.0, Nc = 11.0 },
                new TerzaghiCoefficients { Phi = 20, Ny = 5.0, Nq = 6.4, Nc = 14.8 },
                new TerzaghiCoefficients { Phi = 25, Ny = 9.7, Nq = 10.7, Nc = 20.7 },
                new TerzaghiCoefficients { Phi = 30, Ny = 19.7, Nq = 18.4, Nc = 30.1 },
                new TerzaghiCoefficients { Phi = 35, Ny = 42.4, Nq = 33.3, Nc = 46.1 },
                new TerzaghiCoefficients { Phi = 40, Ny = 100.4, Nq = 64.2, Nc = 75.3 }
            };

            var lower = table.OrderByDescending(t => t.Phi).FirstOrDefault(t => t.Phi <= phi);
            var upper = table.OrderBy(t => t.Phi).FirstOrDefault(t => t.Phi >= phi);

            if (lower == null || upper == null || lower.Phi == upper.Phi) return lower ?? upper;

            double ratio = (phi - lower.Phi) / (upper.Phi - lower.Phi);
            return new TerzaghiCoefficients
            {
                Phi = phi,
                Ny = lower.Ny + ratio * (upper.Ny - lower.Ny),
                Nq = lower.Nq + ratio * (upper.Nq - lower.Nq),
                Nc = lower.Nc + ratio * (upper.Nc - lower.Nc)
            };
        }

        private void CalculateReinforcement()
        {
            if (SelectedSteelDiameter <= 0 || ChieuRongMong <= 0) return;

            double areaPerBar = Math.PI * Math.Pow(SelectedSteelDiameter / 10.0, 2) / 4.0;
            int n = (int)Math.Ceiling(_calculatedSteelArea / areaPerBar);
            if (n < 2) n = 2;
            if (n % 2 == 0) n++; 

            CalculatedSteelBars = n;
            _actualSteelArea = n * areaPerBar;
            OnPropertyChanged(nameof(CalculatedSteelArea));

            CalculatedSteelSpacing = (int)((ChieuRongMong * 1000 - 100) / (n - 1));

            DrawFoundationDrawing();
        }

        private void ClearCalculatedValues()
        {
            TerzaghiP = TerzaghiP120 = ChenhLechKinhTe = 0;
            KetLuanDieuKien1 = KetLuanDieuKien2 = KetLuanKinhTe = KetLuanTongThe = "";
        }

        #endregion

        #region Đồ họa (Drawing Logic)

        public Geometry FoundationOutline { get => _foundationOutline; set { _foundationOutline = value; OnPropertyChanged(); } }
        public Geometry GeometryGroup_LongRebar { get => _geometryGroup_LongRebar; set { _geometryGroup_LongRebar = value; OnPropertyChanged(); } }

        public void DrawFoundationDrawing()
        {
            double B_mm = ChieuRongMong * 1000;
            if (B_mm <= 0) return;

            double Bc_mm = 300; // Bề rộng cổ cột giả định
            double H1 = 300, H2 = 200, H3 = 400; // Các cao độ giả định
            double canvasW = 380, canvasH = 220;
            double scale = Math.Min((canvasW - 40) / B_mm, (canvasH - 40) / (H1 + H2 + H3));
            double oX = canvasW / 2, oY = canvasH - 20;

            // Vẽ bao móng
            StreamGeometry geo = new StreamGeometry();
            using (StreamGeometryContext ctx = geo.Open())
            {
                ctx.BeginFigure(new Point(oX - (B_mm / 2) * scale, oY), true, true);
                ctx.LineTo(new Point(oX + (B_mm / 2) * scale, oY), true, false);
                ctx.LineTo(new Point(oX + (B_mm / 2) * scale, oY - H1 * scale), true, false);
                ctx.LineTo(new Point(oX + (Bc_mm / 2) * scale, oY - (H1 + H2) * scale), true, false);
                ctx.LineTo(new Point(oX + (Bc_mm / 2) * scale, oY - (H1 + H2 + H3) * scale), true, false);
                ctx.LineTo(new Point(oX - (Bc_mm / 2) * scale, oY - (H1 + H2 + H3) * scale), true, false);
                ctx.LineTo(new Point(oX - (Bc_mm / 2) * scale, oY - (H1 + H2) * scale), true, false);
                ctx.LineTo(new Point(oX - (B_mm / 2) * scale, oY - H1 * scale), true, false);
            }
            FoundationOutline = geo;

            // Vẽ thép
            GeometryGroup rebar = new GeometryGroup();
            double cover = 50 * scale;
            rebar.Children.Add(new LineGeometry(
                new Point(oX - (B_mm / 2) * scale + cover, oY - cover),
                new Point(oX + (B_mm / 2) * scale - cover, oY - cover)));

            int n = CalculatedSteelBars;
            if (n > 1)
            {
                double startX = oX - (B_mm / 2) * scale + cover + 5;
                double endX = oX + (B_mm / 2) * scale - cover - 5;
                double step = (endX - startX) / (n - 1);
                for (int i = 0; i < n; i++)
                    rebar.Children.Add(new EllipseGeometry(new Point(startX + i * step, oY - cover - 5), 2, 2));
            }
            GeometryGroup_LongRebar = rebar;
        }

        #endregion

        public new event PropertyChangedEventHandler PropertyChanged;
        protected new virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TerzaghiCoefficients
    {
        public double Phi { get; set; }
        public double Ny { get; set; }
        public double Nq { get; set; }
        public double Nc { get; set; }
    }
}