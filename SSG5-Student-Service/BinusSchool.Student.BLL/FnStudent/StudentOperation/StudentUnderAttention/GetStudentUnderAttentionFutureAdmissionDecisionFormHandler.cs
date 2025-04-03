using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.StudentOperation.StudentUnderAttention;
using BinusSchool.Student.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.StudentOperation.StudentUnderAttention
{
    public class GetStudentUnderAttentionFutureAdmissionDecisionFormHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;

        public GetStudentUnderAttentionFutureAdmissionDecisionFormHandler(IStudentDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetStudentUnderAttentionFutureAdmissionDecisionFormRequest>(
                nameof(GetStudentUnderAttentionFutureAdmissionDecisionFormRequest.IdTrStudentStatus));

            var response = await GetStudentUnderAttentionFutureAdmissionDecisionForm(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<GetStudentUnderAttentionFutureAdmissionDecisionFormResponse> GetStudentUnderAttentionFutureAdmissionDecisionForm(GetStudentUnderAttentionFutureAdmissionDecisionFormRequest request)
        {
            var response = new GetStudentUnderAttentionFutureAdmissionDecisionFormResponse();

            var trStudentStatus = await _context.Entity<TrStudentStatus>()
                .Include(a => a.Student)
                .Include(a => a.StudentStatusSpecial)
                .Where(a => a.IdTrStudentStatus == request.IdTrStudentStatus)
                .FirstOrDefaultAsync(CancellationToken);

            if (!trStudentStatus.StudentStatusSpecial.NeedFutureAdmissionDecision)
                return response = new GetStudentUnderAttentionFutureAdmissionDecisionFormResponse
                {
                    IdTrStudentStatus = request.IdTrStudentStatus,
                    Student = new ItemValueVm
                    {
                        Id = trStudentStatus.IdStudent,
                        Description = NameUtil.GenerateFullName(trStudentStatus.Student.FirstName, trStudentStatus.Student.LastName)
                    },
                };

            var form = await _context.Entity<LtFutureAdmissionDecision>()
                .Include(a => a.FutureAdmissionDecisionDetails)
                .Select(a => new GetStudentUnderAttentionFutureAdmissionFormResponse_FutureAdmissionDecision
                {
                    IdFutureAdmissionDecision = a.Id,
                    BinusUnit = a.BINUSUnit,
                    IsMultipleAnswer = a.IsMutipleAnswer,
                    IsRequired = a.IsRequired,
                    OrderNo = a.OrderNo,
                    FutureAdmissionDecisionDetails = a.FutureAdmissionDecisionDetails.Select(b => new GetStudentUnderAttentionFutureAdmissionFormResponse_FutureAdmissionDecisionDetail
                    {
                        IdFutureAdmissionDecisionDetail = b.Id,
                        Description = b.Description,
                        IsDefault = b.IsDefault,
                        IsChecked = _context.Entity<TrStudentFutureAdmissionDecision>()
                            .Where(c => c.IdTrStudentStatus == request.IdTrStudentStatus)
                            .Any() ? (_context.Entity<TrStudentFutureAdmissionDecision>()
                            .Where(c => c.IdTrStudentStatus == request.IdTrStudentStatus
                                && c.IdFutureAdmissionDecisionDetail == b.Id)
                            .Any() ? true : false) : null,
                        OrderNo = b.OrderNo,
                    })
                    .OrderBy(b => b.OrderNo)
                    .ToList()
                })
                .OrderBy(a => a.OrderNo)
                .ToListAsync(CancellationToken);

            var futureAdmissionDecision = await _context.Entity<TrStudentFutureAdmissionDecision>()
                .Where(a => a.IdTrStudentStatus == request.IdTrStudentStatus)
                .FirstOrDefaultAsync(CancellationToken);

            response = new GetStudentUnderAttentionFutureAdmissionDecisionFormResponse
            {
                IdTrStudentStatus = request.IdTrStudentStatus,
                Reason = futureAdmissionDecision?.Reason ?? "",
                Student = new ItemValueVm
                {
                    Id = trStudentStatus.IdStudent,
                    Description = NameUtil.GenerateFullName(trStudentStatus.Student.FirstName, trStudentStatus.Student.LastName)
                },
                FutureAdmissionDecisions = form
            };

            return response;
        }
    }
}
