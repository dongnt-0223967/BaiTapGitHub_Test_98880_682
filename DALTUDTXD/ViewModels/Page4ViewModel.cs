using DALTUDTXD.Models;
using DALTUDTXD.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using System.Collections.Specialized;
using System.ComponentModel; 

namespace DALTUDTXD.ViewModels
{
    public class Page4ViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        // Đối tượng này chỉ giữ giá trị nội lực đang được nhập (input area)
        private ForceInputEntry _forceInput;
        private string _selectedElementName;

        public string SelectedElementName
        {
            get => _selectedElementName;
            set
            {
                _selectedElementName = value;
                OnPropertyChanged(nameof(SelectedElementName));
            }
        }

        public ForceInputEntry ForceInput
        {
            get => _forceInput;
            set
            {
                _forceInput = value;
                OnPropertyChanged(nameof(ForceInput));
            }
        }

        public ConstructionEntry SelectedMong
        {
            get => ForceInput.Mong;
            set
            {
                // Gán móng được chọn vào ForceInput (đối tượng nhập)
                ForceInput.Mong = value;
                OnPropertyChanged(nameof(SelectedMong));

                // 1. Tìm ForceInputEntry tương ứng trong danh sách DataGrid
                var existingEntry = ForceInputList.FirstOrDefault(e => e.Mong.TenMong == value?.TenMong);

                // 2. Cập nhật các ô nhập nội lực (liên kết với ForceInput)
                if (existingEntry != null)
                {
                    // Lấy nội lực đã lưu
                    UpdateForceInputValues(existingEntry);
                }
                else
                {
                    // Reset nội lực về 0 nếu không tìm thấy (trường hợp móng mới chưa có trong list)
                    UpdateForceInputValues(new ForceInputEntry());
                }
            }
        }

        // Phương thức cập nhật các thuộc tính nội lực và kích hoạt OnPropertyChanged :))
        private void UpdateForceInputValues(ForceInputEntry sourceEntry)
        {
            // Cập nhật giá trị nội lực của đối tượng ForceInput đang được binding
            _forceInput.Moment = sourceEntry.Moment;
            _forceInput.AxialForce = sourceEntry.AxialForce;
            _forceInput.ShearForce = sourceEntry.ShearForce;

            // Kích hoạt cập nhật UI cho các TextBox
            OnPropertyChanged(nameof(ForceInput));
        }

        // Danh sách móng từ Page2 (ConstructionList của MainViewModel)
        public ObservableCollection<ConstructionEntry> DanhSachMong => _mainViewModel.ConstructionList;

        // Danh sách Nội lực (dữ liệu cho DataGrid)
        public ObservableCollection<ForceInputEntry> ForceInputList { get; set; } = new ObservableCollection<ForceInputEntry>();

        public ICommand SaveCommand { get; }
        public ICommand ShowCalculatorViewCommand { get; }
        public ICommand ImportEtabsDataCommand { get; }

        public Page4ViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _forceInput = new ForceInputEntry();
            SaveCommand = new RelayCommand(SaveData);
            ShowCalculatorViewCommand = new RelayCommand(ExecuteShowCalculatorView);

            // Khởi tạo danh sách nội lực
            InitializeForceInputList();

            // Đồng bộ hóa khi danh sách móng ở Page 2 thay đổi (thêm/xóa)
            _mainViewModel.ConstructionList.CollectionChanged += ConstructionList_CollectionChanged;

            // Thiết lập móng đầu tiên làm mặc định sau khi đã khởi tạo danh sách
            if (DanhSachMong.Any())
            {
                SelectedMong = DanhSachMong.First();
            }
        }

        private void ConstructionList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Đồng bộ lại danh sách khi có thay đổi từ View 2
            InitializeForceInputList();

            // Nếu móng đang được chọn bị xóa, chọn lại móng đầu tiên
            if (e.Action == NotifyCollectionChangedAction.Remove && SelectedMong != null)
            {
                if (!DanhSachMong.Contains(SelectedMong))
                {
                    SelectedMong = DanhSachMong.FirstOrDefault();
                }
            }
        }

        private void ExecuteShowCalculatorView()
        {
            _mainViewModel.ExecuteShowCal5View();
        }

        private void InitializeForceInputList()
        {
            // Lưu trữ nội lực hiện tại (trước khi clear)
            var currentForceInputs = ForceInputList.ToList();
            ForceInputList.Clear();

            foreach (var mong in DanhSachMong)
            {
                // Tìm xem móng này đã có nội lực được lưu chưa (dựa vào TenMong)
                var existingEntry = currentForceInputs.FirstOrDefault(e => e.Mong.TenMong == mong.TenMong);

                if (existingEntry != null)
                {
                    // Giữ lại nội lực đã nhập
                    ForceInputList.Add(existingEntry);
                }
                else
                {
                    // Tạo entry mới với nội lực mặc định là 0
                    ForceInputList.Add(new ForceInputEntry
                    {
                        Mong = mong,
                        Moment = 0,
                        AxialForce = 0,
                        ShearForce = 0
                    });
                }
            }

            // Nếu danh sách rỗng, reset SelectedMong
            if (!ForceInputList.Any() && ForceInput.Mong != null)
            {
                SelectedMong = null;
                UpdateForceInputValues(new ForceInputEntry());
            }
        }

        // Trong Page4ViewModel.cs

        private void SaveData()
        {
            if (ForceInput.Mong == null)
            {
                MessageBox.Show("Vui lòng chọn móng!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1. Tìm và CẬP NHẬT entry đã tồn tại trong danh sách hiển thị
            var existingEntry = ForceInputList.FirstOrDefault(e => e.Mong.TenMong == ForceInput.Mong.TenMong);

            if (existingEntry != null)
            {
                // Ghi lại chỉ mục (index) của đối tượng trong danh sách
                int index = ForceInputList.IndexOf(existingEntry);

                // 2. GHI ĐÈ GIÁ TRỊ NỘI LỰC
                existingEntry.Moment = ForceInput.Moment;
                existingEntry.AxialForce = ForceInput.AxialForce;
                existingEntry.ShearForce = ForceInput.ShearForce;

                // 3. ÉP UI CẬP NHẬT TỨC THÌ (REMOVE/RE-ADD TRICK)
                // Dòng này buộc DataGrid phải làm mới, vì DataGrid phản ứng mạnh với 
                // sự kiện Remove và Add của ObservableCollection.
                ForceInputList.RemoveAt(index);
                ForceInputList.Insert(index, existingEntry);

                MessageBox.Show($"Đã GHI ĐÈ nội lực thành công cho móng: {existingEntry.Mong.TenMong}\n" +
                                $"M = {existingEntry.Moment:N2}, N = {existingEntry.AxialForce:N2}, Q = {existingEntry.ShearForce:N2}",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Giữ nguyên giá trị đang nhập
            }
            else
            {
                MessageBox.Show("Lỗi: Không tìm thấy móng trong danh sách để cập nhật.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}