# راهنمای بک‌اند برنامه‌ریزی دستی نوبت‌ها

## هدف

این قابلیت یک روش سوم برای ایجاد و نمایش نوبت‌ها اضافه می‌کند. آرایشگر می‌تواند برای روزهای آینده بازه‌های مشخص بسازد و برای هر بازه خدمت، قیمت متغیر، برچسب، قیمت اختصاصی و بیعانه اختصاصی تعیین کند.

## حالت‌های ایجاد نوبت

مقدار `BookingCreationMode` یکی از موارد زیر است:

| مقدار | رفتار |
|---|---|
| `automatic` | مدت نوبت از مجموع مدت خدمات یا Variantهای انتخاب‌شده محاسبه می‌شود. |
| `manual` | مدت هر نوبت از `SlotIntervalMinutes` گرفته می‌شود. این همان رفتار دستی قبلی پروژه است. |
| `manual-schedule` | مشتری فقط یکی از بازه‌های از قبل ساخته‌شده در `StylistScheduleBlocks` را رزرو می‌کند. |

در هر سه حالت `WorkTime`، مرخصی‌های `StylistPacific`، نوبت‌های موجود و `RestTime` کنترل می‌شوند.

## جداول و فیلدهای جدید

### BookingTags

برچسب‌های اختصاصی هر آرایشگر را نگهداری می‌کند؛ برای مثال VIP، ویژه عروس یا تخفیف صبح.

فیلدهای اصلی:

- `StylistID`
- `Title`
- `Color`
- `SortOrder`
- `IsActive`

عنوان برچسب برای هر آرایشگر Unique است.

### StylistScheduleBlocks

هر رکورد یک بازه قابل نمایش در تقویم دستی است.

- `StylistID`
- `StartDateTime`
- `EndDateTime`
- `ServiceManagementID` اختیاری
- `StylistServicePriceVariantID` اختیاری
- `BookingTagID` اختیاری
- `Title`
- `PriceOverride` اختیاری
- `DepositPercentOverride` اختیاری
- `IsBookable`
- `RowVersion`

`RowVersion` برای جلوگیری از بازنویسی هم‌زمان یک بازه توسط دو کاربر استفاده می‌شود.

وضعیت `reserved` در جدول ذخیره نمی‌شود. وضعیت خروجی به شکل زیر محاسبه می‌شود:

- `available`: فعال، قابل رزرو و بدون Booking فعال
- `reserved`: دارای Booking کنسل‌نشده
- `blocked`: غیرفعال یا دارای `IsBookable = false`

با این روش کنسل شدن Booking باعث آزاد شدن خودکار بازه می‌شود و Status ناسازگار ایجاد نمی‌شود.

### تغییرات Bookings

فیلد nullable زیر اضافه شده است:

```text
ScheduleBlockID
```

یک Block می‌تواند در طول تاریخ چند Booking کنسل‌شده داشته باشد، اما منطق Transaction اجازه نمی‌دهد هم‌زمان بیشتر از یک Booking فعال داشته باشد.

### Snapshot در BookingServices

برای اینکه تغییر قیمت‌های آینده روی رزرو قبلی اثر نگذارد، این فیلدها اضافه شده‌اند:

- `StylistServicePriceVariantID`
- `UnitPriceSnapshot`
- `DiscountPercentSnapshot`
- `PriceAfterDiscountSnapshot`
- `DepositPercentSnapshot`
- `DurationMinutesSnapshot`

محاسبه Payment ابتدا Snapshot را می‌خواند و فقط برای رکوردهای قدیمی که Snapshot ندارند از قیمت جاری استفاده می‌کند.

### تغییرات StylistServicePriceVariants

فیلد nullable `BookingTagID` اضافه شده است. بنابراین یک ترکیب طول و حجم می‌تواند قیمت عادی و چند قیمت برچسب‌دار داشته باشد.

دو Unique Index مستقل وجود دارد:

- ترکیب عادی بدون Tag فقط یک بار مجاز است.
- ترکیب دارای Tag برای هر Tag فقط یک بار مجاز است.

## اکشن‌های BookingTag

مسیر پایه: `BookingTag`

- `GetAllBookingTags_Base`
- `GetBookingTagById_Base`
- `AddBookingTag_Base`
- `EditBookingTag_Base`
- `DeleteBookingTag_Base`

برچسبی که در Variant یا ScheduleBlock استفاده شده قابل حذف نیست و باید غیرفعال شود.

نمونه افزودن:

```json
{
  "stylistID": 80,
  "title": "VIP",
  "color": "#C89B3C",
  "sortOrder": 1,
  "isActive": true
}
```

## اکشن‌های StylistScheduleBlock

مسیر پایه: `StylistScheduleBlock`

- `GetPublicScheduleSlots`: خروجی عمومی بازه‌های آزاد
- `GetAllStylistScheduleBlocks_Base`: تقویم مدیریتی با وضعیت‌های آزاد، رزروشده و مسدود
- `GetStylistScheduleBlockById_Base`
- `AddStylistScheduleBlocks_Base`: افزودن گروهی برنامه یک یا چند روز
- `EditStylistScheduleBlock_Base`
- `DeleteStylistScheduleBlock_Base`

