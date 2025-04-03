using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant
{
    public class AddStudentParticipantByExcelResult
    {
        public int TotalRowData { get; set; }
        public int TotalRowSuccess { get; set; }
        public int TotalRowFailed { get; set; }
        public List<AddStudentParticipantByExcelResult_Error> ErrorList { get; set; }
    }

    public class AddStudentParticipantByExcelResult_Error
    {
        public string Error { get; set; }
    }
}
