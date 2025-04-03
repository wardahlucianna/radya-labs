using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDocument
{
    public class AddStudentDocumentRequest
    {
        public string IdStudent { get; set; }
        public string IdDocument { get; set; }
        public string FileName { get; set; }
        public decimal FileSize { get; set; }
        public string IdVerificationStatus { get; set; }
        public string Comment { get; set; }
        public string IdDocumentStatus { get; set; }
        public string UserIn { get; set; }
        public bool IsStudentView { get; set; }
        //public DateTime DateIn { get; set; }
    }
}
