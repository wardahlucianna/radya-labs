using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Teaching.FnAssignment.NonTeachingLoad.Validator;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.Metadata;
using BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Newtonsoft.Json;

namespace BinusSchool.Teaching.FnAssignment.NonTeachingLoad
{
    public class NonTeachingLoadHandler : FunctionsHttpCrudHandler
    {
        private readonly ITeachingDbContext _teachingDbContext;
        //private readonly IApiService<IMetadata> _metaDataApi;
        private readonly IApiService<IAcademicYear> _academicYears;
        private readonly IStringLocalizer _localizer;

        public NonTeachingLoadHandler(ITeachingDbContext teachingDbContext, IApiService<IMetadata> metaData,
            IApiService<IAcademicYear> academicYears,
            IStringLocalizer localizer)
        {
            _teachingDbContext = teachingDbContext;
            _localizer = localizer;
            _academicYears = academicYears; 
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _teachingDbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _teachingDbContext.Entity<MsNonTeachingLoad>()
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas)
            {
                data.IsActive = false;
                _teachingDbContext.Entity<MsNonTeachingLoad>().Update(data);
            }
            
            await _teachingDbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);
            
            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {

            var query = await _teachingDbContext.Entity<MsNonTeachingLoad>()
                 .Include(x => x.TeacherPosition)
                 .Include(x => x.AcademicYear)
                 .Where(x => x.Id == id)
                  .Select(x => new GetNonTeachLoadDetailResult
                  {
                      Id = x.Id,
                      Acadyear = new CodeWithIdVm
                      {
                          Id = x.AcademicYear.Id,
                          Code = x.AcademicYear.Code,
                          Description = x.AcademicYear.Description
                      },
                      Category = x.Category,
                      Load = x.Load,
                      Position = new CodeWithIdVm
                      {
                          Id = x.TeacherPosition.Id,
                          Code = x.TeacherPosition.Code,
                          Description = x.TeacherPosition.Description
                      },
                      Parameter = x.Parameter
                  })
                  .FirstOrDefaultAsync(CancellationToken);

            List<GetRecentHierarchy> defaultHierarchies = GenerateDefaultHierarchy();
            if (!string.IsNullOrEmpty(query.Parameter))
            {
                var hierarchies = JsonConvert.DeserializeObject<List<CreateHierarchyResult>>(query.Parameter);

                foreach (var defaultHierarchy in defaultHierarchies)
                {
                    if (hierarchies.Any(x => x.Class.ToLower() == defaultHierarchy.Name.ToLower()))
                        defaultHierarchy.IsActive = true;
                }
            }

            query.Hierarchies = defaultHierarchies;

            return Request.CreateApiResult2(query as object);
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
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
