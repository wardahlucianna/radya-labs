using System;
using System.Threading.Tasks;
using BinusSchool.Data.Models.GitHub;
using Refit;

namespace BinusSchool.Data.Apis
{
    [Headers("User-Agent: BinusSchool.Data")]
    public interface IGitHubUserApi
    {
        [Get("/users/{username}")]
        Task<GitHubUser> GetUser(string username);
    }
}