﻿using AutoMapper;
using Core.DTOs;
using Core.Interfaces;
using Core.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Storage.DataAccessLayer;
using Storage.Identity;
using Storage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories
{
    class ReviewRepository : BaseRepository<Review>, IReviewRepository
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        public ReviewRepository(UserManager<User> userManager, IMapper mapper, ApiDbContext context) : base(context)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReviewDto>> GetReviews(int? bookId)
        {
            var reviews = _mapper.Map<IEnumerable<ReviewDto>>(_context.Reviews.Include(r => r.Creator));
            if (bookId != null && await _context.Books.AnyAsync(b => b.Id == bookId)) return reviews.Where(r => r.BookId == bookId);
            return reviews;
        }

        public async Task<bool> ReviewByUserExists(Guid userId, int bookId)
        {
            var book = await _context.Books.Include(b => b.Reviews).FirstOrDefaultAsync(b => b.Id == bookId);
            return book.Reviews.Any(r => r.CreatorId == userId);
        }

        public async Task<ReviewDto> CreateReview(ReviewRequest reviewRequest)
        {
            var review = _mapper.Map<Review>(reviewRequest);
            return _mapper.Map<ReviewDto>(await Create(review));
        }
        public async Task<ReviewDto> GetReview(int reviewId)
        {
            return _mapper.Map<ReviewDto>(await _context.Reviews.Include(r => r.Creator).FirstOrDefaultAsync(r => r.Id == reviewId));
        }
    }
}
