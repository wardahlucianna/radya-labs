using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization
{
    public class GetTeacherForSubstitutionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetTeacherForSubstitutionHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetTeacherForSubstitutionRequest>(nameof(GetTeacherForSubstitutionRequest.IdAcademicYear),
                                                                          nameof(GetTeacherForSubstitutionRequest.StartDate),
                                                                          nameof(GetTeacherForSubstitutionRequest.EndDate),
                                                                          nameof(GetTeacherForSubstitutionRequest.IsSubstituteTeacher));

            var predicate = PredicateBuilder.Create<TrScheduleRealization>(x => x.ScheduleDate >= param.StartDate && x.ScheduleDate <= param.EndDate && x.Homeroom.IdAcademicYear == param.IdAcademicYear);

            if(!string.IsNullOrWhiteSpace(param.IdLevel))
                predicate = predicate.And(x => x.Homeroom.Grade.IdLevel == param.IdLevel);

            if(param.IdGrade != null)
                predicate = predicate.And(x => param.IdGrade.Contains(x.Homeroom.IdGrade));

            IReadOnlyList<IItemValueVm> items;

            if(param.IsSubstituteTeacher == false)
            {
                var query = _dbContext.Entity<TrScheduleRealization>()
                                 .Include(x => x.Homeroom).ThenInclude(x => x.Grade)
                                 .Where(predicate)
                                 .Select(x => new { x.IdBinusian, x.TeacherName })
                                 .Distinct()
                                 .OrderBy(x => x.TeacherName);

                items = await query
                    .Select(x => new ItemValueVm(x.IdBinusian, $"{x.TeacherName}"))
                    .Distinct()
                    .ToListAsync(CancellationToken);

                items = items
                        .Select(x => new ItemValueVm(
                        x.Id,
                        x.Description
                    )
                )
                .OrderBy(x => x.Description)
                .ToList();
            }
            else
            {
                var query = _dbContext.Entity<TrScheduleRealization>()
                                 .Include(x => x.Homeroom).ThenInclude(x => x.Grade)
                                 .Where(predicate)
                                 .Select(x => new { x.IdBinusianSubtitute, x.TeacherNameSubtitute })
                                 .Distinct()
                                 .OrderBy(x => x.TeacherNameSubtitute);

                items = await query
                    .Select(x => new ItemValueVm(x.IdBinusianSubtitute, $"{x.TeacherNameSubtitute}"))
                    .Distinct()
                    .ToListAsync(CancellationToken);

                items = items
                        .Select(x => new ItemValueVm(
                        x.Id,
                        x.Description
                    )
                )
                .OrderBy(x => x.Description)
                .ToList();
            }
            
            return Request.CreateApiResult2(items as object);
        }
    }
}
