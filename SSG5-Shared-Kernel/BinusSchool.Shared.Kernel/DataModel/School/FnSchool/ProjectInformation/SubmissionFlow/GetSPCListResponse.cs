using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.ProjectInformation.SubmissionFlow
{
    public class GetSPCListResponse
    {
        public string IdSchoolProjectCoordinator {  get; set; }
        public string FullName { get; set; }
        public string BinusianEmail { get; set; }
        public ItemValueVm School { get; set; }
        public string Remarks { get; set; }
        public string PhotoUrl { get; set; }
        public string IdBinusian { get; set; }

    }
}
