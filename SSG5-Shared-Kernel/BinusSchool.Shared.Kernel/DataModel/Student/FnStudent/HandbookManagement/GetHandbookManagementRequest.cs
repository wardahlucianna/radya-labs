using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.HandbookManagement
{
    public class GetHandbookManagementRequest : CollectionRequest
    {
        public string IdUser { get; set; } // ini di set valuenya Ketika Menu Hanbook, di Hanbook Management tidak perlu

        public string IdAcademicYear { get; set; }

        public List<HandbookFor> ViewFor { get; set; }

        public string Idlevel { get; set; } // ini di set valuenya Ketika Hanbook Management, di Handbook tidak perlu
        public bool IsHandbookForm { get; set; }
    }
}
