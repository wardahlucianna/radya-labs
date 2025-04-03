using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.CASStudentAdvisor;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.CASStudentAdvisor
{
    public class GetListTeacherForCASHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListTeacherForCASHandler(
                IStudentDbContext dbContext
            )
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListTeacherForCASRequest>(
                nameof(GetListTeacherForCASRequest.IdSchool),
                nameof(GetListTeacherForCASRequest.IdAcademicYear)
                );

            var userRoleGroup = await _dbContext.Entity<LtRoleGroup>()
                                .Where(x => x.Description == "Teacher")
                                .FirstOrDefaultAsync(CancellationToken);

            var userRoleList = await _dbContext.Entity<MsUserRole>()
                            .Include(x => x.User)
                            .Where(x => x.Role.IdRoleGroup == userRoleGroup.Id)
                            .ToListAsync(CancellationToken);

            var advisorList = await _dbContext.Entity<TrCasAdvisor>()
                            .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                            .ToListAsync(CancellationToken);

            var teacherList = await _dbContext.Entity<MsHomeroomTeacher>()
                            .Where(x => x.Homeroom.Grade.MsLevel.MsAcademicYear.Id == param.IdAcademicYear
                                        && x.Homeroom.Grade.MsLevel.MsAcademicYear.IdSchool == param.IdSchool
                                        && x.Staff.IdSchool == param.IdSchool
                                        && userRoleList.Select(y => y.IdUser).ToList().Any(y => y == x.IdBinusian)
                                        && !advisorList.Select(y => y.IdUserCAS).ToList().Any(y => y == x.IdBinusian))
                            .Select(x => new GetListTeacherForCASResult
                            {
                                IdUser = x.IdBinusian,
                                DisplayName = NameUtil.GenerateFullName(x.Staff.FirstName,x.Staff.LastName),
                                IdAcademicYear = x.Homeroom.Grade.MsLevel.IdAcademicYear
                            })
                            .Distinct()
                            .ToListAsync(CancellationToken);

            var getTeacherListByLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                            .Where(x => x.Lesson.AcademicYear.IdSchool == param.IdSchool
                                    && x.Lesson.IdAcademicYear == param.IdAcademicYear
                                    && userRoleList.Select(y => y.IdUser).ToList().Any(y => y == x.IdUser)
                                    && !advisorList.Select(y => y.IdUserCAS).ToList().Any(y => y == x.IdUser)
                            )
                            .Select(x => new GetListTeacherForCASResult
                            {
                                IdUser = x.IdUser,
                                DisplayName = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName),
                                IdAcademicYear = x.Lesson.IdAcademicYear
                            })
                            .Distinct()
                            .ToListAsync(CancellationToken);

            teacherList.AddRange(getTeacherListByLessonTeacher.Where(x => !teacherList.Select(y => y.IdUser).ToList().Any(y => y == x.IdUser)));

            return Request.CreateApiResult2(teacherList.OrderBy(x => x.DisplayName).Distinct().ToList() as object);
        }

    }
}