افزودن گروهی باعث کاهش تعداد درخواست‌های فرانت‌اند می‌شود و همه بازه‌ها را در یک Transaction ثبت می‌کند.

نمونه افزودن:

```json
{
  "blocks": [
    {
      "stylistID": 80,
      "startDateTime": "2026-10-10T09:00:00",
      "endDateTime": "2026-10-10T10:00:00",
      "serviceManagementID": 10123,
      "stylistServicePriceVariantID": 51,
      "bookingTagID": 3,
      "title": "هایلایت VIP",
      "priceOverride": 5500000,
      "depositPercentOverride": 30,
      "isBookable": true,
      "isActive": true
    }
  ]
}
```

در Edit مقدار `rowVersion` دریافتی از Get باید بدون تغییر ارسال شود.

## اکشن عمومی دریافت Slot

اکشن یکپارچه زیر برای هر سه Mode استفاده می‌شود:

```text
POST Booking/GetPublicBookingSlots
```

نمونه ورودی:

```json
{
  "stylistId": 80,
  "fromDate": "2026-10-10T00:00:00",
  "toDate": "2026-10-16T00:00:00",
  "pageIndex": 1,
  "pageSize": 500,
  "services": [
    {
      "serviceID": 10123,
      "optionValueIDs": [1, 5]
    }
  ]
}
```

در `automatic` و `manual` زمان‌های آزاد از WorkTime تولید می‌شوند. در `manual-schedule` بازه‌های آزاد جدول برنامه دستی برگردانده می‌شوند.

خروجی علاوه بر زمان شامل موارد زیر است:

- `scheduleBlockID`
- `bookingCreationMode`
- `servicePrice`
- `discountPercent`
- `priceAfterDiscount`
- `depositPercent`
- `stylistServicePriceVariantID`
- اطلاعات Tag
- OptionValueها و خلاصه گزینه‌ها

## ثبت Booking از روی برنامه دستی

درخواست عادی `AddBooking_Base` استفاده می‌شود و فقط `scheduleBlockID` به آن اضافه شده است:

```json
{
  "stylistID": 80,
  "customerID": 25,
  "bookingDate": "2026-10-10T09:00:00",
  "bookingTime": "09:00:00",
  "status": "1",
  "isCancelled": false,
  "scheduleBlockID": 120,
  "services": [
    {
      "serviceID": 10123,
      "optionValueIDs": [1, 5]
    }
  ]
}
```

در حالت `manual-schedule` مقدار `scheduleBlockID` الزامی است. تاریخ واقعی Booking از خود Block برداشته می‌شود و مقدار ارسالی کلاینت مرجع نهایی نیست.

## کنترل‌های زمان ثبت

ثبت یا ویرایش Block در این شرایط رد می‌شود:

- زمان پایان قبل یا مساوی شروع باشد.
- زمان شروع گذشته باشد.
- بازه خارج از WorkTime باشد.
- بازه با StylistPacific تداخل داشته باشد.
- بازه با Block دیگر یا RestTime آن تداخل داشته باشد.
- خدمت برای آرایشگر تعریف نشده باشد.
- Variant متعلق به همان آرایشگر و خدمت نباشد.
- Tag متعلق به آرایشگر نباشد.
- Tag بازه با Tag مربوط به Variant متفاوت باشد.

ثبت Booking در این شرایط رد می‌شود:

- Block غیرفعال یا مسدود باشد.
- Block قبلاً Booking فعال داشته باشد.
- آرایشگر، خدمت، Variant یا OptionValueها با Block مطابقت نداشته باشند.
- رزرو با Booking دیگری برای آرایشگر یا مشتری تداخل داشته باشد.
- زمان در WorkTime نباشد یا با مرخصی تداخل داشته باشد.

رزرو Block و ثبت Booking داخل Transaction با Isolation Level برابر `Serializable` انجام می‌شود.

## ترتیب راه‌اندازی

1. Migration را با `dotnet ef database update` یا `Update-Database` اعمال کنید.
2. برای آرایشگر `BookingCreationMode` را روی `manual-schedule` قرار دهید.
3. در صورت نیاز BookingTag بسازید.
4. Tag را به Variantهای ویژه متصل کنید.
5. برنامه روزانه را با `AddStylistScheduleBlocks_Base` ثبت کنید.
6. فرانت‌اند Slotها را فقط از `Booking/GetPublicBookingSlots` دریافت کند.
7. هنگام رزرو `scheduleBlockID` و همان Service/OptionValueهای Slot را ارسال کند.

## Migration

Migration این قابلیت:

```text
20261002212839_AddManualScheduleSystem
```

این Migration داده‌های قبلی Booking را حذف یا بازنویسی نمی‌کند. Snapshotها nullable هستند و Payment برای رزروهای قدیمی همچنان مسیر محاسبه قبلی را دارد.
