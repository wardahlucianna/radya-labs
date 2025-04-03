using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingServiceEntry;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselingServiceEntry
{
    public class GetCounselorHomeRoomHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetCounselorHomeRoomHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCounselorHomeRoomRequest>();

            var query = await (from counselor in _dbContext.Entity<MsCounselor>()
                               join counselorGrade in _dbContext.Entity<MsCounselorGrade>() on counselor.Id equals counselorGrade.IdCounselor
                               join grade in _dbContext.Entity<MsGrade>() on counselorGrade.IdGrade equals grade.Id
                               join gradePathWay in _dbContext.Entity<MsGradePathway>() on grade.Id equals gradePathWay.IdGrade
                               join gradePathWayClassRoom in _dbContext.Entity<MsGradePathwayClassroom>() on gradePathWay.Id equals gradePathWayClassRoom.IdGradePathway
                               join classRoom in _dbContext.Entity<MsClassroom>() on gradePathWayClassRoom.IdClassroom equals classRoom.Id
                               join homeroom in _dbContext.Entity<MsHomeroom>() on gradePathWayClassRoom.Id equals homeroom.IdGradePathwayClassRoom
                               where counselor.IdUser == param.IdUser
                               select new GetCounselorHomeRoomResult
                               {
                                   Id = homeroom.Id,
                                   Description = $"{grade.Code}{classRoom.Code}",
                                   Semester = homeroom.Semester
                               }).ToListAsync(CancellationToken);

            var result = query.Distinct().OrderBy(x => x.Description).ThenBy(x => x.Semester);

            return Request.CreateApiResult2(result as object);
        }
    }
}
