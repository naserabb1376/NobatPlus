using Domains;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.Tools;

namespace NobatPlusAPI.Tools
{
    public class JobManager
    {
        private readonly IBookingRep _BookingRep;
        private readonly ISMSMessageRep _sMSMessageRep;
        private readonly INotificationRep _notificationRep;
        private readonly ILogRep _logRep;

        public JobManager(
            IBookingRep bookingRep,
            ISMSMessageRep sMSMessageRep,
            INotificationRep notificationRep,
            ILogRep logRep)
        {
            _BookingRep = bookingRep;
            _sMSMessageRep = sMSMessageRep;
            _notificationRep = notificationRep;
            _logRep = logRep;
        }

        public async Task SendBookingRemindMessage(long bookingId)
        {
            var booking = await _BookingRep.GetBookingByIdAsync(bookingId);

            if (booking.Result == null) return;


            string message = $@"
{booking.Result.Customer.Person.FirstName} عزیز
یادت نره که فردا {booking.Result.BookingDate.ToShamsiString()}
پیش {booking.Result.Stylist.Person.FirstName} {booking.Result.Stylist.Person.LastName} نوبت داری
میبینیمت!
";


            #region SendSMS

            bool sentstatus = await ToolBox.SendSMSMessage(booking.Result.Customer.Person.PhoneNumber, message);



            SMSMessage SMSMessage = new SMSMessage()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                PhoneNumber = booking.Result.Customer.Person.PhoneNumber,
                PersonID = booking.Result.Customer.PersonID,
                Message = message,
                SentDate = DateTime.Now.ToShamsi(),
                Description = message,
                SentStatus = sentstatus,
            };
            var smsresult = await _sMSMessageRep.AddSMSMessageAsync(SMSMessage);
            if (smsresult.Status)
            {
                #region AddLog

                Log log = new Log()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = "SendBookingRemindMessage",

                };
                await _logRep.AddLogAsync(log);

                #endregion
            }

            #endregion

            #region SendNotification

            Notification Notification = new Notification()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                PersonID = booking.Result.Customer.PersonID,
                Message = message,
                SentDate = DateTime.Now.ToShamsi(),
                Description = message,
            };
            var notifresult = await _notificationRep.AddNotificationAsync(Notification);
            if (notifresult.Status)
            {
                #region AddLog

                Log log = new Log()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = "SendBookingRemindMessage",

                };
                await _logRep.AddLogAsync(log);

                #endregion
            }

            #endregion
        }

    }
}
