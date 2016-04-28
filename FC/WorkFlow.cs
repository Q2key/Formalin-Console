using System.Collections.Generic;

namespace FC
{
    public class Workflow
    {
        public string WfName { get; set; }
        public string WfDpi { get; set; }
        public string WfPlateType { get; set; }
        public int TotalPlateCount { get;set; }
        public List<Plates> WfPlateList { get; set; }
}
}