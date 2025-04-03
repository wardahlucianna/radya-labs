using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class GetListExperienceRequest : CollectionSchoolRequest
    {
        public string IdUser { get; set; }
        public string IdStudent { get; set; }
        public bool IsCASCoordinator { get; set; }
        public string ViewAs { get; set; }
        public List<string> IdAcademicYear { get; set; }
    }
}
