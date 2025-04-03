using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Document.FnDocument.ClearanceForm;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.ClearanceForm
{
    public class GetClearanceFormPeriodHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = { nameof(GetAllStudentStatusClearanceFormRequest.Username), nameof(GetAllStudentStatusClearanceFormRequest.IsParent) };

        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetClearanceFormPeriodHandler(IDocumentDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetClearanceFormPeriodRequest>(_requiredParams);

            bool isOpen = false;
            var results = new GetClearanceFormPeriodResult();
            var getrespondent = new List<MsRespondent>();
            var listChildOpened = new List<ClearanceFormPeriod_Student>();
            var getListStudent = new List<MsHomeroomStudent>();

            #region Check Student
            var idStudent = string.Concat(param.Username.Where(char.IsDigit));

            var student = await _dbContext.Entity<MsStudent>()
                    .Where(x => x.Id == idStudent)
                    .SingleOrDefaultAsync(CancellationToken);

            if (student is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["IdStudent"], "Not Exist", "IdStudent"));
            #endregion

            #region Get Needed List
            var listBLPGroupStudent = await _dbContext.Entity<TrBLPGroupStudent>()
                    .Include(x => x.Student)
                    .Include(x => x.BLPStatus)
                    .Where(x => x.Student.IdSchool == student.IdSchool)
                    .ToListAsync(CancellationToken);

            var listClearanceWeekPeriod = await _dbContext.Entity<MsClearanceWeekPeriod>()
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
                    .Where(x => x.Student.IdSchool == student.IdSchool)
                    .ToListAsync(CancellationToken);
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
                    .ToListAsync();
            #endregion

            #region Get Active Survey Period
            var getSurveyPeriod = listSurveyPeriod
                    .Where(x => _dateTime.ServerTime > x.StartDate && _dateTime.ServerTime < x.EndDate)
                    .Where(x => x.Grade.Level.AcademicYear.IdSchool == student.IdSchool)
                    .Where(x => x.SurveyCategory.SurveyName.ToLower() == "clearance form")
                    .ToList();
            #endregion

            var getSemester = getSurveyPeriod.Select(x => x.Semester).FirstOrDefault();
            var getAllGrade = getSurveyPeriod.Select(x => x.IdGrade).Distinct().ToList();
            bool isCustomSchedule;


            if (param.IsParent)
            {
                getListStudent = await _dbContext.Entity<MsHomeroomStudent>()
                    .Include(x => x.Homeroom)
                        .ThenInclude(x => x.Grade)
                        .ThenInclude(x => x.Level)
                        .ThenInclude(x => x.AcademicYear)
                    .Include(x => x.Student)
                    .Where(x => listSibling.Any(y => y == x.IdStudent))
                    .Where(x => x.Semester == getSemester)
                    .ToListAsync(CancellationToken);

                var getCustomSchedule = getSurveyPeriod.Where(x => getListStudent.Select(y => y.Homeroom.IdGrade).ToList().Any(y => y == x.IdGrade)).Select(x => x.CustomSchedule).Distinct().ToList();
                //var getCustomSchedule = getSurveyPeriod.Where(x => getListStudent.Select(y => y.Homeroom.IdGrade).ToList().Any(y => y == x.IdGrade)).Select(x => x.CustomSchedule).ToList();

                if (getCustomSchedule.Count() > 1)
                    isCustomSchedule = false;
                else
                    isCustomSchedule = getCustomSchedule.FirstOrDefault();

                //var isAnyPeriodActiveForStudent = getListStudent.Where(x => getAllGrade.Any(y => y == x.Homeroom.IdGrade)).ToList();
                var isAnyPeriodActiveForStudent = getSurveyPeriod.Where(x => getListStudent.Select(y => y.Homeroom.IdGrade).ToList().Any(y => y == x.IdGrade)).ToList();

                #region Get List Respondent
                getrespondent = listRespondent
                    .Where(x => listSibling.Any(y => y == x.IdStudent))
                    .Where(x => getSurveyPeriod.Any(y => y.Id == x.IdSurveyPeriod))
                    .OrderByDescending(x => x.DateIn)
                    .ToList();
                #endregion
                var inPeriodP = isAnyPeriodActiveForStudent.Count() == 0 ? false : true;
                isOpen = CustomSchedule(getrespondent, isCustomSchedule, inPeriodP);

                results.InPeriod = isAnyPeriodActiveForStudent.Count() == 0 ? false : true;
                results.IsOpened = isOpen;
                results.IsReguler = isCustomSchedule;
                results.IsThisPeriodAnySubmitted = getrespondent.Count() == 0 ? false : true;
                results.StartDate = isAnyPeriodActiveForStudent?.Select(x => x.StartDate).FirstOrDefault();
                results.EndDate = isAnyPeriodActiveForStudent?.Select(x => x.EndDate).FirstOrDefault();
            }
            else
            {
                var getStudent = await _dbContext.Entity<MsHomeroomStudent>()
                    .Include(x => x.Homeroom)
                        .ThenInclude(x => x.Grade)
                        .ThenInclude(x => x.Level)
                        .ThenInclude(x => x.AcademicYear)
                    .Include(x => x.Student)
                    .Where(x => x.IdStudent == idStudent)
                    .Where(x => x.Semester == getSemester)
                    .OrderByDescending(x => x.DateIn)
                    .FirstOrDefaultAsync(CancellationToken);

                var isAnyPeriodActiveForStudent = getSurveyPeriod.Where(x => x.IdGrade == getStudent.Homeroom.IdGrade).FirstOrDefault();
                //var isCustomSchedule = getSurveyPeriod.Where(x => x.IdGrade == getStudent.Homeroom.IdGrade).Select(x => x.CustomSchedule).FirstOrDefault();

                getrespondent = listRespondent
                   .Where(x => x.IdStudent == idStudent)
                   .Where(x => x.IdSurveyPeriod == isAnyPeriodActiveForStudent?.Id)
                   .ToList();

                isOpen = CustomSchedule(getrespondent, isAnyPeriodActiveForStudent.CustomSchedule, isAnyPeriodActiveForStudent == null ? false : true);

                results.InPeriod = isAnyPeriodActiveForStudent == null ? false : true;
                results.IsOpened = isOpen;
                results.IsReguler = isAnyPeriodActiveForStudent.CustomSchedule;
                results.IsThisPeriodAnySubmitted = getrespondent.Count() == 0 ? false : true;
                results.StartDate = isAnyPeriodActiveForStudent?.StartDate;
                results.EndDate = isAnyPeriodActiveForStudent?.EndDate;

                getListStudent.Add(getStudent);
            }

            foreach (var item in getListStudent)
            {
                var data = item;

                //Check is Clearence Form Regular or Not => if true regular
                var getCustomScheduleStudent = getSurveyPeriod.Where(x => x.IdGrade == data.Homeroom.IdGrade).Select(x => x.CustomSchedule).FirstOrDefault();
                var isSurveyOpen = getSurveyPeriod.Where(x => x.IdGrade == data.Homeroom.IdGrade).FirstOrDefault();
                var isAnyPeriodActiveForStudentDetail = getSurveyPeriod.Where(x => x.IdGrade == item.Homeroom.IdGrade).FirstOrDefault();

                var isOpenStudent = CustomSchedule(getrespondent, getCustomScheduleStudent, isAnyPeriodActiveForStudentDetail == null ? false : true);

                if (isSurveyOpen != null)
                {
                    if (getCustomScheduleStudent)
                    {
                        if (isOpenStudent)
                        {
                            var BLPGroupStudent = listBLPGroupStudent
                            .Where(x => x.IdStudent == data.IdStudent)
                            .Where(x => x.HomeroomStudent.IdHomeroom == data.IdHomeroom)
                            .FirstOrDefault();

                            var ClearanceWeekPeriod = listClearanceWeekPeriod
                                .Where(x => x.IdBLPGroup == BLPGroupStudent?.IdBLPGroup)
                                .Where(x => x.IdSurveyPeriod == isSurveyOpen?.Id)
                                .FirstOrDefault();

                            var ChildOpened = new ClearanceFormPeriod_Student()
                            {
                                IdStudent = data.IdStudent,
                                StudentName = NameUtil.GenerateFullName(data.Student.FirstName?.Trim(), data.Student.LastName?.Trim()),
                                IdClearanceWeekPeriod = ClearanceWeekPeriod?.Id,
                                IdSurveyPeriod = isSurveyOpen.Id
                            };
                            listChildOpened.Add(ChildOpened);
                        }
                    }
                    else
                    {
                        var ChildOpened = new ClearanceFormPeriod_Student()
                        {
                            IdStudent = data.IdStudent,
                            StudentName = NameUtil.GenerateFullName(data.Student.FirstName?.Trim(), data.Student.LastName?.Trim()),
                            IdSurveyPeriod = isSurveyOpen.Id
                        };
                        listChildOpened.Add(ChildOpened);
                    }
                }
            }

            results.Student = listChildOpened.Count() == 0 ? null : listChildOpened;

            return Request.CreateApiResult2(results as object);
        }

        public bool CustomSchedule(List<MsRespondent> getrespondent, bool isCustomSchedule, bool inPeriod)
        {
            bool isOpen = false;
            var respondent = getrespondent.FirstOrDefault();
            if (isCustomSchedule)
            {
                if (inPeriod)
                {
                    int getDayToday = (int)_dateTime.ServerTime.DayOfWeek + 1;
                    var excludeDays = new int[] { 1, 7 };

                    if (respondent != null && excludeDays.Contains(getDayToday) != true)
                    {
                        isOpen = false;
                    }
                    else if (respondent != null && excludeDays.Contains(getDayToday) == true)
                    {
                        isOpen = true;
                    }
                    else
                    {
                        isOpen = true;
                    }
                }
                else
                {
                    isOpen = false;
                }
            }
            else
            {
                if (inPeriod)
                {
                    isOpen = true;
                }
                else
                {
                    isOpen = false;
                }
            }

            return isOpen;
        }
    }
}
