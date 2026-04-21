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
    /// Interaction logic for Page1View.xaml
    /// </summary>
    public partial class Page1View : UserControl
    {
        private Point _lastPoint;
        private bool _isDragging = false;

        public Page1View()
        {
            InitializeComponent();
        }

        private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoom = e.Delta > 0 ? 0.1 : -0.1;
            if (MainScale.ScaleX + zoom > 0.1 && MainScale.ScaleX + zoom < 10)
            {
                MainScale.ScaleX += zoom;
                MainScale.ScaleY += zoom;
            }
            e.Handled = true;
        }

        private void Viewport_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _lastPoint = e.GetPosition(this);
            ViewportCanvas.CaptureMouse();
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void Viewport_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ViewportCanvas.ReleaseMouseCapture();
            Mouse.OverrideCursor = null;
        }

        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPoint = e.GetPosition(this);
                Vector delta = currentPoint - _lastPoint;

                MainTranslate.X += delta.X;
                MainTranslate.Y += delta.Y;

                _lastPoint = currentPoint;
            }
        }
    }
}
