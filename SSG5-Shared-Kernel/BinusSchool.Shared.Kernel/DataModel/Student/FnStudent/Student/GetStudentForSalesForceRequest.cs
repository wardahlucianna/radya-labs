using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentForSalesForceRequest : Pagination, IGetAll
    {
        public List<string> IdStudent { get; set; }
        public bool? GetAll { get; set; }
        public bool CanCountWithoutFetchDb(int itemsCount)
        {
            return (GetAll.HasValue && GetAll.Value) || (Page == 1 && itemsCount < Size);
        }
    }
}
