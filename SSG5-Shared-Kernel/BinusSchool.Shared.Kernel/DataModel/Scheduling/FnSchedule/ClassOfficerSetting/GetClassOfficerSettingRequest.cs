using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ClassOfficerSetting
{
    public class GetClassOfficerSettingRequest : CollectionRequest
    {
        public string IdBinusian { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeRoom { get; set; }
        public string Semester { get; set; }
        public string Position { get; set; }
    }
}
