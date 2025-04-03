using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter
{
    public class GetListExemplaryCharacterSummaryRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public int? Semester { get; set; }
        public string IdHomeroom { get; set; }
        // search student by name or ID
        public string SearchStudent { get; set; }
        public bool IsPostedByMe { get; set; }
        public DateTime? StartDateExemplary { get; set; }
        public DateTime? EndDateExemplary { get; set; }
        public string IdExemplaryValue { get; set; }
    }
}
