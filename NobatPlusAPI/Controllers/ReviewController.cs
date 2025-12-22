using AutoMapper;
using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Models.Review;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;
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
        IBookingRep _BookingRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;


        public ReviewController(IReviewRep ReviewRep,IBookingRep bookingRep,ILogRep logRep, IMapper mapper)
        {
           _ReviewRep = ReviewRep;
            _BookingRep = bookingRep;
           _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllReviews_Base")]
        [AllowAnonymous]
        public async Task<ActionResult<ListResultObject<ReviewVM>>> GetAllReviews_Base(GetReviewListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            long roleId = long.Parse(User.FindFirst("Role")?.Value ?? "0");
            var result = await _ReviewRep.GetAllReviewsAsync(roleId,requestBody.BookingId,requestBody.CustomerId,requestBody.StylistId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<ReviewVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("GetReviewById_Base")]
        public async Task<ActionResult<RowResultObject<ReviewVM>>> GetReviewById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            long roleId = long.Parse(User.FindFirst("Role")?.Value ?? "0");
            var result = await _ReviewRep.GetReviewByIdAsync(requestBody.ID,roleId);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<ReviewVM>>(result);
                return Ok(resultVM);
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
            var result = new BitResultObject();

            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var theBooking = await _BookingRep.GetBookingByIdAsync(requestBody.BookingID);
            if (!theBooking.Status)
            {
                result.Status = theBooking.Status;
                result.ErrorMessage = theBooking.ErrorMessage;
                return BadRequest(result);
            }

            if (theBooking.Result.CustomerID != requestBody.CustomerID || theBooking.Result.Status.Trim() != "4")
            {
                result.Status = false;
                result.ErrorMessage = "شما اجازه ثبت بازخورد درباره این نوبت را ندارید";
                return BadRequest(result);
            }
            Review Review = new Review()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                BookingID = requestBody.BookingID,
                Comments = requestBody.Comments,
                CustomerID = requestBody.CustomerID,
                StylistID = requestBody.StylistID,
                DislikeCount = requestBody.DislikeCount,
                LikeCount = requestBody.LikeCount,
                Rating = requestBody.Rating,
                Status = requestBody.Status,
                ReviewDate = requestBody.ReviewDate ?? DateTime.Now.ToShamsi(),
                Description = requestBody.Description,
                IsPrivate = requestBody.IsPrivate,
                IsAccepted = false,
            };
            result = await _ReviewRep.AddReviewAsync(Review);
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

        [HttpPut("AcceptReview_Base")]
        public async Task<ActionResult<BitResultObject>> AcceptReview_Base(GetRowRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            long roleId = long.Parse(User.FindFirst("Role")?.Value ?? "0");
          
            result = await _ReviewRep.AcceptReviewAsync(requestBody.ID,roleId);
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
            long roleId = long.Parse(User.FindFirst("Role")?.Value ?? "0");
            var theRow = await _ReviewRep.GetReviewByIdAsync(requestBody.ID,roleId);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            var theBooking = await _BookingRep.GetBookingByIdAsync(requestBody.BookingID);
            if (!theBooking.Status)
            {
                result.Status = theBooking.Status;
                result.ErrorMessage = theBooking.ErrorMessage;
                return BadRequest(result);
            }

            if (theBooking.Result.CustomerID != requestBody.CustomerID || theBooking.Result.Status.Trim() != "4")
            {
                result.Status = false;
                result.ErrorMessage = "شما اجازه ثبت بازخورد درباره این نوبت را ندارید";
                return BadRequest(result);
            }

            Review Review = new Review()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                BookingID = requestBody.BookingID,
                Comments = requestBody.Comments,
                CustomerID = requestBody.CustomerID,
                StylistID = requestBody.StylistID,
                DislikeCount = requestBody.DislikeCount,
                LikeCount = requestBody.LikeCount,
                Rating = requestBody.Rating,
                Status = requestBody.Status,
                ReviewDate = requestBody.ReviewDate ?? DateTime.Now.ToShamsi(),
                Description = requestBody.Description,
                IsPrivate = requestBody.IsPrivate,
                IsAccepted = false,
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
            long roleId = long.Parse(User.FindFirst("Role")?.Value ?? "0");
            var result = await _ReviewRep.RemoveReviewAsync(requestBody.ID,roleId);
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
