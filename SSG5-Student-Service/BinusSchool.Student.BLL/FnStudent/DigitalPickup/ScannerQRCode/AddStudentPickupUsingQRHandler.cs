using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.ScannerQRCode;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Common.Utils;
using BinusSchool.Common.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;
using BinusSchool.Student.FnStudent.DigitalPickup.Validator;
using BinusSchool.Persistence.StudentDb.Entities.School;

namespace BinusSchool.Student.FnStudent.DigitalPickup.ScannerQRCode
{
    public class AddStudentPickupUsingQRHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private IDbContextTransaction _transaction;
        public AddStudentPickupUsingQRHandler(IStudentDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<AddStudentPickupUsingQRRequest, AddStudentPickupUsingQRValidator>();

            var DateToday = _dateTime.ServerTime;

            var getData = await _dbContext.Entity<MsDigitalPickupQrCode>()
                .Where(x => x.Id == param.IdDigitalPickupQrCode && x.ActiveDate <= DateToday && (x.LastActiveDate == null ? true : x.LastActiveDate >= DateToday))
                .Join(_dbContext.Entity<MsDigitalPickupSetting>(),
                    qr => new { qr.IdAcademicYear, qr.IdGrade },
                    setting => new { setting.IdAcademicYear, setting.IdGrade },
                    (qr, setting) => new { qr, setting })
                .Join(_dbContext.Entity<MsHomeroomStudent>(),
                    qrSetting => new { qrSetting.qr.IdStudent, qrSetting.qr.IdGrade },
                    homeroom => new { homeroom.IdStudent, homeroom.Homeroom.IdGrade },
                    (qrSetting, homeroom) => new { qrSetting, homeroom })
                .GroupJoin(_dbContext.Entity<TrDigitalPickup>()
                    .Where(tr => tr.Date.Date == DateToday.Date),
                    join => new { join.qrSetting.qr.IdStudent, join.qrSetting.qr.IdAcademicYear, join.homeroom.Semester },
                    tr => new { tr.IdStudent, tr.IdAcademicYear, tr.Semester},
                    (join, tr) => new { join, tr })
                .SelectMany(x => x.tr.DefaultIfEmpty(),
                    (join, tr) => new { join.join, tr })
                .Select(x => new
                {
                    Student = new NameValueVm
                    {
                        Id = x.join.qrSetting.qr.IdStudent,
                        Name = NameUtil.GenerateFullName(x.join.qrSetting.qr.Student.FirstName, x.join.qrSetting.qr.Student.MiddleName, x.join.qrSetting.qr.Student.LastName)
                    },
                    IdAcademicYear = x.join.qrSetting.qr.IdAcademicYear,
                    IdSchool = x.join.homeroom.Homeroom.Grade.MsLevel.MsAcademicYear.IdSchool,
                    IdLevel = x.join.qrSetting.qr.Grade.IdLevel,
                    Grade = new ItemValueVm
                    {
                        Id = x.join.qrSetting.qr.IdGrade,
                        Description = x.join.qrSetting.qr.Grade.Description
                    },
                    Semester = x.join.homeroom.Semester,
                    IdHomeroom = x.join.homeroom.Homeroom.Id,
                    Homeroom = x.join.homeroom.Homeroom.Grade.Code + x.join.homeroom.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                    StartTime = x.join.qrSetting.setting.StartTime,
                    EndTime = x.join.qrSetting.setting.EndTime,
                    IdDigitalPickup = x.tr != null ? x.tr.Id : null,
                    QRScanTime = x.tr != null ? x.tr.QrScanTime : (DateTime?)null,
                    PickupTime = x.tr != null ? x.tr.PickupTime : (DateTime?)null
                })
                .ToListAsync(CancellationToken);

            var result = new AddStudentPickupUsingQRResult();

            if (getData == null || !getData.Any())
            {
                //Type: QR Not Valid
                result.Information = "<h5>Your QR Code is invalid.<br/>Please re-check your latest generated QR Code</h5>";
            }
            else
            {
                // get Active AY
                var getActiveAYSemester = _dbContext.Entity<MsPeriod>()
                    .Include(x => x.Grade)
                    .Include(x => x.Grade.MsLevel)
                    .Include(x => x.Grade.MsLevel.MsAcademicYear)
                    .Where(x => x.StartDate.Date <= _dateTime.ServerTime.Date && _dateTime.ServerTime.Date <= x.EndDate.Date)
                    .Where(x => x.Grade.MsLevel.MsAcademicYear.IdSchool == getData.Select(x => x.IdSchool).FirstOrDefault())
                    .OrderByDescending(x => x.StartDate)
                    .Select(x => new
                    {
                        IdAcademicYear = x.Grade.MsLevel.MsAcademicYear.Id,
                        Semester = x.Semester
                    })
                    .FirstOrDefault();

                var filteredPickupData = getData.Where(x => x.Semester == getActiveAYSemester.Semester && x.IdAcademicYear == getActiveAYSemester.IdAcademicYear).FirstOrDefault();

                //Type: Belum waktu jemput
                var TimeToday = _dateTime.ServerTime.TimeOfDay;
                if (!(filteredPickupData.StartTime <= TimeToday && filteredPickupData.EndTime >= TimeToday))
                {
                    result.Information = $"<h5>The QR scan time for Grade {filteredPickupData.Grade.Description} is between {filteredPickupData.StartTime.ToString(@"hh\:mm")} - {filteredPickupData.EndTime.ToString(@"hh\:mm")}</h5>";
                }
                // Type: Jika QRnya baru di scan dan perlu diinformasikan kepada komputer guru
                else if (filteredPickupData.IdDigitalPickup == null)
                {
                    try
                    {
                        _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                        var AddDigitalPickupTransaction = new TrDigitalPickup
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdAcademicYear = filteredPickupData.IdAcademicYear,
                            Semester = filteredPickupData.Semester,
                            Date = DateToday,
                            IdStudent = filteredPickupData.Student.Id,
                            QrScanTime = DateToday,
                            PickupTime = null
                        };
                        _dbContext.Entity<TrDigitalPickup>().Add(AddDigitalPickupTransaction);

                        result.IsNewInformation = true;
                        result.Information = $"<h3>{filteredPickupData.Student.Name} - {filteredPickupData.Student.Id} ({filteredPickupData.Grade.Description})</h3></br><h5>is ready to be picked up</h5>";
                        result.StudentInformation = new AddStudentPickupUsingQR_StudentInformation
                        {
                            IdDigitalPickUp = AddDigitalPickupTransaction.Id,
                            IdSchool = filteredPickupData.IdSchool,
                            IdLevel = filteredPickupData.IdLevel,
                            IdGrade = filteredPickupData.Grade.Id,
                            Homeroom = new ItemValueVm
                            {
                                Id = filteredPickupData.IdHomeroom,
                                Description = filteredPickupData.Homeroom
                            },
                            StudentName = filteredPickupData.Student.Name,
                            QRScanTime = AddDigitalPickupTransaction.QrScanTime
                        };

                        await _dbContext.SaveChangesAsync(CancellationToken);
                        await _transaction.CommitAsync(CancellationToken);
                    }
                    catch(Exception ex)
                    {
                        _transaction?.Rollback();
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        _transaction?.Dispose();
                    }
                }
                // Type: Jika QRnya sudah di scan namun belum picked up
                else if (filteredPickupData.QRScanTime != null && filteredPickupData.PickupTime == null)
                {
                    try
                    {
                        _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                        var UpdateDigitalPickupTransaction = await _dbContext.Entity<TrDigitalPickup>()
                            .Where(x => x.Id == filteredPickupData.IdDigitalPickup)
                            .FirstOrDefaultAsync(CancellationToken);

                        UpdateDigitalPickupTransaction.QrScanTime = DateToday;

                        _dbContext.Entity<TrDigitalPickup>().Update(UpdateDigitalPickupTransaction);

                        result.IsNewInformation = true;
                        result.Information = $"<h3>{filteredPickupData.Student.Name} - {filteredPickupData.Student.Id} ({filteredPickupData.Grade.Description})</h3></br><h5>is ready to be picked up</h5>";
                        result.StudentInformation = new AddStudentPickupUsingQR_StudentInformation
                        {
                            IdDigitalPickUp = UpdateDigitalPickupTransaction.Id,
                            IdSchool = filteredPickupData.IdSchool,
                            IdLevel = filteredPickupData.IdLevel,
                            IdGrade = filteredPickupData.Grade.Id,
                            Homeroom = new ItemValueVm
                            {
                                Id = filteredPickupData.IdHomeroom,
                                Description = filteredPickupData.Homeroom
                            },
                            StudentName = filteredPickupData.Student.Name,
                            QRScanTime = UpdateDigitalPickupTransaction.QrScanTime
                        };

                        await _dbContext.SaveChangesAsync(CancellationToken);
                        await _transaction.CommitAsync(CancellationToken);
                    }
                    catch(Exception ex)
                    {
                        _transaction?.Rollback();
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        _transaction?.Dispose();
                    }
                    
                }
                // Type: Jika QRnya sudah di scan namun sudah picked up
                else if (filteredPickupData.PickupTime != null)
                {
                    result.Information = $"<h3>{filteredPickupData.Student.Name} - {filteredPickupData.Student.Id} ({filteredPickupData.Grade.Description})</h3></br><h5>has been picked up</h5>";
                }
            }


            return Request.CreateApiResult2(result as object);
        }
    }
}
