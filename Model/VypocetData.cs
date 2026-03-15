using System;
using System.Collections.Generic;
using System.Text;

namespace WPFTerminVkladKalk.Model
{
    // datový model aplikace pro uložení výsledků výpočtu a zobrazení v historii
    public class VypocetData
    {
        public DateTime DatumVkladu { get; set; } = DateTime.Now;
        public DateTime DatumSplatnosti { get; set; }
        public double Vklad { get; set; }
        public double UrokPa { get; set; }
        public int PocetDni { get; set; } // Skutečný počet dní (ACT)
        public double CistyUrok { get; set; }
    }
}
