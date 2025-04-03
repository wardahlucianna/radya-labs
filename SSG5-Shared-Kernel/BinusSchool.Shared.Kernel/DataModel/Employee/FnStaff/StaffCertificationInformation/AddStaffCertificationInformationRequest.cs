using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Employee.FnStaff.StaffCertificationInformation
{
    public class AddStaffCertificationInformationRequest
    {
        public string IdBinusian { get; set; }
        public int IdCertificationType { get; set; }
        public string CertificationNumber { get; set; }
        public string CertificationName { get; set; }
        public string CertificationYear { get; set; }
        public string IssuedCertifInstitution { get; set; }
        public DateTime CertificationExpDate { get; set; }
    }
}
