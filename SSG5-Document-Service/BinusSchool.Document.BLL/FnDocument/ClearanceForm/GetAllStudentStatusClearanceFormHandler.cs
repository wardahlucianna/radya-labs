using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using BinusSchool.Data.Model.Document.FnDocument.ClearanceForm;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using BinusSchool.Common.Utils;
using BinusSchool.Common.Abstractions;

namespace BinusSchool.Document.FnDocument.ClearanceForm
{
    public class GetAllStudentStatusClearanceFormHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = { nameof(GetAllStudentStatusClearanceFormRequest.Username), nameof(GetAllStudentStatusClearanceFormRequest.IsParent) };

        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetAllStudentStatusClearanceFormHandler(IDocumentDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAllStudentStatusClearanceFormRequest>(_requiredParams);

            var green = "bg-success";
            var yellow = "bg-warning";
            var red = "bg-danger";
            var clearanceform = "clearance form";
            var concernform = "concern form";

            #region Check Student
            var idStudent = string.Concat(param.Username.Where(char.IsDigit));

            var student = await _dbContext.Entity<MsStudent>()
                    .Where(x => x.Id == idStudent)
                    .SingleOrDefaultAsync(CancellationToken);

            if (student is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["IdStudent"], "Not Exist", "IdStudent"));
            #endregion

            #region Get List Sibling
            var siblingGroupId = await _dbContext.Entity<MsSiblingGroup>()
                    .Where(x => x.IdStudent == student.Id)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(CancellationToken);

            var listSibling = await _dbContext.Entity<MsSiblingGroup>()
                    .Where(x => x.Id == siblingGroupId)
                    .Where(x => x.Student.IdSchool == student.IdSchool)
                    .Select(x => x.IdStudent)
                    .ToListAsync(CancellationToken);
            #endregion

            #region Get Needed List
            var listBLPGroupStudent = await _dbContext.Entity<TrBLPGroupStudent>()
                    .Include(x => x.BLPStatus)
                    .Include(x => x.BLPGroup)
                    .Where(p => listSibling.Any(p2 => p2 == p.IdStudent))
                    .ToListAsync(CancellationToken);

            var listSurveyPeriod = await _dbContext.Entity<MsSurveyPeriod>()
                    .Include(x => x.Grade)
                        .ThenInclude(x => x.Level)
                        .ThenInclude(x => x.AcademicYear)
                    .Include(x => x.SurveyCategory)
                    .Where(x => x.Grade.Level.AcademicYear.IdSchool == student.IdSchool)
                    .ToListAsync(CancellationToken);

            var listRespondent = await _dbContext.Entity<MsRespondent>()
                    .Include(x => x.Parent)
                        .ThenInclude(x => x.StudentParents)
                        .ThenInclude(x => x.Student)
                    .Include(x => x.SurveyPeriod)
                        .ThenInclude(x => x.SurveyCategory)
                    .Include(x => x.ClearanceWeekPeriod)
                    .Include(x => x.SurveyStudentAnswers)
                        .ThenInclude(x => x.SurveyQuestionMapping)
                    .Where(x => listSibling.Any(y => y == x.IdStudent))
                    .ToListAsync(CancellationToken);
            #endregion

            #region Get Survey Period 
            var getSurveyPeriod = listSurveyPeriod
                    .Where(x => _dateTime.ServerTime > x.StartDate && _dateTime.ServerTime < x.EndDate)
                    .Where(x => x.Grade.Level.AcademicYear.IdSchool == student.IdSchool)
                    .Where(x => x.SurveyCategory.SurveyName.ToLower() == clearanceform)
                    .ToList();

            var getSemester = getSurveyPeriod.Select(x => x.Semester).Distinct().FirstOrDefault();
            var getGradeList = getSurveyPeriod.Select(x => x.IdGrade).Distinct().ToList();

            var getListStudent = new List<MsHomeroomStudent>();
            if (param.IsParent)
            {
                getListStudent = await _dbContext.Entity<MsHomeroomStudent>()
                    .Include(x => x.Student)
                    .Include(x => x.Homeroom)
                        .ThenInclude(x => x.GradePathwayClassroom)
                        .ThenInclude(x => x.Classroom)
                    .Include(x => x.Homeroom)
                        .ThenInclude(x => x.Grade)
                        .ThenInclude(x => x.Level)
                        .ThenInclude(x => x.AcademicYear)
                    .Where(x => listSibling.Select(x => x).Any(y => y == x.IdStudent))
                    .Where(x => x.Semester == getSemester)
                    .Where(x => getGradeList.Any(y => y == x.Homeroom.IdGrade))
                    .ToListAsync(CancellationToken);
            }
            else
            {
                getListStudent = await _dbContext.Entity<MsHomeroomStudent>()
                    .Include(x => x.Student)
                    .Include(x => x.Homeroom)
                        .ThenInclude(x => x.GradePathwayClassroom)
                        .ThenInclude(x => x.Classroom)
                    .Include(x => x.Homeroom)
                        .ThenInclude(x => x.Grade)
                        .ThenInclude(x => x.Level)
                        .ThenInclude(x => x.AcademicYear)
                    .Where(x => x.IdStudent == idStudent)
                    .Where(x => x.Semester == getSemester)
                    .Where(x => getGradeList.Any(y => y == x.Homeroom.IdGrade))
                    .ToListAsync(CancellationToken);
            }

            var getrespondent = listRespondent
                    .Where(x => listSibling.Any(y => y == x.IdStudent))
                    .Where(x => getSurveyPeriod.Any(y => y.Id == x.IdSurveyPeriod))
                    .ToList();
            #endregion

            var listChildOpened = new List<AllStudentStatusClearanceForm_Student>();
            var liststudent = getListStudent.Select(x => x.IdStudent).Distinct().ToList();

