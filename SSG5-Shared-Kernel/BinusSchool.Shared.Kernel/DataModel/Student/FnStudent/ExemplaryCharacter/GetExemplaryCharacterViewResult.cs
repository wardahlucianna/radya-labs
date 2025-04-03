using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter
{
    public class GetExemplaryCharacterViewResult : CodeWithIdVm
    {
        public string IdExemplary { set; get; }
        public string IdAcademicYear { set; get; }
        public string Student { set; get; }
        public List<ExemplaryCharacterView_Student> StudentList { set; get; }
        public ItemValueVm Category { set; get; }
        public int CountLikes { set; get; }
        public bool IsYouLiked { set; get; }
        public string Postedby { set; get; }
        public DateTime PostedDate { set; get; }
        public string PostedDateView { set; get; }
        public string Updatedby { set; get; }      
        public string UpdatedDateView { set; get; }

        public List<ExemplaryCharacterView_Attachment> ExemplaryAttachments { set; get; }
        public List<ExemplaryCharacterView_Value> ValueList { set; get; }


    }
    public class ExemplaryCharacterView_Attachment
    {
        public string Url { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileExtension { get; set; }
        public decimal FileSize { get; set; }
        public string UrlWithSASToken { get; set; }
    }
    public class ExemplaryCharacterView_Value
    {
        public string IdExemplaryValue { get; set; }
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
    }
    public class ExemplaryCharacterView_Student
    {
        public NameValueVm Student { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Homeroom { get; set; }
    }
}
