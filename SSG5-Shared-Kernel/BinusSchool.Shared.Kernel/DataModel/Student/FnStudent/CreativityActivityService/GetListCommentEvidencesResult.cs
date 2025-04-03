using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService
{
    public class GetListCommentEvidencesResult : CodeWithIdVm
    {
        public string IdEvidences { get; set; }
        public string IdUserComment { get; set; }
        public string DisplayName { get; set; }
        public DateTime? DateIn { get; set; }
        public string Comment { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
