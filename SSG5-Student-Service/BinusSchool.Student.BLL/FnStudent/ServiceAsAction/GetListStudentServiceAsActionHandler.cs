using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.Schedulling;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;
using NPOI.HPSF;

namespace BinusSchool.Student.FnStudent.ServiceAsAction
{
    public class GetListStudentServiceAsActionHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IServiceAsAction _serviceAsAction;

        public GetListStudentServiceAsActionHandler
        (
            IStudentDbContext studentDbContext,
            IServiceAsAction serviceAsAction
        )
        {
            _dbContext = studentDbContext;
            _serviceAsAction = serviceAsAction;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListStudentServiceAsActionRequest>(
                    nameof(GetListStudentServiceAsActionRequest.IdUser),
                    nameof(GetListStudentServiceAsActionRequest.isAdvisor)
                );

            var result = new List<GetListStudentServiceAsActionResult>();

            if (param.isAdvisor)
            {
                var getAdvisorHomeroomList = await _dbContext.Entity<MsHomeroomTeacher>()
                    .Where(x => x.IdBinusian == param.IdUser && x.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear)
                    .Select(x => x.IdHomeroom)
                    .ToListAsync();

                if (getAdvisorHomeroomList.Count() == 0)
                {
                    return Request.CreateApiResult2(null as object);
                }

                var getListStudent = await _dbContext.Entity<MsHomeroomStudent>()
                    .Include(x => x.Homeroom)
                        .ThenInclude(x => x.Grade)
                    .Include(x => x.Student)
                    .Where(x => x.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                        && getAdvisorHomeroomList.Any(y => y == x.Homeroom.Id)
                    )
                    .ToListAsync(CancellationToken);

                var filteredStudent = getListStudent.Select(x => new
                {
                    IdStudent = x.IdStudent,
                    StudentName = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName),
                    IdGrade = x.Homeroom.Grade.Id,
                    GradeDesc = x.Homeroom.Grade.Description,
                    GradeOrderNumber = x.Homeroom.Grade.OrderNumber
                })
                .Distinct()
                .ToList();

                var defaultStatus = await _dbContext.Entity<MsServiceAsActionStatus>()
                    .Where(x => x.StatusDesc == "On Track")
                    .FirstOrDefaultAsync(CancellationToken);

                var getTrExperienceHeaders = await _dbContext.Entity<TrServiceAsActionHeader>()
                    .Include(x => x.StatusOverall)
                    .Where(x => x.IdAcademicYear == param.IdAcademicYear
                        && filteredStudent.Select(y => y.IdStudent).ToList().Any(y => y == x.IdStudent)
                    )
                    .ToListAsync();

                var finalData = filteredStudent
                    .GroupJoin(
                        getTrExperienceHeaders,
                        c => c.IdStudent,
                        s => s.IdStudent,
                        (c1, t1) => new { c = c1, ts = t1 }
                    )
                    .SelectMany(c1 => c1.ts.DefaultIfEmpty(),
                        (c1, t1) => new
                        {
                            IdStudent = c1.c.IdStudent,
                            StudentName = c1.c.StudentName,
                            IdGrade = c1.c.IdGrade,
                            GradeDesc = c1.c.GradeDesc,
                            GradeOrderNumber = c1.c.GradeOrderNumber,
                            IdExperienceHeader = t1?.Id,
                            IdStatus = t1 == null ? defaultStatus.Id : t1.IdStatusOverall,
                            StutusDesc = t1 == null ? defaultStatus.StatusDesc : t1.StatusOverall.StatusDesc
                        }
                    )
                    .OrderBy(x => x.GradeOrderNumber)
                        .ThenBy(x => x.GradeDesc)
                        .ThenBy(x => x.StudentName)
                    .ToList();

                result = finalData.Select(x => new GetListStudentServiceAsActionResult
                {
                    Grade = new ItemValueVm
                    {
                        Id = x.IdGrade,
                        Description = x.GradeDesc
                    },
                    Student = new ItemValueVm
                    {
                        Id = x.IdStudent,
                        Description = x.StudentName
                    },
                    OverallStatus = new ItemValueVm
                    {
                        Id = x.IdStatus,
                        Description = x.StutusDesc
                    },
                    IdServiceAsActionHeader = x.IdExperienceHeader
                })
                .ToList();
            }
            else
            {
                var getlistStudentEnroll = await _dbContext.Entity<TrServiceAsActionForm>()
                    .Where(x => x.IdSupervisor == param.IdUser)
                    .Select(x => new
                    {
                        IdStudent = x.ServiceAsActionHeader.IdStudent,
                        OverallStatus = new ItemValueVm
                        {
                            Id = x.ServiceAsActionHeader.IdStatusOverall,
                            Description = x.ServiceAsActionHeader.StatusOverall.StatusDesc
                        },
                        IdServiceAsActionHeader = x.IdServiceAsActionHeader
                    })
                    .Distinct()
                    .ToListAsync();

                var getstudents = getlistStudentEnroll.Select(x => x.IdStudent).ToList();

                var getListStudentData = await _dbContext.Entity<MsHomeroomStudent>()
                    .Where(x => getstudents.Any(y => y == x.IdStudent) 
                    && x.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear)
                    .Select(x => new
                    {
                        IdStudent = x.IdStudent,
                        Grade = new ItemValueVm
                        {
                            Id = x.Homeroom.Grade.Id,
                            Description = x.Homeroom.Grade.Description
                        },
                        StudentName = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)
                    })
                    .Distinct()
                    .ToListAsync();

                result = getlistStudentEnroll
                    .GroupJoin(
                        getListStudentData,
                        c => c.IdStudent,
                        s => s.IdStudent,
                        (c1, t1) => new { c = c1, ts = t1 }
                    )
                    .SelectMany(c1 => c1.ts.DefaultIfEmpty(),
                        (c1, t1) => new GetListStudentServiceAsActionResult
                        {
                            Grade = t1.Grade,
                            Student = new ItemValueVm
                            {
                                Id = c1.c.IdStudent,
                                Description = t1.StudentName
                            },
                            OverallStatus = c1.c.OverallStatus,
                            IdServiceAsActionHeader = c1.c.IdServiceAsActionHeader
                        }
                    ).ToList();
            }

            if(!String.IsNullOrEmpty(param.IdStatus))
            {
                result = result.Where(x => x.OverallStatus.Id == param.IdStatus).ToList();
            }

            if(!String.IsNullOrEmpty(param.IdGrade))
            {
                result = result.Where(x => x.Grade.Id == param.IdGrade).ToList();
            }

            return Request.CreateApiResult2(result as object);
        }

    }
}
