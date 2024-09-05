using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.Review;
using NobatPlusAPI.Models.Public;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NobatPlusAPI.Controllers
{
    [Route("Review")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class ReviewController : ControllerBase
    {
        IReviewRep _ReviewRep;
        ILogRep _logRep;

        public ReviewController(IReviewRep ReviewRep,ILogRep logRep)
        {
           _ReviewRep = ReviewRep;
           _logRep = logRep;
        }

        [HttpPost("GetAllReviews_Base")]
        public async Task<ActionResult<ListResultObject<Review>>> GetAllReviews_Base(GetReviewListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ReviewRep.GetAllReviewsAsync(requestBody.BookingId,requestBody.CustomerId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetReviewById_Base")]
        public async Task<ActionResult<ListResultObject<Review>>> GetReviewById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ReviewRep.GetReviewByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistReview_Base")]
        public async Task<ActionResult<BitResultObject>> ExistReview_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ReviewRep.ExistReviewAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddReview_Base")]
        public async Task<ActionResult<BitResultObject>> AddReview_Base(AddEditReviewRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            Review Review = new Review()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                BookingID = requestBody.BookingID,
                Comments = requestBody.Comments,
                CustomerID = requestBody.CustomerID,
                DislikeCount = requestBody.DislikeCount,
                LikeCount = requestBody.LikeCount,
                Rating = requestBody.Rating,
                Status = requestBody.Status,
                ReviewDate = requestBody.ReviewDate ?? DateTime.Now.ToShamsi(),
                Description = requestBody.Description,
            };
            var result = await _ReviewRep.AddReviewAsync(Review);
            if (result.Status)
            {
                #region AddLog

                Log log = new Log()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),

                };
                await _logRep.AddLogAsync(log);

                #endregion


                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPut("EditReview_Base")]
        public async Task<ActionResult<BitResultObject>> EditReview_Base(AddEditReviewRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _ReviewRep.GetReviewByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            Review Review = new Review()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                BookingID = requestBody.BookingID,
                Comments = requestBody.Comments,
                CustomerID = requestBody.CustomerID,
                DislikeCount = requestBody.DislikeCount,
                LikeCount = requestBody.LikeCount,
                Rating = requestBody.Rating,
                Status = requestBody.Status,
                ReviewDate = requestBody.ReviewDate ?? DateTime.Now.ToShamsi(),
                Description = requestBody.Description,
            };
            result = await _ReviewRep.EditReviewAsync(Review);
            if (result.Status)
            {

                #region AddLog

                Log log = new Log()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),

                };
                await _logRep.AddLogAsync(log);

                #endregion

                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete("DeleteReview_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteReview_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ReviewRep.RemoveReviewAsync(requestBody.ID);
            if (result.Status)
            {

                #region AddLog

                Log log = new Log()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),

                };
                await _logRep.AddLogAsync(log);

                #endregion

                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
