using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant
{
    public class ExportExcelStudentParticipantResult
    {
        public NameValueVm Extracurricular { get; set; }
        public int ParticipantMin { get; set; }
        public int ParticipantMax { get; set; }
        public decimal Price { get; set; }

        public List<LevelGrade> LevelGradeList { get; set; }
        public List<GetStudentParticipantByExtracurricularResult> ParticipantList { get; set; }
    }

    public class ExportExcelStudentParticipantResult_ParamDesc
    {
        public string AcademicYear { get; set; }
        public int Semester { get; set; }
    }
}
