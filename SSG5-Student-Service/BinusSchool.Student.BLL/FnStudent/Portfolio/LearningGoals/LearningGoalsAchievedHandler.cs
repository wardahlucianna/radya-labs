using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.LearningGolas;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.Schedulling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.Portfolio.LearningGoals.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.Portfolio.LearningGoals
{
    public class LearningGoalsAchievedHandler : FunctionsHttpCrudHandler
    {

        private readonly IStudentDbContext _dbContext;

        public LearningGoalsAchievedHandler(IStudentDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetLearningGoalsRequest>();
            string[] _columns = { };

            var IsShowButton = false;

            if (param.Role.ToUpper() == RoleConstant.Teacher)
            {
                //SUbject Teacher
                var GetHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                       .Include(e => e.HomeroomStudent)
                       .Where(e => e.HomeroomStudent.IdHomeroom == param.IdHomeroom && e.HomeroomStudent.IdStudent == param.IdStudent && e.HomeroomStudent.Semester == param.Semester)
                      .ToListAsync(CancellationToken);

                var GetSubjetTeacher = await _dbContext.Entity<MsLessonTeacher>()
                                .Include(e => e.Staff)
                                .Where(x => GetHomeroomStudentEnrollment.Select(e => e.IdLesson).ToList().Contains(x.IdLesson) && x.IdUser==param.IdUser)
                                .ToListAsync(CancellationToken);

                var GetHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                        .Include(e => e.Homeroom)
                        .Where(e => e.IdHomeroom == param.IdHomeroom && e.Homeroom.Semester == param.Semester && e.IdBinusian == param.IdUser)
                        .ToListAsync(CancellationToken);

                IsShowButton = GetSubjetTeacher.Count() == 0 || GetHomeroomTeacher.Count() == 0 ? false : true;
            }
            else if (param.Role.ToUpper() == PositionConstant.CoTeacher)
            {
                IsShowButton = true;
            }
            else if (param.Role.ToUpper() == RoleConstant.Parent || param.Role.ToUpper() == RoleConstant.Student)
            {
                IsShowButton = true;
            }


            var query = (from LearningGoals in _dbContext.Entity<TrLearningGoalStudent>()
                         join LearningCategory in _dbContext.Entity<MsLearnerProfile>() on LearningGoals.IdProfile equals LearningCategory.Id
                         join User in _dbContext.Entity<MsUser>() on LearningGoals.UserIn equals User.Id
                         where LearningGoals.IdStudent == param.IdStudent && LearningGoals.IdAcademicYear == param.IdAcademicYear && LearningGoals.IsOngoing == false
                         select new
                         {
                             Id = LearningGoals.Id,
                             Name = LearningGoals.Name,
                             LearningGoalsCategory = LearningCategory.Name,
                             IsOngoing = LearningGoals.IsOngoing,
                             CreateBy = User.DisplayName
                         });

            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.Name.Contains(param.Search));

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetLearningGoalsOnGoingResult
                {
                    Id = x.Id,
                    LearningGoals = x.Name,
                    LearningGoalsCategory = x.LearningGoalsCategory,
                    IsGoing = x.IsOngoing,
                    CreateBy = x.CreateBy,
                    IsShowButton = IsShowButton
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetLearningGoalsOnGoingResult
                {
                    Id = x.Id,
                    LearningGoals = x.Name,
                    LearningGoalsCategory = x.LearningGoalsCategory,
                    IsGoing = x.IsOngoing,
                    CreateBy = x.CreateBy,
                    IsShowButton = IsShowButton
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
               ? items.Count
               : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

     
        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody <UpdateLearningAchievedRequest, UpdateLearningAchievedValidator>();
            var GetLearningGoals = await _dbContext.Entity<TrLearningGoalStudent>()
                .Where(e => body.LearningAchieveds.Select(e=>e.Id).Contains(e.Id))
                .ToListAsync(CancellationToken);

            GetLearningGoals.ForEach(x => x.IsOngoing = body.IsAchieved ? false : true);
            _dbContext.Entity<TrLearningGoalStudent>().UpdateRange(GetLearningGoals);
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
