using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.ReligionSubject;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IReligionSubject : IFnStudent
    {
        [Get("/student/religion-subject")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetReligionSubjects(CollectionRequest query);
    }
}