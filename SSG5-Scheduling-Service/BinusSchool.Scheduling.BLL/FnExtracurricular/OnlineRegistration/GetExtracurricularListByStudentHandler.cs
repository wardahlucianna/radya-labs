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
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
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

namespace BinusSchool.Scheduling.FnExtracurricular.OnlineRegistration
{
    public class GetExtracurricularListByStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IOnlineRegistration _scpOnlineRegistrationApi;
        private readonly IExtracurricularInvoice _paymentApi;
        public GetExtracurricularListByStudentHandler(ISchedulingDbContext dbContext,
            IMachineDateTime dateTime,
            IOnlineRegistration scpOnlineRegistrationApi,
            IExtracurricularInvoice paymentApi)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _scpOnlineRegistrationApi = scpOnlineRegistrationApi;
            _paymentApi = paymentApi;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetExtracurricularListByStudentRequest, GetExtracurricularListByStudentValidator>();

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

            var extracurricularDataList = await _dbContext.Entity<TrExtracurricularGradeMapping>()
                                    .Include(egm => egm.Extracurricular)
                                    .Include(egm => egm.Grade)
                                    .ThenInclude(g => g.Level)
                                    .ThenInclude(l => l.AcademicYear)
                                    .Where(x => x.Grade.Level.AcademicYear.Id == studentDetail.AcadYear.Id &&
                                                x.Extracurricular.Semester == studentDetail.Semester &&
                                                x.Grade.Id == studentDetail.Grade.Id &&
                                                x.Extracurricular.Status == true &&
                                                // Only show official extracurricular
                                                //x.Extracurricular.Category == ExtracurricularCategory.Official
                                                x.Extracurricular.ShowParentStudent == true
                                                )
                                    .Select(x => new
                                    {
                                        Extracurricular = new NameValueVm
                                        {
                                            Id = x.Extracurricular.Id,
                                            Name = x.Extracurricular.Name
                                        },
                                        ExtracurricularPrice = x.Extracurricular.Price,
                                        ExtracurricularDescription = x.Extracurricular.Description,
                                        ParticipantMin = x.Extracurricular.MinParticipant,
                                        ParticipantMax = x.Extracurricular.MaxParticipant,
                                        Price = x.Extracurricular.Price
                                    })
                                    .Distinct()
                                    .OrderBy(x => x.Extracurricular.Name)
                                    .ToListAsync(CancellationToken);

            var studentExtracurricularDataList = extracurricularDataList.GroupJoin(
                                                    _dbContext.Entity<MsExtracurricularParticipant>()
                                                        .Where(x => x.IdStudent == param.IdStudent &&
                                                                    x.IdGrade == studentDetail.Grade.Id &&
                                                                    x.Status == true),
                                                    e => e.Extracurricular.Id,
                                                    s => s.IdExtracurricular,
                                                    (e, s) => new { e, s })
                                                .SelectMany(x => x.s.DefaultIfEmpty(),
                                                (extracurricular, student) => new { extracurricular, student })
                                                .ToList();

            // list all extracurricular id
            List<string> idExtracurricularList = new List<string>();
            foreach (var itemExtracurricular in studentExtracurricularDataList)
            {
                idExtracurricularList.Add(itemExtracurricular.extracurricular.e.Extracurricular.Id);
            }

            // get created invoice for student extracurricular
            var createdInvoiceData = await _paymentApi.GetStudentExtracurricularInvoiceStatus(new GetStudentExtracurricularInvoiceStatusRequest
            {
                IdStudentList = new List<string> { param.IdStudent }
            });

            var createdInvoiceRaw = createdInvoiceData.Payload.Count() <= 0 ? null : createdInvoiceData.Payload.FirstOrDefault();

            var createdInvoiceList = createdInvoiceRaw.ExtracurricularList;

            var allScheduleDayTimeList = _dbContext.Entity<TrExtracurricularSessionMapping>()
                                                    .Include(esm => esm.ExtracurricularSession)
                                                    .ThenInclude(es => es.Day)
                                                    .Where(x => idExtracurricularList.Contains(x.IdExtracurricular))
                                                    .Select(x => new DayTimeSchedule
                                                    {
                                                        IdExtracurricular = x.IdExtracurricular,
                                                        IdDay = x.ExtracurricularSession.IdDay,
                                                        Day = x.ExtracurricularSession.Day.Description,
                                                        StartTime = x.ExtracurricularSession.StartTime.ToString("hh\\:mm"),
                                                        EndTime = x.ExtracurricularSession.EndTime.ToString("hh\\:mm")
                                                    })
                                                    .OrderBy(x => x.IdDay)
                                                    .ToList();

