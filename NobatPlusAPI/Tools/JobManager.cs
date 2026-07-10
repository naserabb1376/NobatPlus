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

        public async Task SendBookingRemindMessage(long bookingId, int leadHours = 24)
        {
            var booking = await _BookingRep.GetBookingByIdAsync(bookingId);
            if (booking.Result == null ||
                booking.Result.IsCancelled ||
                booking.Result.Status != "1")
                return;

            var hoursUntilBooking = (booking.Result.BookingStartDate - DateTime.Now).TotalHours;
            if (hoursUntilBooking < leadHours - 1 || hoursUntilBooking > leadHours + 1)
                return;

            var reminderKey =
                $"booking-reminder:{bookingId}:{booking.Result.BookingStartDate.Ticks}:{leadHours}";
            if (await _sMSMessageRep.HasMessageWithDescriptionAsync(reminderKey))
                return;

            var reminderMessage = await _settingRep.GetSettingRowAsync(
                0,
                leadHours == 2 ? "BookingRemindMessage2Hours" : "BookingRemindMessage");

            var template = reminderMessage.Result?.Value;
            if (string.IsNullOrWhiteSpace(template))
            {
                template =
                    "{customername} عزیز، یادآوری نوبت شما با {stylistname} در تاریخ " +
                    "{bookingdate} ساعت {bookingtime}. نوبتیکس";
            }

            string message = template
                .Replace("{customername}", booking.Result.Customer.Person.FirstName)
                .Replace("{stylistname}", $"{booking.Result.Stylist.Person.FirstName} {booking.Result.Stylist.Person.LastName}")
                .Replace("{bookingdate}", booking.Result.BookingStartDate.ToShamsiString().Split(' ')[0])
                .Replace("{bookingtime}", booking.Result.BookingStartDate.ToString("HH:mm"));


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
                Description = reminderKey,
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

        public async Task SendBookingStatusMessage(long bookingId, string eventType, DateTime? previousBookingDate = null)
        {
            var booking = await _BookingRep.GetBookingByIdAsync(bookingId);
            if (!booking.Status || booking.Result?.Customer?.Person == null)
                return;

            var row = booking.Result;
            var eventKey = eventType == "rescheduled"
                ? $"booking-status:{bookingId}:{eventType}:{row.BookingStartDate.Ticks}"
                : $"booking-status:{bookingId}:{eventType}";

            if (await _sMSMessageRep.HasMessageWithDescriptionAsync(eventKey))
                return;

            var customer = row.Customer.Person;
            var stylistName = row.Stylist?.Person == null
                ? "آرایشگر"
                : $"{row.Stylist.Person.FirstName} {row.Stylist.Person.LastName}".Trim();
            var customerName = string.IsNullOrWhiteSpace(customer.FirstName) ? "کاربر عزیز" : $"{customer.FirstName} عزیز";
            var services = row.Services?
                .Select(x => x.ServiceName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList() ?? new List<string?>();
            var servicesText = services.Any() ? string.Join("، ", services) : "خدمت ثبت‌شده";
            var bookingDetails =
                $"شماره نوبت: {row.ID}\n" +
                $"آرایشگر: {stylistName}\n" +
                $"خدمت: {servicesText}\n" +
                $"تاریخ: {row.BookingStartDate.ToShamsiString().Split(' ')[0]}\n" +
                $"ساعت: {row.BookingStartDate:HH:mm}";

            var message = eventType switch
            {
                "cancelled" =>
                    $"{customerName}، نوبت شما لغو شد.\n{bookingDetails}\n" +
                    (!string.IsNullOrWhiteSpace(row.CancelReason) ? $"علت: {row.CancelReason}\n" : "") +
                    "برای دریافت نوبت جدید وارد نوبتیکس شوید: https://nobatix.com/",
                "completed" =>
                    $"{customerName}، نوبت شما انجام شد.\n{bookingDetails}\n" +
                    $"لطفاً نظر خود را ثبت کنید: https://nobatix.com/customer/appointments?reviewBookingId={row.ID}",
                "no-show" =>
                    $"{customerName}، برای نوبت زیر عدم حضور ثبت شد.\n{bookingDetails}\n" +
                    "برای دریافت نوبت جدید وارد نوبتیکس شوید: https://nobatix.com/",
                "rescheduled" =>
                    $"{customerName}، زمان نوبت شما جابه‌جا شد.\n" +
                    (previousBookingDate.HasValue
                        ? $"زمان قبلی: {previousBookingDate.Value.ToShamsiString().Split(' ')[0]} ساعت {previousBookingDate.Value:HH:mm}\n"
                        : "") +
                    $"زمان جدید:\n{bookingDetails}",
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(message))
                return;

            var sentStatus = !string.IsNullOrWhiteSpace(customer.PhoneNumber) &&
                             await ToolBox.SendSMSMessage(customer.PhoneNumber, message);
            var now = DateTime.Now.ToShamsi();

            await _sMSMessageRep.AddSMSMessageAsync(new SMSMessage
            {
                CreateDate = now,
                UpdateDate = now,
                PhoneNumber = customer.PhoneNumber ?? string.Empty,
                PersonID = customer.ID,
                Message = message,
                SentDate = now,
                Description = eventKey,
                SentStatus = sentStatus,
            });

            await _notificationRep.AddNotificationAsync(new Notification
            {
                CreateDate = now,
                UpdateDate = now,
                PersonID = customer.ID,
                Message = message,
                SentDate = now,
                Description = eventKey,
            });
        }

        public async Task ProcessTodayBirthdays()
        {
            var today = DateTime.Now.ToShamsi();

            var persons = await _personRep.GetPersonsWithBirthdayAsync(today.Month, today.Day);

            if (persons.Results == null || !persons.Results.Any())
                return;

            foreach (var user in persons.Results)
            {
                BackgroundJob.Enqueue<JobManager>(
                    job => job.SendHBDMessage(user.ID)
                );
            }
        }

        public async Task SendHBDMessage(long personId)
        {
            var person = await _personRep.GetPersonByIdAsync(personId);

            var hbdMessage = await _settingRep.GetSettingRowAsync(0, "HBDMessage");

            if (person.Result == null || hbdMessage.Result == null) return;

            var birthdayKey = $"birthday-message:{personId}:{DateTime.Now.ToShamsiString().Split(' ')[0]}";
            if (await _sMSMessageRep.HasMessageWithDescriptionAsync(birthdayKey))
                return;

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
                Description = birthdayKey,
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
                    ActionName = "SendHBDMessage",

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
                Description = birthdayKey,
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
                    ActionName = "SendHBDMessage",

                };
                await _logRep.AddLogAsync(log);

                #endregion
            }

            #endregion
        }

    }
}
