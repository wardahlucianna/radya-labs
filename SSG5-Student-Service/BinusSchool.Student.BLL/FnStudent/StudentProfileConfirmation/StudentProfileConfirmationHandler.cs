using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model;
using BinusSchool.Common.Model.Information;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Student.FnStudent.StudentProfileConfirmation.Validator;
using BinusSchool.Data.Model.Student.FnStudent.StudentProfileConfirmation;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Abstractions;

namespace BinusSchool.Student.FnStudent.StudentProfileConfirmation
{
    public class StudentProfileConfirmationHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public StudentProfileConfirmationHandler(IStudentDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddStudentProfileConfirmationRequest, AddStudentProfileConfirmationValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var acadyear = await _dbContext.Entity<TrStudentProfileConfirmation>().FindAsync(body.IdStudent);
            if (acadyear is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["ID Student"], "Id", body.IdStudent));

            var isExist = await _dbContext.Entity<TrStudentProfileConfirmation>()
                .Where(x => x.IdStudent == body.IdStudent)
                .FirstOrDefaultAsync();
            if (isExist != null)
                throw new BadRequestException($"{body.IdStudent} already Confirm");

            var param = new TrStudentProfileConfirmation
            {
                IdStudent = body.IdStudent,
                IdUser = body.IdUser,
                UserIn = AuthInfo.UserId,
                DateIn = _dateTime.ServerTime
            };

            _dbContext.Entity<TrStudentProfileConfirmation>().Add(param);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
