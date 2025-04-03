using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.DigitalPickup;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.DigitalPickup.DigitalPickup
{
    public class GetStudentDigitalPickupHistoryHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly string _codeCrypt = "@J#m6^N!9z";

        public GetStudentDigitalPickupHistoryHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetStudentDigitalPickupHistoryRequest>
                (nameof(GetStudentDigitalPickupHistoryRequest.IdStudent),
                 nameof(GetStudentDigitalPickupHistoryRequest.IdAcademicYear),
                 nameof(GetStudentDigitalPickupHistoryRequest.Semester));

            var split = SplitIdDatetimeUtil.Split(EncryptStringUtil.Decrypt(HttpUtility.UrlDecode(request.IdStudent), _codeCrypt));

            if (!split.IsValid)
                throw new BadRequestException(split.ErrorMessage);

            var idStudent = split.Id;

            if (idStudent.Where(char.IsDigit).ToArray().Length != 10)
                throw new BadRequestException("Student ID incorrect format");

            var getTrDigitalPickup = await _dbContext.Entity<TrDigitalPickup>()
                .Include(a => a.Student)
                .Where(a => a.IdAcademicYear == request.IdAcademicYear
                    && a.Semester == request.Semester
                    && a.IdStudent == idStudent)
                .ToListAsync(CancellationToken);

            var getHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(a => a.Homeroom.Grade.MsLevel)
                .Include(a => a.Homeroom.MsGradePathwayClassroom.Classroom)
                .Where(a => a.Homeroom.Grade.MsLevel.IdAcademicYear == request.IdAcademicYear
                    && a.Semester == request.Semester
                    && a.Homeroom.Semester == request.Semester
                    && a.IdStudent == idStudent)
                .ToListAsync(CancellationToken);

            var joinData = from digitalPickup in getTrDigitalPickup
                           join homeroomStudent in getHomeroomStudent on
                           new { digitalPickup.IdStudent, digitalPickup.IdAcademicYear, digitalPickup.Semester } equals
                           new { homeroomStudent.IdStudent, homeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear, homeroomStudent.Semester }
                           select new
                           {
                               DigitalPickup = digitalPickup,
                               HomeroomStudent = homeroomStudent
                           };

            var response = joinData
                .Select(a => new GetStudentDigitalPickupHistoryResponse
                {
                    StudentName = NameUtil.GenerateFullNameWithId(a.DigitalPickup.IdStudent, a.DigitalPickup.Student.FirstName, a.DigitalPickup.Student.LastName),
                    Homeroom = a.HomeroomStudent.Homeroom.Grade.Code + a.HomeroomStudent.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                    Date = a.DigitalPickup.Date.ToString("dd-MM-yyyy"),
                    QRScannedTime = a.DigitalPickup.QrScanTime.ToString("dd-MM-yyyy HH:mm"),
                    PickedUpTime = a.DigitalPickup.PickupTime?.ToString("dd-MM-yyyy HH:mm") ?? "-"
                })
                .OrderBy(a => a.StudentName.Substring(a.StudentName.IndexOf("-") + 2))
                    .ThenByDescending(a => a.Date)
                    .ThenByDescending(a => a.QRScannedTime)
                    .ThenByDescending(a => a.PickedUpTime)
                .ToList();

            return Request.CreateApiResult2(response as object);
        }
    }
}
