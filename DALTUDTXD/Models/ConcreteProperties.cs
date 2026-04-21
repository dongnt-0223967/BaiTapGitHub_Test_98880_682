using System;
using System.Collections.Generic;
using System.Linq;

namespace DALTUDTXD.Models
{
    public class ConcreteProperties
    {
        public string Grade { get; set; }  // B15, B20, etc.
        public string SteelGrade { get; set; }  // M200, M250, etc.
        public double Rb { get; set; }  // MPa
        public double Rbt { get; set; }  // MPa
        public double E { get; set; }   // MPa

        private static readonly List<ConcreteProperties> _concreteDatabase = new List<ConcreteProperties>
        {
            new ConcreteProperties { Grade = "B15", SteelGrade = "M200", Rb = 8.5, Rbt = 0.75, E = 24000 },
            new ConcreteProperties { Grade = "B20", SteelGrade = "M250", Rb = 11.5, Rbt = 0.90, E = 27500 },
            new ConcreteProperties { Grade = "B22.5", SteelGrade = "M300", Rb = 13.0, Rbt = 1.00, E = 28750 },
            new ConcreteProperties { Grade = "B25", SteelGrade = "M350", Rb = 14.5, Rbt = 1.05, E = 30000 },
            new ConcreteProperties { Grade = "B30", SteelGrade = "M400", Rb = 17.0, Rbt = 1.15, E = 32500 },
            new ConcreteProperties { Grade = "B35", SteelGrade = "M450", Rb = 19.5, Rbt = 1.30, E = 34500 },
            new ConcreteProperties { Grade = "B40", SteelGrade = "M500", Rb = 22.0, Rbt = 1.40, E = 36000 },
            new ConcreteProperties { Grade = "B45", SteelGrade = "M600", Rb = 25.0, Rbt = 1.50, E = 37000 },
            new ConcreteProperties { Grade = "B50", SteelGrade = "M700", Rb = 27.5, Rbt = 1.60, E = 38000 },
            new ConcreteProperties { Grade = "B55", SteelGrade = "M700", Rb = 30.0, Rbt = 1.70, E = 39000 },
            new ConcreteProperties { Grade = "B60", SteelGrade = "M800", Rb = 33.0, Rbt = 1.80, E = 39500 }
        };

        // Hệ số chuyển đổi từ MPa sang T/m²
        private const double MPA_TO_TM2 = 102; // 1 MPa = 102 T/m²

        public static double GetRbtInTM2(string grade)
        {
            var concrete = _concreteDatabase.FirstOrDefault(c => c.Grade == grade);
            if (concrete == null)
                throw new ArgumentException($"Không tìm thấy cấp độ bê tông {grade}");
            
            return concrete.Rbt * MPA_TO_TM2; // Chuyển đổi từ MPa sang T/m²
        }

        public static ConcreteProperties GetProperties(string grade)
        {
            var concrete = _concreteDatabase.FirstOrDefault(c => c.Grade == grade);
            if (concrete == null)
                throw new ArgumentException($"Không tìm thấy cấp độ bê tông {grade}");
            
            return concrete;
        }
    }
} 