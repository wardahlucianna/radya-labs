using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnConverter.Crypt;
using Refit;

namespace BinusSchool.Data.Api.Util.FnConverter
{   
    public interface ICrypt : IFnConverter
    {
        [Get("/crypt/get-rijndael-crypt")]
        Task<RijndaelResult> GetRijndael([Query] RijndaelRequest param);

        [Get("/crypt/get-sha1-crypt")]
        Task<Sha1Result> GetSHA1([Query] Sha1Request param);

    }
}
