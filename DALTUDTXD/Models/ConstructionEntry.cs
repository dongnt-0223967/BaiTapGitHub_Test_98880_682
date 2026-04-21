using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALTUDTXD.Models
{
    public class ConstructionEntry
    {
        public string TenMong { get; set; }
        public double ChieuSauChonMong { get; set; }
        public double ChieuRongMong { get; set; }
        public string CapDoBeTong { get; set; }
        public string LoaiThep { get; set; }
        public double ChieuDayLopBaoVe { get; set; }
        public double BeDayTuong { get; set; }
        public double ChieuCaoDai { get; set; }
        public FoundationEntry SoilLayer { get; set; }
    }
}
