using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.POI
{
    public class GetPOIPerStudentResult
    {
        public string IdStudent { get; set; }
        public int Semester { get; set; }
        public List<GetPOIPerStudentResult_UnitOfInquiryCollection> UnitOfInquiryList { get; set; }
    }

    public class GetPOIPerStudentResult_UnitOfInquiryCollection
    {

        public ItemValueVm UnitOfInquiry { get; set; }
        public ItemValueVm CentralIdea { get; set; }
        public List<ItemValueVm> LinesOfInquiryList { get; set; }
        public string Comment { get; set; }
    }
}
