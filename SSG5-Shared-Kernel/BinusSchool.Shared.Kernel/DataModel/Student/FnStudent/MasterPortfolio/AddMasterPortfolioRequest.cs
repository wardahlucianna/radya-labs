using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.MasterPortfolio
{
    public class AddMasterPortfolioRequest
    {
        public string Name { get; set; }
        public LearnerProfile Type { get; set; }
    }
}