            var results = new GetAllStudentStatusClearanceFormResult();

            foreach (var item in liststudent)
            {
                var data = getListStudent.Where(x => x.IdStudent == item).OrderByDescending(x => x.DateIn).FirstOrDefault();

                var ConcernForm = getrespondent
                    .Where(x => x.IdStudent == data.IdStudent)
                    .Where(x => x.SurveyPeriod.SurveyCategory.SurveyName.ToLower() == concernform)
                    .OrderByDescending(x => x.DateIn)
                    .FirstOrDefault();
                var ClearanceForm = getrespondent
                    .Where(x => x.IdStudent == data.IdStudent)
                    .Where(x => x.SurveyPeriod.SurveyCategory.SurveyName.ToLower() == clearanceform)
                    .OrderByDescending(x => x.DateIn)
                    .FirstOrDefault();
                //var StudentRespond = getrespondent.Where(x => x.IdStudent == data.IdStudent).SingleOrDefault();

                //Check is Clearence Form Regular or Not => if true regular
                var isCustomSchedule = getSurveyPeriod.Where(x => x.IdGrade == data.Homeroom.IdGrade).Select(x => x.CustomSchedule).FirstOrDefault();

                #region Child Data
                var childOpened = new AllStudentStatusClearanceForm_Student();
                childOpened.IsSubmitted = getrespondent
                    .Where(x => x.IdStudent == data.IdStudent)
                    .OrderByDescending(x => x.DateIn)
                    .FirstOrDefault() == null ? false : true;
                childOpened.Student = new NameValueVm
                {
                    Id = data.IdStudent,
                    Name = NameUtil.GenerateFullName(data.Student.FirstName?.Trim(), data.Student.LastName?.Trim())
                };
                childOpened.Grade = new ItemValueVm
                {
                    Id = data.Homeroom.IdGrade,
                    Description = data.Homeroom.Grade.Description
                };
                childOpened.Homeroom = new ItemValueVm
                {
                    Id = data.IdHomeroom,
                    Description = data.Homeroom.Grade.Description //+ " " + data.Homeroom?.GradePathwayClassroom?.Classroom?.Code ==> hide classroom for now
                };
                childOpened.AcademicYear = new CodeWithIdVm
                {
                    Id = data.Homeroom.IdAcademicYear,
                    Code = data.Homeroom.AcademicYear.Code,
                    Description = data.Homeroom.AcademicYear.Description
                };
                #endregion

                if (isCustomSchedule)
                {
                    var BLPGroupStudent = listBLPGroupStudent
                        .Where(x => x.IdStudent == data.IdStudent)
                        .Where(x => x.BLPStatus.Id == "1")
                        .FirstOrDefault();

                    childOpened.BLPStatus = BLPGroupStudent == null ? null : new ItemValueVm
                    {
                        Id = BLPGroupStudent.IdBLPStatus,
                        Description = BLPGroupStudent.BLPStatus.BLPStatusName
                    };
                    childOpened.Group = BLPGroupStudent == null ? null : new ItemValueVm
                    {
                        Id = BLPGroupStudent.IdBLPGroup,
                        Description = BLPGroupStudent.BLPGroup?.GroupName
                    };
                }

                #region ConcernForm
                if (ConcernForm != null)
                {
                    if (ConcernForm.ResultSummary == true)
                    {
                        childOpened.ConsentForm = new StudentColorDescription()
                        {
                            StatusEnum = BLPFinalStatus.Allowed,
                            Description = BLPFinalStatus.Allowed.GetDescription().ToString(),
                            Color = green
                        };
                    }
                    if (ConcernForm.ResultSummary == false)
                    {
                        childOpened.ConsentForm = new StudentColorDescription()
                        {
                            StatusEnum = BLPFinalStatus.NotAllowed,
                            Description = BLPFinalStatus.NotAllowed.GetDescription().ToString(),
                            Color = red
                        };
                    }
                }
                #endregion

                #region Final Status and Clearance Form
                if (ClearanceForm != null)
                {
                    if (ClearanceForm.ResultSummary == true)
                    {
                        childOpened.ClearanceForm = new StudentColorDescription()
                        {
                            StatusEnum = BLPFinalStatus.Allowed,
                            Description = BLPFinalStatus.Allowed.GetDescription().ToString(),
                            Color = green
                        };

                        childOpened.FinalStatus = new StudentColorDescription()
                        {
                            StatusEnum = BLPFinalStatus.Allowed,
                            Description = BLPFinalStatus.Allowed.GetDescription().ToString(),
                            Color = green
                        };
                    }
                    if (ClearanceForm.ResultSummary == false)
                    {
                        childOpened.ClearanceForm = new StudentColorDescription()
                        {
                            StatusEnum = BLPFinalStatus.NotAllowed,
                            Description = BLPFinalStatus.NotAllowed.GetDescription().ToString(),
                            Color = red
                        };

                        childOpened.FinalStatus = new StudentColorDescription()
                        {
                            StatusEnum = BLPFinalStatus.NotAllowed,
                            Description = BLPFinalStatus.NotAllowed.GetDescription().ToString(),
                            Color = red
                        };
                    }
                }
                #endregion

                if (param.IsParent)
                {
                    results.IsAnySubmitted = getrespondent.Count() > 0 ? true : false;
                }
                else
                {
                    results.IsAnySubmitted = getrespondent
                        .Where(x => x.IdStudent == data.IdStudent)
                        .OrderByDescending(x => x.DateIn)
                    .FirstOrDefault() == null ? false : true;
                }
                listChildOpened.Add(childOpened);
            }
            results.ListChild = listChildOpened;

            return Request.CreateApiResult2(results as object);
        }
    }
}
