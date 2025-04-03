using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class TransferMasterExtracurricularRequest
    {
        public string IdAcademicYearFrom { get; set; }
        public string IdAcademicYearDest { get; set; }
        public int SemesterFrom { get; set; }
        public int SemesterDest { get; set; }
        public List<MasterExtracurricularData> MasterExtracurricularData { get; set; }
    }

    public class MasterExtracurricularData
    {
        public string IdExtracurricular { get; set; }
        public bool IsTransferParticipant { get; set; }
    }
}
