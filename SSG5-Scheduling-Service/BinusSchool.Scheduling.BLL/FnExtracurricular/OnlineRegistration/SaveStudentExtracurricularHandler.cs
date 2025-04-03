using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Finance.FnPayment;
using BinusSchool.Data.Api.Scheduling.FnExtracurricular;
using BinusSchool.Data.Model.Finance.FnPayment.ExtracurricularInvoice;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.OnlineRegistration;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnExtracurricular.OnlineRegistration.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnExtracurricular.OnlineRegistration
{
    public class SaveStudentExtracurricularHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private IDbContextTransaction _transaction;
        private readonly IOnlineRegistration _scpOnlineRegistrationApi;
        private readonly IExtracurricularInvoice _extracurricularInvoiceApi;

        public SaveStudentExtracurricularHandler(ISchedulingDbContext dbContext,
            IOnlineRegistration scpOnlineRegistrationApi,
            IExtracurricularInvoice extracurricularInvoiceApi,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _scpOnlineRegistrationApi = scpOnlineRegistrationApi;
            _extracurricularInvoiceApi = extracurricularInvoiceApi;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<SaveStudentExtracurricularRequest, SaveStudentExtracurricularValidator>();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var resultData = new SaveStudentExtracurricularResult();
            resultData.ExtraccurricularList = new List<NameValueVm>();

            try
            {
                var idStudentListParam = new List<string>();
                idStudentListParam.Add(param.IdStudent);

                var studentGradeDetailResult = await _scpOnlineRegistrationApi.GetActiveStudentsGradeByStudent(new GetActiveStudentsGradeByStudentRequest
                {
                    IdStudent = idStudentListParam
                });

                if (studentGradeDetailResult.Payload.Count() <= 0)
                {
                    throw new BadRequestException(null);
                }

                var studentDetail = studentGradeDetailResult.Payload.FirstOrDefault();

                var extracurricularRule = _dbContext.Entity<TrExtracurricularRuleGradeMapping>()
                                                    .Include(ergm => ergm.ExtracurricularRule)
                                                    .Where(x => x.IdGrade == studentDetail.Grade.Id &&
                                                                x.ExtracurricularRule.Status == true &&
                                                                // validating if current date is inside registration period
                                                                (_dateTime.ServerTime >= x.ExtracurricularRule.RegistrationStartDate &&
                                                                _dateTime.ServerTime <= x.ExtracurricularRule.RegistrationEndDate)
                                                            )
                                                    .FirstOrDefault();

                if (extracurricularRule == null)
                {
                    throw new BadRequestException("Failed! No extracurricular rule has been set for this grade. Please contact the staff.");
                }
                else
                {
                    // get max effective number for the grade
                    var effectiveCount = new
                    {
                        maxEffective = extracurricularRule.ExtracurricularRule.MaxEffectives,
                        minEffective = extracurricularRule.ExtracurricularRule.MinEffectives
                    };

                    // list all extracurricular id
                    List<string> idExtracurricularList = new List<string>();
                    foreach (var paramExtracurricular in param.ExtracurricularList)
                    {
                        idExtracurricularList.Add(paramExtracurricular.IdExtracurricular);
                    }

                    var getExtracurricularList = _dbContext.Entity<MsExtracurricular>()
                        .Include(a => a.ExtracurricularType)
                        .Where(x => param.ExtracurricularList.Select(y => y.IdExtracurricular).Any(y => y == x.Id) &&
                            x.Status == true)
                        .ToList();

                    var allMaxExtracurricularParticipantList = getExtracurricularList
                                                            .Select(x => new
                                                            {
                                                                IdExtracurricular = x.Id,
                                                                MaxParticipant = x.MaxParticipant
                                                            })
                                                            .ToList();

                    var allExtracurricularParticipantCountList = _dbContext.Entity<MsExtracurricularParticipant>()
                                                                .Where(x => param.ExtracurricularList.Select(y => y.IdExtracurricular).Any(y => y == x.IdExtracurricular))
                                                                .ToList()
                                                                .GroupBy(x => x.IdExtracurricular)
                                                                .Select(x => new
                                                                {
                                                                    IdExtracurricular = x.Key,
                                                                    TotalParticipant = x.Select(x => x.IdStudent).Distinct().Count()
                                                                })
                                                                .Distinct()
                                                                .ToList();

                    var studentExtracurricular = _dbContext.Entity<MsExtracurricularParticipant>()
                                                        .Include(ep => ep.Extracurricular)
                                                        .Where(x => x.IdStudent == param.IdStudent &&
                                                                    x.IdGrade == studentDetail.Grade.Id &&
                                                                    x.Extracurricular.Semester == studentDetail.Semester &&
                                                                    x.Extracurricular.Status == true)
                                                        .Distinct()
                                                        .ToList();

                    var studentExtracurricularCount = studentExtracurricular
                                                        .Count();

                    var updateExistingStudentExtracurricularCount = studentExtracurricular
                                                                    .Where(x => param.ExtracurricularList.Select(y => y.IdExtracurricular).Any(y => y == x.IdExtracurricular))
                                                                    .Count();

                    // list invoice student extracurricular
                    var studentExtracurricularInvoiceStatusData = await _extracurricularInvoiceApi.GetStudentExtracurricularInvoiceStatus(new GetStudentExtracurricularInvoiceStatusRequest
                    {
                        IdStudentList = new List<string>() { param.IdStudent }
                    });

                    if (studentExtracurricularInvoiceStatusData.Payload.Count() <= 0)
                    {
                        throw new BadRequestException(null);
                    }

                    var studentExtracurricularInvoiceStatusListRaw = studentExtracurricularInvoiceStatusData.Payload.FirstOrDefault();

                    var studentExtracurricularInvoiceStatusList = studentExtracurricularInvoiceStatusListRaw.ExtracurricularList;

                    var createNewExtracurricularInvoiceList = new List<CreateStudentExtracurricularInvoiceRequest_ExtracurricularData>();

                    var listExceedParticipantCount = new List<string>();
                    foreach (var paramExtracurricular in param.ExtracurricularList)
                    {

                        #region no longer using priority
                        //// Validating value of priority (min value = 1, max value = student extracurricular count)
                        //var isValidPriority = (paramExtracurricular.Priority >= 1 && paramExtracurricular.Priority <= effectiveCount.maxEffective);

                        //if (!isValidPriority)
                        //{
                        //    throw new BadRequestException("Failed! Invalid priority value: minimum priority is 1 and maximum priority is " + studentExtracurricularCount);
                        //}
                        //else
                        //{
                        //    // Validating same priority (cannot input same priority)
                        //    var hasDuplicatePriority = studentExtracurricular.Any(x => x.Priority == paramExtracurricular.Priority);

                        //    if (hasDuplicatePriority)
                        //    {
                        //        throw new BadRequestException("Failed! Cannot insert the same priority value for each extracurriculars");
                        //    }
                        //    else
                        //    {
                        //        var addQuery = _dbContext.Entity<MsExtracurricularParticipant>()
                        //                    .Add(new MsExtracurricularParticipant
                        //                    {
                        //                        Id = Guid.NewGuid().ToString(),
                        //                        IdExtracurricular = paramExtracurricular.IdExtracurricular,
                        //                        IdStudent = param.IdStudent,
                        //                        IdGrade = studentDetail.Grade.Id,
                        //                        JoinDate = _dateTime.ServerTime,
                        //                        Status = true,
                        //                        Priority = paramExtracurricular.Priority,
                        //                        UserIn = param.IdUserIn
                        //                    });
                        //    }
                        //}
                        #endregion

                        var studentCurrentExtracurricularParticipantList = studentExtracurricular
                                                                .Where(x => x.IdExtracurricular == paramExtracurricular.IdExtracurricular)
                                                                .ToList();

                        if (studentCurrentExtracurricularParticipantList.Count > 1)
                            throw new BadRequestException(null);

                        var studentCurrentExtracurricularParticipant = studentCurrentExtracurricularParticipantList.FirstOrDefault();

                        var isAlreadyRegisteredThisExcul = studentCurrentExtracurricularParticipantList.Any();

                        // checked condition
                        if (paramExtracurricular.IsChecked == true)
                        {
                            if (isAlreadyRegisteredThisExcul == false)
                            {

                                // get max extracurricular participant
                                var maxExtracurricularParticipant = allMaxExtracurricularParticipantList
                                                                    .Where(x => x.IdExtracurricular == paramExtracurricular.IdExtracurricular)
                                                                    .Select(x => x.MaxParticipant)
                                                                    .FirstOrDefault();

                                // get total participants
                                int extracurricularParticipantCount = allExtracurricularParticipantCountList
                                                                        .Where(x => x.IdExtracurricular == paramExtracurricular.IdExtracurricular)
                                                                        .Select(x => x.TotalParticipant)
                                                                        .FirstOrDefault();

                                var extracurricularData = getExtracurricularList.FirstOrDefault(x => x.Id == paramExtracurricular.IdExtracurricular);

                                // check if total participants is not exceeding the participant max
                                if (extracurricularParticipantCount >= maxExtracurricularParticipant)
                                {
                                    listExceedParticipantCount.Add(extracurricularData.Name);
                                    continue;
                                    //throw new BadRequestException("Failed! Number of participants has extracurricularData the maximum number of participants allowed");
                                }

                                // Check if the student has reached the maximum number of extracurriculars
                                var isJoinedMaxExtracurricular = (studentExtracurricularCount - updateExistingStudentExtracurricularCount) >= effectiveCount.maxEffective ? true : false;

                                if (isJoinedMaxExtracurricular)
                                    throw new BadRequestException("Failed! Student has joined the maximum number of extracurriculars");

                                var addQuery = _dbContext.Entity<MsExtracurricularParticipant>()
                                            .Add(new MsExtracurricularParticipant
                                            {
                                                Id = Guid.NewGuid().ToString(),
                                                IdExtracurricular = paramExtracurricular.IdExtracurricular,
                                                IdStudent = param.IdStudent,
                                                IdGrade = studentDetail.Grade.Id,
                                                JoinDate = _dateTime.ServerTime,
                                                Status = true,
                                                //Priority = paramExtracurricular.Priority,
                                                UserIn = param.IdUserIn??AuthInfo.UserId,
                                                IsPrimary = true
                                            });

                                // if today is more than review date, then directly create the invoice
                                if (extracurricularRule.ExtracurricularRule.ReviewDate.HasValue ? _dateTime.ServerTime >= extracurricularRule.ExtracurricularRule.ReviewDate : true)
                                {
                                    // check only one extracurricular
                                    var getExtracurricular = getExtracurricularList
                                                                .Where(x => x.Id == paramExtracurricular.IdExtracurricular)
                                                                .FirstOrDefault();

                                    // list all extracurricular invoice that want to be created
                                    var createNewExtracurricularInvoice = new CreateStudentExtracurricularInvoiceRequest_ExtracurricularData
                                    {
                                        IdExtracurricular = paramExtracurricular.IdExtracurricular,
                                        ExtracurricularPrice = getExtracurricular.Price,
                                        ExtracurricularType = getExtracurricular.ExtracurricularType.Code,
                                    };

                                    createNewExtracurricularInvoiceList.Add(createNewExtracurricularInvoice);
                                }

                                resultData.ExtraccurricularList.Add(new NameValueVm
                                {
                                    Id = extracurricularData.Id,
                                    Name = extracurricularData.Name
                                });
                            }
                        }

                        // un-checked condition
                        else if (paramExtracurricular.IsChecked == false)
                        {
                            if (isAlreadyRegisteredThisExcul == true)
                            {

                                // Check if current date is more than review date, then cannot delete extracurricular
                                if (extracurricularRule.ExtracurricularRule.ReviewDate != null && _dateTime.ServerTime >= extracurricularRule.ExtracurricularRule.ReviewDate && studentCurrentExtracurricularParticipant.Extracurricular.Price != 0)
                                    throw new BadRequestException("Cannot delete extracurricular from the student because it's already passed the review date");


                                // Check if already created invoice, then cannot delete extracurricular
                                var hasCreatedInvoice = studentExtracurricularInvoiceStatusList
                                                        .Where(x => x.Extracurricular.Id == paramExtracurricular.IdExtracurricular &&
                                                                    _dateTime.ServerTime <= x.DueDatePayment &&
                                                                    x.ExtracurricularPrice > 0)
                                                        .Any();

                                if (hasCreatedInvoice)
                                    throw new BadRequestException("Cannot delete extracurricular from the student because it's already created the invoice");

                                _dbContext.Entity<MsExtracurricularParticipant>().Remove(studentCurrentExtracurricularParticipant);

                                var hasInvoiceFree = studentExtracurricularInvoiceStatusList
                                                     .Where(x => x.Extracurricular.Id == paramExtracurricular.IdExtracurricular &&
                                                                 _dateTime.ServerTime <= x.DueDatePayment &&
                                                                 x.ExtracurricularPrice == 0)
                                                     .Any();

                                //add By Steven-7.08.23, fix error unregist elective payment when review date period
                                if (hasInvoiceFree)
                                {
                                    // delete invoice for free elective
                                    var deleteInvoiceApi = await _extracurricularInvoiceApi.DeleteStudentExtracurricularInvoice(new DeleteStudentExtracurricularInvoiceRequest
                                    {
                                        IdExtracurricular = studentCurrentExtracurricularParticipant.IdExtracurricular,
                                        IdStudent = param.IdStudent
                                    });

                                    if (deleteInvoiceApi.Payload.IsSuccess == false)
                                        throw new BadRequestException(null);
                                }      
                            
                            }
                        }
                    }

                    // continuation of check if total participants is not exceeding the participant max
                    if (listExceedParticipantCount.Count > 0)
                    {
                        throw new Exception($"Registration failed. Number of participants of {FormatStringList(listExceedParticipantCount)} has exceeded the maximum number of participants allowed.");
                    }

                    await _dbContext.SaveChangesAsync(CancellationToken);
                    await _transaction.CommitAsync(CancellationToken);

                    // create invoice
                    if (createNewExtracurricularInvoiceList.Count > 0)
                    {
                        //var homeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                        //                        .Where(x => x.IdHomeroom == studentDetail.Homeroom.Id &&
                        //                                    x.IdStudent == param.IdStudent &&
                        //                                    x.Semester == studentDetail.Semester)
                        //                        .FirstOrDefaultAsync(CancellationToken);

                        var createInvoiceData = new CreateStudentExtracurricularInvoiceRequest
                        {
                            ExtracurricularList = createNewExtracurricularInvoiceList,
                            IdStudent = param.IdStudent,
                            IdHomeroomStudent = studentDetail.IdHomeroomStudent,
                            Semester = studentDetail.Semester,
                            InvoiceStartDate = _dateTime.ServerTime,
                            InvoiceEndDate = _dateTime.ServerTime.AddDays(extracurricularRule.ExtracurricularRule.DueDayInvoice),
                            SendEmailNotification = true,
                        };

                        var createInvoiceDataList = new List<CreateStudentExtracurricularInvoiceRequest>();
                        createInvoiceDataList.Add(createInvoiceData);

                        var createInvoice = await _extracurricularInvoiceApi.CreateStudentExtracurricularInvoice(createInvoiceDataList);

                        //if (createInvoice.Payload.IsSuccess == false)
                        //    throw new BadRequestException(null);
                    }
                }
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }


            return Request.CreateApiResult2(resultData as object);
        }

        static string FormatStringList(List<string> list)
        {
            if (list == null || list.Count == 0)
            {
                return string.Empty;
            }

            if (list.Count == 1)
            {
                return list[0];
            }
            else if (list.Count == 2)
            {
                return $"{list[0]} and {list[1]}";
            }
            else
            {
                string commaSeparated = string.Join(", ", list.GetRange(0, list.Count - 1));
                return $"{commaSeparated}, and {list[list.Count - 1]}";
            }
        }
    }
}
