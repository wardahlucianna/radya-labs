using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.QrCode;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Entities;
using System.Collections.Generic;
using BinusSchool.Persistence.StudentDb.Entities.School;
using System.Web;
using BinusSchool.Common.Exceptions;

namespace BinusSchool.Student.FnStudent.DigitalPickup.QrCode
{
    public class GetStudentDigitalPickupQRHandler : FunctionsHttpSingleHandler
    {
        private readonly string _codeCrypt = "s0/.P3l";
        private readonly IStudentDbContext _dbContext;
        public GetStudentDigitalPickupQRHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<StudentDigitalPickupQRRequest>(
                nameof(StudentDigitalPickupQRRequest.IdStudent), 
                nameof(StudentDigitalPickupQRRequest.IdGrade), 
                nameof(StudentDigitalPickupQRRequest.IdAcademicYear));

            var split = SplitIdDatetimeUtil.Split(EncryptStringUtil.Decrypt(HttpUtility.UrlDecode(param.IdStudent), _codeCrypt));

            if (!split.IsValid)
            {
                throw new BadRequestException(split.ErrorMessage);
            }

            var StudentId = split.Id;
            if (StudentId.Where(char.IsDigit).ToArray().Length != 10)
            {
                throw new BadRequestException("Student ID incorrect format");
            }

            var res = await _dbContext.Entity<MsDigitalPickupQrCode>()
                .Where(x => x.IdStudent == StudentId && x.IdAcademicYear == param.IdAcademicYear && x.IdGrade == param.IdGrade)
                .Select(x => new StudentDigitalPickupQRResult
                {
                    IdDigitalPickupQrCode = x.Id,
                    ActiveDate = x.ActiveDate
                })
                .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(res as object);
        }
    }
}
