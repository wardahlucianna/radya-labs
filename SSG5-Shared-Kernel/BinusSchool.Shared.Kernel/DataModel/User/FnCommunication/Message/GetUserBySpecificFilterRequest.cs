using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;


namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class GetUserBySpecificFilterRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdRole { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string Semester { get; set; }
        public string IdHomeroom { get; set; }
    }
}
