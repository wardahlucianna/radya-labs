using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class GetListEvidencesRequest : CollectionSchoolRequest
    {
        public string IdExperience { get; set; }
        public string Role { get; set; }
    }
}
