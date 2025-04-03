using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework
{
    public class EmailCommentPortofolioResult
    {
        public string IdReceive{ get; set; }

        public string ReceiveName { get; set; }

        public string IdAcademicYear { get; set; }

        public string IdStudent { get; set; }

        public int Semester { get; set; }

        public string UserNameComment { get; set; }

        public string IdUserCreate { get; set; }

        public string UserNameCreate { get; set; }

        public string TabMenu { get; set; }

        public string TabMenuIdx { get; set; }

        public DateTime Date { get; set; }

        public string SchoolName { get; set; }

        public string ForWho { get; set; }

        public string Comment { get; set; }

        public List<DataParentCourseworkAnecdotal> DataParents { get; set; }


    }
}
