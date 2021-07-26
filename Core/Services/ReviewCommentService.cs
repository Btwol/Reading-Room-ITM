﻿using Core.DTOs;
using Core.Interfaces;
using Core.Requests;
using Core.ServiceResponses;
using Storage.Interfaces;
using Storage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class ReviewCommentService : IReviewCommentService
    {
        private readonly IReviewCommentRepository _reviewCommentRepository;
        private readonly ILoggedUserProvider _loggedUserProvider;
        private readonly IReviewRepository _reviewRepository;
        private readonly IGetterService<Book> _bookGetter;
        public ReviewCommentService(IReviewCommentRepository reviewCommentRepository, ILoggedUserProvider loggedUserProvider,
            IReviewRepository reviewRepository, IGetterService<Book> bookGetter)
        {
            _reviewCommentRepository = reviewCommentRepository;
            _loggedUserProvider = loggedUserProvider;
            _reviewRepository = reviewRepository;
            _bookGetter = bookGetter;
        }
        public async Task<ServiceResponse> AddReviewComment(ReviewCommentRequest comment)
        {
            if (!(await _reviewCommentRepository.ReviewExists(comment.ReviewId)))
                return ServiceResponse.Error($"Review you're trying to post a comment for doesn't exist.", HttpStatusCode.BadRequest);

            var userId = _loggedUserProvider.GetUserId();
            if (await _reviewCommentRepository.CheckCommentCount(comment.ReviewId, userId))
                return ServiceResponse.Error($"You can post only {_reviewCommentRepository.MaxCommentPerReview} " +
                    $"comments per single review", HttpStatusCode.BadRequest);

            if (await _reviewCommentRepository.CheckCommentsDate(comment.ReviewId, userId))
                return ServiceResponse.Error($"You can post only {_reviewCommentRepository.MaxCommentPerHourPerReview} " +
                    $"comments per hour on a single review. Try again later", HttpStatusCode.BadRequest);

            return ServiceResponse<ReviewCommentDto>.Success(await _reviewCommentRepository.CreateReviewComment(comment),
                "Comment posted.", HttpStatusCode.Created);
        }

        public async Task<ServiceResponse> GetComment(int reviewCommentId)
        {
            var reviewComment = await _reviewCommentRepository.GetComment(reviewCommentId);
            if (reviewComment != null)
                return ServiceResponse<ReviewCommentDto>.Success(reviewComment, "Comment retrieved.");

            return ServiceResponse.Error("Comment not found.", HttpStatusCode.NotFound);
        }

        public async Task<ServiceResponse> GetComments(int? reviewId, Guid? userId, bool currentUser)
        {
            if (reviewId != null && !(await _reviewCommentRepository.ReviewExists((int)reviewId)))
                return ServiceResponse.Error($"Review doesn't exist.", HttpStatusCode.BadRequest);

            if (reviewId == null && userId == null && currentUser != true)
                return ServiceResponse<IEnumerable<ReviewCommentDto>>.Success(_reviewCommentRepository.GetComments(),
                    $"All comments retrieved.");

            var message = await CreateMessage(reviewId, userId, currentUser);
            if (currentUser) userId = _loggedUserProvider.GetUserId();
            var comments = _reviewCommentRepository.GetComments(reviewId, userId);

            return ServiceResponse<IEnumerable<ReviewCommentDto>>.Success(comments, message);
        }

        private async Task<string> CreateMessage(int? reviewId, Guid? userId, bool currentUser)
        {
            StringBuilder message = new StringBuilder("All comments ");
            if (reviewId.HasValue) message.Append($"for review of {await GetBookTitle((int)reviewId)} ");
            if (currentUser) message.Append("by current user ");
            else if (userId != null) message.Append("by user ");
            message.Append("retrieved.");
            return message.ToString();
        }

        private async Task<string> GetBookTitle(int reviewId)
        {
            return (await _bookGetter.GetById<BookDto>((await _reviewRepository.GetReview(reviewId)).BookId)).Content.Title;
        }
    }
}
