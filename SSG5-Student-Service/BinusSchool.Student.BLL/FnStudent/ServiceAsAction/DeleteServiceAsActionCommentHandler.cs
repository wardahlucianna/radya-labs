using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using System.Threading.Tasks;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection.Metadata.Ecma335;
using Azure.Core;
using Org.BouncyCastle.Asn1.Ocsp;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using BinusSchool.Student.FnStudent.ServiceAsAction.Validator;
using BinusSchool.Persistence.StudentDb.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.ServiceAsAction
{
    public class DeleteServiceAsActionCommentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteServiceAsActionCommentHandler
        (
            IStudentDbContext studentDbContext
        )
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            try
            {
                var param = await Request.ValidateBody<DeleteServiceAsActionCommentRequest, DeleteServiceAsActionCommentValidator>();

                var dataComment = await _dbContext.Entity<TrServiceAsActionComment>()
                    .Where(x => x.Id == param.IdServiceAsActionComment)
                    .FirstOrDefaultAsync(CancellationToken);

                if(dataComment == null) throw new Exception("Comment Not Found");

                dataComment.IsActive = false;
                _dbContext.Entity<TrServiceAsActionComment>().Update(dataComment);

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

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
