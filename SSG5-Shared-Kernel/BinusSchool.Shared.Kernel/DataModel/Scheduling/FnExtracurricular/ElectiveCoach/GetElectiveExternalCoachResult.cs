using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoach
{
    public class GetElectiveExternalCoachResult : ItemValueVm
    {
        //public string IdUser { set; get; }
        //public string UserName { set; get; }
        public string IdExternalCoach { set; get; }        
        public ItemValueVm TaxStatus { set; get; }
        public string NPWP { set; get; }
        public string AccountBank { set; get; }
        public string AcountNumber { set; get; }
        public string AcountName { set; get; }
        public bool CanDelete { set; get; }
    }
}
