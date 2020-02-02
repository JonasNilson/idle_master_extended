using System.Collections.Generic;

namespace IdleMaster
{
    public class EnhancedsteamHelper
    {
        // ReSharper disable once InconsistentNaming
        public List<Avg> Avg_Values { get; set; }
    }

    public class Avg
    {
        public int AppId { get; set; }

        // ReSharper disable once InconsistentNaming
        public double Avg_Price { get; set; }
    }
}
