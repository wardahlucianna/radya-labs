using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.MapStudentHomeroom;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.MapStudentHomeroom
{
    public class GetHomeroomCopyToNextOrPreviousSemesterHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetHomeroomCopyToNextOrPreviousSemesterHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetHomeroomCopyToNextOrPreviousSemesterRequest>();

            var GetHomeroom = await _dbContext.Entity<MsHomeroom>()
               .Where(e=>e.Id==param.IdHomeroom)
               .FirstOrDefaultAsync(CancellationToken);

            var IdHomeroomNextOrPrevious = "";

            if (GetHomeroom.Semester == 1)
            {
                IdHomeroomNextOrPrevious = await _dbContext.Entity<MsHomeroom>()
                   .Where(e => e.IdGrade == GetHomeroom.IdGrade
                            && e.IdGradePathwayClassRoom == GetHomeroom.IdGradePathwayClassRoom
                            && e.Semester == 2)
                   .Select(e=>e.Id)
                   .FirstOrDefaultAsync(CancellationToken);
            }
            else if (GetHomeroom.Semester == 2)
            {
                IdHomeroomNextOrPrevious = await _dbContext.Entity<MsHomeroom>()
                   .Where(e => e.IdGrade == GetHomeroom.IdGrade
                            && e.IdGradePathwayClassRoom == GetHomeroom.IdGradePathwayClassRoom
                            && e.Semester == 1)
                   .Select(e => e.Id)
                   .FirstOrDefaultAsync(CancellationToken);
            }

            var ExsisHomeroomNextOrPrevious = await _dbContext.Entity<MsHomeroom>()
               .Where(e => e.Id == IdHomeroomNextOrPrevious)
               .AnyAsync(CancellationToken);

            var ExsisHomeroomStudentNextOrPrevious = await _dbContext.Entity<MsHomeroomStudent>()
               .Where(e => e.IdHomeroom == IdHomeroomNextOrPrevious)
               .AnyAsync(CancellationToken);

            bool IsDisabledButtonNextOrPrevious = true;
            if (ExsisHomeroomNextOrPrevious)
            {
                IsDisabledButtonNextOrPrevious = ExsisHomeroomStudentNextOrPrevious;
            }
            

            GetHomeroomCopyToNextOrPreviousSemesterResult Item = new GetHomeroomCopyToNextOrPreviousSemesterResult
            {
                IdHomeroomNextOrPrevious = IdHomeroomNextOrPrevious,
                IsDisabledButtonNextOrPrevious = IsDisabledButtonNextOrPrevious
            };

            return Request.CreateApiResult2(Item as object);
        }
    }
}
