using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALTUDTXD.Models
{
    public class GeologicalAxis
    {
        public string Name { get; set; }
        public ObservableCollection<FoundationEntry> Entries { get; set; }

        public GeologicalAxis()
        {
            Entries = new ObservableCollection<FoundationEntry>();
        }
    }
} 