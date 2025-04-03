using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselorData
{
    public class GetDetailCounselorDataResult : GetCounselorDataResult
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public ItemValueVm Role { get; set; }
        public ItemValueVm Position { get; set; }
        public List<ItemValueVm> ListDetailGradeCounselorData { get; set; }
        public List<ItemValueVm> ListDetailLevelCounselorData { get; set; }
        public List<AttachmentCounselorData> ListAttachmentCounselorData { get; set; }
        
    }

}
