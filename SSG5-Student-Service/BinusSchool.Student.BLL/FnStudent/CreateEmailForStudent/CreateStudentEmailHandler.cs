using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.CreateEmailForStudent;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;

namespace BinusSchool.Student.FnStudent.CreateEmailForStudent
{
    public class CreateStudentEmailHandler : FunctionsHttpSingleHandler
    {

        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public CreateStudentEmailHandler(IStudentDbContext schoolDbContext, IMachineDateTime dateTime)
        {
            _dbContext = schoolDbContext;
            _dateTime = dateTime;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var param = await Request.GetBody<CreateStudentEmailRequest>();

            var insertToMsStudentEmail = _dbContext.Entity<MsStudentLicenseEmail>().Add(
            new MsStudentLicenseEmail
            {
                DateIn = _dateTime.ServerTime,
                UserIn = param.UserIn,
                IdStudent = param.StudentId,
                Email = param.Email,
                IsLicensed = 0
            });

            var insertToTrEmailGenerate = _dbContext.Entity<TrEmailGenerate>().Add(new TrEmailGenerate
            {
                DateIn = _dateTime.ServerTime,
                UserIn = param.UserIn,
                IdStudent = param.StudentId,
                GivenName = string.IsNullOrEmpty(param.FirstName) ? "" : param.FirstName,
                Surname = string.IsNullOrEmpty(param.LastName) ? "" : param.LastName,
                DisplayName = (string.IsNullOrEmpty(param.FirstName) ? "" : param.FirstName) + (string.IsNullOrEmpty(param.LastName) ? "" : param.LastName),
                Description = "",
                EmailAddress = param.Email,
                SamAccountName = param.StudentId,
                Password = "",
                Division = "BINUSIAN",
                IsSync = 0
            }) ;

            var query = _dbContext.Entity<MsStudent>().Where(x => x.Id == param.StudentId).FirstOrDefault();

            if(string.IsNullOrEmpty(query.BinusianEmailAddress))
            {
                query.BinusianEmailAddress = param.Email;
                var updateMsStudent = _dbContext.Entity<MsStudent>().Update(query);

                await _dbContext.SaveChangesAsync(CancellationToken);
                return Request.CreateApiResult2();
            }
            else
            {
                throw new Exception("Email Already exist");
            }

            //throw new NotImplementedException();

        }
    }
}
