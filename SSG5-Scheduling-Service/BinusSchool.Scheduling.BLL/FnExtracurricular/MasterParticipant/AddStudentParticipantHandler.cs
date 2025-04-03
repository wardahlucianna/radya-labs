using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
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
    public class AddStudentParticipantHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IExtracurricularInvoice _extracurricularInvoiceApi;
        private IDbContextTransaction _transaction;

        public AddStudentParticipantHandler(
            ISchedulingDbContext dbContext, 
            IMachineDateTime dateTime,
            IExtracurricularInvoice extracurricularInvoiceApi)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _extracurricularInvoiceApi = extracurricularInvoiceApi;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<List<AddStudentParticipantRequest>, AddStudentParticipantValidator>();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            try
            {
                var homeroomStudentList = await _dbContext.Entity<MsHomeroomStudent>()
                                            .Include(hs => hs.Homeroom)
                                            .Where(x => param.Select(y => y.IdHomeroom+"/"+ y.IdStudent).Any(y => y == (x.Homeroom.Id+"/"+x.IdStudent)))
                                            .Select(x => new
                                            {
                                                IdSchool = x.Homeroom.Grade.Level.AcademicYear.IdSchool,
                                                IdHomeroomStudent = x.Id,
                                                IdHomeroom = x.Homeroom.Id,
                                                IdGrade = x.Homeroom.IdGrade,
                                                Semester = x.Homeroom.Semester,
                                                IdStudent = x.IdStudent
                                            })
                                            .ToListAsync(CancellationToken);

                var extracurricularList = await _dbContext.Entity<MsExtracurricular>()
                                            .Include(x => x.ExtracurricularType)
                                            .Where(x => param.Select(y => y.IdExtracurricular).Any(y => y == x.Id))
                                            .ToListAsync(CancellationToken);

                if (extracurricularList == null || !extracurricularList.Any())
                    throw new BadRequestException("Extracurricular not found.");

                if (extracurricularList.All(x => x.Status == false))
                    throw new BadRequestException("Failed! Cannot add participant to the inactive extracurricular.");

                var extracurricularParticipantList = await _dbContext.Entity<MsExtracurricularParticipant>().Include(x => x.Extracurricular)
                                                            .Where(x => param.Select(y => y.IdExtracurricular).Any(y => y == x.IdExtracurricular))
                                                            .Select(x => new
                                                            {
                                                                IdStudent = x.IdStudent,
                                                                IdGrade = x.IdGrade,
                                                                IdExtracurricular = x.IdExtracurricular,
                                                                Semester = x.Extracurricular.Semester
                                                            })
                                                            .Distinct()
                                                            .ToListAsync(CancellationToken);

                var extracurricularRuleList = await _dbContext.Entity<TrExtracurricularRuleGradeMapping>()
                                                    .Include(ergm => ergm.ExtracurricularRule)
                                                    .Where(x => homeroomStudentList.Select(y => y.IdGrade).Any(y => y == x.IdGrade) &&
                                                                x.ExtracurricularRule.Status == true)
                                                    .ToListAsync(CancellationToken);

                var createInvoiceDataList = new List<CreateStudentExtracurricularInvoiceRequest>();

                var createNewExtracurricularInvoiceList = new List<CreateStudentExtracurricularInvoiceRequest_ExtracurricularData>();

                foreach (AddStudentParticipantRequest request in param)
                {
                    var homeroomStudent = homeroomStudentList
                                            .Where(x => x.IdHomeroom == request.IdHomeroom
                                            && x.IdStudent == request.IdStudent)
                                            .Select(x => new
                                            {
                                                IdSchool = x.IdSchool,
                                                IdHomeroomStudent = x.IdHomeroomStudent,
                                                IdHomeroom = x.IdHomeroom,
                                                IdGrade = x.IdGrade,
                                                Semester = x.Semester,
                                                IdStudent = x.IdStudent
                                            })
                                            .FirstOrDefault();

                    // get extracurricular detail
                    var extracurricular = extracurricularList
                                            .Where(x => x.Id == request.IdExtracurricular)
                                            .FirstOrDefault();

                    if(extracurricular.Status == false)
                        throw new BadRequestException("Failed! Cannot add participant to the inactive extracurricular.");

                    // get max extracurricular participant
                    var maxExtracurricularParticipant = extracurricularList
                                                        .Where(x => x.Id == request.IdExtracurricular)
                                                        .Select(x => x.MaxParticipant)
                                                        .FirstOrDefault();

                    var extracurricularParticipantCount = extracurricularParticipantList
                                                            .Where(x => x.IdExtracurricular == request.IdExtracurricular)
                                                            .Count();

                    // get extracurricular rule
                    var extracurricularRule = extracurricularRuleList
                                                        .Where(x => x.IdGrade == homeroomStudent.IdGrade &&
                                                                    x.ExtracurricularRule.Status == true)
                                                        .FirstOrDefault();

                    if (extracurricularRule == null)
                        throw new BadRequestException("Failed! No extracurricular rule has been set for this grade. Please contact the staff.");

                    var studentExtracurricular = extracurricularParticipantList
                                            .Where(x => x.IdStudent == request.IdStudent &&
                                                        x.IdGrade == homeroomStudent.IdGrade &&
                                                        x.IdExtracurricular == request.IdExtracurricular &&
                                                        x.Semester == homeroomStudent.Semester)
                                            .ToList();

                    var studentExtracurricularCount = studentExtracurricular.Count();

                    #region No longer using Priority
                    //// Check max priority
                    //int maxPriority = 0;

                    //if (studentExtracurricular != null)
                    //    maxPriority = studentExtracurricular.Select(x => x.Priority).OrderByDescending(priority => priority).FirstOrDefault();
                    #endregion

                    // Check if the extracurricular has reached the maximum number of participant
                    var isMaxParticipant = extracurricularParticipantCount >= maxExtracurricularParticipant ? true : false;

                    if (isMaxParticipant)
                    {
                        throw new BadRequestException("Failed! This extracurricular has reached the maximum number of participants");
                    }
                    else if (studentExtracurricularCount != 0)
                    {
                        throw new BadRequestException("Failed! Students are already participants in this extracurricular");
                    }
                    else
                    {
                        // if today is more than review date not create add participant and invoice in Simprug
                        if (homeroomStudent.IdSchool == "1" && (extracurricularRule.ExtracurricularRule.ReviewDate.HasValue ? _dateTime.ServerTime <= extracurricularRule.ExtracurricularRule.ReviewDate : false))
                        {
                            var addQuery = _dbContext.Entity<MsExtracurricularParticipant>()
                                       .Add(new MsExtracurricularParticipant
                                       {
                                           Id = Guid.NewGuid().ToString(),
                                           IdExtracurricular = request.IdExtracurricular,
                                           IdStudent = request.IdStudent,
                                           IdGrade = homeroomStudent.IdGrade,
                                           JoinDate = request.JoinDate,
                                           Status = true,
                                            //Priority = maxPriority + 1,
                                            IsPrimary = true
                                       });
                        }
                        else
                        {
                            var addQuery = _dbContext.Entity<MsExtracurricularParticipant>()
                                        .Add(new MsExtracurricularParticipant
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            IdExtracurricular = request.IdExtracurricular,
                                            IdStudent = request.IdStudent,
                                            IdGrade = homeroomStudent.IdGrade,
                                            JoinDate = request.JoinDate,
                                            Status = true,
                                            //Priority = maxPriority + 1,
                                            IsPrimary = true
                                        });

                            #region unused validation
                            // if today is more than review date, then directly create the invoice
                            //if (extracurricularRule.ExtracurricularRule.ReviewDate.HasValue ? _dateTime.ServerTime >= extracurricularRule.ExtracurricularRule.ReviewDate : true)
                            //{
                            //    // list all extracurricular invoice that want to be created
                            //    var createNewExtracurricularInvoice = new CreateStudentExtracurricularInvoiceRequest_ExtracurricularData
                            //    {
                            //        IdExtracurricular = request.IdExtracurricular,
                            //        ExtracurricularPrice = extracurricular.Price
                            //    };

                            //    createNewExtracurricularInvoiceList.Add(createNewExtracurricularInvoice);
                            //}
                            #endregion

                            // list all extracurricular invoice that want to be created
                            var createNewExtracurricularInvoice = new CreateStudentExtracurricularInvoiceRequest_ExtracurricularData
                            {
                                IdExtracurricular = request.IdExtracurricular,
                                ExtracurricularPrice = request.ExtracurricularPrice == null ? extracurricular.Price : request.ExtracurricularPrice.Value,
                                ExtracurricularType = extracurricular.ExtracurricularType.Code
                            };


                            if (createNewExtracurricularInvoiceList.Where(a => a.IdExtracurricular == request.IdExtracurricular).Count() == 0)
                            {
                                createNewExtracurricularInvoiceList.Add(createNewExtracurricularInvoice);
                            }


                            if (createNewExtracurricularInvoiceList.Count > 0)
                            {
                                var createInvoiceData = new CreateStudentExtracurricularInvoiceRequest
                                {
                                    ExtracurricularList = createNewExtracurricularInvoiceList,
                                    IdStudent = request.IdStudent,
                                    IdHomeroomStudent = homeroomStudent.IdHomeroomStudent,
                                    Semester = homeroomStudent.Semester,
                                    InvoiceStartDate = _dateTime.ServerTime,
                                    InvoiceEndDate = _dateTime.ServerTime.AddDays(extracurricularRule.ExtracurricularRule.DueDayInvoice),
                                    SendEmailNotification = request.SendEmailNotification
                                };

                                createInvoiceDataList.Add(createInvoiceData);
                            }
                        }
                    }

                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                // create invoice
                if(createInvoiceDataList.Count > 0)
                {
                    var createInvoice = await _extracurricularInvoiceApi.CreateStudentExtracurricularInvoice(createInvoiceDataList);
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

            return Request.CreateApiResult2();
        }
    }
}
