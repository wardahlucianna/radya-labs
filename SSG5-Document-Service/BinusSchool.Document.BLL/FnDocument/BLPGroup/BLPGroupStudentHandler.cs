using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Document.FnDocument;
using BinusSchool.Data.Model.Document.FnDocument.BLPGroup;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.BLPGroup
{
    public class BLPGroupStudentHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = { 
            nameof(GetBLPGroupStudentRequest.IdAcademicYear), nameof(GetBLPGroupStudentRequest.Semester),
            //nameof(GetBLPGroupStudentRequest.IdLevel), nameof(GetBLPGroupStudentRequest.IdGrade),
            //nameof(GetBLPGroupStudentRequest.BLPFinalStatus)
        };

        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IClearanceForm _clearanceForm;
        
        public BLPGroupStudentHandler(IDocumentDbContext dbContext, IClearanceForm clearanceForm, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _clearanceForm = clearanceForm;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetBLPGroupStudentRequest>(_requiredParams);
            var predicate = PredicateBuilder.True<MsHomeroomStudent>();
            var predicateSurveyPeriod = PredicateBuilder.True<MsSurveyPeriod>();
            var predicateRespondent = PredicateBuilder.True<MsRespondent>();

            if (param.IdAcademicYear != null)
            {
                predicateSurveyPeriod = predicateSurveyPeriod.And(x => x.Grade.Level.IdAcademicYear == param.IdAcademicYear);
                predicateRespondent = predicateRespondent.And(x => x.SurveyPeriod.Grade.Level.IdAcademicYear == param.IdAcademicYear);
                predicate = predicate.And(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear);
            }
            if (param.Semester != 0)
            {
                predicateSurveyPeriod = predicateSurveyPeriod.And(x => x.Semester == param.Semester);
                predicateRespondent = predicateRespondent.And(x => x.SurveyPeriod.Semester == param.Semester);
                predicate = predicate.And(x => x.Semester == param.Semester);
            }
            if (param.IdLevel != null)
            {
                predicateSurveyPeriod = predicateSurveyPeriod.And(x => x.Grade.IdLevel == param.IdLevel);
                predicateRespondent = predicateRespondent.And(x => x.SurveyPeriod.Grade.IdLevel == param.IdLevel);
                predicate = predicate.And(x => x.Homeroom.Grade.IdLevel == param.IdLevel);
            }
            if (param.IdGrade != null)
            {
                predicateSurveyPeriod = predicateSurveyPeriod.And(x => x.IdGrade == param.IdGrade);
                predicateRespondent = predicateRespondent.And(x => x.SurveyPeriod.IdGrade == param.IdGrade);
                predicate = predicate.And(x => x.Homeroom.IdGrade == param.IdGrade);
            }
                
            //if (param.BLPFinalStatus != null)
            //    predicate = predicate.And(x => x.Status == param.Status);

            //Var Color
            var green = "bg-success";
            var red = "bg-danger";

            var getListStudent = await _dbContext.Entity<MsHomeroomStudent>()
                    .Include(x => x.Student)
                    .Include(x => x.Homeroom)
                        .ThenInclude(x => x.Grade)
                    .Include(x => x.Homeroom)
                        .ThenInclude(x => x.GradePathwayClassroom)
                        .ThenInclude(x => x.Classroom)
                    .Include(x => x.BLPGroupStudents)
                        .ThenInclude(x => x.BLPGroup)
                    .Include(x => x.BLPGroupStudents)
                        .ThenInclude(x => x.BLPStatus)
                    .Where(predicate)
                    .ToListAsync(CancellationToken);

            #region Get Final Result without API
            var listSurveyPeriod = await _dbContext.Entity<MsSurveyPeriod>()
                    .Include(x => x.Grade)
                        .ThenInclude(x => x.Level)
                        .ThenInclude(x => x.AcademicYear)
                    .Include(x => x.SurveyCategory)
                    .Where(predicateSurveyPeriod)
                    .Where(x => x.StartDate <= _dateTime.ServerTime && x.EndDate >= _dateTime.ServerTime)
                    .ToListAsync(CancellationToken);

            var listRespondent = await _dbContext.Entity<MsRespondent>()
                    .Include(x => x.Parent)
                        .ThenInclude(x => x.StudentParents)
                        .ThenInclude(x => x.Student)
                    .Include(x => x.SurveyPeriod)
                        .ThenInclude(x => x.SurveyCategory)
                    .Include(x => x.SurveyPeriod)
                        .ThenInclude(x => x.Grade)
                        .ThenInclude(x => x.Level)
                        .ThenInclude(x => x.AcademicYear)
                    .Include(x => x.ClearanceWeekPeriod)
                    .Include(x => x.SurveyStudentAnswers)
                        .ThenInclude(x => x.SurveyQuestionMapping)
                    .Where(predicateRespondent)
                    .ToListAsync(CancellationToken);
            #endregion

            var results = new List<GetBLPGroupStudentResult>();

            foreach (var data in getListStudent)
            {
                var valBLPFinalStatus = new FinalStatusDescription();

                #region Get Final Result with API (Slow and Cost Damage)
                //var getDataClearance = await _clearanceForm.GetAllStudentStatusClearanceForm(new GetAllStudentStatusClearanceFormRequest
                //{
                //    Username = data.IdStudent,
                //    IsParent = false
                //});

                //if (getDataClearance?.Payload != null)
                //{
                //    var dataClearance = getDataClearance.Payload.ListChild.FirstOrDefault();

                //    valBLPFinalStatus = dataClearance.FinalStatus != null ? new FinalStatusDescription()
                //    {
                //        FinalStatusEnum = dataClearance.FinalStatus.StatusEnum,
                //        Color = dataClearance.FinalStatus.Color,
                //        Description = dataClearance.FinalStatus.Description
                //    }: null;
                //}
                #endregion

                #region Get Final Result without API
                var getSurveyPeriod = listSurveyPeriod
                    .Where(x => _dateTime.ServerTime > x.StartDate && _dateTime.ServerTime < x.EndDate)
                    .Where(x => x.Grade.Level.AcademicYear.IdSchool == data.Student.IdSchool)
                    .Where(x => x.SurveyCategory.SurveyName.ToLower() == "clearance form")
                    .ToList();

                var getrespondent = listRespondent
                    .Where(x => x.IdStudent == data.IdStudent)
                    .Where(x => getSurveyPeriod.Any(y => y.Id == x.IdSurveyPeriod))
                    .ToList();

                var ClearanceForm = getrespondent
                    .Where(x => x.IdStudent == data.IdStudent)
                    .Where(x => x.SurveyPeriod.SurveyCategory.SurveyName.ToLower() == "clearance form")
                    .OrderByDescending(x => x.DateIn)
                    .FirstOrDefault();

                if (ClearanceForm != null)
                {
                    if (ClearanceForm.ResultSummary == true)
                    {
                        valBLPFinalStatus = new FinalStatusDescription()
                        {
                            FinalStatusEnum = BLPFinalStatus.Allowed,
                            Color = green,
                            Description = BLPFinalStatus.Allowed.GetDescription()
                        };
                    }
                    if (ClearanceForm.ResultSummary == false)
                    {
                        valBLPFinalStatus = new FinalStatusDescription()
                        {
                            FinalStatusEnum = BLPFinalStatus.NotAllowed,
                            Color = red,
                            Description = BLPFinalStatus.NotAllowed.GetDescription()
                        };
                    }
                }
                else
                {
                    valBLPFinalStatus = new FinalStatusDescription()
                    {
                        FinalStatusEnum = BLPFinalStatus.NotApplicable,
                        Description = BLPFinalStatus.NotApplicable.GetDescription()
                    };
                }
                #endregion

                var CheckSurveyPeriod = listSurveyPeriod.Where(x => x.IdGrade == data.Homeroom.IdGrade).ToList();

                var getBLPGroupStudent = data.BLPGroupStudents.Where(x => x.IdStudent == data.IdStudent).SingleOrDefault();
                var query = new GetBLPGroupStudentResult
                {
                    IsOpenPeriod = CheckSurveyPeriod == null ? false : true,
                    IdBLPGroupStudent = getBLPGroupStudent == null ? null : getBLPGroupStudent.Id,
                    Student = new NameValueVm
                    {
                        Id = data.IdStudent,
                        Name = NameUtil.GenerateFullName(data.Student.FirstName, data.Student.MiddleName, data.Student.LastName) 
                    },
                    Homeroom = new ItemValueVm
                    {
                        Id = data.IdHomeroom,
                        Description = data.Homeroom.Grade.Description //+ " " + data.Homeroom?.GradePathwayClassroom?.Classroom?.Code ==>untuk sementara di comment karena blm ada data classroom
                    },
                    BLPStatus = new ItemValueVm
                    {
                        Id = getBLPGroupStudent == null ? "2" : getBLPGroupStudent.IdBLPStatus,
                        Description = getBLPGroupStudent == null ? "HLP" : getBLPGroupStudent.BLPStatus.ShortName
                    },
                    BLPGroup = new ItemValueVm
                    {
                        Id = getBLPGroupStudent == null ? "-" : getBLPGroupStudent.IdBLPGroup,
                        Description = getBLPGroupStudent == null ? "-" : getBLPGroupStudent.BLPGroup.GroupName
                    },
                    HardCopySubmissionDate = getBLPGroupStudent?.HardCopySubmissionDate,
                    LastModifiedDate = getBLPGroupStudent == null ? null : (getBLPGroupStudent.DateUp == null ? getBLPGroupStudent.DateIn : getBLPGroupStudent.DateUp),
                    FinalStatus = valBLPFinalStatus //ClearanceForm == null ? null : valBLPFinalStatus
                };
                results.Add(query);
            }

            // filter by param BLP Final Status
            results = results
                .Where(x => param.BLPFinalStatus == null ? true : x.FinalStatus.FinalStatusEnum == param.BLPFinalStatus)
                .ToList();

            return Request.CreateApiResult2(results as object);
        }
    }
}
