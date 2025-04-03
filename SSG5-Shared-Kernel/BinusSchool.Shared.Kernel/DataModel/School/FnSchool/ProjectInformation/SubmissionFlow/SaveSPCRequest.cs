using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.ProjectInformation.SubmissionFlow
{
    public class SaveSPCRequest
    {
        public string IdSchoolProjectCoordinator { get; set; }
        public string IdSchool { get; set; }
        public string IdBinusian { get; set; }
        public string Remarks { get; set; }
        public string PhotoUrl { get; set; }
        public string FileType { get; set; }
    }
}
