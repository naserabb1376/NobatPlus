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
        private readonly IStylistServiceFollowUpSettingRep _followUpSettingRep;
        private readonly IBookingScheduledMessageRep _bookingScheduledMessageRep;

        public JobManager(
            IBookingRep bookingRep,
            IPersonRep personRep,
            ISMSMessageRep sMSMessageRep,
            ISettingRep settingRep,
            INotificationRep notificationRep,
            ILogRep logRep,
            IStylistServiceFollowUpSettingRep followUpSettingRep,
            IBookingScheduledMessageRep bookingScheduledMessageRep)
        {
            _BookingRep = bookingRep;
            _personRep = personRep;
            _sMSMessageRep = sMSMessageRep;
            _settingRep = settingRep;
            _notificationRep = notificationRep;
            _logRep = logRep;
            _followUpSettingRep = followUpSettingRep;
            _bookingScheduledMessageRep = bookingScheduledMessageRep;
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
            if (reminderMessage != null && reminderMessage.Result.IsActive)
            {
                var message = reminderMessage.Result.Value.MakeMessageOnPattern(new List<ToolBox.MessagePatternObj>
                            {
                                new ToolBox.MessagePatternObj
                                {
                                    Variable = "stylistname",
                                    Value = $"{booking.Result.Stylist.Person.FirstName} {booking.Result.Stylist.Person.LastName}"
                                },
                                 new ToolBox.MessagePatternObj
                                {
                                    Variable = "bookingdate",
                                    Value = booking.Result.BookingStartDate.ToShamsiString().Split(' ')[0]
                                },
                                 new ToolBox.MessagePatternObj
                                {
                                    Variable = "bookingtime",
                                    Value =booking.Result.BookingStartDate.ToString("HH:mm")
                    }
                            });




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
            string rescheduledmessage = "";

            var rescheduledMessageRow = await _settingRep.GetSettingRowAsync(0, "rescheduledmessage");

            if (rescheduledMessageRow != null && rescheduledMessageRow.Result.IsActive)
            {
                 rescheduledmessage = rescheduledMessageRow.Result.Value.MakeMessageOnPattern(new List<ToolBox.MessagePatternObj>
                            {
                                new ToolBox.MessagePatternObj
                                {
                                    Variable = "customerfirstname",
                                    Value = customerName
                                },
                                 new ToolBox.MessagePatternObj
                                {
                                    Variable = "servicename",
                                    Value = servicesText
                                },
                                  new ToolBox.MessagePatternObj
                                {
                                    Variable = "stylistname",
                                    Value = stylistName
                                },
                                 new ToolBox.MessagePatternObj
                                {
                                    Variable = "bookingdate",
                                    Value = row.BookingStartDate.ToShamsiString().Split(' ')[0]
                                },
                                 new ToolBox.MessagePatternObj
                                {
                                    Variable = "bookingtime",
                                    Value =row.BookingStartDate.ToString("HH:mm")
                    }
                            });
            }

            var message = eventType switch
            {
                "cancelled" =>
                    $"",
                "completed" =>
                  "",
                "no-show" =>
                    $"",
                "rescheduled" =>
                   rescheduledmessage,
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

        public async Task ScheduleBookingFollowUpMessages(long bookingId)
        {
            var bookingRow = await _BookingRep.GetBookingByIdAsync(bookingId);
            var booking = bookingRow.Result;

            if (booking == null || booking.IsCancelled || booking.Status != "4")
                return;

            var serviceSelections = booking.Services?.Any() == true
                ? booking.Services
                : booking.ServiceIDs.Select(x => new BookingServiceSelectionDTO
                {
                    ServiceID = x,
                    ServiceName = "خدمت ثبت‌شده"
                }).ToList();

            var serviceIds = serviceSelections
                .Select(x => x.ServiceID)
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (serviceIds == null || !serviceIds.Any())
                return;

            var settings = await _followUpSettingRep.GetActiveStylistServiceFollowUpSettingsAsync(booking.StylistID, serviceIds);
            if (!settings.Any())
                return;

            foreach (var service in serviceSelections)
            {
                var serviceSetting = settings.FirstOrDefault(x => x.ServiceManagementID == service.ServiceID);
                if (serviceSetting == null)
                    continue;

                if (serviceSetting.AfterCareEnabled &&
                    serviceSetting.AfterCareDelayMinutes.HasValue &&
                    !string.IsNullOrWhiteSpace(serviceSetting.AfterCareMessageSettingKey))
                {
                    await CreateAndScheduleBookingMessageAsync(
                        booking,
                        service,
                        serviceSetting,
                        BookingScheduledMessageType.AfterCare,
                        booking.BookingEndDate.AddMinutes(serviceSetting.AfterCareDelayMinutes.Value),
                        serviceSetting.AfterCareMessageSettingKey);
                }

                if (serviceSetting.RepairEnabled &&
                    serviceSetting.RepairReminderEnabled &&
                    serviceSetting.RepairAfterDays.HasValue &&
                    serviceSetting.RepairReminderBeforeDays.HasValue &&
                    !string.IsNullOrWhiteSpace(serviceSetting.RepairReminderMessageSettingKey))
                {
                    var repairDate = booking.BookingEndDate.AddDays(serviceSetting.RepairAfterDays.Value);
                    var reminderDate = repairDate.AddDays(-serviceSetting.RepairReminderBeforeDays.Value);

                    await CreateAndScheduleBookingMessageAsync(
                        booking,
                        service,
                        serviceSetting,
                        BookingScheduledMessageType.RepairReminder,
                        reminderDate,
                        serviceSetting.RepairReminderMessageSettingKey);
                }
            }
        }

        public async Task SendBookingScheduledMessage(long bookingScheduledMessageId)
        {
            var rowResult = await _bookingScheduledMessageRep.GetBookingScheduledMessageByIdAsync(bookingScheduledMessageId);
            var row = rowResult.Result;

            if (row == null || !row.IsActive || row.Status != (byte)BookingScheduledMessageStatus.Pending)
                return;

            if (row.StylistServiceFollowUpSetting == null || !row.StylistServiceFollowUpSetting.IsActive)
            {
                await _bookingScheduledMessageRep.MarkBookingScheduledMessageAsync(
                    row.ID,
                    BookingScheduledMessageStatus.Cancelled,
                    null,
                    false,
                    errorMessage: "تنظیمات follow up غیرفعال شده است");
                return;
            }

            if (row.Booking == null || row.Booking.IsCancelled || row.Customer?.Person == null)
            {
                await _bookingScheduledMessageRep.MarkBookingScheduledMessageAsync(
                    row.ID,
                    BookingScheduledMessageStatus.Cancelled,
                    null,
                    false,
                    errorMessage: "رزرو لغو شده یا اطلاعات مشتری ناقص است");
                return;
            }

            var messageKey = $"booking-scheduled-message:{row.ID}";
            if (await _sMSMessageRep.HasMessageWithDescriptionAsync(messageKey))
            {
                await _bookingScheduledMessageRep.MarkBookingScheduledMessageAsync(
                    row.ID,
                    BookingScheduledMessageStatus.Sent,
                    DateTime.Now.ToShamsi(),
                    false,
                    errorMessage: "پیام قبلا ارسال شده است");
                return;
            }

            var sentStatus = !string.IsNullOrWhiteSpace(row.Customer.Person.PhoneNumber) &&
                             await ToolBox.SendSMSMessage(row.Customer.Person.PhoneNumber, row.MessageText);

            var now = DateTime.Now.ToShamsi();
            var smsResult = await _sMSMessageRep.AddSMSMessageAsync(new SMSMessage
            {
                CreateDate = now,
                UpdateDate = now,
                PhoneNumber = row.Customer.Person.PhoneNumber ?? "",
                PersonID = row.Customer.PersonID,
                Message = row.MessageText,
                SentDate = now,
                Description = messageKey,
                SentStatus = sentStatus
            });

            var notificationResult = await _notificationRep.AddNotificationAsync(new Notification
            {
                CreateDate = now,
                UpdateDate = now,
                PersonID = row.Customer.PersonID,
                Message = row.MessageText,
                SentDate = now,
                Description = messageKey
            });

            await _bookingScheduledMessageRep.MarkBookingScheduledMessageAsync(
                row.ID,
                sentStatus ? BookingScheduledMessageStatus.Sent : BookingScheduledMessageStatus.Failed,
                now,
                !sentStatus,
                smsResult.Status ? smsResult.ID : null,
                notificationResult.Status ? notificationResult.ID : null,
                null,
                sentStatus ? "" : smsResult.ErrorMessage);

            await _logRep.AddLogAsync(new Log
            {
                CreateDate = now,
                UpdateDate = now,
                LogTime = now,
                ActionName = "SendBookingScheduledMessage",
                Description = messageKey
            });
        }

        public async Task CancelBookingScheduledMessages(long bookingId)
        {
            var result = await _bookingScheduledMessageRep.CancelPendingMessagesForBookingAsync(bookingId);
            if (result.Status)
            {
                await _logRep.AddLogAsync(new Log
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = "CancelBookingScheduledMessages",
                    Description = $"BookingID: {bookingId}"
                });
            }
        }

        private async Task CreateAndScheduleBookingMessageAsync(
            BookingDTO booking,
            BookingServiceSelectionDTO service,
            StylistServiceFollowUpSetting followUpSetting,
            BookingScheduledMessageType messageType,
            DateTime scheduledAt,
            string messageSettingKey)
        {
            if (!followUpSetting.IsActive)
                return;

            var settingRow = await _settingRep.GetSettingRowAsync(0, messageSettingKey);
            if (settingRow.Result == null || !settingRow.Result.IsActive)
                return;

            var now = DateTime.Now.ToShamsi();
            if (scheduledAt <= now)
                scheduledAt = now.AddMinutes(1);

            var message = BuildBookingScheduledMessageText(
                settingRow.Result.Value,
                booking,
                service,
                followUpSetting,
                scheduledAt,
                messageType);

            var addResult = await _bookingScheduledMessageRep.AddBookingScheduledMessageAsync(new BookingScheduledMessage
            {
                CreateDate = now,
                UpdateDate = now,
                BookingID = booking.ID,
                StylistID = booking.StylistID,
                CustomerID = booking.CustomerID,
                ServiceManagementID = service.ServiceID,
                StylistServiceFollowUpSettingID = followUpSetting.ID,
                StylistServicePriceVariantID = followUpSetting.StylistServicePriceVariantID,
                MessageType = (byte)messageType,
                MessageText = message,
                ScheduledAt = scheduledAt,
                Status = (byte)BookingScheduledMessageStatus.Pending,
                IsActive = true,
                RetryCount = 0,
                Description = $"booking-scheduled-message:{booking.ID}:{service.ServiceID}:{(byte)messageType}:{scheduledAt.Ticks}"
            });

            if (!addResult.Status)
                return;

            var jobId = BackgroundJob.Schedule<JobManager>(
                job => job.SendBookingScheduledMessage(addResult.ID),
                scheduledAt);

            await _bookingScheduledMessageRep.SetHangfireJobIdAsync(addResult.ID, jobId);

            await _logRep.AddLogAsync(new Log
            {
                CreateDate = now,
                UpdateDate = now,
                LogTime = now,
                ActionName = "ScheduleBookingFollowUpMessages",
                Description = $"BookingID: {booking.ID}, ScheduledMessageID: {addResult.ID}"
            });
        }

        private static string BuildBookingScheduledMessageText(
            string template,
            BookingDTO booking,
            BookingServiceSelectionDTO service,
            StylistServiceFollowUpSetting followUpSetting,
            DateTime scheduledAt,
            BookingScheduledMessageType messageType)
        {
            var customer = booking.Customer.Person;
            var stylistName = booking.Stylist?.Person == null
                ? ""
                : $"{booking.Stylist.Person.FirstName} {booking.Stylist.Person.LastName}".Trim();
            var customerFullName = $"{customer.FirstName} {customer.LastName}".Trim();
            var repairDate = followUpSetting.RepairAfterDays.HasValue
                ? booking.BookingEndDate.AddDays(followUpSetting.RepairAfterDays.Value)
                : (DateTime?)null;

            return template.MakeMessageOnPattern(new List<ToolBox.MessagePatternObj>
            {
                new ToolBox.MessagePatternObj { Variable = "customerfirstname", Value = customer.FirstName ?? "" },
                new ToolBox.MessagePatternObj { Variable = "customerfullname", Value = customerFullName },
                new ToolBox.MessagePatternObj { Variable = "stylistname", Value = stylistName },
                new ToolBox.MessagePatternObj { Variable = "salonname", Value = booking.Stylist?.StylistName ?? "" },
                new ToolBox.MessagePatternObj { Variable = "servicename", Value = service.ServiceName ?? "خدمت ثبت‌شده" },
                new ToolBox.MessagePatternObj { Variable = "bookingdate", Value = booking.BookingEndDate.ToShamsiString().Split(' ')[0] },
                new ToolBox.MessagePatternObj { Variable = "bookingtime", Value = booking.BookingEndDate.ToString("HH:mm") },
                new ToolBox.MessagePatternObj { Variable = "scheduleddate", Value = scheduledAt.ToShamsiString().Split(' ')[0] },
                new ToolBox.MessagePatternObj { Variable = "scheduledtime", Value = scheduledAt.ToString("HH:mm") },
                new ToolBox.MessagePatternObj { Variable = "repairdate", Value = repairDate?.ToShamsiString().Split(' ')[0] ?? "" },
                new ToolBox.MessagePatternObj
                {
                    Variable = "aftercareinstructions",
                    Value = messageType == BookingScheduledMessageType.AfterCare
                        ? followUpSetting.AfterCareInstructions ?? ""
                        : ""
                },
                new ToolBox.MessagePatternObj { Variable = "messagetype", Value = messageType == BookingScheduledMessageType.AfterCare ? "مراقبت بعد از خدمت" : "یادآوری ترمیم" }
            });
        }

    }
}
