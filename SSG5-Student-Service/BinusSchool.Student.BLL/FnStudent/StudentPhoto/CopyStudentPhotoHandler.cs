using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.StudentPhoto;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.StudentPhoto.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Student.FnStudent.StudentPhoto
{
    public class CopyStudentPhotoHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _dateTime;

        public CopyStudentPhotoHandler(
            IStudentDbContext dbContext,
            IConfiguration configuration, 
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<CopyStudentPhotoRequest, CopyStudentPhotoValidator>();

            var getStudentPhoto = await _dbContext.Entity<TrStudentPhoto>()
                        .Where(x => x.IdAcademicYear == param.IdAcademicYearFrom)
                        .Where(x => param.IdStudents.Any(y => y == x.IdStudent))
                        .ToListAsync(CancellationToken);
            
            try
            {
                // Copy student photo
                var newPhotoData = new List<TrStudentPhoto>();
                foreach (var photo in getStudentPhoto)
                {
                    var paramNewPhoto = new TrStudentPhoto
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdAcademicYear = param.IdAcademicYearDest,
                        IdStudent = photo.IdStudent,
                        IdBinusian = photo.IdBinusian,
                        FileName = photo.FileName,
                        FilePath = photo.FilePath
                    };
                    newPhotoData.Add(paramNewPhoto);
                }
                _dbContext.Entity<TrStudentPhoto>().AddRange(newPhotoData);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return Request.CreateApiResult2();
        }
    }
}
