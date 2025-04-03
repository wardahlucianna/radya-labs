using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{
    public class DetailExperienceHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public DetailExperienceHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<DetailExperienceRequest>();

            var dataExperience = await _dbContext.Entity<TrExperience>()
                                .Include(x => x.AcademicYear)
                                .Include(x => x.HomeroomStudent)
                                .Include(x => x.TrExperienceTypes)
                                .Include(x => x.TrExperienceLearnings).ThenInclude(x => x.LearningOutcome)
                                .Include(x => x.TrExperienceStatusChangeHs).ThenInclude(x => x.UserApproval)
                                .Where(e => e.Id == param.IdExperience)
                                .FirstOrDefaultAsync(CancellationToken);

            if(dataExperience == null)
               throw new BadRequestException($"{param.IdExperience} not found");

            var result = new DetailExperienceResult
            {
                Id = dataExperience.Id,
                IdStudent = dataExperience.HomeroomStudent.IdStudent,
                IdHomeroomStudent = dataExperience.IdHomeroomStudent,
                AcademicYear = new CodeWithIdVm(dataExperience.AcademicYear.Id,dataExperience.AcademicYear.Code,dataExperience.AcademicYear.Description),
                ExperienceName = dataExperience.ExperienceName,
                ExperienceType = dataExperience.TrExperienceTypes != null ? dataExperience.TrExperienceTypes.Select(y => y.ExperienceType).ToList() : null,
                ExperienceLocation = dataExperience.ExperienceLocation,
                StartDate = dataExperience.StartDate,
                EndDate = dataExperience.EndDate,
                IdUserSupervisor = dataExperience.IdUserSupervisor,
                SupervisorName = dataExperience.SupervisorName,
                RoleName = dataExperience.RoleName,
                PositionName = dataExperience.PositionName,
                SupervisorTitle = dataExperience.SupervisorTitle,
                SupervisorEmail = dataExperience.SupervisorEmail,
                SupervisorContact = dataExperience.SupervisorContact,
                Organization = dataExperience.Organizer,
                Description = dataExperience.Description,
                ContributionOrganizer = dataExperience.ContributionOrganizer,
                LearningOutcomes = dataExperience.TrExperienceLearnings != null ? dataExperience.TrExperienceLearnings.Select(y => new CodeWithIdVm
                {
                    Id = y.IdLearningOutcome,
                    Code = y.IdLearningOutcome,
                    Description = y.LearningOutcome.LearningOutcomeName
                }).ToList() : null,
                Status = dataExperience.Status,
                CanEdit = dataExperience.Status == ExperienceStatus.NeedRevision && (param.Role == "STUDENT" || param.Role == "STD"),
                Approver = dataExperience.TrExperienceStatusChangeHs != null ? dataExperience.TrExperienceStatusChangeHs.Select(y => new ListApproverNote
                {
                    ApproverDate = y.ExperienceStatusChangeDate.ToString(),
                    ApproverName = y.UserApproval.DisplayName,
                    ApproverNote = y.Note
                }).ToList() : null
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}
