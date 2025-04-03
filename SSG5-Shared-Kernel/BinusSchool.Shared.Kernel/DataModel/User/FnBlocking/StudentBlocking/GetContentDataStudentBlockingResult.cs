using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnBlocking.StudentBlocking
{
    public class GetContentDataStudentBlockingResult
    {

    }
    public class GetContentDataColumnsNameQueryResult 
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string Homeroom { get; set; }
        public string IdBlockingCategory { get; set; }
        public string BlockingCategory { get; set; }
        public string IdBlockingType { get; set; }
        public string BlockingType { get; set; }
        public bool IsBlocking { get; set; }
        public bool CanEdit { get; set; }
        public int Order { get; set; }
    }
}
