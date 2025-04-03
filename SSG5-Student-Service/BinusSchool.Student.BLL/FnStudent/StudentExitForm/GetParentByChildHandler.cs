using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitForm;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;

namespace BinusSchool.Student.FnStudent.StudentExitForm
{
    public class GetParentByChildHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetParentByChildHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.GetParams<GetParentByChildRequest>();

            var predicate = PredicateBuilder.Create<MsHomeroomStudent>(x => x.IdStudent == param.IdStudent);

            predicate = predicate.And(x => x.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear);
            predicate = predicate.And(x => x.Homeroom.Semester == param.Semester);


            var query = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.MsLevel)
                .Where(predicate)
                .Select(x => new GetParentByChildResult
                {
                    IdHomeroomStudent = x.Id,
                    Homeroom = new ItemValueVm
                    {
                        Id = x.Homeroom.Id,
                        Description = $"{x.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code}{x.Homeroom.MsGradePathwayClassroom.Classroom.Code}"
                    },
                    FamilyOfStudents = new List<FamilyOfStudent>()
                }).FirstOrDefaultAsync(CancellationToken);

            if (query != null)
            {
                var getDataParent = _dbContext.Entity<MsStudentParent>().Include(x => x.Parent)
                    .Where(x => x.IdStudent == param.IdStudent)
                    .Select(x => new FamilyOfStudent()
                    {
                        Iduser = x.Parent.Id,
                        Email = x.Parent.PersonalEmailAddress,
                        Phone = x.Parent.MobilePhoneNumber1,
                        Name = NameUtil.GenerateFullName(x.Parent.FirstName, x.Parent.MiddleName, x.Parent.LastName),
                        ParentRole = x.Parent.ParentRole.ParentRoleName
                    })
                    .ToList();

                query.FamilyOfStudents.AddRange(getDataParent);
            }

            return Request.CreateApiResult2(query as object);
        }
    }
}
