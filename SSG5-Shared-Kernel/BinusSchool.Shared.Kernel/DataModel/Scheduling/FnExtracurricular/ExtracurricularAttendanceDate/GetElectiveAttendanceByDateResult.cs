using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendanceDate
{
    public class GetElectiveAttendanceByDateResult
    {
        public NameValueVm Elective { set; get; }   
        public List<NameValueVm> Supervisor { set; get; }
        public string  Time { set; get; }  
        public string LasUpdateBy { set; get; }
        public string IdUserUpdate { set; get; }
        public DateTime? UpdateTime { set; get; }
        public string IdElectiveGeneratedAtt { get; set; }
    }
}
