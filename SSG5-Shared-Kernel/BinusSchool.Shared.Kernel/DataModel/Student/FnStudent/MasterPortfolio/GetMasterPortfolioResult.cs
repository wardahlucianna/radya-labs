using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.MasterPortfolio
{
    public class GetMasterPortfolioResult : CodeWithIdVm
    {
        public string Name { get; set; }
        public LearnerProfile Type { get; set; }
        public string TypeName { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
