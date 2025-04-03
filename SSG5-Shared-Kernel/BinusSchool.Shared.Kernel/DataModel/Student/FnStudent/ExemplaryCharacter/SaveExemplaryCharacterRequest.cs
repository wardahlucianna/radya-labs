using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter
{
    public class SaveExemplaryCharacterRequest
    {
        public string IdExemplary { get; set; }
        public string IdAcademicYear { get; set; }
        //public DateTime DatePosted { get; set; }
        public DateTime? ExemplaryDate { get; set; }
        public string IdExemplaryCategory { get; set; }
        public string Description { get; set; }
        public List<SaveExemplaryCharacterRequest_Student> Student { get; set; }
        public List<SaveExemplaryCharacterRequest_Value> ValueList { get; set; }
        public List<SaveExemplaryCharacterRequest_Attachment> Attachment { get; set; }
    }

    public class SaveExemplaryCharacterRequest_Value
    {
        // public string IdExemplaryCategory { get; set; }
        public string IdLtExemplaryValue { get; set; }
    }

    public class SaveExemplaryCharacterRequest_Student
    {
        public string IdExemplaryStudent { get; set; }
        public string IdStudent { get; set; }
        public string IdHomeroom { get; set; }
    }

    public class SaveExemplaryCharacterRequest_Attachment
    {
        public string IdExemplaryAttachment { get; set; }
        public string Url { get; set; }
        public string OriginalFileName { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public decimal FileSize { get; set; }
    }
}
