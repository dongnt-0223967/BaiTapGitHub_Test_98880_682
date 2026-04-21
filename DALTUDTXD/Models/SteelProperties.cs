using System;
using System.Collections.Generic;
using System.Linq;

namespace DALTUDTXD.Models
{
    public class SteelProperties
    {
        public string Grade { get; set; }  // CB240-T, CB300-V, etc.
        public double Rs { get; set; }     // MPa
        public double Rsc { get; set; }    // MPa
        public double Rsw { get; set; }    // MPa
        public double E { get; set; }      // MPa

        private static readonly List<SteelProperties> _steelDatabase = new List<SteelProperties>
        {
            new SteelProperties { Grade = "CB240-T", Rs = 210, Rsc = 210, Rsw = 170, E = 200000 },
            new SteelProperties { Grade = "CB300-V", Rs = 260, Rsc = 260, Rsw = 210, E = 200000 },
            new SteelProperties { Grade = "CB400-V", Rs = 350, Rsc = 350, Rsw = 280, E = 200000 },
            new SteelProperties { Grade = "CB500-V", Rs = 435, Rsc = 400, Rsw = 300, E = 200000 }
        };

        // Hệ số chuyển đổi từ MPa sang T/m²
        private const double MPA_TO_TM2 = 102; // 1 MPa = 102 T/m²

        public static double GetRsInTM2(string grade)
        {
            var steel = _steelDatabase.FirstOrDefault(s => s.Grade == grade);
            if (steel == null)
                throw new ArgumentException($"Không tìm thấy loại thép {grade}");
            
            return steel.Rs * MPA_TO_TM2; // Chuyển đổi từ MPa sang T/m²
        }

        public static SteelProperties GetProperties(string grade)
        {
            var steel = _steelDatabase.FirstOrDefault(s => s.Grade == grade);
            if (steel == null)
                throw new ArgumentException($"Không tìm thấy loại thép {grade}");
            
            return steel;
        }
    }
} 