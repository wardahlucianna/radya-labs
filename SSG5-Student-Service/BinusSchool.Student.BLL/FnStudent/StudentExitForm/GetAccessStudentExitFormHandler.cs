using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.Parent;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitForm;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.StudentExitForm.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.StudentExitForm
{
    public class GetAccessStudentExitFormHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IParent _serviceParent;

        public GetAccessStudentExitFormHandler(IStudentDbContext dbContext, IParent serviceParent)
        {
            _dbContext = dbContext;
            _serviceParent = serviceParent;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAccessStudentExitFormRequest, GetAccessStudentExitFormValidator>();

            var getApiChildren = await _serviceParent.GetChildrens(new GetChildRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdParent = param.IdParent
            });

            var getChildren = getApiChildren.IsSuccess ? getApiChildren.Payload.ToList() : null;
            if (getChildren == null)
                throw new BadRequestException("Children from parent data not found!");

            var listChildren = getChildren.Select(x => x.Id).ToList();

            if (listChildren.Count == 0)
            {
                return Request.CreateApiResult2(new GetAccessStudentExitFormResult
                {
                    IsAllowed = false,
                    Childrens = null
                } as object);
            }

            var studentExitSettings = await _dbContext.Entity<MsStudentExitSetting>()
                .Include(x => x.HomeroomStudent)
                    .ThenInclude(x => x.Student)
                .Where(x => listChildren.Contains(x.HomeroomStudent.IdStudent) && x.IsExit)
                .Select(x => new
                {
                    idHomeroomStudent = x.IdHomeroomStudent,
                    idStudent = x.HomeroomStudent.IdStudent,
                    firstName = x.HomeroomStudent.Student.FirstName,
                    middleName = x.HomeroomStudent.Student.MiddleName,
                    lastName = x.HomeroomStudent.Student.LastName
                }).AsNoTracking().ToListAsync();

            if (studentExitSettings.Count == 0)
            {
                return Request.CreateApiResult2(new GetAccessStudentExitFormResult
                {
                    IsAllowed = false,
                    Childrens = null
                } as object);
            }

            var childrens = new List<GetListStudentByParentModel>();
            foreach (var studentExitSetting in studentExitSettings)
            {
                childrens.Add(new GetListStudentByParentModel
                {
                    IdHomeroomStudent = studentExitSetting.idHomeroomStudent,
                    IdStudent = studentExitSetting.idStudent,
                    Name = NameUtil.GenerateFullName(studentExitSetting.firstName, studentExitSetting.middleName, studentExitSetting.lastName),
                });
            }

            var result = new GetAccessStudentExitFormResult
            {
                IsAllowed = true,
                Childrens = childrens
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}
