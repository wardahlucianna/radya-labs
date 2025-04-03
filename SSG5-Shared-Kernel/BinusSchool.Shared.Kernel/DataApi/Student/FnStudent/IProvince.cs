using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.Province;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IProvince : IFnStudent
    {
        [Get("/student/province")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetProvinces(GetProvinceRequest query);
    }
}
