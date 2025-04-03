using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.MasterPortfolio
{
    public class GetMasterPortfolioRequest : CollectionSchoolRequest
    {
        public LearnerProfile? Type { get; set; }
    }
}
