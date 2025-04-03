using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselorData;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Persistence.StudentDb.Entities.School;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselorData
{
    public class GetLevelGradeByConcellorHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IRolePosition _rolePositionService;
        public GetLevelGradeByConcellorHandler(IStudentDbContext studentDbContext, IRolePosition rolePositionService)
        {
            _dbContext = studentDbContext;
            _rolePositionService = rolePositionService;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLevelGradeByConcellorRequest>();

            var predicate = PredicateBuilder.Create<TrRolePosition>(x => x.IdRole == param.IdRole && x.TeacherPosition.IdSchool==param.IdSchool);

            if (!string.IsNullOrEmpty(param.IdPosition))
                predicate = predicate.And(e => e.TeacherPosition.IdPosition == param.IdPosition);


            var listIdTeacherPosition = await _dbContext.Entity<TrRolePosition>()
                                        .Where(predicate)
                                        .Select(e=>e.IdTeacherPosition)
                                        .ToListAsync(CancellationToken);

            var getSubjectByUserApi = await _rolePositionService.GetSubjectByUser(new GetSubjectByUserRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdUser = param.IdUser,
                ListIdTeacherPositions = listIdTeacherPosition
            });

            var getSubjectByUser = getSubjectByUserApi.IsSuccess? getSubjectByUserApi.Payload:null;

            var listLevel = await _dbContext.Entity<MsLevel>()
                        .Include(e=>e.MsGrades)
                        .Where(e => e.IdAcademicYear == param.IdAcademicYear)
                        .ToListAsync(CancellationToken);

            var items = listLevel
                        .Select(e => new GetLevelGradeByConcellorResult
                        {
                            Id = e.Id,
                            Code = e.Code,
                            Description = e.Description,
                            IsDisabled = getSubjectByUser.Any()
                                            ? getSubjectByUser.Where(f => f.Level.Id == e.Id).Any()?false:true
                                            : true,
                            Grade = e.MsGrades.Select(f => new GetItemByConcellor
                            {
                                Id = f.Id,
                                Code = f.Code,
                                Description = f.Description,
                                IsDisabled = getSubjectByUser.Any() 
                                                ? getSubjectByUser.Where(g => g.Grade.Id == f.Id).Any() ?false :true
                                                : true
                            }).ToList(),
                        })
                        .ToList();



            return Request.CreateApiResult2(items as object);
        }


    }
}
