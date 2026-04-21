using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DALTUDTXD.Views
{
    /// <summary>
    /// Interaction logic for Page2View.xaml
    /// </summary>
    public partial class Page2View : UserControl
    {
        private Point _lastMousePos;
        private bool _isPanning;

        public Page2View()
        {
            InitializeComponent();
        }

        //  ZOOM BẰNG LĂN CHUỘT 
        private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;

            double oldScale = ScaleTf.ScaleX;
            double newScale = oldScale * zoomFactor;

            if (newScale < 0.3 || newScale > 5)
                return;

            Point mousePos = e.GetPosition(ViewportCanvas);

            // tọa độ chuột trong không gian DrawingCanvas
            double absX = (mousePos.X - TranslateTf.X) / oldScale;
            double absY = (mousePos.Y - TranslateTf.Y) / oldScale;

            ScaleTf.ScaleX = newScale;
            ScaleTf.ScaleY = newScale;

            // bù trừ translate để giữ điểm dưới chuột
            TranslateTf.X = mousePos.X - absX * newScale;
            TranslateTf.Y = mousePos.Y - absY * newScale;
        }


        //  PAN BẰNG CHUỘT PHẢI 
        private void Viewport_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isPanning = true;
            _lastMousePos = e.GetPosition(ViewportCanvas);
            ViewportCanvas.CaptureMouse();
            Cursor = Cursors.SizeAll;
        }

        private void Viewport_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isPanning = false;
            ViewportCanvas.ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
        }

        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isPanning) return;

            Point currentPos = e.GetPosition(ViewportCanvas);
            Vector delta = currentPos - _lastMousePos;

            TranslateTf.X += delta.X;
            TranslateTf.Y += delta.Y;

            _lastMousePos = currentPos;
        }
    }
}
