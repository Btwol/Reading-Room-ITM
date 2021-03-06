using AutoMapper;
using Core.DTOs;
using Core.Interfaces.Profile;
using Core.Response;
using Microsoft.AspNetCore.Http;
using Storage.Interfaces;
using System;
using System.Threading.Tasks;

namespace Core.Services.Profile
{
    class ProfileService : IProfileService
    {
        private readonly IMapper _mapper;
        private readonly IProfileHelper _helper;

        public ProfileService(IMapper mapper, IProfileHelper helper)
        {
            _mapper = mapper;
            _helper = helper;
        }

        public async Task<ServiceResponse> GetProfile(Guid? id)
        {
            var profile = await _helper.GetUserProfile(id);
            return profile.User == null
                ? ReturnErrorResponse()
                : (id == null || id == profile.User.Id ? ReturnProfileResponse<UserProfileDto>(profile) : ReturnProfileResponse<ForeignUserProfileDto>(profile));
        }

        private ServiceResponse ReturnProfileResponse<T>(UserProfile profile)
        {
            var profileDto = _mapper.Map<T>(profile);

            return ServiceResponse<T>.Success(profileDto, "User profile retrieved");
        }
        private ServiceResponse ReturnErrorResponse()
        {
            return ServiceResponse.Error("User profile not found");
        }

        public Task<ServiceResponse> UpdatePhoto(IFormFile image)
        {
            return _helper.EditPhoto(image);
        }
    }
}
