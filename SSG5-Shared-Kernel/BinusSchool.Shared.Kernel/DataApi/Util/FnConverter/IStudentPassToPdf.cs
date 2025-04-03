using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnConverter.StudentPassToPdf;
using Refit;

namespace BinusSchool.Data.Api.Util.FnConverter
{
    public interface IStudentPassToPdf : IFnConverter
    {
        [Post("/student-pass-to-pdf")]
        Task<StudentPassToPdfResult> StudentPassToPdf([Body] StudentPassToPdfRequest body);
    }
}
