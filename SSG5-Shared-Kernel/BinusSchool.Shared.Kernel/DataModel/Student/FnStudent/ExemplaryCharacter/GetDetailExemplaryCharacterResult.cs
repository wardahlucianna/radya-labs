using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter
{
    public class GetDetailExemplaryCharacterResult
    {
        public string IdExemplary { get; set; }
        public DateTime DatePosted { get; set; }
        public DateTime? ExemplaryDate { get; set; }
        public ItemValueVm Category { get; set; }
        public string Description { get; set; }
        public int CountLikes { set; get; }
        public bool IsYouLiked { set; get; }
        public string Postedby { set; get; }
        public DateTime PostedDate { set; get; }
        public string PostedDateView { set; get; }
        public string Updatedby { set; get; }
        public string UpdatedDateView { set; get; }
        public List<GetDetailExemplaryCharacterResult_Student> StudentList { get; set; }
        public List<GetDetailExemplaryCharacterResult_Value> ValueList { get; set; }
        public List<GetDetailExemplaryCharacterResult_Attachment> AttachmentList { get; set; }
    }
    public class GetDetailExemplaryCharacterResult_Value
    {
        public string IdExemplaryValue { get; set; }
        public ItemValueVm Value { get; set; }
    }

    public class GetDetailExemplaryCharacterResult_Student
    {
        public string IdExemplaryStudent { get; set; }
        public NameValueVm Student { get; set; }
        public ItemValueVm Homeroom { get; set; }
    }

    public class GetDetailExemplaryCharacterResult_Attachment
    {
        public string IdExemplaryAttachment { get; set; }
        public string Url { get; set; }
        public string OriginalFileName { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
        public string FileExtension { get; set; }
        public string UrlWithSASToken { get; set; }
    }
}
