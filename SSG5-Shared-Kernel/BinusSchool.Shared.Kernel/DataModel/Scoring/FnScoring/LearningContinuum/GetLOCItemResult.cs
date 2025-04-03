using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum
{
    public class GetLOCItemResult
    {
        public DateTime? PeriodEndDate { get; set; }
        public List<GetLOCItemResult_LOCItem> LOCList { get;set; }
    }

    public class GetLOCItemResult_LOCItem
    {
        public ItemValueVm LearningContinuumType { get; set; }
        public List<GetLOCItemResult_LOCItem_Categories> LearningContinuumCategories { get; set; }
    }
    
    public class GetLOCItemResult_LOCItem_Categories : ItemValueVm
    {
        public List<GetLOCItemResult_LOCItem_Categories_Phase> Phases { get; set; }
    }

    public class GetLOCItemResult_LOCItem_Categories_Phase
    {
        public int Phase { get; set; }
        public List<GetLOCItemResult_LOCItem_Categories_Phase_Item> LOCItems { get; set; }
    }

    public class GetLOCItemResult_LOCItem_Categories_Phase_Item : ItemValueVm
    {
        public string IdLearningContinuum { get; set; }
    }
}
