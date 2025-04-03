using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparation
{
    public class GetEmailTextbookResult
    {
        public List<string> IdUserPic { get; set; }
        public string NameCreated { get; set; }
        public string NameUser { get; set; }
        public string IdUser { get; set; }
        public List<GetEmailTextbook> Textbooks { get; set; }
    }

    public class GetEmailTextbook
    {
        public string IdUserApproval { get; set; }
        public string IdUserCreated { get; set; }
        public string Id { get; set; }
        public string Subject { get; set; }
        public string Grade { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Isbn { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
        public string Link { get; set; }
    }
}
