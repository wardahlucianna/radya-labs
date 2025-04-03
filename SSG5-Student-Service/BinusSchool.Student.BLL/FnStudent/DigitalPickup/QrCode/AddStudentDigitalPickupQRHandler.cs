using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.QrCode;
using BinusSchool.Student.FnStudent.DigitalPickup.Validator;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using BinusSchool.Common.Utils;
using System.Web;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Abstractions;

namespace BinusSchool.Student.FnStudent.DigitalPickup.QrCode
{
    public class AddStudentDigitalPickupQRHandler : FunctionsHttpSingleHandler
    {
        private readonly string _codeCrypt = "s0/.P3l";
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;
        private readonly IMachineDateTime _dateTime;
        public AddStudentDigitalPickupQRHandler(IStudentDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<StudentDigitalPickupQRRequest, AddStudentDigitalPickupQRValidator>();

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

            var checkQr = await _dbContext.Entity<MsDigitalPickupQrCode>()
                .Where(x => x.IdAcademicYear == param.IdAcademicYear &&
                            x.IdGrade == param.IdGrade &&
                            x.IdStudent == StudentId)
                .FirstOrDefaultAsync(CancellationToken);

            var res = new StudentDigitalPickupQRResult();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
                if(checkQr != null && checkQr.DateIn?.AddMinutes(2) >= _dateTime.ServerTime)
                {
                    throw new Exception("You have recently generated a QR code. Please wait before generating a new QR code.");
                }

                if(checkQr != null)
                {
                    checkQr.IsActive = false;
                    checkQr.LastActiveDate = _dateTime.ServerTime;
                    _dbContext.Entity<MsDigitalPickupQrCode>().Update(checkQr);
                }

                var newQr = new MsDigitalPickupQrCode
                {
                    Id = Guid.NewGuid().ToString(),
                    IdAcademicYear = param.IdAcademicYear,
                    IdGrade = param.IdGrade,
                    IdStudent = StudentId,
                    ActiveDate = _dateTime.ServerTime
                };
                _dbContext.Entity<MsDigitalPickupQrCode>().Add(newQr);

                res.IdDigitalPickupQrCode = newQr.Id;
                res.ActiveDate = newQr.ActiveDate;

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

            return Request.CreateApiResult2(res as object);
        }
    }
}
