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
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Finance.FnPayment;
using BinusSchool.Data.Model.Finance.FnPayment.ExtracurricularInvoice;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.SendEmail;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant
{
    public class DeleteAllStudentParticipantHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IExtracurricularInvoice _extracurricularInvoiceApi;
        private readonly Data.Api.Scheduling.FnExtracurricular.IEmailNotification _extracurricularEmailNotification;
        private IDbContextTransaction _transaction;

        public DeleteAllStudentParticipantHandler(
            ISchedulingDbContext dbContext,
            IExtracurricularInvoice extracurricularInvoiceApi,
            Data.Api.Scheduling.FnExtracurricular.IEmailNotification extracurricularEmailNotification,
            IMachineDateTime dateTime
            )
        {
            _dbContext = dbContext;
            _extracurricularInvoiceApi = extracurricularInvoiceApi;
            _extracurricularEmailNotification = extracurricularEmailNotification;
            _dateTime = dateTime;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteAllStudentParticipantRequest, DeleteAllStudentParticipantValidator>();

            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var resultList = new List<DeleteAllStudentParticipantResult>();

            try
            {
                var getExtracurricularList = await _dbContext.Entity<MsExtracurricular>()
                                                        .Where(x => param.IdExtracurricularList.Any(y => y == x.Id))
                                                        .ToListAsync(CancellationToken);

                if (getExtracurricularList == null || getExtracurricularList.Count() == 0)
                    throw new BadRequestException("Extracurricular not found");

                var getExtracurricularParticipantList = getExtracurricularList.Join(
                                                            _dbContext.Entity<MsExtracurricularParticipant>()
                                                                .Include(ep => ep.Extracurricular)
                                                                .Include(ep => ep.Student)
                                                                .Include(ep => ep.Grade)
                                                                .ThenInclude(g => g.Level)
                                                                .ThenInclude(l => l.AcademicYear)
                                                                .ThenInclude(ay => ay.School),
                                                            extracurricular => extracurricular.Id,
                                                            participant => participant.IdExtracurricular,
                                                            (extracurricular, participant) => new { extracurricular, participant }
                                                            )
                                                        .ToList();

                #region Unused code
                //// get student invoice detail
                //var studentExtracurricularInvoiceStatusApi = await _extracurricularInvoiceApi.GetStudentExtracurricularInvoiceStatusFormatDataTable(new GetStudentExtracurricularInvoiceStatusRequest
                //{
                //    IdStudentList = getExtracurricularParticipantList.Select(x => x.IdStudent).ToList()
                //});

                //var studentExtracurricularInvoiceStatusList = new List<GetStudentExtracurricularInvoiceStatusFormatDataTableResult>();

                //var deletedInvoiceIdExtracurricularList = new List<string>();
                //var deletedParticipantIdExtracurricularList = new List<string>();

                //// not allowed to delete participant if student has already paid the invoice
                //if (studentExtracurricularInvoiceStatusApi.Payload != null && studentExtracurricularInvoiceStatusApi.Payload.Count() > 0)
                //    studentExtracurricularInvoiceStatusList = studentExtracurricularInvoiceStatusApi.Payload.ToList();

                //foreach (var idExtracurricularParam in param.IdExtracurricularList)
                //{
                //    isAllowDelete = true;
                //    hasCreatedInvoice = false;

                //    var studentExtracurricularInvoiceStatus = studentExtracurricularInvoiceStatusList
                //                                                .Where(x => x.Extracurricular.Id == idExtracurricularParam)
                //                                                .ToList();

                //    if (studentExtracurricularInvoiceStatus != null || studentExtracurricularInvoiceStatus.Count() > 0)
                //    {
                //        hasCreatedInvoice = true;
                //        if (studentExtracurricularInvoiceStatus.Where(x => x.PaymentStatus == true).Any())
                //        {
                //            isAllowDelete = false;
                //        }
                //    }

                //    if (isAllowDelete)
                //    {
                //        // if invoice is already created, then delete invoice
                //        if (hasCreatedInvoice)
                //        {
                //            deletedInvoiceIdExtracurricularList.Add(idExtracurricularParam);
                //        }
                //    }
                //}
                #endregion

                var deleteAllInvoiceApi = await _extracurricularInvoiceApi.DeleteAllStudentExtracurricularInvoice(new DeleteAllStudentExtracurricularInvoiceRequest
                    {
                        IdExtracurricularList = param.IdExtracurricularList
                    });

                //if (deleteAllInvoiceApi.Payload.IsSuccess == false)
                //    throw new BadRequestException(null);

                // hard delete
                var deletedInvoiceIdExtracurricularList = new List<string>();
                var undeletedInvoiceIdExtracurricularList = new List<string>();
                var deleteParticipantExtracurricularFinalList = new List<string>();

                if (deleteAllInvoiceApi.Payload != null && deleteAllInvoiceApi.Payload.Any())
                {
                    var deleteAllInvoiceList = deleteAllInvoiceApi.Payload.ToList();

                    deletedInvoiceIdExtracurricularList = deleteAllInvoiceList
                                                                .Where(x => x.IsSuccess == true)
                                                                .Select(x => x.IdExtracurricular)
                                                                .ToList();

                    undeletedInvoiceIdExtracurricularList = deleteAllInvoiceList
                                                                .Where(x => x.IsSuccess == false)
                                                                .Select(x => x.IdExtracurricular)
                                                                .ToList();

                    var uncreatedInvoiceExtracurricularList = param.IdExtracurricularList
                                                                .Where(x => !undeletedInvoiceIdExtracurricularList.Any(y => y == x))
                                                                .ToList();

                    deleteParticipantExtracurricularFinalList.AddRange(deletedInvoiceIdExtracurricularList);
                    deleteParticipantExtracurricularFinalList.AddRange(uncreatedInvoiceExtracurricularList);
                }

                // all extracurriculars haven't created invoice
                else
                {
                    deleteParticipantExtracurricularFinalList.AddRange(param.IdExtracurricularList);
                }



                var removeExtracurricularParticipant = getExtracurricularParticipantList
                                                    .Where(x => deleteParticipantExtracurricularFinalList.Any(y => y == x.participant.IdExtracurricular))
                                                    .Select(x => x.participant)
                                                    .ToList();

                foreach (var removedParticipant in removeExtracurricularParticipant)
                {
                    var result = new DeleteAllStudentParticipantResult()
                    {
                        School = new NameValueVm
                        {
                            Id = removedParticipant.Grade.Level.AcademicYear.School.Id,
                            Name = removedParticipant.Grade.Level.AcademicYear.School.Name
                        },
                        Extracurricular = new NameValueVm
                        {
                            Id = removedParticipant.Extracurricular?.Id,
                            Name = removedParticipant.Extracurricular?.Name
                        },
                        Student = new NameValueVm
                        {
                            Id = removedParticipant.Student?.Id,
                            Name = NameUtil.GenerateFullName(removedParticipant.Student?.FirstName, removedParticipant.Student?.MiddleName, removedParticipant.Student?.LastName)
                        }
                    };
                    resultList.Add(result);
                }

                // remove participant
                _dbContext.Entity<MsExtracurricularParticipant>().RemoveRange(removeExtracurricularParticipant);

                // set extracurricular to inactive
                var extracurricularSetInactiveList = getExtracurricularList
                                                    .Where(x => deleteParticipantExtracurricularFinalList.Any(y => y == x.Id))
                                                    .Distinct()
                                                    .ToList();

                extracurricularSetInactiveList = extracurricularSetInactiveList.Select(x => { x.Status = false; x.UserUp = ((string.IsNullOrEmpty(AuthInfo.UserId) || AuthInfo.UserId.Contains("00000000-0000-0000-0000-000000000000")) ? "SYS0001" : AuthInfo.UserId); return x; }).ToList();

                _dbContext.Entity<MsExtracurricular>().UpdateRange(extracurricularSetInactiveList);

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                // send email
                var sendEmailParamList = resultList
                                            .Select(x => new SendEmailCancelExtracurricularToParentRequest
                                            {
                                                School = new NameValueVm
                                                {
                                                    Id = x.School.Id,
                                                    Name = x.School.Name
                                                },
                                                Extracurricular = new NameValueVm
                                                {
                                                    Id = x.Extracurricular.Id,
                                                    Name = x.Extracurricular.Name
                                                },
                                                Student = new NameValueVm
                                                {
                                                    Id = x.Student.Id,
                                                    Name = x.Student.Name
                                                }
                                            })
                                            .ToList();

                // send email
                var sendCancelationEmail = _extracurricularEmailNotification.SendEmailCancelledExtracurricularToParent(sendEmailParamList);
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new BadRequestException(ex.Message.ToString());
            }
            finally
            {
                _transaction?.Dispose();
            }

            return Request.CreateApiResult2(resultList as object);
        }
    }
}
