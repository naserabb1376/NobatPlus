using AutoMapper;
using Domain;
using Domains;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.Booking;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;
using SixLabors.ImageSharp.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NobatPlusAPI.Controllers
{
    [Route("Booking")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class BookingController : ControllerBase
    {
        IBookingRep _BookingRep;
        IBookingServiceRep _BookingServiceRep;
        ICustomerRep _CustomerRep;
        IStylistRep _StylistRep;
        ISettingRep _SettingRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;


        public BookingController(IBookingRep BookingRep,IBookingServiceRep BookingServiceRep,ICustomerRep customerRep,IStylistRep stylistRep,ISettingRep settingRep,ILogRep logRep, IMapper mapper)
        {
            _BookingRep = BookingRep;
            _BookingServiceRep = BookingServiceRep;
            _CustomerRep = customerRep;
            _StylistRep = stylistRep;
            _SettingRep = settingRep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllBookings_Base")]
        public async Task<ActionResult<ListResultObject<BookingVM>>> GetAllBookings_Base(GetBookingListRequestBody requestBody)
        {
            var result = new ListResultObject<BookingDTO>();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var roleId = User.GetCurrentRoleId();
            if (roleId == (long)DbTools.BaseRole.Customer)
            {
                var currentCustomerId = await GetCurrentCustomerIdAsync();
                if (currentCustomerId <= 0) return Forbid();
                requestBody.CustomerId = currentCustomerId;
            }
            else if (roleId == (long)DbTools.BaseRole.Stylist)
            {
                if (requestBody.ViewAsCustomer)
                {
                    var currentCustomerId = await GetCurrentCustomerIdAsync();
                    if (currentCustomerId <= 0) return Forbid();
                    requestBody.CustomerId = currentCustomerId;
                    requestBody.StylistId = 0;
                }
                else
                {
                    var currentStylistId = await GetCurrentStylistIdAsync();
                    if (currentStylistId <= 0) return Forbid();
                    requestBody.StylistId = currentStylistId;
                }
            }
            else if (roleId == (long)DbTools.BaseRole.Salon)
            {
                var currentStylistId = await GetCurrentStylistIdAsync();
                if (currentStylistId <= 0) return Forbid();
                if (requestBody.StylistId > 0)
                {
                    if (!await CanAccessStylistAsync(requestBody.StylistId)) return Forbid();
                }
                else
                {
                    requestBody.StylistId = currentStylistId;
                }
            }

            result = await _BookingRep.GetAllBookingsAsync(requestBody.ServiceId,requestBody.CustomerId,requestBody.StylistId,requestBody.CancelState,requestBody.FromDate,requestBody.ToDate, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery, requestBody.Status);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<BookingVM>>(result);
                return Ok(resultVM);
            }

            return BadRequest(result);
        }

        [HttpPost("GetAllBookingsAdmin_Base")]
        public async Task<ActionResult<ListResultObject<BookingVM>>> GetAllBookingsAdmin_Base(GetBookingListRequestBody requestBody)
        {
            if (User.GetCurrentRoleId() != (long)DbTools.BaseRole.Admin)
                return Forbid();

            return await GetAllBookings_Base(requestBody);
        }

        [HttpPost("GetPublicBookingSlots")]
        [AllowAnonymous]
        public async Task<ActionResult<ListResultObject<PublicBookingSlotVM>>> GetPublicBookingSlots(GetBookingListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            if (requestBody.StylistId <= 0)
                return BadRequest("کد آرایشگر برای دریافت نوبت‌های اشغال الزامی است.");

            if (requestBody.FromDate == null || requestBody.ToDate == null)
                return BadRequest("بازه تاریخ برای دریافت نوبت‌های اشغال الزامی است.");

            if (requestBody.ToDate < requestBody.FromDate)
                return BadRequest("بازه تاریخ نامعتبر است.");

            if ((requestBody.ToDate.Value - requestBody.FromDate.Value).TotalDays > 31)
                return BadRequest("بازه تاریخ عمومی نمی‌تواند بیشتر از ۳۱ روز باشد.");

            var pageIndex = Math.Max(requestBody.PageIndex, 1);
            var pageSize = Math.Clamp(requestBody.PageSize <= 0 ? 200 : requestBody.PageSize, 1, 500);

            var result = await _BookingRep.GetAllBookingsAsync(
                ServiceManagementId: 0,
                customerId: 0,
                stylistId: requestBody.StylistId,
                cancelState: 2,
                fromDate: requestBody.FromDate,
                toDate: requestBody.ToDate,
                pageIndex: pageIndex,
                pageSize: pageSize,
                searchText: "",
                sortQuery: "BookingStartDate-asc",
                status: "");

            if (!result.Status)
                return BadRequest(result);

            return Ok(new ListResultObject<PublicBookingSlotVM>
            {
                Status = true,
                TotalCount = result.TotalCount,
                PageCount = result.PageCount,
                Results = result.Results.Select(x => new PublicBookingSlotVM
                {
                    StylistID = x.StylistID,
                    BookingStartDate = x.BookingStartDate,
                    BookingEndDate = x.BookingEndDate,
                    TotalDurationMinutes = x.TotalDurationMinutes,
                    TotalBlockMinutes = x.TotalBlockMinutes,
                    ServiceIDs = x.ServiceIDs,
                    Status = x.Status,
                    IsCancelled = x.IsCancelled
                }).ToList()
            });
        }

        [HttpGet("GetPublicBookingStats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicBookingStats()
        {
            var result = await _BookingRep.GetAllBookingsAsync(
                cancelState: 2,
                pageIndex: 1,
                pageSize: 1,
                searchText: "",
                sortQuery: "");

            if (!result.Status)
                return BadRequest(new { status = false, errorMessage = result.ErrorMessage });

            return Ok(new
            {
                status = true,
                totalBookingsCount = result.TotalCount
            });
        }


        [HttpPost("GetBookingById_Base")]
        public async Task<ActionResult<RowResultObject<BookingVM>>> GetBookingById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _BookingRep.GetBookingByIdAsync(requestBody.ID);
            if (result.Status)
            {
                if (!await CanAccessBookingAsync(result.Result))
                {
                    return Forbid();
                }

                var resultVM = _mapper.Map<RowResultObject<BookingVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistBooking_Base")]
        public async Task<ActionResult<BitResultObject>> ExistBooking_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _BookingRep.ExistBookingAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddBooking_Base")]
        public async Task<ActionResult<BitResultObject>> AddBooking_Base(AddEditBookingRequestBody requestBody)
        {
            var result = new BitResultObject();

            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var roleId = User.GetCurrentRoleId();
            if (roleId == (long)DbTools.BaseRole.Customer)
            {
                var currentCustomerId = await GetCurrentCustomerIdAsync();
                if (currentCustomerId <= 0) return Forbid();
                requestBody.CustomerID = currentCustomerId;
            }
            else if (roleId == (long)DbTools.BaseRole.Stylist && requestBody.ViewAsCustomer)
            {
                var currentCustomerId = await GetCurrentCustomerIdAsync();
                if (currentCustomerId <= 0) return Forbid();
                requestBody.CustomerID = currentCustomerId;
            }
            else if (roleId == (long)DbTools.BaseRole.Stylist || roleId == (long)DbTools.BaseRole.Salon)
            {
                if (!await CanAccessStylistAsync(requestBody.StylistID)) return Forbid();
            }

            var currentStylistId = await GetCurrentStylistIdAsync();

            if (currentStylistId > 0 && currentStylistId == requestBody.StylistID)
            {
                result.Status = false;
                result.ID = 0;
                result.ErrorMessage = "شما امکان ثبت نوبت برای خودتان را ندارید";
                return BadRequest(result);
            }

            Booking Booking = new Booking()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                BookingDate = requestBody.BookingDate,
                //BookingTime = requestBody.BookingTime,
                CancelReason = requestBody.CancelReason ?? "",
                CustomerID = requestBody.CustomerID,
                IsCancelled = requestBody.IsCancelled,
                Status = requestBody.Status,
                StylistID = requestBody.StylistID,
                Description = requestBody.Description,
            };

            Booking.BookingServices = BuildBookingServices(requestBody);

           result = await _BookingRep.AddBookingAsync(Booking);

            if (result.Status)
            {
                ScheduleBookingReminders(result.ID, requestBody.BookingDate);
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

      

        [HttpPut("EditBooking_Base")]
        public async Task<ActionResult<BitResultObject>> EditBooking_Base(AddEditBookingRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var theRow = await _BookingRep.GetBookingByIdAsync(requestBody.ID);

            var currentRoleId = User.GetCurrentRoleId();


            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            if (!theRow.Status || !await CanAccessBookingAsync(theRow.Result))
            {
                return Forbid();
            }

            if ( currentRoleId != (long)DbTools.BaseRole.Admin)
            {
                requestBody.CustomerID = theRow.Result.CustomerID;
                requestBody.StylistID = theRow.Result.StylistID;
            }

            var cancelableBookingHrsRow = await _SettingRep.GetSettingRowAsync(0,"cancelablebookinghours");
            var cancelableBookingHrs = int.Parse(cancelableBookingHrsRow.Result.Value);

            DateTime bookingDateTime = new DateTime(requestBody.BookingDate.Year,requestBody.BookingDate.Month,requestBody.BookingDate.Day,requestBody.BookingTime.Hours,requestBody.BookingTime.Minutes,requestBody.BookingTime.Seconds);
            TimeSpan timeDiff = DateTime.Now.ToShamsi() - bookingDateTime.ToShamsi();

            if (currentRoleId == (long) DbTools.BaseRole.Customer && requestBody.IsCancelled != theRow.Result.IsCancelled && timeDiff.TotalHours >= cancelableBookingHrs )
            {
                result.Status = false;
                result.ID = 0;
                result.ErrorMessage = $"شما امکان لغو نوبت را فقط تا {cancelableBookingHrs} ساعت قبل از زمان رزرو را دارید";
                return BadRequest(result);
            }

            Booking Booking = new Booking()
            {
                ID = requestBody.ID,
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                BookingDate = requestBody.BookingDate,
                //BookingTime = requestBody.BookingTime,
                CancelReason = requestBody.CancelReason ?? "",
                CustomerID = requestBody.CustomerID,
                IsCancelled = requestBody.IsCancelled,
                Status = requestBody.Status,
                StylistID = requestBody.StylistID,
                Description = requestBody.Description,
            };

            Booking.BookingServices = BuildBookingServices(requestBody, Booking.ID);

            result = await _BookingRep.EditBookingAsync(Booking);
            if (result.Status)
            {
                if (!requestBody.IsCancelled && requestBody.Status == "1")
                    ScheduleBookingReminders(result.ID, requestBody.BookingDate);

                var bookingEvent = GetBookingEvent(
                    theRow.Result.IsCancelled,
                    theRow.Result.Status,
                    theRow.Result.BookingStartDate,
                    requestBody.IsCancelled,
                    requestBody.Status,
                    requestBody.BookingDate);
                if (!string.IsNullOrWhiteSpace(bookingEvent))
                {
                    BackgroundJob.Enqueue<JobManager>(job =>
                        job.SendBookingStatusMessage(result.ID, bookingEvent, theRow.Result.BookingStartDate));
                }

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

        [HttpDelete("DeleteBooking_Base")]
        [RequireRole(4)]
        public async Task<ActionResult<BitResultObject>> DeleteBooking_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }      
            var result = await _BookingRep.RemoveBookingAsync(requestBody.ID);
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

        private static List<BookingService> BuildBookingServices(AddEditBookingRequestBody requestBody, long bookingId = 0)
        {
            return requestBody.Services
                .Where(x => x.ServiceID > 0)
                .GroupBy(x => x.ServiceID)
                .Select(x => x.First())
                .Select(service => new BookingService
                {
                    BookingID = bookingId,
                    ServiceManagementID = service.ServiceID,
                    OptionValues = service.OptionValueIDs?
                        .Where(optionValueId => optionValueId > 0)
                        .Distinct()
                        .Select(optionValueId => new BookingServiceOptionValue
                        {
                            BookingID = bookingId,
                            ServiceManagementID = service.ServiceID,
                            ServiceOptionValueID = optionValueId
                        })
                        .ToList() ?? new List<BookingServiceOptionValue>()
                })
                .ToList();
        }

        private async Task<long> GetCurrentCustomerIdAsync()
        {
            var result = await _CustomerRep.ExistCustomerAsync(User.GetCurrentUserId().ToString(), "personid");
            return result.Status ? result.ID : 0;
        }

        private async Task<long> GetCurrentStylistIdAsync()
        {
            var result = await _StylistRep.ExistStylistAsync(User.GetCurrentUserId().ToString(), "personid");
            return result.Status ? result.ID : 0;
        }

        private async Task<bool> CanAccessBookingAsync(BookingDTO? booking)
        {
            if (booking == null) return false;

            var roleId = User.GetCurrentRoleId();
            if (roleId == (long)DbTools.BaseRole.Admin)
                return true;

            if (roleId == (long)DbTools.BaseRole.Customer)
            {
                var currentCustomerId = await GetCurrentCustomerIdAsync();
                return currentCustomerId > 0 && booking.CustomerID == currentCustomerId;
            }

            if (roleId == (long)DbTools.BaseRole.Stylist)
            {
                if (await CanAccessStylistAsync(booking.StylistID))
                    return true;

                var currentCustomerId = await GetCurrentCustomerIdAsync();
                return currentCustomerId > 0 && booking.CustomerID == currentCustomerId;
            }

            if (roleId == (long)DbTools.BaseRole.Salon)
                return await CanAccessStylistAsync(booking.StylistID);

            return false;
        }

        private async Task<bool> CanAccessStylistAsync(long stylistId)
        {
            if (stylistId <= 0) return false;

            var roleId = User.GetCurrentRoleId();
            if (roleId == (long)DbTools.BaseRole.Admin)
                return true;

            var currentStylistId = await GetCurrentStylistIdAsync();
            if (currentStylistId <= 0)
                return false;

            if (roleId == (long)DbTools.BaseRole.Stylist)
                return stylistId == currentStylistId;

            if (roleId == (long)DbTools.BaseRole.Salon)
            {
                if (stylistId == currentStylistId)
                    return true;

                var target = await _StylistRep.GetStylistByIdAsync(stylistId);
                return target.Status && target.Result?.StylistParentID == currentStylistId;
            }

            return false;
        }

        private static void ScheduleBookingReminders(long bookingId, DateTime bookingDate)
        {
            foreach (var leadHours in new[] { 24, 2 })
            {
                var scheduleAt = bookingDate.AddHours(-leadHours);
                if (scheduleAt <= DateTime.Now)
                    continue;

                BackgroundJob.Schedule<JobManager>(
                    job => job.SendBookingRemindMessage(bookingId, leadHours),
                    scheduleAt);
            }
        }

        private static string GetBookingEvent(
            bool wasCancelled,
            string previousStatus,
            DateTime previousDate,
            bool isCancelled,
            string nextStatus,
            DateTime nextDate)
        {
            if (!wasCancelled && isCancelled)
                return "cancelled";
            if (!isCancelled && previousStatus != nextStatus && nextStatus == "4")
                return "completed";
            if (!isCancelled && previousStatus != nextStatus && nextStatus == "3")
                return "no-show";
            if (!isCancelled && previousDate != nextDate)
                return "rescheduled";

            return string.Empty;
        }
    }

    public class PublicBookingSlotVM
    {
        public long StylistID { get; set; }
        public DateTime BookingStartDate { get; set; }
        public DateTime BookingEndDate { get; set; }
        public string Status { get; set; } = "";
        public int TotalDurationMinutes { get; set; }
        public int TotalBlockMinutes { get; set; }
        public List<long> ServiceIDs { get; set; } = new();
        public bool IsCancelled { get; set; }
    }
}
