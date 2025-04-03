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
using BinusSchool.Data.Api.Finance.FnPayment;
using BinusSchool.Data.Model.Finance.FnPayment.ExtracurricularInvoice;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant
{
    public class DeleteStudentParticipantHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IExtracurricularInvoice _extracurricularInvoiceApi;
        private IDbContextTransaction _transaction;

        public DeleteStudentParticipantHandler(
            ISchedulingDbContext dbContext,
            IExtracurricularInvoice extracurricularInvoiceApi,
            IMachineDateTime dateTime
            )
        {
            _dbContext = dbContext;
            _extracurricularInvoiceApi = extracurricularInvoiceApi;
            _dateTime = dateTime;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var paramList = await Request.ValidateBody<List<DeleteStudentParticipantRequest>, DeleteStudentParticipantValidator>();

            var resultList = new List<DeleteStudentParticipantResult>();

            try
            {
                var GradeHomeroomRawList = await _dbContext.Entity<MsHomeroomStudent>()
                                        .Include(hs => hs.Homeroom)
                                        .Where(x => paramList.Select(y => y.IdHomeroom).Any(y => y == x.Homeroom.Id))
                                        .Select(x => new
                                        {
                                            IdHomeroom = x.IdHomeroom,
                                            IdGrade = x.Homeroom.IdGrade
                                        })
                                        .ToListAsync(CancellationToken);

                var getExtracurricularParticipantRawList = await _dbContext.Entity<MsExtracurricularParticipant>()
                                                            .Where(x => GradeHomeroomRawList.Select(y => y.IdGrade).Any(y => y == x.IdGrade) &&
                                                                        paramList.Select(y => y.IdStudent).Any(y => y == x.IdStudent))
                                                            .ToListAsync(CancellationToken);

                // get student invoice detail
                var studentExtracurricularInvoiceStatusApi = await _extracurricularInvoiceApi.GetStudentExtracurricularInvoiceStatus(new GetStudentExtracurricularInvoiceStatusRequest
                {
                    IdStudentList = paramList.Select(x => x.IdStudent).ToList()
                });

                var studentExtracurricularInvoiceStatusRawList = new List<GetStudentExtracurricularInvoiceStatusResult>();

                if (studentExtracurricularInvoiceStatusApi.Payload != null && studentExtracurricularInvoiceStatusApi.Payload.Count() > 0)
                {
                    studentExtracurricularInvoiceStatusRawList = studentExtracurricularInvoiceStatusApi.Payload.ToList();
                }

                foreach (var param in paramList)
                {
                    _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                    try
                    {
                        var result = new DeleteStudentParticipantResult();
                        result.IdExtracurricular = param.IdExtracurricular;
                        result.IdStudent = param.IdStudent;
                        result.IdHomeroom = param.IdHomeroom;
                        result.IsSuccess = false;

                        var isAllowDelete = true;
                        var hasCreatedInvoice = false;

                        var idGrade = GradeHomeroomRawList
                                        .Where(x => x.IdHomeroom == param.IdHomeroom)
                                        .Select(x => x.IdGrade)
                                        .FirstOrDefault();

                        var getExtracurricularParticipant = getExtracurricularParticipantRawList
                                                            .Where(x => x.IdGrade == idGrade &&
                                                                        x.IdStudent == param.IdStudent &&
                                                                        x.IdExtracurricular == param.IdExtracurricular)
                                                            .FirstOrDefault();

                        if (getExtracurricularParticipant == null)
                            throw new BadRequestException("Student extracurricular not found");

                        var studentExtracurricularListInvoice = studentExtracurricularInvoiceStatusRawList
                                                                    .Where(x => x.IdStudent == param.IdStudent)
                                                                    .Select(x => x.ExtracurricularList)
                                                                    .FirstOrDefault();

                        var studentExtracurricularInvoiceStatus = new GetStudentExtracurricularInvoiceStatusResult_Extracurricular();

                        if (studentExtracurricularListInvoice != null)
                        {
                            studentExtracurricularInvoiceStatus = studentExtracurricularListInvoice
                                                                    .Where(x => x.Extracurricular.Id == param.IdExtracurricular 
                                                                    //&& x.ExtracurricularPrice > 0
                                                                    )
                                                                    .FirstOrDefault();
                        }
                        else
                            studentExtracurricularInvoiceStatus = null;


                        if (studentExtracurricularInvoiceStatus != null)
                        {
                            hasCreatedInvoice = true;
                            if (studentExtracurricularInvoiceStatus.PaymentStatus == true && studentExtracurricularInvoiceStatus.ExtracurricularPrice > 0)
                            {
                                isAllowDelete = false;
                            }
                        }

                        if (isAllowDelete)
                        {
                            // if invoice is already created, then delete invoice
                            if (hasCreatedInvoice)
                            {
                                var deleteInvoiceApi = await _extracurricularInvoiceApi.DeleteStudentExtracurricularInvoice(new DeleteStudentExtracurricularInvoiceRequest
                                {
                                    IdExtracurricular = param.IdExtracurricular,
                                    IdStudent = param.IdStudent
                                });

                                if (deleteInvoiceApi.Payload.IsSuccess == false)
                                    throw new BadRequestException(null);
                            }

                            // hard delete
                            _dbContext.Entity<MsExtracurricularParticipant>().Remove(getExtracurricularParticipant);
                            await _dbContext.SaveChangesAsync(CancellationToken);
                            await _transaction.CommitAsync(CancellationToken);

                            result.IsSuccess = true;
                            resultList.Add(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        _transaction?.Rollback();
                    }
                    finally
                    {
                        _transaction?.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message.ToString());
            }

            return Request.CreateApiResult2(resultList as object);
        }
    }
}
