using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignmentHistory
{
    public class GetTeacherAssignmentHistoryRequest : Pagination, IGetAll
    {
        public List<string> IdUser { get; set; }
        public bool? GetAll { get; set; }
        public bool CanCountWithoutFetchDb(int itemsCount)
        {
            return (GetAll.HasValue && GetAll.Value) || (Page == 1 && itemsCount < Size);
        }
    }
}
