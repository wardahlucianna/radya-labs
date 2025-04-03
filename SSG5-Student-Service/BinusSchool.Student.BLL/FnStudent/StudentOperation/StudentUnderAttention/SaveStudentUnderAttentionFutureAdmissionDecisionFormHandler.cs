using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.StudentOperation.StudentUnderAttention;
using BinusSchool.Student.BLL.FnStudent.StudentOperation.StudentUnderAttention.Validator;
using BinusSchool.Student.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.StudentOperation.StudentUnderAttention
{
    public class SaveStudentUnderAttentionFutureAdmissionDecisionFormHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private IDbContextTransaction _transaction;

        public SaveStudentUnderAttentionFutureAdmissionDecisionFormHandler(IStudentDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<SaveStudentUnderAttentionFutureAdmissionDecisionFormRequest, SaveStudentUnderAttentionFutureAdmissionDecisionFormValidator>();

            await SaveStudentUnderAttentionFutureAdmissionDecisionForm(request);

            return Request.CreateApiResult2();
        }

        public async Task SaveStudentUnderAttentionFutureAdmissionDecisionForm(SaveStudentUnderAttentionFutureAdmissionDecisionFormRequest request)
        {
            _transaction = null;
            using (_transaction = await _context.BeginTransactionAsync())
            {
                try
                {
                    var listAnswers = await _context.Entity<TrStudentFutureAdmissionDecision>()
                        .Where(a => a.IdTrStudentStatus == request.IdTrStudentStatus)
                        .AsNoTracking()
                        .ToListAsync(CancellationToken);

                    var addAnswer = request.IdFutureAdmissionDecisionDetails?
                        .Where(a => !listAnswers.Any(b => b.IdFutureAdmissionDecisionDetail == a))
                        .ToList() ?? new List<string>();

                    var deleteAnswer = listAnswers
                        .Where(a => request.IdFutureAdmissionDecisionDetails?.All(b => b != a.IdFutureAdmissionDecisionDetail) ?? true)
                        .Select(a => a.IdFutureAdmissionDecisionDetail)
                        .ToList();

                    var editAnswer = listAnswers
                        .Where(a => !deleteAnswer.Contains(a.IdFutureAdmissionDecisionDetail) &&
                                    request.IdFutureAdmissionDecisionDetails?.Contains(a.IdFutureAdmissionDecisionDetail) == true)
                        .Select(a => a.IdFutureAdmissionDecisionDetail)
                        .ToList();

                    var listQuestion = await _context.Entity<LtFutureAdmissionDecisionDetail>()
                        .Include(a => a.FutureAdmissionDecision)
                        .Where(a => a.FutureAdmissionDecision.IsRequired == true)
                        .ToListAsync(CancellationToken);

                    var checkRequiredQuestion = listQuestion
                        .Where(a => request.IdFutureAdmissionDecisionDetails.Any(b => b == a.Id))
                        .Any();

                    if (!checkRequiredQuestion)
                        throw new BadRequestException($"Choose at least one answer of required BINUS unit decision section");

                    if (string.IsNullOrWhiteSpace(request.Reason))
                        throw new BadRequestException($"Please fill the reason!");

                    // add answer
                    if (!(request.IdFutureAdmissionDecisionDetails?.Any() ?? false))
                    {
                        var insertAnswer = new TrStudentFutureAdmissionDecision
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdTrStudentStatus = request.IdTrStudentStatus,
                            IdFutureAdmissionDecisionDetail = null,
                            Reason = request.Reason,
                        };

                        _context.Entity<TrStudentFutureAdmissionDecision>().Add(insertAnswer);
                    }

                    foreach (var insert in addAnswer)
                    {
                        var insertAnswer = new TrStudentFutureAdmissionDecision
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdTrStudentStatus = request.IdTrStudentStatus,
                            IdFutureAdmissionDecisionDetail = insert,
                            Reason = request.Reason,
                        };

                        _context.Entity<TrStudentFutureAdmissionDecision>().Add(insertAnswer);
                    }

                    // update answer
                    foreach (var update in editAnswer)
                    {
                        var updateAnswer = listAnswers
                            .FirstOrDefault(a => a.IdFutureAdmissionDecisionDetail == update);

                        if (updateAnswer != null)
                        {
                            updateAnswer.Reason = request.Reason;
                            _context.Entity<TrStudentFutureAdmissionDecision>().Update(updateAnswer);
                        }
                    }

                    // delete answer
                    var removeAnswers = listAnswers.Where(a => deleteAnswer.Contains(a.IdFutureAdmissionDecisionDetail)).ToList();
                    removeAnswers.ForEach(a => a.IsActive = false);
                    _context.Entity<TrStudentFutureAdmissionDecision>().UpdateRange(removeAnswers);

                    await _context.SaveChangesAsync(CancellationToken);
                    await _transaction.CommitAsync(CancellationToken);
                }
                catch (Exception ex)
                {
                    await _transaction?.RollbackAsync(CancellationToken);
                    throw new Exception($"{ex.Message}{Environment.NewLine}{ex.InnerException?.Message ?? ""}");
                }
            }
        }
    }
}
