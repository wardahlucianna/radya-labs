using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Teaching.FnAssignment.NonTeachingLoad
{
    public class GetPreviousHierarchyNonTeachingLoadHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetPreviousHierarchyNonTeachingLoadRequest.IdAcadyear),
            nameof(GetPreviousHierarchyNonTeachingLoadRequest.IdPosition),
        });

        private readonly ITeachingDbContext _dbContext;

        public GetPreviousHierarchyNonTeachingLoadHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetPreviousHierarchyNonTeachingLoadRequest>(_requiredParams.Value);
            string previousAY = param.IdAcadyear.GetPreviousAcademicYear(4);

            var previousNonTeachingLoad = await _dbContext.Entity<MsNonTeachingLoad>()
                .Where(x => x.IdAcademicYear == previousAY && x.Category == param.Category && x.IdTeacherPosition == param.IdPosition).FirstOrDefaultAsync(CancellationToken);

            List<GetRecentHierarchy> defaultHierarchies = GenerateDefaultHierarchy();

            if (previousNonTeachingLoad != null && param.Category == Common.Model.Enums.AcademicType.Academic)
            {
                var hierarchies = JsonConvert.DeserializeObject<List<CreateHierarchyResult>>(previousNonTeachingLoad.Parameter);

                foreach (var defaultHierarchy in defaultHierarchies)
                {
                    if (hierarchies.Any(x => x.Class.ToLower() == defaultHierarchy.Name.ToLower()))
                        defaultHierarchy.IsActive = true;
                }
            }

            return Request.CreateApiResult2(defaultHierarchies as object);
        }

        private List<GetRecentHierarchy> GenerateDefaultHierarchy()
        {
            var result = new List<GetRecentHierarchy>();

            for (int i = 0; i < 6; i++)
            {
                result.Add(new GetRecentHierarchy
                {
                    Name = i == 0 ? "Level" :
                                i == 1 ? "Department" :
                                    i == 2 ? "Grade" :
                                        i == 3 ? "Subject" :
                                            i == 4 ? "Streaming" :
                                                i == 5 ? "Classroom" :
                                            string.Empty
                });
            }

            return result;
        }
    }
}
