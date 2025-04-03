using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.ServiceAsAction
{
    public class GetListExperiencePerStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListExperiencePerStudentHandler
        (
            IStudentDbContext studentDbContext
        )
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListExperiencePerStudentRequest>(
                    nameof(GetListExperiencePerStudentRequest.IdStudent),
                    nameof(GetListExperiencePerStudentRequest.IdAcademicYear)
                );

            var result = new GetListExperiencePerStudentResult();

            var getServiceAsActionHeader = await _dbContext.Entity<TrServiceAsActionHeader>()
                .Include(x => x.Student)
                .Include(x => x.StatusOverall)
                .Include(x => x.AcademicYear)
                .Where(x => x.IdAcademicYear == param.IdAcademicYear && x.IdStudent == param.IdStudent)
                .FirstOrDefaultAsync(CancellationToken);

            var getGrade = await _dbContext.Entity<MsHomeroomStudent>()
                .Where(x => x.IdStudent == param.IdStudent && x.Homeroom.Grade.MsLevel.MsAcademicYear.Id == param.IdAcademicYear)
                .Select(x => new ItemValueVm
                {
                    Id = x.Homeroom.Grade.Id,
                    Description = x.Homeroom.Grade.Description
                })
                .FirstOrDefaultAsync(CancellationToken);


            if (getServiceAsActionHeader == null)
            {
                var defaultStatus = await _dbContext.Entity<MsServiceAsActionStatus>()
                    .Where(x => x.StatusDesc == "On Track")
                    .FirstOrDefaultAsync(CancellationToken);

                var getStudent = await _dbContext.Entity<MsStudent>()
                    .Where(x => x.Id == param.IdStudent)
                    .FirstOrDefaultAsync(CancellationToken);

                result.OverallStatus = new ItemValueVm
                {
                    Id = defaultStatus.Id,
                    Description = defaultStatus.StatusDesc
                };
                result.Student = new ItemValueVm
                {
                    Id = getStudent.Id,
                    Description = NameUtil.GenerateFullName(getStudent.FirstName, getStudent.MiddleName, getStudent.LastName)
                };
                result.IsStatusDisabled = true;
                result.ExperienceList = null;

            }
            else
            {
                var getExperienceList = await _dbContext.Entity<TrServiceAsActionForm>()
                    .Where(x => x.IdServiceAsActionHeader == getServiceAsActionHeader.Id)
                    .Select(x => new
                    {
                        IdServiceAsActionForm = x.Id,
                        ExperienceName = x.ExpName,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        Location = new ItemValueVm
                        {
                            Id = x.ServiceAsActionLocation.Id,
                            Description = x.ServiceAsActionLocation.SALocDes
                        },
                        Status = new ItemValueVm
                        {
                            Id = x.IdServiceAsActionStatus,
                            Description = x.ServiceAsActionStatus.StatusDesc,
                        },
                        IdSupervisor = x.IdSupervisor ?? null
                    })
                    .ToListAsync(CancellationToken);

                if (param.IsSupervisor == true)
                {
                    getExperienceList = getExperienceList.Where(x => x.IdSupervisor == param.IdUser).ToList();
                }

                var finalExperienceList = getExperienceList.Select(x => new GetListExperiencePerStudentResult_Experience
                {
                    IdServiceAsActionForm = x.IdServiceAsActionForm,
                    ExperienceName = x.ExperienceName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Location = x.Location,
                    Status = x.Status
                }).ToList();

                result.AcademicYear = new ItemValueVm
                {
                    Id = getServiceAsActionHeader.IdAcademicYear,
                    Description = getServiceAsActionHeader.AcademicYear.Code
                };

                result.IsStatusDisabled = false;
                result.ExperienceList = finalExperienceList;
                result.Student = new ItemValueVm
                {
                    Id = getServiceAsActionHeader.Student.Id,
                    Description = NameUtil.GenerateFullName(getServiceAsActionHeader.Student.FirstName, getServiceAsActionHeader.Student.MiddleName, getServiceAsActionHeader.Student.LastName)
                };
                result.OverallStatus = new ItemValueVm
                {
                    Id = getServiceAsActionHeader.IdStatusOverall,
                    Description = getServiceAsActionHeader.StatusOverall.StatusDesc
                };

            }

            result.Grade = getGrade;
            return Request.CreateApiResult2(result as object);
        }
    }
}
