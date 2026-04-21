using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DALTUDTXD.Models
{
    public class FoundationEntry
    {
        public int Sothutulopdat { get; set; }
        public int Sohieudiachat { get; set; }
        public string Tenlopdat { get; set; }
        public float Chieudaylopdat { get; set; }
        public double Khoiluongtunhien { get; set; }
        public double Gocmasattrong { get; set; }
        public double Lucdinhket { get; set; }
        public double Modunbiendang { get; set; }
        public string Vitrimong { get; set; }
        public double RenderTop { get; set; }    // Vị trí Y bắt đầu vẽ
        public double RenderHeight { get; set; } // Chiều cao 
        public SolidColorBrush SoilColor { get; set; } // Màu sắc hiển thị
    }
}
