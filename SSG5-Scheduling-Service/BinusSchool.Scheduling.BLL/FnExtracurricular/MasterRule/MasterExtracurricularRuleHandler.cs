using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterRule;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnExtracurricular.MasterRule.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterRule
{
    public class MasterExtracurricularRuleHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;


        public MasterExtracurricularRuleHandler(ISchedulingDbContext DbContext, IMachineDateTime dateTime)
        {
            _dbContext = DbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsExtracurricularRule>()                  
                            .Where(x => ids.Any(y => y == x.Id))
                            .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));


            _dbContext.Entity<MsExtracurricularRule>().RemoveRange(datas);
           
            // find already used ids
            //foreach (var data in datas)
            //{                
            //    // data yang di bind dengan extracurricular tidak boleh di delete
            //    //if (data.Extracurriculars.Count != 0)
            //    //{
            //    //    datas.Remove(data);
            //    //    undeleted.AlreadyUse ??= new Dictionary<string, string>();
            //    //    undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.Name ?? data.Id));
            //    //}
            //    //else
            //    //{
            //        data.IsActive = false;
            //        _dbContext.Entity<MsExtracurricularRule>().Update(data);
            //    //}
            //}

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var extracurricularRule = await _dbContext.Entity<MsExtracurricularRule>()
                                    .Include(x => x.ExtracurricularRuleGradeMappings)
                                        .ThenInclude(y => y.Grade)
                                    .Include(x => x.AcademicYear)
                                    .Where(a => a.Id == id).FirstOrDefaultAsync();

            if (extracurricularRule is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["ExtracurricularRule"], "Id", id));

            var ReturnResult = new GetMasterExtracurricularRuleResult() {
                Id = extracurricularRule.Id,
                AcademicYear = new ItemValueVm(extracurricularRule.IdAcademicYear, extracurricularRule.AcademicYear.Description),
                Semester = new ItemValueVm(extracurricularRule.Semester.ToString(), extracurricularRule.Semester.ToString()),
                ExtracurricularRuleName = extracurricularRule.Name,
                MinEffectives = extracurricularRule.MinEffectives,
                MaxEffectives = extracurricularRule.MaxEffectives,
                Status = extracurricularRule.Status,
                RegistrationStartDate = extracurricularRule.RegistrationStartDate,
                RegistrationEndDate = extracurricularRule.RegistrationEndDate,
                Grades = extracurricularRule.ExtracurricularRuleGradeMappings.Select(a => new ItemValueVm() { Id = a.IdGrade, Description = a.Grade.Description }).ToList(),
                HasReviewDate = (extracurricularRule.ReviewDate != null ? true : false),
                ReviewDate = extracurricularRule.ReviewDate,
                DueDayPayment = extracurricularRule.DueDayInvoice
            };


            return Request.CreateApiResult2(ReturnResult as object);
        
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetMasterExtracurricularRuleRequest>(nameof(GetMasterExtracurricularRuleRequest.IdSchool));

            var columns = new[] { "academicyear", "semester", "rulename", "registrationstartdate" };

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "IdAcademicYear" },
                { columns[1], "Semester" },
                { columns[2], "Name" },
                { columns[3], "RegistrationStartDate" }
            };

            var predicate = PredicateBuilder.Create<MsExtracurricularRule>(x => x != null);

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Name, param.SearchPattern()));
         
            var query = _dbContext.Entity<MsExtracurricularRule>()            
                .Where(predicate)
                .Where(x => x.AcademicYear.IdSchool == param.IdSchool
                && x.IdAcademicYear == (param.IdAcademicYear != null ? param.IdAcademicYear : x.IdAcademicYear)
                && x.Semester == (param.Semester != null ? param.Semester : x.Semester)
                && x.Status == (param.Status != null ? param.Status : x.Status)              
                )
                //.OrderByDescending(x => x.AcademicYear).ThenByDescending(x => x.Semester)
                .OrderByDynamic(param, aliasColumns);


            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                   .Select(x => new ItemValueVm(x.Id, x.Name))
                    .ToListAsync(CancellationToken);
                
            else

                items = await query
                    .SetPagination(param)
                    .Select(x => new GetMasterExtracurricularRuleResult
                    {
                        Id = x.Id,
                        AcademicYear =  new ItemValueVm (x.IdAcademicYear, x.AcademicYear.Description),
                        Semester = new ItemValueVm(x.Semester.ToString(), x.Semester.ToString()),                    
                        ExtracurricularRuleName = x.Name,
                        MinEffectives = x.MinEffectives,
                        MaxEffectives = x.MaxEffectives,
                        Status = x.Status,
                        RegistrationStartDate = x.RegistrationStartDate,
                        RegistrationEndDate = x.RegistrationEndDate,
                        Grades = x.ExtracurricularRuleGradeMappings.Select(a => new ItemValueVm() { Id = a.IdGrade, Description = a.Grade.Description }).ToList(),
                        HasReviewDate = (x.ReviewDate != null ? true : false),
                        ReviewDate = x.ReviewDate,
                        DueDayPayment = x.DueDayInvoice
                    })                    
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

          

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
            //return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));

        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {

            var body = await Request.ValidateBody<UpdateMasterExtracurricularRuleRequest, AddMasterExtracurricularRuleValidator>();

            if(body.ReviewDate != null)
            {
                if(body.ReviewDate < _dateTime.ServerTime)
                {
                    throw new BadRequestException("Review Date must be greater then now");
                }
            }
            #region unused code - comment at 8 december 2023
            //if (body.RegistrationStartDate != null)
            //{
            //    DateTime serverDate = DateTimeUtil.ServerTime.Date; // Get the current server date without the time
            //    DateTime registrationStartDate = body.RegistrationStartDate.Value.Date; // Get the date part of the RegistrationStartDate

            //    if (registrationStartDate < serverDate)
            //    {
            //        throw new BadRequestException("Registration Start Date must be greater than today");
            //    }
            //}
            #endregion

            if (body.ActionUpdateStatus == false)
            {
                var academicyear = await _dbContext.Entity<MsAcademicYear>().FindAsync(body.IdAcademicYear);
                if (academicyear is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["AcademicYear"], "Id", body.IdAcademicYear));

                var grades = await _dbContext.Entity<MsGrade>().Where(a => body.Grades.Select(b => b.Id).Contains(a.Id)).Select(c => c.Id).ToListAsync(CancellationToken);
                // find not found ids
                var gradeNotFound = body.Grades.Select(a => a.Id).Except(grades);
               
                if ((gradeNotFound?.Count() ?? 0) > 0)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Grade"], "Id", string.Join(",", gradeNotFound)));

                if (body.RegistrationStartDate != null)
                {
                    // untuk rules tidak boleh beririsan pada grade assigned
                    var ExistsMasterRule = await _dbContext.Entity<MsExtracurricularRule>()
                                        .Where(a => body.RegistrationStartDate <= a.RegistrationEndDate
                                        && body.RegistrationEndDate >= a.RegistrationStartDate
                                        && a.ExtracurricularRuleGradeMappings.Any(b => body.Grades.Select(c => c.Id).Contains(b.IdGrade))
                                        && a.Id != body.IdExtracurricularRule
                                        && a.IdAcademicYear == body.IdAcademicYear
                                        && a.Semester == body.Semester
                                        )
                                        .ToListAsync();

                    if ((ExistsMasterRule?.Count() ?? 0) > 0)
                        throw new BadRequestException("Have rules that overlap with other yearlevels and periods. Desc: " + string.Join(",", ExistsMasterRule.Select(a => a.Name)));

                }
            }

            var getElectiveRule = _dbContext.Entity<MsExtracurricularRule>()
                    .Where(a => a.IdAcademicYear == body.IdAcademicYear);

            if (string.IsNullOrEmpty(body.IdExtracurricularRule) && body.BothSemester == false)
            {
                var validateElectiveRule = getElectiveRule.Where(b => b.Semester == body.Semester
                            && b.ExtracurricularRuleGradeMappings.Any(c => body.Grades.Select(d => d.Id).Equals(c.IdGrade)))
                        .ToList();

                foreach (var items in validateElectiveRule)
                {
                    if (items.RegistrationStartDate == body.RegistrationStartDate || items.RegistrationEndDate == body.RegistrationEndDate)
                    {
                        throw new BadRequestException("StartDate or EndDate cannot be the same.");
                    }
                }

                var tempId = Guid.NewGuid().ToString();
                var param = new MsExtracurricularRule
                {
                    Id = tempId,
                    IdAcademicYear = body.IdAcademicYear,
                    Semester = body.Semester,
                    Name = body.Name,
                    MinEffectives = body.MinEffectives,
                    MaxEffectives = body.MaxEffectives,
                    RegistrationStartDate = body.RegistrationStartDate,
                    RegistrationEndDate = body.RegistrationEndDate?.Date.AddDays(1).AddTicks(-1),
                    Status = body.Status,                  
                    ReviewDate = body.ReviewDate,
                    DueDayInvoice = body.DueDayPayment,
                    RegisterForOneAY = body.BothSemester
                };

                _dbContext.Entity<MsExtracurricularRule>().Add(param);


                var paramGrades = body.Grades.Select(a => new TrExtracurricularRuleGradeMapping() { 
                                                        Id = Guid.NewGuid().ToString(),
                                                        IdExtracurricularRule = tempId,
                                                        IdGrade = a.Id}).ToList();

                _dbContext.Entity<TrExtracurricularRuleGradeMapping>().AddRange(paramGrades);


                await _dbContext.SaveChangesAsync(CancellationToken);
                
                return Request.CreateApiResult2();
            }
            else if (string.IsNullOrEmpty(body.IdExtracurricularRule) && body.BothSemester == true)
            {
                var validateElectiveRule = getElectiveRule
                        .Where(a => a.ExtracurricularRuleGradeMappings.Any(b => body.Grades.Select(c => c.Id).Equals(b.IdGrade))
                            && a.IdAcademicYear == body.IdAcademicYear)
                        .ToList();

                foreach (var items in validateElectiveRule)
                {
                    if (items.RegistrationStartDate == body.RegistrationStartDate || items.RegistrationEndDate == body.RegistrationEndDate)
                    {
                        throw new BadRequestException("StartDate or EndDate cannot be the same.");
                    }
                }

                for (int i = 1; i <= 2; i++)
                {
                    var tempId = Guid.NewGuid().ToString();
                    var insertElectiveRule = new MsExtracurricularRule
                    {
                        Id = tempId,
                        IdAcademicYear = body.IdAcademicYear,
                        Semester = i,
                        Name = body.Name,
                        MinEffectives = body.MinEffectives,
                        MaxEffectives = body.MaxEffectives,
                        RegistrationStartDate = body.RegistrationStartDate,
                        RegistrationEndDate = body.RegistrationEndDate?.Date.AddDays(1).AddTicks(-1),
                        Status = body.Status,
                        ReviewDate = body.ReviewDate,
                        DueDayInvoice = body.DueDayPayment,
                        RegisterForOneAY = body.BothSemester
                    };

                    _dbContext.Entity<MsExtracurricularRule>().Add(insertElectiveRule);

                    var paramGrades = body.Grades.Select(a => new TrExtracurricularRuleGradeMapping()
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdExtracurricularRule = tempId,
                        IdGrade = a.Id
                    }).ToList();

                    _dbContext.Entity<TrExtracurricularRuleGradeMapping>().AddRange(paramGrades);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);

                return Request.CreateApiResult2();
            }
            
            throw new BadRequestException(null);

        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateMasterExtracurricularRuleRequest, UpdateMasterExtracurricularRuleValidator>();
         

            if (!string.IsNullOrEmpty(body.IdExtracurricularRule))
            {
                var data = await _dbContext.Entity<MsExtracurricularRule>().FindAsync(body.IdExtracurricularRule);
                
                if (data is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["ExtracurricularRule"], "Id", body.IdExtracurricularRule));

                //if (data.RegistrationStartDate > _dateTime.ServerTime &&
                //    (data.ReviewDate ?? _dateTime.ServerTime) > _dateTime.ServerTime &&
                //    data.ReviewDate != null &&
                //    (body.ReviewDate == null || data.ReviewDate != body.ReviewDate))
                //{
                //    throw new BadRequestException("Can't change review date");
                //}

                #region unused code - comment at 8 december 2023
                //if(data.ReviewDate != null && (data.RegistrationStartDate < _dateTime.ServerTime || (data.ReviewDate ?? _dateTime.ServerTime) < _dateTime.ServerTime) && (data.ReviewDate != body.ReviewDate || body.ReviewDate == null))
                //{
                //    throw new BadRequestException("Can't change review date");
                //}
                #endregion

                if (body.ActionUpdateStatus == false)
                {
                    var academicyear = await _dbContext.Entity<MsAcademicYear>().FindAsync(body.IdAcademicYear);
                    if (academicyear is null)
                        throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["AcademicYear"], "Id", body.IdAcademicYear));

                    var grades = await _dbContext.Entity<MsGrade>().Where(a => body.Grades.Select(b => b.Id).Contains(a.Id)).Select(c => c.Id).ToListAsync(CancellationToken);

                    // find not found ids
                    var gradeNotFound = body.Grades.Select(a => a.Id).Except(grades);                  
                    if ((gradeNotFound?.Count() ?? 0) > 0)
                        throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Grade"], "Id", string.Join(",", gradeNotFound)));


                    if(body.RegistrationStartDate != null)
                    {                       
                        // untuk rules tidak boleh beririsan pada grade assigned
                        var ExistsMasterRule = await _dbContext.Entity<MsExtracurricularRule>()
                                            .Where(a => body.RegistrationStartDate <= a.RegistrationEndDate
                                            && body.RegistrationEndDate >= a.RegistrationStartDate
                                            && a.ExtracurricularRuleGradeMappings.Any(b => body.Grades.Select(c => c.Id).Contains(b.IdGrade))
                                            && a.Id != body.IdExtracurricularRule
                                            && a.IdAcademicYear == body.IdAcademicYear
                                            && a.Semester == body.Semester
                                            )
                                            .ToListAsync();

                        if ((ExistsMasterRule?.Count() ?? 0) > 0)
                            throw new BadRequestException("Have rules that overlap with other yearlevels and periods. Desc: " + string.Join(",", ExistsMasterRule.Select(a => a.Name)));

                    }

                }



                if (body.ActionUpdateStatus == true)
                {
                    data.Status = body.Status;
                    data.UserUp = AuthInfo.UserId;

                    _dbContext.Entity<MsExtracurricularRule>().Update(data);
                }
                else
                {
                    if (body.IsEnableReviewDate == false)
                        data.ReviewDate = null;
                    else
                        data.ReviewDate = body.ReviewDate;

                    data.Name = body.Name;
                    data.MinEffectives = body.MinEffectives;
                    data.MaxEffectives = body.MaxEffectives;
                    data.RegistrationStartDate = body.RegistrationStartDate;
                    data.RegistrationEndDate = body.RegistrationEndDate;                    
                    data.UserUp = AuthInfo.UserId;          
                    data.DueDayInvoice = body.DueDayPayment;
                    _dbContext.Entity<MsExtracurricularRule>().Update(data);


                    var existsGrades = await _dbContext.Entity<TrExtracurricularRuleGradeMapping>()
                                .Where(a => a.IdExtracurricularRule == body.IdExtracurricularRule)
                                .ToListAsync(CancellationToken);

                    var gradeDeleted = existsGrades.Where(a => !body.Grades.Select(b => b.Id).Contains(a.IdGrade)).ToList();
                
                    _dbContext.Entity<TrExtracurricularRuleGradeMapping>().RemoveRange(gradeDeleted);

                  
                    var gradeInserted = body.Grades.Where(a => !existsGrades.Select(b => b.IdGrade).Contains(a.Id)).Select(c => new TrExtracurricularRuleGradeMapping() {
                                                                                                                Id = Guid.NewGuid().ToString(),
                                                                                                                IdExtracurricularRule = body.IdExtracurricularRule,
                                                                                                                IdGrade = c.Id
                                                                                                            }).ToList();
                    _dbContext.Entity<TrExtracurricularRuleGradeMapping>().AddRange(gradeInserted);


                }




                await _dbContext.SaveChangesAsync(CancellationToken);
                return Request.CreateApiResult2();
            }

            throw new NotImplementedException();
        }
    }
}
