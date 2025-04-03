using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Employee.FnStaff.StaffCertificationInformation
{
    public class GetStaffCertificationInformationResult : ItemValueVm
    {
        public string IdCertificationStaff { get; set; }
        public string IdBinusian { get; set; }
        public ItemValueVm IdCertificationType { get; set; }
        public string CertificationNumber { get; set; }
        public string CertificationName { get; set; }
        public string CertificationYear { get; set; }
        public string IssuedCertifInstitution { get; set; }
        public DateTime CertificationExpDate { get; set; }
    }
}
