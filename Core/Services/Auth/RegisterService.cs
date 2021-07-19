﻿using Core.Common;
using Core.DTOs;
using Core.Interfaces.Auth;
using Core.Requests;
using Core.ServiceResponses;
using Core.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Storage.Identity;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Core.Services.Auth
{
    class RegisterService : IRegisterService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        public RegisterService(UserManager<User> userManager, IConfiguration config, IEmailService emailService){
            _userManager = userManager;
            _config = config;
            _emailService = emailService;
        }

        public async Task<ServiceResponse> Register(RegisterRequest model)
        {
            if (await _userManager.FindByNameAsync(model.Username) != null || await _userManager.FindByEmailAsync(model.Email) != null)
                return new ErrorResponse { StatusCode = HttpStatusCode.UnprocessableEntity, Message = "Account already exists!" };

            User user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return new ErrorResponse { StatusCode = HttpStatusCode.UnprocessableEntity, Message = AdditionalAuthMetods.CreateValidationErrorMessage(result) };

            await _userManager.AddToRoleAsync(user, UserRoles.User);

            user = await _userManager.FindByNameAsync(user.UserName);
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var urlString = AdditionalAuthMetods.BuildUrl(token, user.UserName, _config["Paths:ConfirmEmail"]);

            await _emailService.SendEmailAsync(_config["SMTP:Name"], user.Email, "Confirm your email address", urlString);

            return new SuccessResponse { StatusCode = HttpStatusCode.Created, Message = "User created successfully! Confirm your email." };
        }

        public async Task<ServiceResponse> ConfirmEmail(EmailDto model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                var isConfirmed = user.EmailConfirmed;
                var result = await _userManager.ConfirmEmailAsync(user, model.Token);

                if (isConfirmed || !result.Succeeded)
                    throw new();

                return new SuccessResponse { Message = "Email confirmed succesfully" };
            }
            catch
            {
                return new ErrorResponse { StatusCode = HttpStatusCode.BadRequest, Message = "Link is invalid" };
            }
        }
    }
}