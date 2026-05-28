using Domains;
using Hangfire;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.Tools;

namespace NobatPlusAPI.Tools
{
    public class JobManager
    {
        private readonly IBookingRep _BookingRep;
        private readonly IPersonRep _personRep;
        private readonly ISMSMessageRep _sMSMessageRep;
        private readonly ISettingRep _settingRep;
        private readonly INotificationRep _notificationRep;
        private readonly ILogRep _logRep;

        public JobManager(
            IBookingRep bookingRep,
            IPersonRep personRep,
            ISMSMessageRep sMSMessageRep,
            ISettingRep settingRep,
            INotificationRep notificationRep,
            ILogRep logRep)
        {
            _BookingRep = bookingRep;
            _personRep = personRep;
            _sMSMessageRep = sMSMessageRep;
            _settingRep = settingRep;
            _notificationRep = notificationRep;
            _logRep = logRep;
        }

        public async Task SendBookingRemindMessage(long bookingId)
        {
            var booking = await _BookingRep.GetBookingByIdAsync(bookingId);

            var reminderMessage = await _settingRep.GetSettingRowAsync(0, "BookingRemindMessage");

            if (booking.Result == null || reminderMessage.Result == null) return;


            string message = reminderMessage.Result.Value.ToLower().Replace("{stylistname}", $"{booking.Result.Stylist.Person.FirstName} {booking.Result.Stylist.Person.LastName}")
                .Replace("{bookingdate}",booking.Result.BookingStartDate.ToShamsi().ToString("yyyy/MM/dd"))
                .Replace("{bookingtime}", $"{booking.Result.BookingStartDate.Hour}:{booking.Result.BookingStartDate.Minute}");


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

        public async Task ProcessTodayBirthdays()
        {
            var today = DateTime.Now.ToShamsi();

            var persons = await _personRep.GetAllPersonsAsync(today.Month, today.Day);

            if (persons.Results == null || !persons.Results.Any())
                return;

            foreach (var user in persons.Results)
            {
                BackgroundJob.Enqueue<JobManager>(
                    job => job.SendHBDMessage(user.ID)
                );
            }
        }

        private async Task SendHBDMessage(long personId)
        {
            var person = await _personRep.GetPersonByIdAsync(personId);

            var hbdMessage = await _settingRep.GetSettingRowAsync(0, "HBDMessage");

            if (person.Result == null || hbdMessage.Result == null) return;


            string message = hbdMessage.Result.Value
       .Replace("{fullname}", $"{person.Result.FirstName} {person.Result.LastName}");



            #region SendSMS

            bool sentstatus = await ToolBox.SendSMSMessage(person.Result.PhoneNumber, message);



            SMSMessage SMSMessage = new SMSMessage()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                PhoneNumber = person.Result.PhoneNumber,
                PersonID = person.Result.ID,
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
                PersonID = person.Result.ID,
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
