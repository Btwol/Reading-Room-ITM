﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Common;
using WebAPI.DTOs;
using Core.Exceptions;
using Storage.Models;
using Core.Interfaces;
using Core.DTOs;
using Core.Requests;
using Core.ServiceResponses;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private readonly ICrudService<Photo> _crud;
        private readonly IPhotoService _photoService;

        public PhotoController(ICrudService<Photo> crud, IPhotoService photoService)
        {
            _crud = crud;
            _photoService = photoService;
        }

        [HttpGet("All")]
        public async Task<ServiceResponse> GetPhotos(int? book_id)
        {
            if (book_id != null)
            {
                return new SuccessResponse<IEnumerable<PhotoDto>>() { Content = _crud.GetAll<PhotoDto>().Result.Where(p => p.BookId == book_id) };
            }
            return new SuccessResponse<IEnumerable<PhotoDto>>(){ Content = await _crud.GetAll<PhotoDto>() };
        }

        [HttpGet("{id:int}")]
        public async Task<ServiceResponse> GetPhoto(int id)
        {
            var result = await _crud.GetById<PhotoDto>(id);
            if (result == null) return new ErrorResponse() { Message = "Photo not found.", StatusCode = System.Net.HttpStatusCode.NotFound };
            return new SuccessResponse<PhotoDto>() { Content = result };
        }

        [HttpPost()]
        public async Task<ServiceResponse> Upload(IFormFile image, int bookId)
        {
            try
            {
                var result = await _photoService.UploadPhoto(image, bookId);
                return result;
            }
            catch (DbUpdateException e)
            {
                return new ErrorResponse() { Message = e.InnerException.Message, StatusCode = System.Net.HttpStatusCode.BadRequest };
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ServiceResponse> Edit(int id, PhotoUpdateRequest photo_bookId)
        {
            try
            {
                await _crud.Update(photo_bookId, id);
                return new SuccessResponse() { Message = "Image updated" };
            }
            catch (DbUpdateException e)
            {
                return new ErrorResponse() { Message = e.InnerException.Message, StatusCode = System.Net.HttpStatusCode.BadRequest };
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ServiceResponse> Delete(int id)
        {
            try
            {
                var result = await _photoService.DeletePhoto(id);
                return new SuccessResponse() { Message = "Image deleted." };
            }
            catch (NotFoundException e)
            {
                return new ErrorResponse() { Message = e.Message, StatusCode = System.Net.HttpStatusCode.BadRequest };

            }
            catch (DbUpdateException e)
            {
                return new ErrorResponse() { Message = e.InnerException.Message, StatusCode = System.Net.HttpStatusCode.BadRequest };
            }
            catch (Exception e)
            {
                if (e.InnerException.GetType() == typeof(NotFoundException)) return new ErrorResponse()
                { Message = "Image not found", StatusCode = System.Net.HttpStatusCode.NotFound };

                return new ErrorResponse() { Message = e.Message };
            }
        }
    }
}