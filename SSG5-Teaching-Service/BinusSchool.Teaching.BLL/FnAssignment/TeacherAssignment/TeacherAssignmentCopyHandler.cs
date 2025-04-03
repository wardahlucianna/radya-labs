using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using Microsoft.Extensions.Localization;
using Org.BouncyCastle.Asn1.Cmp;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment;
using BinusSchool.Teaching.FnAssignment.TeacherAssignment.Validator;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Newtonsoft.Json;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Data.Api.School.FnSchool;
using NPOI.HSSF.Record;
using BinusSchool.Common.Exceptions;
using NPOI.Util;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignment
{
    public class TeacherAssignmentCopyHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _teachingDbContext;

        public TeacherAssignmentCopyHandler(ITeachingDbContext teachingDbContext)
        {
            _teachingDbContext = teachingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<TeacherAssignmentCopyRequest, TeacherAssignmentCopyValidator>();
            
            foreach (var item in body.NonTeachingLoads)
            {
                var newTrNonTeachingLoad = new TrNonTeachingLoad
                {
                    Id = Guid.NewGuid().ToString(),
                    IdMsNonTeachingLoad = item.IdMsNonTeachingLoad,
                    IdUser = item.IdUser,
                    Load = item.Load,
                    Data = item.Data,
                };
                _teachingDbContext.Entity<TrNonTeachingLoad>().Add(newTrNonTeachingLoad);
            }

            await _teachingDbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
