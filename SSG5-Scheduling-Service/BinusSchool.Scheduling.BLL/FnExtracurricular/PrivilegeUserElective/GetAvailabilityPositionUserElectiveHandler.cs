using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.PrivilegeUserElective;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.PrivilegeUserElective
{
    public class GetAvailabilityPositionUserElectiveHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetAvailabilityPositionUserElectiveRequest.IdUser),
            nameof(GetAvailabilityPositionUserElectiveRequest.IdAcademicyear)
        });

        private readonly ISchedulingDbContext _dbContext;
        public GetAvailabilityPositionUserElectiveHandler(ISchedulingDbContext DbContext)
        {
            _dbContext = DbContext;

        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAvailabilityPositionUserElectiveRequest>(_requiredParams.Value);
            var positions = await GetAvailablePosition(param.IdUser, param.IdAcademicyear);

            return Request.CreateApiResult2(positions as object);
        }

        public async Task<IReadOnlyList<CodeWithIdVm>> GetAvailablePosition(string idUser, string idAcadyear)
        {
            var query =
                from user in _dbContext.Entity<MsUser>()
                let hasCa = _dbContext
                    .Entity<MsHomeroomTeacher>()
                    .Any(x => x.IdBinusian == idUser && x.Homeroom.IdAcademicYear == idAcadyear)
                let hasSt = _dbContext
                    .Entity<MsLessonTeacher>()
                    .Any(x => x.IdUser == idUser && x.Lesson.IdAcademicYear == idAcadyear)
                let tp = _dbContext.Entity<TrNonTeachingLoad>()
                    .Where(x => x.MsNonTeachingLoad.IdAcademicYear == idAcadyear)
                    .Where(x => x.IdUser == idUser)
                    .Select(x => new CodeWithIdVm
                    {
                        Id = x.MsNonTeachingLoad.TeacherPosition.Position.Code,
                        Code = x.MsNonTeachingLoad.TeacherPosition.Description,
                        Description = x.MsNonTeachingLoad.TeacherPosition.Description
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

        public async Task<GetAvailabilityPositionUserElectiveResult> GetAvailablePositionDetail(string idUser, string idAcadyear)
        {
            var query =
                from user in _dbContext.Entity<MsUser>()
                let ca = _dbContext
                    .Entity<MsHomeroomTeacher>()
                    .Where(x => x.IdBinusian == idUser && x.Homeroom.IdAcademicYear == idAcadyear)
                    .Select(x => new ItemValueVm(x.Homeroom.Id))
                    .ToList()
                let st = _dbContext
                    .Entity<MsLessonTeacher>()
                    .Where(x => x.IdUser == idUser && x.Lesson.IdAcademicYear == idAcadyear)
                    .Select(x => new ItemValueVm(x.Lesson.Id))
                    .ToList()
                let tp = _dbContext.Entity<TrNonTeachingLoad>()
                    .Where(x => x.MsNonTeachingLoad.IdAcademicYear == idAcadyear)
                    .Where(x => x.IdUser == idUser)
                    .Select(x => new OtherPositionUserElectiveResult
                    {
                        Id = x.MsNonTeachingLoad.TeacherPosition.Position.Code,
                        Code = x.MsNonTeachingLoad.TeacherPosition.Description,
                        Description = x.MsNonTeachingLoad.TeacherPosition.Description,
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

            var positions = new GetAvailabilityPositionUserElectiveResult
            {
                ClassAdvisors = result.ca,
                SubjectTeachers = result.st,
                OtherPositions = result.tp
            };

            return positions;
        }
    }
}
