using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff.KITASStatus;
using Refit;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface IKITASStatus : IFnStaff
    {
        [Get("/staff/kitas-status")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetKITASStatus(CollectionRequest query);
    }
}