using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentExitSetting
{
    public class UpdateStudentExitSettingRequest
    {
        public UpdateStudentExitSettingRequest()
        {
            Request = new List<ListStudentExitSettingRequest>();
        }
        public List<ListStudentExitSettingRequest> Request { get; set; }
        
    }

    public class ListStudentExitSettingRequest
    {
        public string IdHomeroomStudent { get; set; }
        public string AcademicYear { get; set; }
        public bool IsExit { get; set; }
    }
}
