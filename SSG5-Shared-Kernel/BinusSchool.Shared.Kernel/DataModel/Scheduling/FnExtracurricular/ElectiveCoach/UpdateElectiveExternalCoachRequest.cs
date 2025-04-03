using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoach
{
    public class UpdateElectiveExternalCoachRequest
    {       
        public string IdUser { set; get; }
        public string UserName { set; get; }
        public string IdExternalCoach { set; get; }
        public string IdExtracurricularExtCoachTaxStatus { set; get; }
        public string NPWP { set; get; }
        public string AccountBank { set; get; }
        public string AccountBankBranch { set; get; }
        public string AcountNumber { set; get; }
        public string AccountName { set; get; }
    }
}
