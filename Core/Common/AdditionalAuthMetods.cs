using Core.Interfaces.Auth;
using Core.Response;
using Core.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Storage.Identity;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Core.Common
{
    class AdditionalAuthMetods : AuthServicesProvider, IAdditionalAuthMetods
    {
        public AdditionalAuthMetods(UserManager<User> _userManager, IConfiguration _config, IJwtGenerator _jwtGenerator) 
            : base(_userManager, config: _config, jwtGenerator: _jwtGenerator) {}

        public string BuildUrl(string token, string username, string path)
        {
            var uriBuilder = new UriBuilder(path);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["token"] = token;
            query["username"] = username;
            uriBuilder.Query = query.ToString();
            var urlString = uriBuilder.ToString();

            return urlString;
        }

        public string CreateValidationErrorMessage(IdentityResult result)
        {
            StringBuilder builder = new ();
            foreach (var error in result.Errors)
            {
                builder.Append(error.Description + " ");
            }

            return builder.ToString();
        }

        public async Task<ServiceResponse> GetUserTokenResponse(string userInfo)
        {
            User user = new ();
            if (userInfo.Contains('@'))
            {
                user = await _userManager.FindByEmailAsync(userInfo);
            }
            else
            {
                user = await _userManager.FindByNameAsync(userInfo);
            }

            var roles = await _userManager.GetRolesAsync(user);

            return ServiceResponse<string>.Success($"{_jwtGenerator.GenerateJWTToken(_config, user, roles)}", "Successful login");
        }
    }
}
