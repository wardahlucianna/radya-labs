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
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Util.FnConverter;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry;
using BinusSchool.Data.Model.Util.FnConverter.MedicalStudentPassToPdf;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry
{
    public class PrintMedicalRecordEntryStudentPassHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _context;
        private readonly IMachineDateTime _date;
        private readonly IMedicalStudentPassToPdf _convert;
        private IDbContextTransaction _transaction;

        private const string _template = "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>Thermal Printer Test</title>\r\n    <style>\r\n        body {\r\n            font-family: Arial, sans-serif;\r\n            width: 100%;\r\n            max-width: 80mm;\r\n            /* height: 210mm; */\r\n            margin: 0;\r\n            /* padding: 20px 0; */\r\n            /* border: 1px solid black; */\r\n        }\r\n        .header {\r\n            text-align: center;\r\n            font-size: 16pt;\r\n            font-weight: bold;\r\n        }\r\n        .section {\r\n            padding: 1mm 1mm;\r\n            font-size: 12pt;\r\n        }\r\n        table {\r\n            width: 100%;\r\n            border-collapse: collapse;\r\n            font-size: 12pt;\r\n        }\r\n        td {\r\n            padding: 2mm 1mm;\r\n        }\r\n    </style>\r\n</head>\r\n<body>\r\n    <div class=\"header\">\r\n        <img src=\"https://bssschoolstorage.blob.core.windows.net/floor/Binus School Logo.bmp\" alt=\"\" height=\"64px\">\r\n        <p>Student Clinic Pass</p>\r\n        <p>================</p>\r\n    </div>\r\n\r\n    <div class=\"section\">\r\n        <table>\r\n            <tr>\r\n                <td>Name</td>\r\n                <td>:</td>\r\n                <td>{{StudentName}}</td>\r\n            </tr>\r\n            <tr>\r\n                <td>Student ID<br>/Grade</td>\r\n                <td>:</td>\r\n                <td>{{IdStudentGrade}}</td>\r\n            </tr>\r\n            <tr>\r\n                <td>Check-in Time</td>\r\n                <td>:</td>\r\n                <td>{{CheckIn}}</td>\r\n            </tr>\r\n            <tr>\r\n                <td>Check-out Time</td>\r\n                <td>:</td>\r\n                <td>{{CheckOut}}</td>\r\n            </tr>\r\n            <tr>\r\n                <td>Printed On</td>\r\n                <td>:</td>\r\n                <td>{{PrintedDate}}</td>\r\n            </tr>\r\n            <tr>\r\n                <td>Printed By</td>\r\n                <td>:</td>\r\n                <td>{{PrintedBy}}</td>\r\n            </tr>\r\n        </table>\r\n    </div>\r\n</body>\r\n</html>";

        public PrintMedicalRecordEntryStudentPassHandler(IStudentDbContext context, IMachineDateTime date, IMedicalStudentPassToPdf convert)
        {
            _context = context;
            _date = date;
            _convert = convert;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<PrintMedicalRecordEntryStudentPassRequest>(
                nameof(PrintMedicalRecordEntryStudentPassRequest.IdMedicalRecordEntry));

            var response = await PrintMedicalRecordEntryStudentPass(request);

            return Request.CreateApiResult2(response as object);
        }

        public async Task<PrintMedicalRecordEntryStudentPassResponse> PrintMedicalRecordEntryStudentPass(PrintMedicalRecordEntryStudentPassRequest request)
        {
            var response = new PrintMedicalRecordEntryStudentPassResponse();

            var IdUser = AuthInfo.UserId;

            var template = _template;

            var medicalRecordEntry = await _context.Entity<TrMedicalRecordEntry>()
                .Where(a => a.Id == request.IdMedicalRecordEntry)
                .FirstOrDefaultAsync(CancellationToken);

            var student = await _context.Entity<MsHomeroomStudent>()
                .Include(a => a.Student)
                .Include(a => a.Homeroom.Grade.MsLevel)
                .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                .Where(a => a.Student.Id == medicalRecordEntry.IdUser)
                .OrderByDescending(a => a.DateIn)
                .FirstOrDefaultAsync(CancellationToken);

            var user = await _context.Entity<MsUser>()
                .Where(a => a.Id == IdUser)
                .Select(a => a.DisplayName.Trim())
                .FirstOrDefaultAsync(CancellationToken);

            template = template
                .Replace("{{StudentName}}", NameUtil.GenerateFullName(student.Student.FirstName, student.Student.LastName))
                .Replace("{{IdStudentGrade}}", $"{student.Student.Id} / {student.Homeroom.Grade.Code}{student.Homeroom.MsGradePathwayClassroom.Classroom.Code}")
                .Replace("{{CheckIn}}", medicalRecordEntry.CheckInDateTime.ToString("dd MMMM yyyy (HH:mm)"))
                .Replace("{{CheckOut}}", medicalRecordEntry.CheckOutDateTime.HasValue ? medicalRecordEntry.CheckOutDateTime.Value.ToString("dd MMMM yyyy (HH:mm)")
                    : _date.ServerTime.ToString("dd MMMM yyyy (HH:mm)"))
                .Replace("{{PrintedDate}}", _date.ServerTime.ToString("dd MMMM yyyy (HH:mm)"))
                .Replace("{{PrintedBy}}", user);

            if (!medicalRecordEntry.CheckOutDateTime.HasValue)
            {
                using (_transaction = await _context.BeginTransactionAsync(CancellationToken))
                {
                    try
                    {
                        medicalRecordEntry.CheckOutDateTime = _date.ServerTime;

                        _context.Entity<TrMedicalRecordEntry>().Update(medicalRecordEntry);

                        await _context.SaveChangesAsync(CancellationToken);
                        await _transaction.CommitAsync(CancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _transaction?.Rollback();

                        throw new Exception(ex.Message.ToString(), ex);
                    }
                }
            }

            var convert = await _convert.MedicalStudentPassToPdf(new MedicalStudentPassToPdfRequest
            {
                Html = template
            });

            response.DocumentUrl = convert.Url;

            return response;
        }
    }
}
