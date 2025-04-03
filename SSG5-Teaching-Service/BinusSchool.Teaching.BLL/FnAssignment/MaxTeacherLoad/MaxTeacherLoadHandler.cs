using System;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Common.Model;
using System.Collections.Generic;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Teaching.FnAssignment.MaxTeacherLoad.Validator;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Data.Model.Teaching.FnAssignment.MaxTeacherLoad;
using BinusSchool.Data.Model;
using Microsoft.Extensions.Localization;
using BinusSchool.Common.Utils;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Model.School.FnSchool.Metadata;

namespace BinusSchool.Teaching.FnAssignment.MaxTeacherLoad
{
    public class MaxTeacherLoadHandler : FunctionsHttpCrudHandler
    {
        private readonly ITeachingDbContext _teachingDbContext;
        private readonly IStringLocalizer _localizer;

        public MaxTeacherLoadHandler(ITeachingDbContext teachingDbContext, IApiService<IMetadata> metaData, IStringLocalizer localizer)
        {
            _teachingDbContext = teachingDbContext;
            _localizer = localizer;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _teachingDbContext.Entity<MsMaxTeacherLoad>()
                .Include(x=>x.AcademicYear)
               .Where(p => p.IdAcademicYear == id)
               .Select(p => new GetTeacherMaxLoadDetailResult()
               {
                   Id = p.Id,
                   Code = p.AcademicYear.Code,
                   Description = p.AcademicYear.Description,
                   MaxLoad = p.Max,
                   IdAcademicYear = p.IdAcademicYear
               }).FirstOrDefaultAsync();
            
            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<CollectionSchoolRequest>(nameof(CollectionSchoolRequest.IdSchool));
            var predicate = PredicateBuilder.Create<MsMaxTeacherLoad>(x
             => param.IdSchool.Contains(x.AcademicYear.IdSchool) && x.IsActive);

            var query = _teachingDbContext.Entity<MsMaxTeacherLoad>()
               .Where(predicate);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                       .Select(x => new ItemValueVm(x.Id, x.Max.ToString()))
                       .ToListAsync(CancellationToken);
            }
            else
            {
                items = query
                        .Include(e => e.AcademicYear)
                        .SetPagination(param)
                        .Select(e => new GetTeacherMaxLoadResult
                        {
                            Id = e.Id,
                            Code = e.AcademicYear.Code,
                            Description = e.AcademicYear.Description,
                            MaxLoad = e.Max
                        })
                        .Distinct().ToList();
            }
            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty());
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<AddTeacherMaxLoadRequest, AddTeacherMaxLoadValidator>();
            
            var maxLoad = await _teachingDbContext.Entity<MsMaxTeacherLoad>().Where(x => x.IdAcademicYear == body.IdAcadyear).FirstOrDefaultAsync();
            if (maxLoad == null)
            {
                MsMaxTeacherLoad msMaxTeacherLoad = new MsMaxTeacherLoad
                {
                    Id = Guid.NewGuid().ToString(),
                    IdAcademicYear = body.IdAcadyear,
                    Max = body.MaxLoad
                };
                _teachingDbContext.Entity<MsMaxTeacherLoad>().Add(msMaxTeacherLoad);
            }
            else
            {
                maxLoad.Max = body.MaxLoad;
                _teachingDbContext.Entity<MsMaxTeacherLoad>().Update(maxLoad);
            }
            await _teachingDbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
