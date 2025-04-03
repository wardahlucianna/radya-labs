using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Finance.FnPayment;
using BinusSchool.Data.Model.Finance.FnPayment.ExtracurricularInvoice;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant
{
    public class GetStudentParticipantByExtracurricularHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IExtracurricularInvoice _extracurricularInvoiceApi;
        private readonly IMachineDateTime _dateTime;
        public GetStudentParticipantByExtracurricularHandler(
            ISchedulingDbContext dbContext,
            IExtracurricularInvoice extracurricularInvoiceApi,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _extracurricularInvoiceApi = extracurricularInvoiceApi;
            _dateTime = dateTime;
        }
        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetStudentParticipantByExtracurricularRequest, GetStudentParticipantByExtracurricularValidator>();

            var queryList = _dbContext.Entity<MsExtracurricularParticipant>()
                                    .Include(ep => ep.Student)
                                    .Include(ep => ep.Extracurricular)
                                    .Where(x => x.IdExtracurricular == param.IdExtracurricular)
                                    .Join(_dbContext.Entity<MsHomeroomStudent>()
                                        .Include(hs => hs.Homeroom)
                                        .ThenInclude(h => h.GradePathwayClassroom)
                                        .ThenInclude(gpc => gpc.Classroom)
                                        .Include(hs => hs.Homeroom)
                                        .ThenInclude(h => h.Grade)
                                        .ThenInclude(g => g.Level)
                                        .ThenInclude(l => l.AcademicYear),
                                        extracurricular => new { p1 = extracurricular.Student.Id, p2 = extracurricular.IdGrade, p3 = extracurricular.Extracurricular.Semester },
                                        homeroom => new { p1 = homeroom.IdStudent, p2 = homeroom.Homeroom.IdGrade, p3 = homeroom.Homeroom.Semester },
                                        (extracurricular, homeroom) => new GetStudentParticipantByExtracurricularResult
                                        {
                                            Student = new NameValueVm
                                            {
                                                Id = extracurricular.Student.Id,
                                                Name = NameUtil.GenerateFullName(extracurricular.Student.FirstName, extracurricular.Student.LastName)
                                            },
                                            AcademicYear = new NameValueVm
                                            {
                                                Id = homeroom.Homeroom.Grade.Level.AcademicYear.Id,
                                                Name = homeroom.Homeroom.Grade.Level.AcademicYear.Description
                                            },
                                            Homeroom = new NameValueVm
                                            {
                                                Id = homeroom.Homeroom.Id,
                                                Name = string.Format("{0}{1}", homeroom.Homeroom.Grade.Code, homeroom.Homeroom.GradePathwayClassroom.Classroom.Code),
                                            },
                                            Semester = homeroom.Semester,
                                            JoinDate = extracurricular.JoinDate,
                                            Status = extracurricular.Status,
                                            // disable change when the extracurricular is primary excul
                                            EnableChange = true,
                                            // disable delete when the extracurricular is already paid
                                            EnableDelete = false,
                                            // disable resend when he extracurricular is already paid || payment date has been expired || with status "No Invoice"
                                            EnableResendEmail = false
                                        })
                                    .Select(x => x)
                                    .Distinct()
                                    .ToList();

            // get student invoice detail
            var studentExtracurricularInvoiceStatusApi = await _extracurricularInvoiceApi.GetStudentExtracurricularInvoiceStatus(new GetStudentExtracurricularInvoiceStatusRequest
            {
                IdAcademicYear = queryList.Select(x => x.AcademicYear.Id).FirstOrDefault(),
                Semester = queryList.Select(x => x.Semester).FirstOrDefault(),
                IdStudentList = queryList.Select(x => x.Student.Id).ToList()
            });

            var studentExtracurricularInvoiceStatus = studentExtracurricularInvoiceStatusApi.Payload == null ? null : studentExtracurricularInvoiceStatusApi.Payload.ToList();

            var queryListRemoveList = new List<GetStudentParticipantByExtracurricularResult>();

            foreach (var studentList in queryList)
            {
                var paidExtracurricular = new GetStudentExtracurricularInvoiceStatusResult_Extracurricular();

                

                if (studentExtracurricularInvoiceStatus != null && studentExtracurricularInvoiceStatus.Any())
                {
                    var tempInvoiceStatusDataList = studentExtracurricularInvoiceStatus
                                                .Where(x => x.IdStudent == studentList.Student.Id)
                                                .FirstOrDefault();

                    paidExtracurricular = tempInvoiceStatusDataList?
                                            .ExtracurricularList
                                            .Where(x => x.Extracurricular.Id == param.IdExtracurricular)
                                            .FirstOrDefault();

                    // validation for param PaymentStatus
                    if (param.PaymentStatus != null)
                    {
                        var samePaymentStatusParam = (paidExtracurricular == null && param.PaymentStatus.Value == false) || paidExtracurricular?.PaymentStatus == param.PaymentStatus.Value;

                        if (samePaymentStatusParam == false)
                        {
                            queryListRemoveList.Add(studentList);
                            continue;
                        }
                    }
                }
                else
                {
                    paidExtracurricular = null;

                    // validation for param PaymentStatus
                    if (param.PaymentStatus != null && param.PaymentStatus.Value == true)
                    {
                        queryListRemoveList.Add(studentList);
                        continue;
                    }
                }

                studentList.DueDatePayment = paidExtracurricular?.DueDatePayment;

                studentList.PaymentStatus = paidExtracurricular?.PaymentStatus;
                studentList.Price = paidExtracurricular?.ExtracurricularPrice;

                studentList.EnableDelete = paidExtracurricular == null ? true : (paidExtracurricular.ExtracurricularPrice == 0) ? true : (paidExtracurricular.PaymentStatus ? false : true);

                studentList.EnableResendEmail = (paidExtracurricular == null ? 
                                                                        false : 
                                                                        ((paidExtracurricular.ExtracurricularPrice > 0 && paidExtracurricular.PaymentStatus == false && _dateTime.ServerTime <= paidExtracurricular.DueDatePayment) ? true : false)
                                                );

                studentList.EnableExtendDueDate = (paidExtracurricular == null ?
                                                                        false :
                                                                        ((paidExtracurricular.ExtracurricularPrice > 0 && paidExtracurricular.PaymentStatus == false && paidExtracurricular.DueDatePayment != null) ? true : false)
                                                );
                studentList.IdTransanction = paidExtracurricular?.IdTransaction;
            }

            // remove from list
            foreach (var removeList in queryListRemoveList)
            {
                queryList.Remove(removeList);
            }
            
            var resultList = queryList.OrderBy(x => x.Student.Name).ThenBy(x => x.Homeroom.Name).ToList();

            IReadOnlyList<GetStudentParticipantByExtracurricularResult> items;
            if (param.Return == CollectionType.Lov)
                items = resultList.AsQueryable().OrderByDynamic(param).ToList();
            else
                items = resultList.AsQueryable().OrderByDynamic(param).SetPagination(param).ToList();

            var resultCount = resultList.Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(resultCount));
        }
    }
}
