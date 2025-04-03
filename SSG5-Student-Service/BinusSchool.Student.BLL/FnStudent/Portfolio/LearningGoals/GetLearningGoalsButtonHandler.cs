using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using BinusSchool.Data.Model.Student.FnStudent.MeritDemerit;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.LearningGolas;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.Schedulling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.MeritDemerit.Validator;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.Portfolio.LearningGoals
{
    public class GetLearningGoalsButtonHandler :FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetLearningGoalsButtonHandler(IStudentDbContext EntryMeritDemetitDbContext)
        {
            _dbContext = EntryMeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLearningGoalsButtonRequest>();

            GetLearningGoalsButtonResult result = new GetLearningGoalsButtonResult();

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

                result.IsShowButton = GetSubjetTeacher.Count() != 0 || GetHomeroomTeacher.Count() != 0 ? true : false;
            }
            else if (param.Role.ToUpper() == PositionConstant.CoTeacher)
            {
                result.IsShowButton = true;
            }
            else if (param.Role.ToUpper() == RoleConstant.Parent || param.Role.ToUpper() == RoleConstant.Student)
            {
                result.IsShowButton = true;
            }

            return Request.CreateApiResult2(result as object);
        }

    }
}
