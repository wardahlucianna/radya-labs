using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using System.Collections.Generic;
using System.Linq;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Constants;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.User;

namespace BinusSchool.Scheduling.FnSchedule.RolePosition
{
    public class GetPositionByUserHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetPositionByUserHandler(ISchedulingDbContext DbContext) 
        {
            _dbContext = DbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetPositionByUserRequest>();
            List<CodeWithIdVm> items = new List<CodeWithIdVm>();

            #region LessonTeacher
            var listLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                                    .Where(e=>e.Lesson.IdAcademicYear==param.IdAcademicYear && e.IdUser==param.IdUser)
                                    .ToListAsync(CancellationToken);

            if(listLessonTeacher.Any() )
            {
                var _ListSubjectTeacher = await _dbContext.Entity<MsTeacherPosition>()
                                    .Where(e => e.Position.Code == PositionConstant.SubjectTeacher)
                                    .GroupBy(e => new
                                    {
                                        e.Id,
                                        e.Code,
                                        e.Description
                                    })
                                    .Select(e=>new CodeWithIdVm
                                    {
                                        Id = e.Key.Id,
                                        Code = e.Key.Code,
                                        Description = e.Key.Description,
                                    })
                                    .ToListAsync(CancellationToken);

                items.AddRange(_ListSubjectTeacher);
            }
            #endregion

            #region HomeroomTeacher
            var listHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                                    .Include(e=>e.TeacherPosition)
                                   .Where(e => e.Homeroom.IdAcademicYear == param.IdAcademicYear && e.IdBinusian == param.IdUser)
                                   .GroupBy(e => new
                                   {
                                       e.TeacherPosition.Id,
                                       e.TeacherPosition.Code,
                                       e.TeacherPosition.Description
                                   })
                                    .Select(e => new CodeWithIdVm
                                    {
                                        Id = e.Key.Id,
                                        Code = e.Key.Code,
                                        Description = e.Key.Description,
                                    })
                                   .ToListAsync(CancellationToken);
            items.AddRange(listHomeroomTeacher);
            #endregion

            #region NonTeacherLoad
            var listNonTeacherLoad = await _dbContext.Entity<TrNonTeachingLoad>()
                                   .Where(e => e.MsNonTeachingLoad.IdAcademicYear == param.IdAcademicYear && e.IdUser == param.IdUser)
                                   .GroupBy(e => new
                                   {
                                       e.MsNonTeachingLoad.TeacherPosition.Id,
                                       e.MsNonTeachingLoad.TeacherPosition.Code,
                                       e.MsNonTeachingLoad.TeacherPosition.Description
                                   })
                                    .Select(e => new CodeWithIdVm
                                    {
                                        Id = e.Key.Id,
                                        Code = e.Key.Code,
                                        Description = e.Key.Description,
                                    })
                                   .ToListAsync(CancellationToken);
            items.AddRange(listNonTeacherLoad);
            #endregion

            #region All
            if (!items.Any())
            {
                var getUser = await _dbContext.Entity<MsUser>()
                                  .Where(e => e.Id== param.IdUser)
                                  .FirstOrDefaultAsync(CancellationToken);

                if (getUser != null)
                    items.Add(new CodeWithIdVm
                    {
                        Id = "All",
                        Code = "All",
                        Description = "All",
                    });
            }
            #endregion

            items= items.GroupBy(e => new
            {
                e.Id,
                e.Code,
                e.Description
            })
            .Select(e => new CodeWithIdVm
            {
                Id = e.Key.Id,
                Code = e.Key.Code,
                Description = e.Key.Description,
            })
            .ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
