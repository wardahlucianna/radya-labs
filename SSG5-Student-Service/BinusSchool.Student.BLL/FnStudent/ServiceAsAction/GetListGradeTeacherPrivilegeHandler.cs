using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.Schedulling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.ServiceAsAction
{
    public class GetListGradeTeacherPrivilegeHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListGradeTeacherPrivilegeHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListGradeTeacherPrivilegeRequest>(
                    nameof(GetListGradeTeacherPrivilegeRequest.IdAcademicYear),
                    nameof(GetListGradeTeacherPrivilegeRequest.IdUser),
                    nameof(GetListGradeTeacherPrivilegeRequest.IsAdvisor)
                );

            var result = new List<ItemValueVm>();

            if (param.IsAdvisor)
            {
                var getGrades = await _dbContext.Entity<MsHomeroomTeacher>()
                    .Where(x => x.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                        && x.IdBinusian == param.IdUser
                    )
                    .Select(x => new
                    {
                        Id = x.Homeroom.Grade.Id,
                        Grade = x.Homeroom.Grade.Description,
                        OrderNumber = x.Homeroom.Grade.OrderNumber
                    })
                    .Distinct()
                    .OrderBy(x => x.OrderNumber)
                        .ThenBy(x => x.Grade)
                    .ToListAsync(CancellationToken);

                result.AddRange(getGrades.Select(x => new ItemValueVm
                {
                    Id = x.Id,
                    Description = x.Grade
                }));
            }
            else
            {
                var getExperienceListByUser = await _dbContext.Entity<TrServiceAsActionForm>()
                    .Where(x => x.IdSupervisor == param.IdUser && x.ServiceAsActionHeader.IdAcademicYear == param.IdAcademicYear)
                    .Select(x => new
                    {
                        IdAcademicYear = x.ServiceAsActionHeader.IdAcademicYear,
                        IdStudent = x.ServiceAsActionHeader.IdStudent
                    })
                    .Distinct()
                    .ToListAsync(CancellationToken);

                if(getExperienceListByUser.Count == 0)
                {
                    return Request.CreateApiResult2();
                }
                else
                {
                    var studentList = getExperienceListByUser.Select(x => x.IdStudent).Distinct().ToList();

                    var getGrades = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                        .Where(x => studentList.Any(y => y == x.HomeroomStudent.IdStudent) 
                            && x.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                        )
                        .Select(x => new
                        {
                            Id = x.HomeroomStudent.Homeroom.Grade.Id,
                            Grade = x.HomeroomStudent.Homeroom.Grade.Description,
                            OrderNumber = x.HomeroomStudent.Homeroom.Grade.OrderNumber
                        })
                        .Distinct()
                        .OrderBy(x => x.OrderNumber)
                        .ThenBy(x => x.Grade)
                    .ToListAsync(CancellationToken);

                    result.AddRange(getGrades.Select(x => new ItemValueVm
                    {
                        Id = x.Id,
                        Description = x.Grade
                    }));
                }
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}
