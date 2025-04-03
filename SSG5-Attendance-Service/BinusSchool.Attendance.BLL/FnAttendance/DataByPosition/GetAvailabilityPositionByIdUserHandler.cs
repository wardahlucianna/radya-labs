using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.DataByPosition;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.DataByPosition
{
    public class GetAvailabilityPositionByIdUserHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetAvailabilityPositionByIdUserRequest.IdUser),
            nameof(GetAvailabilityPositionByIdUserRequest.IdAcademicyear)
        });

        private readonly IAttendanceDbContext _dbContext;
        public GetAvailabilityPositionByIdUserHandler(IAttendanceDbContext DbContext)
        {
            _dbContext = DbContext;

        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAvailabilityPositionByIdUserRequest>(_requiredParams.Value);
            var positions = await GetAvailablePosition(param.IdUser, param.IdAcademicyear, param.Semester);

            return Request.CreateApiResult2(positions as object);
        }

        public async Task<IReadOnlyList<CodeWithIdVm>> GetAvailablePosition(string idUser, string idAcadyear, int? Semester)
        {
            var query =
                from user in _dbContext.Entity<MsUser>()
                let hasCa = _dbContext
                    .Entity<MsHomeroomTeacher>()
                    .Any(x => x.IdBinusian == idUser && x.Homeroom.IdAcademicYear == idAcadyear && 
                              x.Homeroom.Semester == (Semester != null ? Semester : x.Homeroom.Semester))
                let hasSt = _dbContext
                    .Entity<MsLessonTeacher>()
                    .Any(x => x.IdUser == idUser && x.Lesson.IdAcademicYear == idAcadyear && 
                              x.Lesson.Semester == (Semester != null ? Semester : x.Lesson.Semester))
                let tp = _dbContext.Entity<TrNonTeachingLoad>()
                    .Where(x => x.NonTeachingLoad.IdAcademicYear == idAcadyear)
                    .Where(x => x.IdUser == idUser)
                    .Select(x => new CodeWithIdVm
                    {
                        Id = x.NonTeachingLoad.TeacherPosition.LtPosition.Code,
                        Code = x.NonTeachingLoad.TeacherPosition.Description,
                        Description = x.NonTeachingLoad.TeacherPosition.Description
                    })
                    .ToList()
                where user.Id == idUser
                select new
                {
                    user.Id,
                    hasCa,
                    hasSt,
                    tp
                };
            var result = await query.FirstOrDefaultAsync(CancellationToken);

            var positions = new List<CodeWithIdVm>();
            if (result.hasCa)
                positions.Add(new CodeWithIdVm
                {
                    Id = PositionConstant.ClassAdvisor,
                    Code = "Class Advisor",
                    Description = "Class Advisor"
                });
            if (result.hasSt)
                positions.Add(new CodeWithIdVm
                {
                    Id = PositionConstant.SubjectTeacher,
                    Code = "Subject Teacher",
                    Description = "Subject Teacher"
                });
            if (result.tp.Count != 0)
            {
                var op = result.tp.GroupBy(x => new
                {
                    x.Id,
                    x.Code,
                    x.Description
                })
                .Select(x => new CodeWithIdVm
                {
                    Id = x.Key.Id,
                    Code = x.Key.Code,
                    Description = x.Key.Description
                }).ToList();
                foreach (var tp in op)
                    positions.Add(tp);
            }

            positions = positions.GroupBy(x => new
            {
                x.Id,
                x.Code,
                x.Description
            }).Select(x => new CodeWithIdVm
            {
                Id = x.Key.Id,
                Code = x.Key.Code,
                Description = x.Key.Description
            }).ToList();

            return positions;
        }

        public async Task<GetAvailabilityPositionByIdUserResult> GetAvailablePositionDetail(string idUser, string idAcadyear, int? Semester)
        {
            var query =
                from user in _dbContext.Entity<MsUser>()
                let ca = _dbContext
                    .Entity<MsHomeroomTeacher>()
                    .Where(x => x.IdBinusian == idUser && x.Homeroom.IdAcademicYear == idAcadyear &&
                                x.Homeroom.Semester == (Semester != null ? Semester : x.Homeroom.Semester))
                    .Select(x => new ItemValueVm(x.Homeroom.Id))
                    .ToList()
                let st = _dbContext
                    .Entity<MsLessonTeacher>()
                    .Where(x => x.IdUser == idUser && x.Lesson.IdAcademicYear == idAcadyear && 
                                x.Lesson.Semester == (Semester != null ? Semester : x.Lesson.Semester))
                    .Select(x => new ItemValueVm(x.Lesson.Id))
                    .ToList()
                let tp = _dbContext.Entity<TrNonTeachingLoad>()
                    .Where(x => x.NonTeachingLoad.IdAcademicYear == idAcadyear)
                    .Where(x => x.IdUser == idUser)
                    .Select(x => new OtherPositionByIdUserResult
                    {
                        Id = x.NonTeachingLoad.TeacherPosition.LtPosition.Code,
                        Code = x.NonTeachingLoad.TeacherPosition.Description,
                        Description = x.NonTeachingLoad.TeacherPosition.Description,
                        Data = x.Data
                    })
                    .ToList()
                where user.Id == idUser
                select new
                {
                    user.Id,
                    ca,
                    st,
                    tp
                };
            var result = await query.FirstOrDefaultAsync(CancellationToken);

            var positions = new GetAvailabilityPositionByIdUserResult
            {
                ClassAdvisors = result.ca,
                SubjectTeachers = result.st,
                OtherPositions = result.tp
            };

            return positions;
        }
    }
}
