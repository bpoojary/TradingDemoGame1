using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IndiaHacksGame
{
    public class SymbolData
    {
        public string SymbolName { get; set; }
        public CompanyNews News { get; set; }
    }
    public class CompanyNews
    {
        public string Content { get; set; }
        public bool isPositive { get; set; }
        public double SwingPercentage { get; set; }
    }
}
