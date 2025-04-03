using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.Bank;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IBank : IFnStudent
    {
        [Get("/student/bank")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetBanks(CollectionRequest query);
    }
}
