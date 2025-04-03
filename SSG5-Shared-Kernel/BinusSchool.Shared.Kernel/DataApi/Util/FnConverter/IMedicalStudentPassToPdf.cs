using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnConverter.MedicalStudentPassToPdf;
using Refit;

namespace BinusSchool.Data.Api.Util.FnConverter
{
    public interface IMedicalStudentPassToPdf : IFnConverter
    {
        [Post("/medical-student-pass-to-pdf")]
        Task<MedicalStudentPassToPdfResponse> MedicalStudentPassToPdf([Body] MedicalStudentPassToPdfRequest request);
    }
}