            var allTotalParticipantList = _dbContext.Entity<MsExtracurricularParticipant>()
                                        .Where(x => idExtracurricularList.Contains(x.IdExtracurricular))
                                        .ToList()
                                        .GroupBy(x => x.IdExtracurricular)
                                        .Select(x => new
                                        {
                                            IdExtracurricular = x.Key,
                                            totalParticipant = x.Select(x => x.IdStudent).Distinct().Count()
                                        })
                                        .Distinct()
                                        .ToList();

            List<GetExtracurricularListByStudentResult> resultList = new List<GetExtracurricularListByStudentResult>();

            foreach (var itemExtracurricular in studentExtracurricularDataList)
            {
                // get list schedule day time
                var scheduleDayTimeList = allScheduleDayTimeList
                                            .Where(x => x.IdExtracurricular == itemExtracurricular.extracurricular.e.Extracurricular.Id)
                                            .ToList();

                // get total participants
                int totalParticipant = allTotalParticipantList
                                            .Where(x => x.IdExtracurricular == itemExtracurricular.extracurricular.e.Extracurricular.Id)
                                            .Select(x => x.totalParticipant)
                                            .FirstOrDefault();

                // get created invoice for this extracurricular
                var extracurricularInvoiceList = createdInvoiceList
                                                .Where(x => x.Extracurricular.Id == itemExtracurricular.extracurricular.e.Extracurricular.Id &&
                                                            (_dateTime.ServerTime <= x.DueDatePayment || x.PaymentStatus == true) &&
                                                            x.ExtracurricularPrice > 0)
                                                .FirstOrDefault();

                // insert to body
                GetExtracurricularListByStudentResult body = new GetExtracurricularListByStudentResult
                {
                    Extracurricular = itemExtracurricular.extracurricular.e.Extracurricular,
                    ExtracurricularDescription = itemExtracurricular.extracurricular.e.ExtracurricularDescription,
                    ExtracurricularPrice = itemExtracurricular.extracurricular.e.ExtracurricularPrice,
                    ScheduleDayTimeList = scheduleDayTimeList.OrderBy(x => x.IdDay).ToList(),
                    AvailableSeat = itemExtracurricular.extracurricular.e.ParticipantMax - totalParticipant,
                    //SelectedPriority = itemExtracurricular.student?.Priority
                    IsPrimary = itemExtracurricular.student?.IsPrimary == null ? false : itemExtracurricular.student.IsPrimary,
                    IsSelected = itemExtracurricular.student == null ? false : true,
                    HasCreatedInvoice = extracurricularInvoiceList == null ? false : true,
                    Price = itemExtracurricular.extracurricular.e.Price,
                    PaymentStatus = (itemExtracurricular.extracurricular.e.Price != 0 && extracurricularInvoiceList != null) ? new GetExtracurricularListByStudentResult_PaymentStatus
                    {
                        Status = extracurricularInvoiceList.PaymentStatus ? "Paid" : "Unpaid",
                        Message = extracurricularInvoiceList.PaymentStatus ? "Cannot cancel the registration as the elective has already been paid." : "Cannot cancel the registration as the invoice has been generated."
                    } : null
                };

                resultList.Add(body);
            }

            var query = resultList.AsQueryable().OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Extracurricular.Id,
                        Description = x.Extracurricular.Name
                    })
                    .Distinct()
                    .ToList();
            else
                items = query
                    .SetPagination(param)
                    .Select(x => new GetExtracurricularListByStudentResult
                    {
                        Extracurricular = x.Extracurricular,
                        ExtracurricularDescription = x.ExtracurricularDescription,
                        ExtracurricularPrice = x.ExtracurricularPrice,
                        ScheduleDayTimeList = x.ScheduleDayTimeList,
                        AvailableSeat = x.AvailableSeat,
                        IsPrimary = x.IsPrimary,
                        IsSelected = x.IsSelected,
                        HasCreatedInvoice = x.HasCreatedInvoice,
                        Price = x.Price,
                        PaymentStatus = x.PaymentStatus
                    })
                    .ToList();

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.Select(x => x.Extracurricular.Id).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
