using System.Windows;
using System.Windows.Input;

namespace DALTUDTXD.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        // Cho phép kéo cửa sổ khi nhấn giữ chuột trái trên Window
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // Xử lý nút thu nhỏ cửa sổ
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Xử lý nút đóng cửa sổ
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Reset_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Tính năng đang được phát triển", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        // Xử lý nút đăng nhập
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Lấy thông tin đăng nhập từ TextBox và PasswordBox
            string username = txtUser.Text;
            string password = txtPassword.Password;

            // Kiểm tra tài khoản và mật khẩu
            if (username == "admin" && password == "admin123")
            {
                // Nếu đăng nhập thành công, mở MainView và đóng LoginView
                MainView mainView = new MainView();
                mainView.Show();
                this.Close();
            }
            else
            {
                // Nếu đăng nhập sai, hiển thị thông báo lỗi màu đỏ
                txtError.Text = "Tài khoản hoặc mật khẩu không đúng!";
            }

        }
    }
}
