using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDocument
{
    public class AdditionalDocumentByStudentIDRequest : CollectionRequest
    {
        public string Role { get; set; }
    }
}
