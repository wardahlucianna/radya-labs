using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Document.FnDocument.BLPSettingPeriod;
using BinusSchool.Document.FnDocument.BLPSettingPeriod.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace BinusSchool.Document.FnDocument.BLPSettingPeriod
{
    public class BLPSettingPeriodHandler : FunctionsHttpCrudHandler
    {
        private readonly IDocumentDbContext _dbContext;
        public BLPSettingPeriodHandler(IDocumentDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsSurveyPeriod>()
                            .Include(x => x.Respondents)
                            .Where(x => ids.Any(y => y == x.Id))
                            .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            if (datas.Count > 0)
            {
                if(datas.Where(a => a.Respondents.Count() > 0).ToList().Count() > 0)
                {
                    undeleted.AlreadyUse = ids.ToDictionary(x => x, x => string.Format(Localizer["ExAlreadyUse"], x));                   
                    await Transaction.RollbackAsync(CancellationToken);
                    return Request.CreateApiResult2(errors: undeleted.AsErrors());
                }
            }          

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));


            _dbContext.Entity<MsSurveyPeriod>().RemoveRange(datas);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var SurveyPeriod = await _dbContext.Entity<MsSurveyPeriod>()
                                                    .Include(x => x.Grade).ThenInclude(y => y.Level).ThenInclude(y => y.AcademicYear)
                                                    .Include(x => x.ClearanceWeekPeriods)
                                                    .Include(x => x.ConsentCustomSchedules)
                                                    .Include(x => x.SurveyCategory)
                                                    .Where(a => a.Id == id).FirstOrDefaultAsync();

            if (SurveyPeriod is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["SurveyPeriod"], "Id", id));

            var ReturnResult = new GetBLPSettingDetailResult()
            {
                IdSurvey = SurveyPeriod.Id,
                IdSurveyCategory = SurveyPeriod.IdSurveyCategory,
                SurveyCategoryName = SurveyPeriod.SurveyCategory.SurveyName,
                IdAcademicYear = SurveyPeriod.Grade.Level.IdAcademicYear,
                AcademicYear = SurveyPeriod.Grade.Level.AcademicYear.Code,
                Semester = SurveyPeriod.Semester,
                IdLevel = SurveyPeriod.Grade.IdLevel,
                LevelName = SurveyPeriod.Grade.Level.Description,
                IdGrade = SurveyPeriod.IdGrade,
                Grade = SurveyPeriod.Grade.Description,
                StartDate = SurveyPeriod.StartDate,
                EndDate = SurveyPeriod.EndDate,
                HasConsentCustomSchedule = (SurveyPeriod.IdSurveyCategory == "1" && SurveyPeriod.CustomSchedule),
                ConsentCustomSchedule = SurveyPeriod.ConsentCustomSchedules?.Select(a => new GetBLPSettingDetailResult_CustomScheduleVm { IdConsentCustomSchedule  = a.Id,
                                                                                                                                        StartDay = a.StartDay,
                                                                                                                                        EndDay = a.EndDay,
                                                                                                                                        StartTime = a.StartTime,
                                                                                                                                        EndTime = a.EndTime
                                                                                                                                        }).FirstOrDefault() ?? null,
                HasClearanceCustomSchedule = (SurveyPeriod.IdSurveyCategory == "2" && SurveyPeriod.CustomSchedule),
                ClearanceWeekPeriod = SurveyPeriod.ClearanceWeekPeriods?.Select(a => new GetBLPSettingDetailResult_ClearanceWeekVm
                                                                                {
                                                                                    IdClearanceWeekPeriod = a.Id,
                                                                                    WeekID = a.WeekID,
                                                                                    IdBLPGroup = a.IdBLPGroup,
                                                                                    StartDate = a.StartDate,
                                                                                    EndDate = a.EndDate
                                                                                }).ToList() ?? null
            };

            return Request.CreateApiResult2(ReturnResult as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetBLPSettingRequest>(nameof(GetBLPSettingRequest.IdAcademicYear), nameof(GetBLPSettingRequest.Semester));
                       
            IReadOnlyList<IItemValueVm> returnResult;

            returnResult = await _dbContext.Entity<MsSurveyPeriod>()
                                .Include(x => x.Grade)
                                    .ThenInclude(y => y.Level)
                                    .ThenInclude(y => y.AcademicYear)
                                .Include(x => x.SurveyCategory)
                                .Where(x => x.Grade.Level.IdAcademicYear == param.IdAcademicYear &&
                                x.Semester == param.Semester &&
                                x.IdSurveyCategory == (param.IdSurveyCategory != null ? param.IdSurveyCategory : x.IdSurveyCategory)
                                )
                                .OrderByDescending(x => x.StartDate)
                                .Select(a => new GetBLPSettingResult
                                {
                                    IdSurvey = a.Id,
                                    IdSurveyCategory = a.IdSurveyCategory,
                                    SurveyCategoryName = a.SurveyCategory.SurveyName,
                                    AcademicYear = a.Grade.Level.AcademicYear.Code,
                                    Semester = a.Semester,
                                    Grade = a.Grade.Description,
                                    StartDate = a.StartDate.ToString("dd-MM-yyyy"),
                                    EndDate = a.EndDate.ToString("dd-MM-yyyy")
                                })
                                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(returnResult);
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddBLPSettingPeriodRequest, AddBLPSettingPeriodValidator>();

         

            var academicyear = await _dbContext.Entity<MsAcademicYear>().FindAsync(body.IdAcademicYear);
            if (academicyear is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["AcademicYear"], "Id", body.IdAcademicYear));


            var grades = await _dbContext.Entity<MsGrade>().Where(a => body.GradeList.Select(b => b.ToString()).Contains(a.Id)).Select(c => c.Id).ToListAsync(CancellationToken);
            // find not found ids
            var gradeNotFound = body.GradeList.Select(a => a.ToString()).Except(grades);

            if ((gradeNotFound?.Count() ?? 0) > 0)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Grade"], "Id", string.Join(",", gradeNotFound)));

            foreach(var itemGrade in body.GradeList)
            {
                var getSurvey = await _dbContext.Entity<MsSurveyPeriod>().Where(a => a.IdGrade == itemGrade && 
                                                                                    a.Semester == body.Semester && 
                                                                                    a.IdSurveyCategory == body.IdSurveyCategory
                                                                                    ).FirstOrDefaultAsync();
                if(getSurvey == null)
                {
                    // add
                    var generatedSurveyPeriod = Guid.NewGuid().ToString();
                    var param = new MsSurveyPeriod
                    {
                        Id = generatedSurveyPeriod,                       
                        Semester = body.Semester,
                        IdSurveyCategory = body.IdSurveyCategory,
                        StartDate = body.StartDateSurvey,
                        EndDate = body.EndDateSurvey,
                        CustomSchedule = (body.IdSurveyCategory == "1" ? body.HasConsentCustomSchedule : body.HasClearanceWeekPeriod),
                        IdGrade = itemGrade
                    };
                    _dbContext.Entity<MsSurveyPeriod>().Add(param);

                    if(body.IdSurveyCategory == "1" && body.HasConsentCustomSchedule)
                    {
                        var paramConsentCustomSchedule = new MsConsentCustomSchedule
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdSurveyPeriod = generatedSurveyPeriod,
                            StartDay = body.ConsentCustomSchedule.StartDay,
                            EndDay = body.ConsentCustomSchedule.EndDay,
                            StartTime = TimeSpan.Parse(body.ConsentCustomSchedule.StartTime),
                            EndTime = TimeSpan.Parse(body.ConsentCustomSchedule.EndTime)
                        };
                        _dbContext.Entity<MsConsentCustomSchedule>().Add(paramConsentCustomSchedule);

                    }
                    else if(body.IdSurveyCategory == "2" && body.HasClearanceWeekPeriod)
                    {
                        var paramClearanceWeekPeriod = body.ClearanceWeekPeriod.Select(a => new MsClearanceWeekPeriod { 
                                                            Id = Guid.NewGuid().ToString(),
                                                            IdBLPGroup = a.IdBLPGroup,
                                                            IdSurveyPeriod = generatedSurveyPeriod,
                                                            WeekID = a.WeekID,
                                                            StartDate = a.StartDate,
                                                            EndDate = a.EndDate
                                                            }).ToList();

                        _dbContext.Entity<MsClearanceWeekPeriod>().AddRange(paramClearanceWeekPeriod);
                    }
                }
                else
                {
                    //update
                    getSurvey.StartDate = body.StartDateSurvey;
                    getSurvey.EndDate = body.EndDateSurvey;
                    getSurvey.CustomSchedule = (body.IdSurveyCategory == "1" ? body.HasConsentCustomSchedule : body.HasClearanceWeekPeriod);
                    _dbContext.Entity<MsSurveyPeriod>().Update(getSurvey);

                    if (body.IdSurveyCategory == "1" && body.HasConsentCustomSchedule)
                    {
                        var getConsentCustomSchedule = await _dbContext.Entity<MsConsentCustomSchedule>().Where(a => a.IdSurveyPeriod == getSurvey.Id).FirstOrDefaultAsync();

                        if (body.HasConsentCustomSchedule)
                        {                            
                            if (getConsentCustomSchedule != null)
                            {
                                getConsentCustomSchedule.StartDay = body.ConsentCustomSchedule.StartDay;
                                getConsentCustomSchedule.EndDay = body.ConsentCustomSchedule.EndDay;
                                getConsentCustomSchedule.StartTime = TimeSpan.Parse(body.ConsentCustomSchedule.StartTime);
                                getConsentCustomSchedule.EndTime = TimeSpan.Parse(body.ConsentCustomSchedule.EndTime);
                                _dbContext.Entity<MsConsentCustomSchedule>().Update(getConsentCustomSchedule);
                            }
                            else
                            {
                                var paramConsentCustomSchedule = new MsConsentCustomSchedule
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdSurveyPeriod = getSurvey.Id,
                                    StartDay = body.ConsentCustomSchedule.StartDay,
                                    EndDay = body.ConsentCustomSchedule.EndDay,
                                    StartTime = TimeSpan.Parse(body.ConsentCustomSchedule.StartTime),
                                    EndTime = TimeSpan.Parse(body.ConsentCustomSchedule.EndTime)
                                };
                                _dbContext.Entity<MsConsentCustomSchedule>().Add(paramConsentCustomSchedule);
                            }
                        }
                        else
                        {
                            getConsentCustomSchedule.IsActive = false;
                            _dbContext.Entity<MsConsentCustomSchedule>().Update(getConsentCustomSchedule);
                            // _dbContext.Entity<MsConsentCustomSchedule>().Remove(getConsentCustomSchedule);
                        }
                      
                    }
                    else if (body.IdSurveyCategory == "2")
                    {
                        var UpdatedClearanceWeekPeriod = await _dbContext.Entity<MsClearanceWeekPeriod>().Where(a => a.IdSurveyPeriod == getSurvey.Id).ToListAsync();
                    
                        if (body.HasClearanceWeekPeriod)
                        {
                            foreach (var weekPeriod in body.ClearanceWeekPeriod)
                            {
                                var WeekPeriodUpdated = UpdatedClearanceWeekPeriod.Where(a => a.WeekID == weekPeriod.WeekID).FirstOrDefault();

                                if (WeekPeriodUpdated != null)
                                {
                                    WeekPeriodUpdated.IdBLPGroup = weekPeriod.IdBLPGroup;
                                    WeekPeriodUpdated.StartDate = weekPeriod.StartDate;
                                    WeekPeriodUpdated.EndDate = weekPeriod.EndDate;
                                    _dbContext.Entity<MsClearanceWeekPeriod>().Update(WeekPeriodUpdated);
                                }
                                else
                                {
                                    var InsertWeekPeriod = new MsClearanceWeekPeriod()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdBLPGroup = weekPeriod.IdBLPGroup,
                                        IdSurveyPeriod = getSurvey.Id,
                                        WeekID = weekPeriod.WeekID,
                                        StartDate = weekPeriod.StartDate,
                                        EndDate = weekPeriod.EndDate
                                    };
                                    _dbContext.Entity<MsClearanceWeekPeriod>().Add(InsertWeekPeriod);
                                }
                            }
                            var deletedWeekPeriod = UpdatedClearanceWeekPeriod.Where(a => !body.ClearanceWeekPeriod.Select(b => b.WeekID).Contains(a.WeekID)).ToList();
                            deletedWeekPeriod.ForEach(a => a.IsActive = false);
                            _dbContext.Entity<MsClearanceWeekPeriod>().UpdateRange(deletedWeekPeriod);
                     
                        }       
                    }
                }
                await _dbContext.SaveChangesAsync(CancellationToken);
            }           

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateBLPSettingPeriodRequest, UpdateBLPSettingPeriodValidator>();
                       
            var grades = await _dbContext.Entity<MsGrade>().FindAsync(body.IdGrade);
            if (grades is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Grade"], "Id", body.IdGrade));

            var getSurvey = await _dbContext.Entity<MsSurveyPeriod>().Where(a => a.Id == body.IdSurveyPeriod).FirstOrDefaultAsync();
            if (getSurvey != null)
            {
                getSurvey.StartDate = body.StartDateSurvey;
                getSurvey.EndDate = body.EndDateSurvey;
                getSurvey.CustomSchedule = (body.IdSurveyCategory == "1" ? body.HasConsentCustomSchedule : body.HasClearanceWeekPeriod);
                _dbContext.Entity<MsSurveyPeriod>().Update(getSurvey);

                if (body.IdSurveyCategory == "1" && body.HasConsentCustomSchedule)
                {
                    var getConsentCustomSchedule = await _dbContext.Entity<MsConsentCustomSchedule>().Where(a => a.IdSurveyPeriod == getSurvey.Id).FirstOrDefaultAsync();

                    if (body.HasConsentCustomSchedule)
                    {
                        if (getConsentCustomSchedule != null)
                        {
                            getConsentCustomSchedule.StartDay = body.ConsentCustomSchedule.StartDay;
                            getConsentCustomSchedule.EndDay = body.ConsentCustomSchedule.EndDay;
                            getConsentCustomSchedule.StartTime = TimeSpan.Parse(body.ConsentCustomSchedule.StartTime);
                            getConsentCustomSchedule.EndTime = TimeSpan.Parse(body.ConsentCustomSchedule.EndTime);
                            _dbContext.Entity<MsConsentCustomSchedule>().Update(getConsentCustomSchedule);
                        }
                        else
                        {
                            var paramConsentCustomSchedule = new MsConsentCustomSchedule
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdSurveyPeriod = getSurvey.Id,
                                StartDay = body.ConsentCustomSchedule.StartDay,
                                EndDay = body.ConsentCustomSchedule.EndDay,
                                StartTime = TimeSpan.Parse(body.ConsentCustomSchedule.StartTime),
                                EndTime = TimeSpan.Parse(body.ConsentCustomSchedule.EndTime)
                            };
                            _dbContext.Entity<MsConsentCustomSchedule>().Add(paramConsentCustomSchedule);
                        }
                    }
                    else
                    {
                         getConsentCustomSchedule.IsActive = false;
                        _dbContext.Entity<MsConsentCustomSchedule>().Update(getConsentCustomSchedule);
                        //_dbContext.Entity<MsConsentCustomSchedule>().Remove(getConsentCustomSchedule);
                    }

                }
                else if (body.IdSurveyCategory == "2")
                {
                    var UpdatedClearanceWeekPeriod = await _dbContext.Entity<MsClearanceWeekPeriod>().Where(a => a.IdSurveyPeriod == getSurvey.Id).ToListAsync();
                             
                    if (body.HasClearanceWeekPeriod)
                    {
                        foreach (var weekPeriod in body.ClearanceWeekPeriod)
                        {
                            var WeekPeriodUpdated = UpdatedClearanceWeekPeriod.Where(a => a.WeekID == weekPeriod.WeekID).FirstOrDefault();

                            if(WeekPeriodUpdated != null)
                            {
                                WeekPeriodUpdated.IdBLPGroup = weekPeriod.IdBLPGroup;
                                WeekPeriodUpdated.StartDate = weekPeriod.StartDate;
                                WeekPeriodUpdated.EndDate = weekPeriod.EndDate;
                                _dbContext.Entity<MsClearanceWeekPeriod>().Update(WeekPeriodUpdated);
                            }
                            else 
                            {
                                var InsertWeekPeriod = new MsClearanceWeekPeriod() {
                                                    Id = Guid.NewGuid().ToString(),
                                                    IdBLPGroup = weekPeriod.IdBLPGroup,
                                                    IdSurveyPeriod = getSurvey.Id,
                                                    WeekID = weekPeriod.WeekID,
                                                    StartDate = weekPeriod.StartDate,
                                                    EndDate = weekPeriod.EndDate
                                };
                                _dbContext.Entity<MsClearanceWeekPeriod>().Add(InsertWeekPeriod);
                            }
                        }
                        var deletedWeekPeriod = UpdatedClearanceWeekPeriod.Where(a => !body.ClearanceWeekPeriod.Select(b => b.WeekID).Contains(a.WeekID)).ToList();
                        deletedWeekPeriod.ForEach(a => a.IsActive = false);
                        _dbContext.Entity<MsClearanceWeekPeriod>().UpdateRange(deletedWeekPeriod);

                    }
                }

                await _dbContext.SaveChangesAsync(CancellationToken);

            }
            else
            {
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["SurveyPeriod"], "Id", body.IdSurveyPeriod));
            }
            return Request.CreateApiResult2();
        }
    }
}
