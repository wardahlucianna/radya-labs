using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class GetListCommentEvidencesRequest : CollectionSchoolRequest
    {
        public string IdEvidences { get; set; }
        public string IdUser { get; set; }
        public string Role { get; set; }
    }
}
