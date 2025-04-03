using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetDownloadStudentCertificationResult : CodeWithIdVm
    {
        public string StudentName { get; set; }
        public string SchoolName { get; set; }
        public string AdmissionAY { get; set; }
        public string CurrentAY { get; set; }
        public string Gender { get; set; }
        public string VicePrincipalName { get; set; }
        public string SchoolDescription { get; set; }
        public string SchoolAddress { get; set; }
        public string SchoolTel { get; set; }
        public string SchoolExt { get; set; }
        public string SchoolEmail { get; set; }
        public List<DataActivityAward> DataActivityAward { get; set; }

    }

    public class DataActivityAward
    {
        public string SchoolActivityName { get; set; }
        public string DateYear { get; set; }
        public string StatusInvolvement { get; set; }
    }
}