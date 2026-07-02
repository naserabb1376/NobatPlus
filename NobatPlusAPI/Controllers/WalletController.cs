using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.Wallet;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusAPI.Controllers
{
    [Route("Wallet")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class WalletController : ControllerBase
    {
        private static readonly bool SelfWalletChargeEnabled = false;
        private readonly IWalletRep _walletRep;
        private readonly ICustomerRep _customerRep;

        public WalletController(IWalletRep walletRep, ICustomerRep customerRep)
        {
            _walletRep = walletRep;
            _customerRep = customerRep;
        }

        [HttpPost("GetMyWallet_Base")]
        public async Task<ActionResult<RowResultObject<WalletReport>>> GetMyWallet_Base(GetWalletRequestBody requestBody)
        {
            var customer = await GetCurrentCustomerAsync();
            if (!customer.Status)
            {
                return BadRequest(new RowResultObject<WalletReport> { Status = false, ErrorMessage = "مشتری یافت نشد" });
            }

            var result = await _walletRep.GetWalletAsync(customer.ID, requestBody.PageIndex, requestBody.PageSize);
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("GetCustomerWallet_Base")]
        public async Task<ActionResult<RowResultObject<WalletReport>>> GetCustomerWallet_Base(GetCustomerWalletRequestBody requestBody)
        {
            if (!RequireAdmin(out var forbidden))
            {
                return forbidden;
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var result = await _walletRep.GetWalletAsync(requestBody.CustomerId, requestBody.PageIndex, requestBody.PageSize);
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("GetWalletTransactions_Base")]
        public async Task<ActionResult<ListResultObject<AdminWalletTransactionReport>>> GetWalletTransactions_Base(GetWalletTransactionsRequestBody requestBody)
        {
            if (!RequireAdmin(out var forbidden))
            {
                return forbidden;
            }

            var result = await _walletRep.GetWalletTransactionsAsync(
                requestBody.PageIndex,
                requestBody.PageSize,
                requestBody.SearchText ?? "",
                requestBody.TransactionType ?? "");

            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("ChargeWallet_Base")]
        public async Task<ActionResult<BitResultObject>> ChargeWallet_Base(ChargeWalletRequestBody requestBody)
        {
            if (!SelfWalletChargeEnabled)
            {
                return StatusCode(503, new BitResultObject
                {
                    Status = false,
                    ErrorMessage = "شارژ مستقیم کیف پول تا زمان اتصال درگاه پرداخت فعال نیست"
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var customer = await GetCurrentCustomerAsync();
            if (!customer.Status)
            {
                return BadRequest(new BitResultObject { Status = false, ErrorMessage = "مشتری یافت نشد" });
            }

            var result = await _walletRep.ChargeWalletAsync(customer.ID, requestBody.Amount, requestBody.Description ?? "شارژ کیف پول");
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("ChargeCustomerWallet_Base")]
        public async Task<ActionResult<BitResultObject>> ChargeCustomerWallet_Base(ChargeCustomerWalletRequestBody requestBody)
        {
            if (User.GetCurrentRoleId() != 4)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var result = await _walletRep.ChargeWalletAsync(requestBody.CustomerId, requestBody.Amount, requestBody.Description ?? "شارژ کیف پول توسط ادمین");
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("PayBookingWithWallet_Base")]
        public async Task<ActionResult<BitResultObject>> PayBookingWithWallet_Base(PayBookingWithWalletRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var customer = await GetCurrentCustomerAsync();
            if (!customer.Status)
            {
                return BadRequest(new BitResultObject { Status = false, ErrorMessage = "مشتری یافت نشد" });
            }

            var result = await _walletRep.PayBookingAsync(
                customer.ID,
                requestBody.BookingID,
                requestBody.DiscountID ?? 0,
                requestBody.Description ?? "پرداخت رزرو با کیف پول");

            return result.Status ? Ok(result) : BadRequest(result);
        }

        private async Task<BitResultObject> GetCurrentCustomerAsync()
        {
            var userId = User.GetCurrentUserId();
            return await _customerRep.ExistCustomerAsync(userId.ToString(), "personid");
        }

        private bool RequireAdmin(out ActionResult forbidden)
        {
            forbidden = Forbid();
            return User.GetCurrentRoleId() == 4;
        }
    }
}
