using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.Student
{
    public class GetStudentForUploadASCHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetStudentForUploadASCHandler(IStudentDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.GetBody<GetStudentUploadAscRequest>();
            var gradeCode = await _dbContext.Entity<MsStudent>()
                                      .Include(p=> p.StudentGrades)
                                      .Include(p=> p.Religion)
                                      .Include(p=> p.ReligionSubject)
                                      .Where(p => param.BinusianId.Any(x => x == p.Id))
                                      .Select(p => new GetStudentUploadAscResult
                                      {
                                          IdStudent = p.Id,
                                          FullName = p.FirstName,
                                          BinusianId = p.Id,
                                          Gender = p.Gender.ToString(),
                                          Religion= p.Religion != null ? p.Religion.ReligionName:"",
                                          ReligionSubject=p.ReligionSubject!=null? p.ReligionSubject.ReligionSubjectName:"",
                                          IdGrade=p.StudentGrades.Select(p=> p.IdGrade).ToList()
                                      }).ToListAsync();

            return Request.CreateApiResult2(gradeCode as object);
        }
    }
}
