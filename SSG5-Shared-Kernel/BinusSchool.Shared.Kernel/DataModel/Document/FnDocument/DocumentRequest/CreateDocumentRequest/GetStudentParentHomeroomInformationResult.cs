using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class GetStudentParentHomeroomInformationResult
    {
        public GetStudentParentHomeroomInformationResult_StudentInformation StudentInformation { get; set; }
        public List<GetStudentParentHomeroomInformationResult_ParentInformation> ParentInformationList { get; set; }
        public List<GetStudentParentHomeroomInformationResult_HomeroomHistory> HomeroomHistoryList { get; set; }
    }

    public class GetStudentParentHomeroomInformationResult_StudentInformation
    {
        public NameValueVm Student { get; set; }
        public DateTime? BirthDate { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public GetStudentParentHomeroomInformationResult_StudentStatusInfo StudentStatus { get; set; }
        public int Semester { get; set; }
        public DateTime? JoinDate { get; set; }
    }

    public class GetStudentParentHomeroomInformationResult_StudentStatusInfo : ItemValueVm
    {
        public DateTime? StudentStatusStartDate { get; set; }
    }

    public class GetStudentParentHomeroomInformationResult_ParentInformation
    {
        public NameValueVm Parent { get; set; }
        public ItemValueVm ParentRole { get; set; }
        public string ResidencePhoneNumber { get; set; }
        public string MobilePhoneNumber { get; set; }
        public string Email { get; set; }
    }

    public class GetStudentParentHomeroomInformationResult_HomeroomHistory
    {
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public int Semester { get; set; }
        public NameValueVm HomeroomTeacher { get; set; }
    }
}
