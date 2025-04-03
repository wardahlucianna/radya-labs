using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant
{
    public class GetUnselectedStudentByHomeroomResult
    {
        public NameValueVm Student { get; set; }
        public NameValueVm Homeroom { get; set; }
        public bool IsJoinedMaxExtracurricular { get; set; }
        public string ReviewDate { get; set; }
    }
}
