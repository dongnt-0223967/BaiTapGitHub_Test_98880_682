using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALTUDTXD.Models
{
    public class ForceInputEntry
    {
        public ConstructionEntry Mong { get; set; } //Tham chiếu đến đối tượng
        public double Moment { get; set; } // Mômen uốn (M)
        public double AxialForce { get; set; } // Lực dọc (N)
        public double ShearForce { get; set; } // Lực cắt (Q)
    }
}
