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
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.CASStudentAdvisor
{
    public class GetListCASStudentAdvisorHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetListCASStudentAdvisorHandler(
               IStudentDbContext DbContext
           )
        {
            _dbContext = DbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListCASStudentAdvisorRequest>
                (nameof(GetListCASStudentAdvisorRequest.IdAcademicYear),
                nameof(GetListCASStudentAdvisorRequest.IdGrade)
                );

            var homeroomList = await _dbContext.Entity<MsHomeroom>()
                            .Where(x => x.IdGrade == param.IdGrade)
                            .ToListAsync(CancellationToken);

            var studentList = await _dbContext.Entity<MsHomeroomStudent>()
                            .Where(x => homeroomList.Select(y => y.Id).ToList().Any(y => y == x.IdHomeroom))
                            .Select(x => new
                            {
                                IdHomeroomStudent = x.Id,
                                IdStudent = x.IdStudent,
                                StudentName = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName),
                                IdGradePathwayClassroom = x.Homeroom.IdGradePathwayClassRoom,
                                Class = x.Homeroom.Grade.Code + x.Homeroom.MsGradePathwayClassroom.Classroom.Description,
                                Classroom = x.Homeroom.MsGradePathwayClassroom.Classroom.Description,
                                Semester = x.Semester,
                                Homeroom = x.Homeroom
                            })
                            .OrderBy(x => x.Semester)
                            .Where(x => param.HomeroomCode == null ? true : x.Classroom == param.HomeroomCode) //Filter by Homeroom
                            .ToListAsync(CancellationToken);

            var filteredStundentList = studentList
                            .GroupBy(x => x.IdStudent)
                            .Select(x => new
                            {
                                IdStudent = x.Key,
                                Semester = x.OrderBy(x => x.Semester).Select(x => x.Semester).FirstOrDefault()
                            })
                            .ToList();

            var finalStudentList = studentList
                            .Join(filteredStundentList,
                                c => new { c.IdStudent, c.Semester },
                                s => new { s.IdStudent, s.Semester },
                                (c1, t1) => new { c = c1, ts = t1 }
                            ).Select(x => new
                            {
                                IdHomeroomStudent = x.c.IdHomeroomStudent,
                                IdStudent = x.c.IdStudent,
                                StudentName = x.c.StudentName,
                                Semester = x.c.Semester,
                                IdGradePathwayClassroom = x.c.IdGradePathwayClassroom,
                                Class = x.c.Class,
                                Homeroom = x.c.Homeroom
                            }
                            )
                            .ToList();

            var advisorList = await _dbContext.Entity<TrCasAdvisorStudent>()
                                    .Include(x => x.CasAdvisor.UserCAS)
                                    .Where(x => finalStudentList.Select(y => y.IdHomeroomStudent).ToList().Any(y => y == x.IdHomeroomStudent))
                                    .ToListAsync(CancellationToken);

            var result = finalStudentList
                        .GroupJoin(advisorList,
                        c => c.IdHomeroomStudent,
                        s => s.IdHomeroomStudent,
                        (c1, t1) => new { c = c1, ts = t1 }
                        ).SelectMany(c1 => c1.ts.DefaultIfEmpty(),
                        (c1, t1) => new GetListCASStudentAdvisorResult
                        {
                            Advisor = new GetListCASStudentAdvisorResult_Advisor
                            {
                                Id = t1?.CasAdvisor?.IdUserCAS,
                                Description = t1?.CasAdvisor?.UserCAS?.DisplayName,
                                IdCasAdvisor = t1?.IdCasAdvisor
                            },
                            Student = new GetListCASStudentAdvisorResult_Student
                            {
                                Id = c1?.c?.IdStudent,
                                Description = c1.c?.StudentName,
                                IdHomeroomStudent = c1.c?.IdHomeroomStudent
                            },
                            Homeroom = c1?.c?.Class,
                            IdCASAdvisorStudent = t1?.Id
                        })
                        .OrderBy(x => x.Student.Description)
                        .OrderBy(x => x.Advisor.Description)
                        .ToList();

            if (!string.IsNullOrWhiteSpace(param.IdCASAdvisor))
                result = result.Where(x => x.Advisor.IdCasAdvisor == param.IdCASAdvisor).OrderBy(x => x.Student.Description).ToList();

            return Request.CreateApiResult2(result as object);
        }
    }
}
