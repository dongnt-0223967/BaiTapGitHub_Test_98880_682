using DALTUDTXD.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DALTUDTXD.ViewModels
{
    public class Page1ViewModel : ViewModelBase
    {
        private FoundationEntry _newEntry = new FoundationEntry();
        public FoundationEntry NewEntry
        {
            get => _newEntry;
            set { _newEntry = value; OnPropertyChanged(nameof(NewEntry)); }
        }

        public ObservableCollection<FoundationEntry> FoundationList { get; set; } = new ObservableCollection<FoundationEntry>();
        public ObservableCollection<GeologicalAxis> GeologicalAxes { get; set; } = new ObservableCollection<GeologicalAxis>();

        private int _currentAxisNumber = 1;

        public ICommand AddFoundationCommand { get; }
        public ICommand SaveGeologicalAxisCommand { get; }
        private FoundationEntry _selectedSoilLayer;
        public FoundationEntry SelectedSoilLayer
        {
            get => _selectedSoilLayer;
            set
            {
                _selectedSoilLayer = value;
                OnPropertyChanged(nameof(SelectedSoilLayer));
            }
        }
        public Page1ViewModel()
        {
            AddFoundationCommand = new RelayCommand(AddFoundationEntry);
            SaveGeologicalAxisCommand = new RelayCommand(SaveGeologicalAxis);
        }
        private double _currentYOffset = 0; // Lưu vị trí Y tích lũy
        private const double Scale = 20;    // Tỷ lệ: 1 mét = 20 pixel

        private readonly List<SolidColorBrush> _soilBrushes = new List<SolidColorBrush>
{
    new SolidColorBrush(Color.FromRgb(139, 69, 19)),  // Nâu đậm
    new SolidColorBrush(Color.FromRgb(210, 180, 140)), // Màu cát
    new SolidColorBrush(Color.FromRgb(169, 169, 169)), // Xám
    new SolidColorBrush(Color.FromRgb(244, 164, 96)),  // Cam đất
    new SolidColorBrush(Color.FromRgb(107, 142, 35)),  // Xanh rêu
    new SolidColorBrush(Color.FromRgb(205, 133, 63))   // Vàng đất
};

        private void AddFoundationEntry()
        {
            // 1. Tạo đối tượng mới từ Form
            var newEntry = new FoundationEntry
            {
                Sothutulopdat = NewEntry.Sothutulopdat,
                Sohieudiachat = NewEntry.Sohieudiachat,
                Tenlopdat = NewEntry.Tenlopdat,
                Chieudaylopdat = NewEntry.Chieudaylopdat,
                Khoiluongtunhien = NewEntry.Khoiluongtunhien,
                Gocmasattrong = NewEntry.Gocmasattrong,
                Lucdinhket = NewEntry.Lucdinhket,
                Modunbiendang = NewEntry.Modunbiendang,
                Vitrimong = NewEntry.Vitrimong,
                RenderHeight = NewEntry.Chieudaylopdat * 20 
            };

            // 2. Thêm vào danh sách tạm
            FoundationList.Add(newEntry);

            // 3. Sắp xếp và tính toán lại vị trí
            RefreshVisualAxis();

            // 4. Reset form
            NewEntry = new FoundationEntry();
            OnPropertyChanged(nameof(NewEntry));
        }

        private void RefreshVisualAxis()
        {
            // Sắp xếp danh sách theo STT lớp đất
            var sortedList = FoundationList.OrderBy(x => x.Sothutulopdat).ToList();

            // Clear danh sách hiện tại để nạp lại theo đúng thứ tự
            FoundationList.Clear();

            double currentTop = 0;
            int colorIndex = 0;

            foreach (var item in sortedList)
            {
                item.RenderTop = currentTop;
                // Gán màu xoay vòng từ danh sách màu
                item.SoilColor = _soilBrushes[colorIndex % _soilBrushes.Count];

                FoundationList.Add(item);

                currentTop += item.RenderHeight; // Cộng dồn cho lớp tiếp theo
                colorIndex++;
            }
        }
        private void SaveGeologicalAxis()
        {
            _currentYOffset = 0;
            if (FoundationList.Count == 0)
            {
                MessageBox.Show("Vui lòng nhập dữ liệu trước khi lưu trục địa chất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newAxis = new GeologicalAxis
            {
                Name = $"Trục địa chất {_currentAxisNumber}"
            };

            foreach (var entry in FoundationList)
            {
                newAxis.Entries.Add(new FoundationEntry
                {
                    Sothutulopdat = entry.Sothutulopdat,
                    Sohieudiachat = entry.Sohieudiachat,
                    Tenlopdat = entry.Tenlopdat,
                    Chieudaylopdat = entry.Chieudaylopdat,
                    Khoiluongtunhien = entry.Khoiluongtunhien,
                    Gocmasattrong = entry.Gocmasattrong,
                    Lucdinhket = entry.Lucdinhket,
                    Modunbiendang = entry.Modunbiendang,
                    Vitrimong = entry.Vitrimong
                });
            }

            GeologicalAxes.Add(newAxis);
            _currentAxisNumber++;

            // Xóa dữ liệu trong bảng hiện tại
            FoundationList.Clear();
            MessageBox.Show("Đã lưu trục địa chất thành công!");
        }
    }
}

