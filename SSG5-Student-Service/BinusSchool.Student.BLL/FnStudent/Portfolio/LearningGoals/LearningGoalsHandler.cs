

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
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
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.Portfolio.LearningGoals.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.Portfolio.LearningGoals
{
    public class LearningGoalsHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        public LearningGoalsHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetLearningGoalStudent = await _dbContext.Entity<TrLearningGoalStudent>()
               .Where(x => ids.Contains(x.Id))
               .ToListAsync(CancellationToken);

            GetLearningGoalStudent.ForEach (x => x.IsActive = false) ;
            _dbContext.Entity<TrLearningGoalStudent>().UpdateRange(GetLearningGoalStudent);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var items = await _dbContext.Entity<TrLearningGoalStudent>()
                .Include(e => e.AcademicYear)
                .Include(e => e.LearnerProfile)
                .Where(e => e.Id == id)
                .Select(x => new
                {
                    Id = x.Id,
                    LearningGoals = x.Name,
                    LearningGoalsCategory = new CodeWithIdVm
                    {
                        Id = x.LearnerProfile.Id,
                        Code = x.LearnerProfile.Name,
                        Description = x.LearnerProfile.Name
                    },
                    IsIbLearningProfile = x.LearnerProfile.Type == (LearnerProfile)0 ? true : false,
                    IsAppoachesToLearning = x.LearnerProfile.Type == (LearnerProfile)1 ? true : false,
                    AcademicYear = new ItemValueVm(x.IdAcademicYear,x.AcademicYear.Description),
                    Semester = x.Semester
                }).SingleOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(items as object);
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
                                .Where(x => GetHomeroomStudentEnrollment.Select(e => e.IdLesson).ToList().Contains(x.IdLesson) && x.IdUser == param.IdUser)
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
                         where LearningGoals.IdStudent == param.IdStudent && LearningGoals.IdAcademicYear == param.IdAcademicYear && LearningGoals.IsOngoing == true
                         select new
                         {
                             Id = LearningGoals.Id,
                             Name = LearningGoals.Name,
                             LearningGoalsCategory = LearningCategory.Name,
                             IsOngoing = LearningGoals.IsOngoing,
                             CreateBy = User.DisplayName,
                             IdAcademicYear = LearningGoals.IdAcademicYear,
                             Semester = LearningGoals.Semester
                         });

            if(param.IdAcademicYear != null)
                query = query.Where(x => x.IdAcademicYear == param.IdAcademicYear);

            if(param.Semester != null)
                query = query.Where(x => x.Semester == param.Semester);

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
            var body = await Request.ValidateBody<AddLearningGoalsRequest, AddLearningGoalsValidator>();

            var exsis = _dbContext.Entity<TrLearningGoalStudent>().Any(e => e.Name == body.LearningGoals && e.IdProfile == body.IdLearningGoalsCategory && e.IdStudent == body.IdStudent);

            if (!exsis)
            {
                var NewLearningGoalStudent = new TrLearningGoalStudent
                {
                    Id = Guid.NewGuid().ToString(),
                    IdProfile = body.IdLearningGoalsCategory,
                    IdStudent = body.IdStudent,
                    Name = body.LearningGoals,
                    IsOngoing = true,
                    IdAcademicYear = body.IdAcademicYear,
                    Semester = body.Semester
                };
                _dbContext.Entity<TrLearningGoalStudent>().Add(NewLearningGoalStudent);
            }
            else
            {
                throw new BadRequestException("Learning Goals with name: " + body.LearningGoals + " is exists.");
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateLearningGoalsRequest, UpdateLearningGoalsValidator>();
            var GetLearningGoal = _dbContext.Entity<TrLearningGoalStudent>().SingleOrDefault(e => e.Id==body.Id);

            if (GetLearningGoal == null)
                throw new BadRequestException("Learning Goals with id: " + body.Id + " is not found.");

            var exsis = _dbContext.Entity<TrLearningGoalStudent>().Any(e => e.Name == body.LearningGoals && e.IdProfile == body.IdLearningGoalsCategory && e.Id != body.Id && e.IdStudent == GetLearningGoal.IdStudent);
            if (!exsis)
            {
                GetLearningGoal.Name = body.LearningGoals;
                GetLearningGoal.IdProfile = body.IdLearningGoalsCategory;
                GetLearningGoal.IdAcademicYear = body.IdAcademicYear;
                GetLearningGoal.Semester = body.Semester;

                _dbContext.Entity<TrLearningGoalStudent>().Update(GetLearningGoal);
            }
            else
            {
                throw new BadRequestException("Laerning Goals with name: " + body.LearningGoals + " is exists.");
            }
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
