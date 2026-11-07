# راهنمای جامع سیستم مالی نوباتیکس

نسخه مستند: ۲۰۲۶/۱۱/۰۵ | مخاطب: مالک محصول، توسعه‌دهنده بک‌اند، توسعه‌دهنده فرانت‌اند و مسئول استقرار

این راهنما از روی کد موجود پروژه Nobatix بازسازی شده است. موضوع آن معماری مالی پلتفرم، اشتراک کسب‌وکار، کارمزد رزرو، اعتبار، صورتحساب، درگاه پرداخت و بازپرداخت است؛ نه راهنمای تمام قابلیت‌های قدیمی سامانه. نام‌های فعلی Worker و Business به‌جای نام‌های تاریخی Stylist و Salon استفاده شده‌اند. مثال‌ها آموزشی‌اند و هیچ رکورد یا مبلغی در دیتابیس ایجاد نمی‌کنند.

## ۱. خلاصه محصول و مرز حساب‌ها

سیستم دو جریان مالی مستقل دارد:

1. مشتری بابت خدمات رزروش به ارائه‌دهنده خدمت پرداخت می‌کند؛ این جریان از Payments و PaymentDetails و PaymentBookings استفاده می‌کند.
2. کسب‌وکار یا متخصص بابت استفاده از پلتفرم اشتراک و کارمزد نوبت می‌پردازد؛ این جریان با Subscription و Billing مدیریت می‌شود.

BillingAccount کیف پول مشتری یا حساب درآمد ارائه‌دهنده خدمت نیست. این حساب فقط بدهی و اعتبار همان پروفایل در برابر پلتفرم را نشان می‌دهد. خرید اشتراک نیز خودبه‌خود شارژ BillingAccount محسوب نمی‌شود.

در همه پرداخت‌های اینترنتی، ابتدا Payment ساخته می‌شود؛ RequestPayment شناسه همان Payment را می‌گیرد؛ تلاش درگاه در PaymentHistory نگهداری می‌شود؛ پس از Verify معتبر، سابقه موفق و اثر مالی مربوط در تراکنش دیتابیس ثبت می‌شوند. فرانت نباید با درج مستقیم سابقه، پرداخت را موفق کند.

### قابلیت‌های موجود

- پلن اشتراک با مدت‌های ۱، ۳، ۶ و ۱۲ ماه و نسخه قرارداد.
- نگهداری قیمت و کارمزد و قرارداد به‌صورت Snapshot در اشتراک خریداری‌شده.
- تعیین پرداخت‌کننده کارمزد: کسب‌وکار یا متخصص، با استثنای اختصاصی متخصص.
- پذیرش مسئولیت کارمزد توسط خود متخصص و تطبیق نسخه قرارداد.
- روش‌های Prepaid، Postpaid و Hybrid و سقف بدهی قابل تنظیم.
- یک کارمزد برای هر رزرو تکمیل‌شده، تخصیص اعتبار، صورتحساب و برگشت کارمزد.
- درخواست و تعیین تکلیف بازپرداخت اعتبار آزاد با رزرو وجه تا تصمیم مدیر.
- گزارش مانده و مغایرت، نگهداری پایدار داده‌های Parbad و پیام‌های زمان‌بندی‌شده.
- خواندن موجودیت‌های مالی از Repository، کنترلر و ViewModel اختصاصی؛ تغییر مالی از فرمان‌های کنترل‌شده Billing.

### مواردی که نباید از این قابلیت نتیجه گرفت

بازپرداخت خودکار بانکی، تسویه خودکار درآمد خدمات، دوره ماهانه شمسی، اعلان لحظه‌ای WebSocket، و تضمین دقیقاً یک‌بار ارسال پیامک جزو پیاده‌سازی حاضر نیستند. تست واقعی درگاه و پیامک باید جداگانه در محیط کنترل‌شده انجام شود.

## ۲. شناسه‌ها، مالکیت و چندپروفایلی

| شناسه | معنی و کاربرد |
| --- | --- |
| PersonID | هویت شخص لاگین‌شده و مبنای مالکیت پروفایل و پرداخت |
| WorkerID | شناسه رکورد پروفایل متخصص یا کسب‌وکار در Workers |
| BusinessID | رکورد کسب‌وکار در همان Workers؛ جدول Business جدید ایجاد نشده است |
| BillingAccountID | حساب مالی پلتفرم برای یک Worker؛ با WorkerID یکی نیست |
| PaymentID | شناسه درخواست پرداخت کلی؛ ورودی RequestPayment |
| PaymentHistoryID | شناسه یک تلاش پرداخت؛ ورودی PayId در callback |
| BillingPaymentID | رکورد شارژ یا پرداخت صورتحساب، متفاوت از PaymentID |
| BusinessSubscriptionID | رکورد اشتراک خریداری‌شده، متفاوت از شناسه پلن |

برای تشخیص کسب‌وکار رزرو، اگر WorkerParentID متخصص بزرگ‌تر از صفر باشد همان والد BusinessID است؛ در غیر این صورت ID خود متخصص استفاده می‌شود. این منطق یک سطح والد مستقیم را در نظر می‌گیرد، نه جست‌وجوی نامحدود تا ریشه.

خرید اشتراک و تنظیمات کسب‌وکار باید برای سالن یا متخصص مستقل انجام شود؛ رکورد فرزند غیرسالن مجاز به خرید اشتراک مستقل از این مسیر نیست. تشخیص نوع حساب هنگام ساخت با IsWorkShop انجام می‌شود: Business برای سالن و Worker برای سایر موارد.

مالکیت بر اساس PersonID رکورد Workers کنترل می‌شود، نه صرفاً نوع پروفایل فعال. گزارش حساب و زمینه مالی، WorkerID=0 را به شناسه پروفایل جاری تبدیل می‌کنند. در فرمان‌هایی که BusinessID یا WorkerID اجباری دارند باید شناسه واقعی ارسال شود؛ این شناسه‌ها از ProfileType قابل حدس نیستند.

مالک سالن به‌صرف والد بودن، مجوز مشاهده حساب مالی خصوصی متخصص را ندارد. مدیر دسترسی گسترده‌تری دارد. تنظیم پرداخت‌کننده توسط صاحب کسب‌وکار، جای رضایت شخصی متخصص را نمی‌گیرد.

## ۳. جداول و دلیل وجود آن‌ها

۱۳ جدول کسب‌وکاری و ۲ جدول داخلی درگاه در این معماری تعریف شده‌اند:

| جدول | دلیل و مسئولیت |
| --- | --- |
| SubscriptionPlans | تعریف پیشنهادهای اشتراک و کارمزد ثابت هر نوبت |
| BusinessSubscriptions | خرید مشخص یک پلن، اتصال به Payment، Snapshot و بازه اعتبار |
| BillingAccounts | هویت حساب پلتفرم برای هر Worker؛ هر Worker یک حساب |
| BusinessBillingSettings | سیاست پرداخت‌کننده، روش وصول، دوره، سقف و فعال‌سازی کسب‌وکار |
| WorkerBillingSettings | استثنای پرداخت‌کننده و رضایت شخصی متخصص به قرارداد |
| BookingPlatformFees | یک کارمزد یکتا برای یک Booking با اطلاعات ثابت زمان ایجاد |
| BillingTransactions | دفتر گردش علامت‌دار؛ مرجع محاسبه خالص مانده |
| BillingInvoices | سربرگ صورتحساب بدهی‌های قبلاً ایجادشده |
| BillingInvoiceItems | اتصال هر کارمزد به صورتحساب و مبلغ باز در زمان صدور |
| BillingPayments | شارژ یا پرداخت صورتحساب و اتصال آن به Payment |
| BillingAllocations | مشخص می‌کند کدام مبلغ پرداخت به کدام کارمزد اختصاص یافته است |
| BillingRefunds | درخواست بازپرداخت، رزرو اعتبار، تصمیم مدیر و مرجع بانکی |
| BillingSettingHistories | تاریخچه BeforeJson و AfterJson تنظیمات با شخص اقدام‌کننده |
| GatewayPaymentRecords | نگهداری پایدار درخواست درگاه برای Parbad |
| GatewayTransactionRecords | ثبت عملیات درگاه مرتبط با GatewayPaymentRecord |

برای ۱۳ جدول اول کنترلر، Interface Repository، پیاده‌سازی Repository و ViewModel وجود دارد. عملیات خواندن از BillingReadRep مشترک استفاده می‌کنند. دو جدول درگاه داخلی‌اند و CRUD عمومی ندارند. فرمان‌های تغییر مالی در BillingRep متمرکزند تا کنترل مالکیت، تراکنش و جلوگیری از ثبت تکراری دور زده نشود.

### روابط اصلی

```text
Workers -> BusinessSubscriptions -> SubscriptionPlans
BusinessSubscriptions -> Payments -> PaymentHistories
Workers -> BillingAccounts -> BillingTransactions
Bookings -> BookingPlatformFees -> BusinessSubscriptions
BillingAccounts -> BookingPlatformFees
BillingAccounts -> BillingInvoices -> BillingInvoiceItems -> BookingPlatformFees
BillingAccounts -> BillingPayments -> Payments
BillingPayments -> BillingAllocations -> BookingPlatformFees
BillingPayments -> BillingRefunds
GatewayPaymentRecords -> GatewayTransactionRecords
```

Payment مشتری می‌تواند چند Booking داشته باشد و Booking هم از طریق PaymentBookings به Payment متصل است. جزئیات خدمات و قیمت‌ها در PaymentDetails نگهداری می‌شوند. پرداخت اشتراک یا شارژ لازم نیست Booking داشته باشد؛ خالی بودن bookings در آن‌ها طبیعی است.

### تغییر در جداول موجود

- Payment: فیلدهای Purpose با پیش‌فرض service، CurrencyCode با پیش‌فرض IRR و OwnerPersonID nullable اضافه شده‌اند.
- PaymentHistory: فیلدهای GatewayRequestState، RequestExpiresAt و GatewayUrl برای چرخه تلاش پرداخت وجود دارند.
- طول GatewayName حداکثر ۶۰ و TransactionCode حداکثر ۲۰۰ است.
- ترکیب GatewayName و TransactionCode برای سابقه موفق verified یکتا است؛ داده تاریخی legacy مشمول این فیلتر خاص نیست.

### یکپارچگی و حذف

رابطه‌های مالی جدید با NoAction تعریف شده‌اند؛ حذف آبشاری اسناد مالی مقصود سیستم نیست. یکتایی‌های مهم شامل BookingID کارمزد، WorkerID حساب، BusinessID تنظیمات، BusinessID+RequestID اشتراک، BillingAccountID+RequestID پرداخت، PaymentID اتصال‌های مالی و BillingPaymentID+RequestID بازپرداخت‌اند.

صورتحساب جدید بدهی جدید ایجاد نمی‌کند. Allocation هم پرداخت جدید نیست. حذف مستقیم رکوردهای دفتر، سوابق درگاه، تخصیص‌ها یا صورتحساب برای اصلاح مانده مجاز نیست؛ از برگشت کارمزد یا بازپرداخت استفاده شود.

## ۴. پلن و چرخه اشتراک

مدیر پلن را با نام، مدت، قیمت، BookingFeeAmount، IRR و ContractVersion ثبت می‌کند. Price باید مثبت و مبلغ صحیح ریالی باشد؛ کارمزد می‌تواند صفر باشد. سرویس فقط مدت‌های 1، 3، 6، 12 را می‌پذیرد، هرچند annotation ورودی بازه ۱ تا ۱۲ دارد.

خرید، ابتدا Payment با purpose=subscription و BusinessSubscription با status=pending می‌سازد. قیمت، عنوان، مدت، کارمزد و قرارداد از پلن کپی می‌شوند؛ تغییر بعدی پلن، خرید قبلی را بازنویسی نمی‌کند. ID پاسخ خرید، PaymentID است؛ نباید آن را BusinessSubscriptionID فرض کرد.

پس از پرداخت کامل و Verify موفق، اشتراک active می‌شود. اگر اشتراک پرداخت‌شده دیگری با پایان آینده موجود باشد، شروع خرید جدید بعد از آخرین پایان قرار می‌گیرد؛ در غیر این صورت از زمان تأیید UTC آغاز می‌شود. پایان با AddMonths مدت Snapshot محاسبه می‌شود.

نکته مهم فرانت: status=active به‌تنهایی به معنی قابل استفاده بودن همین لحظه نیست. خرید آینده‌دار هم می‌تواند active باشد. اعتبار جاری به این شرط است:

```text
status == "active" && startsAt <= utcNow && utcNow < endsAt
```

پردازش دوره‌ای وضعیت اشتراک پایان‌یافته را expired می‌کند. تا اجرای job ممکن است رکورد هنوز active باشد ولی شرط زمانی اجازه استفاده ندهد؛ UI باید زمان‌ها را نیز بررسی کند.

## ۵. تنظیمات و رضایت متخصص

DefaultFeePayer مقدار Business یا Worker دارد. WorkerBillingSetting.FeePayerOverride اگر null یا خالی باشد از تنظیم پیش‌فرض استفاده می‌کند؛ در غیر این صورت استثنای همان متخصص است. مقادیر انتخابی تنظیمات با مقایسه غیرحساس به حروف و Trim به مقدار استاندارد تبدیل می‌شوند؛ فرانت همچنان مقادیر استاندارد را ارسال کند.

اگر متخصص فرزند مسئول کارمزد باشد، باید خود او قرارداد جاری را بپذیرد. سیستم PersonID پذیرنده را با مالک متخصص و AcceptedContractVersion را با ContractVersionSnapshot اشتراک جاری تطبیق می‌دهد. پذیرش از طرف مدیر یا صاحب سالن جای این شرط را نمی‌گیرد.

GetWorkerBillingContext_Base برای ساخت صفحه رضایت طراحی شده: BusinessID، مبلغ کارمزد جاری، نسخه قرارداد، پرداخت‌کننده و نیاز یا وجود رضایت را برمی‌گرداند. فرانت همان contractVersion دریافتی را همراه accept=true بفرستد؛ اگر نسخه عوض شده باشد پذیرش رد می‌شود. accept=false رضایت ثبت‌شده را پس می‌گیرد.

تغییر تنظیمات کسب‌وکار یا متخصص در BillingSettingHistories ثبت می‌شود. تغییر سیاست، Snapshot کارمزدهای قبلی را عوض نمی‌کند. این جدول تاریخچه تنظیمات است، نه تاریخچه همه عملیات مالی پروژه.

## ۶. زمان ایجاد و برگشت کارمزد

BookingRep با سرویس مالی یکپارچه شده است. رزرو صرفاً در زمان ثبت یا تأیید اولیه کارمزد نمی‌سازد. شرط ایجاد:

```text
booking.Status == "4" && booking.IsCancelled == false
```

هنگام افزودن رزرو، وضعیت بررسی می‌شود؛ در ویرایش، تغییر وضعیت یا لغو باعث بررسی مالی می‌شود. ویرایش توضیح یک رزرو قدیمی نباید به‌تنهایی کارمزد تاریخی ایجاد کند.

اگر تنظیم فعال و IsBookingFeeEnabled وجود نداشته باشد، کارمزد جدید ساخته نمی‌شود. اگر وصول فعال باشد ولی اشتراک معتبر، رضایت لازم یا اعتبار کافی وجود نداشته باشد، تغییر وضعیت می‌تواند با خطا برگردد و تراکنش رزرو باید برگردانده شود. فرانت نباید قبل از موفقیت پاسخ، رزرو را قطعی تکمیل‌شده نشان دهد.

مبلغ کارمزد از Snapshot اشتراک جاری گرفته می‌شود؛ نه قیمت خدمات رزرو و نه قیمت جدید پلن. کارمزد مبلغ ثابت هر Booking است، نه درصد قیمت هر خدمت و نه مبلغ جدا برای هر BookingService.

برای Booking فقط یک BookingPlatformFee ساخته می‌شود. اگر نوبت از تکمیل خارج شود یا لغو گردد، کارمزد قبلی reversed و تخصیص‌هایش برگشت‌خورده می‌شوند و گردش منفی اصلاحی ثبت می‌شود. رکورد اصلی حذف نمی‌شود. در پیاده‌سازی فعلی کارمزد reversed برای همان Booking با تکمیل دوباره خودکار مجدداً accrued نمی‌شود؛ این سیاست را در UI و فرایند اصلاح مالی لحاظ کنید.

## ۷. روش وصول، اعتبار و دفتر گردش

| روش | رفتار |
| --- | --- |
| Prepaid | ثبت کارمزد فقط با اعتبار قابل استفاده کافی؛ تخصیص خودکار اعتبار |
| Hybrid | مصرف اعتبار موجود و اجازه بدهکارشدن در محدوده سقف؛ تخصیص خودکار |
| Postpaid | ایجاد بدهی و تسویه از مسیر صورتحساب؛ تخصیص عمومی خودکار به این کارمزدها انجام نمی‌شود |

CreditLimit=0 در منطق فعلی یعنی سقف بدهی اعمال نمی‌شود، نه سقف صفر ریال. مقدار مثبت برای حالت غیرPrepaid محدودیت ایجاد می‌کند. DueDays تعداد روز مهلت از زمان صدور صورتحساب است و بین صفر و ۳۶۵ پذیرفته می‌شود.

علامت گردش‌ها از دید بدهی پروفایل به پلتفرم است:

| TransactionType | علامت | SourceType |
| --- | --- | --- |
| fee | مثبت؛ افزایش بدهی | BookingPlatformFee |
| payment | منفی؛ کاهش بدهی یا افزایش اعتبار | BillingPayment |
| reversal | منفی؛ برگشت کارمزد | BookingPlatformFee |
| refund | مثبت؛ خروج اعتبار بازپرداخت‌شده | BillingRefund |

Allocation از پرداخت‌های تأییدشده به کارمزدها به ترتیب پرداخت و زمان کارمزد انجام می‌شود. مبلغ رزروشده یا بازپرداخت‌شده قابل تخصیص نیست. یک پرداخت می‌تواند چند کارمزد را پوشش دهد و یک کارمزد هم از چند پرداخت تسویه شود.

### فرمول‌های گزارش حساب

```text
NetBalance = Sum(BillingTransactions.Amount)
AmountDue = Max(NetBalance, 0)
AvailableCredit = Max(-NetBalance - ReservedCredit, 0)
OpenFees = TotalFees - ReversedFees - ActiveAllocations
UnallocatedCredit = ConfirmedPayments - ActiveAllocations - RefundedAmount - ReservedCredit
ReconciliationDifference = NetBalance - (TotalFees - ReversedFees - ConfirmedPayments + RefundedAmount)
IsReconciled = ReconciliationDifference == 0
```

TotalFees شامل مبلغ همه کارمزدهای ثبت‌شده، از جمله مواردی است که بعداً reversed شده‌اند؛ ReversedFees جداگانه کسر می‌شود. ReservedCredit مجموع درخواست‌های بازپرداخت pending است. ConfirmedPayments جمع BillingPaymentهای confirmed است، نه قیمت خرید اشتراک یا پرداخت مشتری بابت خدمت.

در Postpaid ممکن است هم OpenFees و هم UnallocatedCredit مثبت باشند؛ این دو شاخص تخصیص را توضیح می‌دهند، نه صرفاً خالص مانده. برای تصمیم درباره بدهی کلی از AmountDue و برای خرج‌کردن از AvailableCredit استفاده کنید.

مثال: شارژ ۲۰٬۰۰۰ ریال و ۸ کارمزد ۳٬۰۰۰ ریالی، بدون برگشت و بازپرداخت، NetBalance=4,000 و AmountDue=4,000 می‌سازد. نمایش بدهی با علامت منفی در UI خلاف قرارداد این گزارش است.

## ۸. صورتحساب و تسویه

InvoiceCycle فقط Weekly یا Monthly است. در پردازش موجود، هفته از دوشنبه UTC و ماه از اول ماه میلادی UTC محاسبه می‌شود؛ روز اجرای پیام‌ها براساس منطقه زمانی ایران است. این دو مفهوم را با دوره شمسی یکسان ندانید.

صدور دستی بازه‌ای با پایان بزرگ‌تر از شروع، پایان‌نیافته‌نبودن بازه و سال شروع حداقل ۲۰۰۰ را کنترل می‌کند. محدوده انتخاب کارمزد start-inclusive و end-exclusive است. کارمزدهای accrued، بدون صورتحساب قبلی و دارای مانده وارد آیتم‌ها می‌شوند. هر کارمزد فقط در یک InvoiceItem مجاز است.

TotalAmount مبلغ آیتم‌های باز در زمان صدور است و لزوماً مانده لحظه‌ای نیست. در GET، RemainingAmount، AllocatedAmount و ReversedAmount محاسبه و اضافه می‌شوند. برای دکمه پرداخت از remainingAmount استفاده کنید، نه totalAmount اولیه.

وضعیت‌ها open، overdue و paid هستند. سررسید گذشته با مانده مثبت overdue می‌شود. برگشت کارمزد یا تخصیص جدید می‌تواند مانده را تغییر دهد؛ بعد از هر فرمان، GET را تازه کنید.

CreateBillingPayment_Base با billingInvoiceID ابتدا اعتبار موجود را به همان صورتحساب تخصیص می‌دهد. اگر کامل تسویه شود، پاسخ status=true و id=0 است و Payment جدیدی ساخته نمی‌شود. در غیر این صورت Payment فقط به مبلغ مانده ساخته می‌شود؛ amount ارسالی فرانت در حالت صورتحساب مبنای محاسبه نهایی نیست.

صدور مجدد برای همان حساب و همان بازه رکورد قبلی را برمی‌گرداند. پردازش job برای کارمزدهای صورتحساب‌نشده قدیمی تا پایان دوره بسته‌شده نیز رسیدگی می‌کند؛ الزاماً برای هر ماه ازدست‌رفته یک سربرگ جدا تولید نمی‌کند.

## ۹. پرداخت و درگاه Parbad

| Purpose | کاربرد Payment |
| --- | --- |
| service | پرداخت خدمات یک یا چند رزرو |
| subscription | خرید یا تمدید اشتراک |
| billing_credit | شارژ حساب پلتفرم |
| billing_invoice | پرداخت مانده صورتحساب |

مالک پرداخت جدید با OwnerPersonID مشخص می‌شود. برای پرداخت خدمات تاریخی که مالک مستقیم ندارند، وجود رزرو و مالکیت مشتری تمام رزروهای مرتبط بررسی می‌شود. صرف دانستن PaymentID مجوز پرداخت یا مشاهده اطلاعات آن نیست.

### درخواست پرداخت

```http
POST /Payment/RequestPayment
Authorization: Bearer <access-token>
Content-Type: application/json
```

```json
{
  "paymentID": 7001
}
```

پاسخ نمونه آموزشی:

```json
{
  "status": true,
  "errorMessage": "",
  "result": {
    "payGatewayUrl": "https://gateway.example/payment/opaque-token",
    "paymentHistoryID": 9001,
    "amount": 3000000
  }
}
```

PaymentHistory پیش از رفتن به درگاه با paymentStatus=false ساخته می‌شود؛ پس وجود رکورد سابقه الزاماً به معنی موفقیت نیست. در صورت موفقیت درخواست، gatewayRequestState=requested و URL ذخیره می‌شود. در صورت شکست failed و پس از Verify موفق verified می‌شود. creating مرحله ساخت درخواست است و legacy برای داده‌های قدیمی است.

برای هر Payment یک تلاش creating/requested غیرمنقضی تا ۱۵ دقیقه جلوی درخواست هم‌زمان جدید را می‌گیرد. UI لینک موجود را حفظ کند و با کلیک تکراری Payment تازه نسازد. زمان انقضای داخلی به معنی تضمین اعتبار URL بانک یا تضمین شکست بانکی نیست.

برای service، اگر هنوز وجهی پرداخت نشده و DepositAmount مثبت باشد، مبلغ تلاش اول کمینه بیعانه و مانده است؛ تلاش بعدی مانده را می‌گیرد. برای اشتراک و Billing، پرداخت کامل مانده لازم است. PayedAmount خدمات قدیمی قبل از درخواست از سوابق موفق دوباره محاسبه می‌شود.

### callback و تأیید

```http
GET /Payment/VerifyPayment?PayId=9001&paymentToken=...&Authority=...&Status=...
```

این endpoint برای برگشت درگاه AllowAnonymous است؛ PayId شناسه PaymentHistory است، نه Payment. پارامترهای واقعی callback وابسته به درگاه‌اند و توسط خود درگاه/Parbad تأمین می‌شوند. ساختن دستی URL بالا و گذاشتن Status=OK پرداخت را موفق نمی‌کند.

سرور Fetch و Verify انجام می‌دهد و مبلغ، TrackingNumber و نام درگاه را با تلاش ثبت‌شده تطبیق می‌دهد. کد تراکنش باید دریافت شود. در تراکنش مالی، PaymentHistory موفق، Payment به‌روز و اثر اشتراک یا Billing اعمال می‌شود. تکرار callback موفق، همان سابقه را برمی‌گرداند و نباید دوباره شارژ یا تمدید ایجاد کند.

پاسخ موفق id=PaymentHistoryID دارد. پاسخ callback فعلاً JSON است و redirect اختصاصی به صفحه فرانت در کد وجود ندارد. برخی شکست‌های اعلام‌شده درگاه HTTP 200 با status=false دارند؛ فرانت صرفاً از HTTP 200 نتیجه موفقیت نگیرد.

پس از تأیید، Payment.PaymentStatus برابر paid یا partially_paid می‌شود و PaymentFinished براساس صفر شدن مانده محاسبه می‌گردد. PaymentHistory.PaymentStatus از نوع bool است؛ این دو فیلد هم‌نام، قرارداد نوع و معنی متفاوت دارند.

### نمایش نتیجه در فرانت

1. PaymentID و PaymentHistoryID را جدا نگهداری کنید.
2. پس از برگشت، نتیجه مورد اعتماد را از API بگیرید، نه query مرورگر.
3. GetPaymentById و لیست PaymentHistory همان Payment را دوباره بخوانید.
4. گزارش حساب، صورتحساب یا اشتراک مرتبط را نیز تازه کنید.
5. در قطع ارتباط یا وضعیت نامشخص، پیام «در حال بررسی» بدهید؛ پرداخت را دستی موفق یا تکراری ایجاد نکنید.

GetAllPayments و GetPaymentById و معادل‌های _Base، Purpose و CurrencyCode و OwnerPersonID را برمی‌گردانند. خدمات دارای bookings و paymentItems هستند؛ اشتراک و شارژ می‌توانند bookings خالی داشته باشند. برای تفکیک نوع صفحه از purpose استفاده کنید؛ برای نوع پرداخت به وجود رزرو اتکا نکنید.

## ۱۰. قرارداد عمومی API و پاسخ‌ها

تمام مسیرهای Billing و GETهای جداول مالی در این سند POST با JSON body هستند، مگر VerifyPayment که GET است. Base URL همان API مستقرشده است؛ پیشوند /api خودکار به مسیرهای ذکرشده اضافه نکنید مگر پراکسی شما چنین مسیری تعریف کرده باشد.

ورود با Bearer لازم است. RequireRole و PermissionCenter و کنترل مالکیت، هر سه می‌توانند روی دسترسی اثر بگذارند. شناسه‌ها در JSON عدد long هستند؛ اگر فرانت در آینده از بازه امن JavaScript عبور کند باید قرارداد تبدیل شناسه را جداگانه هماهنگ کند.

شکل پاسخ فرمان:

```json
{
  "status": true,
  "errorMessage": "",
  "id": 7001
}
```

شکل دریافت تکی و لیستی:

```json
{
  "status": true,
  "errorMessage": "",
  "result": {
    "id": 12
  }
}
```

```json
{
  "status": true,
  "errorMessage": "",
  "totalCount": 1,
  "pageCount": 1,
  "results": [
    {
      "id": 12
    }
  ]
}
```

خطای کنترل‌شده معمولاً HTTP 400 با status=false و errorMessage است. 401 برای احراز هویت و 403 برای دسترسی هم ممکن است. Middleware خطای موجود پروژه می‌تواند logId به پاسخ خطا اضافه کند؛ فرانت آن را برای پشتیبانی نمایش دهد، نه به‌عنوان شناسه Payment. فیلدهای داخلی ErrorOrigin قرار نیست به‌عنوان قرارداد نمایشی مصرف شوند. خطاهای اعتبارسنجی خودکار ApiController نیز ممکن است شکل متفاوتی داشته باشند؛ کلاینت تحمل این تفاوت را داشته باشد.

مبالغ API ریال، صحیح و بدون اعشار هستند. اگر UI تومان نشان می‌دهد تبدیل فقط در نمایش انجام شود و پیش از ارسال به ریال برگردد. مثال‌های زیر قیمت تجاری واقعی محسوب نمی‌شوند.

### وضعیت‌های رشته‌ای و مقدار nullable

| مدل | مقادیر وضعیت فعلی |
| --- | --- |
| BusinessSubscription.Status | pending، active، expired |
| BookingPlatformFee.Status | accrued، reversed |
| BillingInvoice.Status | open، overdue، paid |
| BillingPayment.Status | pending، confirmed |
| BillingRefund.Status | pending، confirmed، rejected |
| Payment.PaymentStatus در مسیر پرداخت جدید | pending، partially_paid، paid؛ داده‌های تاریخی ممکن است مقادیر دیگری داشته باشند |
| PaymentHistory.GatewayRequestState | legacy، creating، requested، failed، verified |

PaymentHistory.PaymentStatus یک bool است. StartsAt و EndsAt اشتراک pending، ConfirmedAt پرداخت pending، ReversedAt تخصیص فعال و مشخصات تعیین تکلیف بازپرداخت pending می‌توانند null باشند. UI باید این موارد را بدون تبدیل null به تاریخ صفر نمایش دهد.

## ۱۱. مرجع فرمان‌های Billing

همه مسیرهای این بخش با /Billing/ شروع می‌شوند. نقش‌های مجاز پیش‌فرض ۲ متخصص، ۳ کسب‌وکار و ۴ مدیر هستند؛ موارد مدیر مشخص شده‌اند. مجوز اکشن و مالکیت مستقل از نقش بررسی می‌شوند.

### ۱۱.۱ SaveSubscriptionPlan_Base — مدیر

```json
{
  "id": 0,
  "name": "اشتراک سه‌ماهه نمونه",
  "durationMonths": 3,
  "price": 3000000,
  "bookingFeeAmount": 3000,
  "currencyCode": "IRR",
  "contractVersion": "2026-01",
  "isActive": true,
  "description": "نمونه آموزشی"
}
```

id=0 درج و id مثبت ویرایش است. پاسخ ID پلن است. name اجباری و حداکثر ۲۰۰، contractVersion اجباری و حداکثر ۱۰۰ است. غیرفعال‌سازی پلن جلوی خرید جدید را می‌گیرد؛ خرید قبلی Snapshot خود را دارد.

### ۱۱.۲ SaveBusinessBillingSettings_Base

```json
{
  "businessID": 10144,
  "defaultFeePayer": "Business",
  "collectionMode": "Hybrid",
  "invoiceCycle": "Monthly",
  "isBookingFeeEnabled": true,
  "creditLimit": 500000,
  "dueDays": 7,
  "isActive": true
}
```

این فرمان تنظیمات همان کسب‌وکار را درج یا ویرایش می‌کند. پاسخ ID تنظیمات است. بعد از ذخیره تنظیمات و زمینه مالی را تازه کنید. برای خاموش‌کردن وصول از isBookingFeeEnabled یا isActive استفاده می‌شود؛ سوابق موجود پاک نمی‌شوند.

### ۱۱.۳ SaveWorkerBillingSettings_Base

```json
{
  "businessID": 10144,
  "workerID": 10145,
  "feePayerOverride": "Worker"
}
```

پاسخ ID تنظیم متخصص است. مقدار null یعنی ارث‌بری از پیش‌فرض کسب‌وکار؛ Business یا Worker استثنا ایجاد می‌کند. عضویت متخصص در کسب‌وکار کنترل می‌شود. این فرمان رضایت ثبت نمی‌کند.

### ۱۱.۴ GetWorkerBillingContext_Base

```json
{
  "workerID": 10145
}
```

WorkerID=0 یعنی پروفایل فعال. پاسخ result شامل workerID، businessID، isBookingFeeEnabled، hasActiveSubscription، bookingFeeAmount، currencyCode، contractVersion، feePayer، collectionMode، requiresWorkerConsent و hasWorkerConsent است. نتیجه برای همان پروفایل مجاز به خواندن است؛ مدیر می‌تواند زمینه پروفایل‌های دیگر را بخواند.

### ۱۱.۵ AcceptWorkerBillingLiability_Base

```json
{
  "businessID": 10144,
  "workerID": 10145,
  "accept": true,
  "contractVersion": "2026-01"
}
```

همان نسخه واقعی زمینه مالی را بفرستید، نه مقدار ثابت مثال. پاسخ ID تنظیم متخصص است. پیش از تأیید، مبلغ و طرف پرداخت‌کننده و متن قرارداد را به کاربر نشان دهید. API نسخه را نگه می‌دارد؛ متن کامل حقوقی قرارداد در این مدل ذخیره نشده است و باید از منبع محتوای مورد توافق محصول تأمین شود.

### ۱۱.۶ PurchaseSubscription_Base

```json
{
  "businessID": 10144,
  "subscriptionPlanID": 12,
  "requestID": "6fd322ee-3883-4bbd-bebe-6203c8d95956"
}
```

پاسخ ID، شناسه Payment است. سپس RequestPayment را فراخوانی کنید. requestID باید UUID غیرخالی باشد. برای retry همان عملیات، UUID قبلی را بفرستید؛ برای خرید واقعاً جدید UUID جدید بسازید. استفاده همان UUID با پلن دیگر رد می‌شود.

### ۱۱.۷ GetBillingAccountReport_Base

```json
{
  "workerID": 10144
}
```

WorkerID=0 پروفایل فعال است. اگر حساب هنوز ساخته نشده باشد، این گزارش آن را به‌صورت کنترل‌شده ایجاد می‌کند؛ پس پیش از شارژ برای گرفتن billingAccountID مناسب است. نام تمام فیلدهای گزارش و فرمول‌ها در بخش ۷ آمده و تعریف نوع آن‌ها در پیوست موجود است.

### ۱۱.۸ CreateBillingPayment_Base — شارژ اعتبار

```json
{
  "billingAccountID": 21,
  "billingInvoiceID": null,
  "amount": 200000,
  "requestID": "a423d831-5703-42c7-9713-25e6aef00aa4"
}
```

در حالت شارژ amount باید صحیح و مثبت باشد. پاسخ ID=PaymentID است. سپس RequestPayment انجام می‌شود. صرف ایجاد Payment اعتبار حساب را زیاد نمی‌کند. همان UUID با مبلغ دیگر معتبر نیست.

### ۱۱.۹ CreateBillingPayment_Base — پرداخت صورتحساب

```json
{
  "billingAccountID": 21,
  "billingInvoiceID": 31,
  "amount": 0,
  "requestID": "ae7d3b9b-7acc-48b1-8989-7fcd70634153"
}
```

مانده از سرور محاسبه می‌شود. اگر id>0 به RequestPayment بروید. اگر status=true و id=0 بود، با اعتبار موجود تسویه شده و نباید درگاه صدا زده شود. GET صورتحساب و حساب را به‌روز کنید.

### ۱۱.۱۰ IssueBillingInvoice_Base

```json
{
  "billingAccountID": 21,
  "periodStart": "2026-10-01T00:00:00Z",
  "periodEnd": "2026-11-01T00:00:00Z"
}
```

پاسخ ID صورتحساب است. بازه بدون آیتم قابل صدور ممکن است ID=0 بدهد؛ در این حالت سند جدیدی برای نمایش وجود ندارد. بازه باید بسته شده باشد. نمونه تاریخ باید با زمان واقعی محیط تطبیق داده شود.

### ۱۱.۱۱ ReverseBookingPlatformFee_Base — مدیر

```json
{
  "bookingPlatformFeeID": 41,
  "reason": "اصلاح مالی با تأیید مدیر"
}
```

reason اجباری و حداکثر ۱۰۰۰ است. پاسخ ID کارمزد است. عمل برگشت تکراری کارمزد را دوباره از دفتر کسر نمی‌کند. این عملیات بازپرداخت بانکی یا حذف رزرو نیست.

### ۱۱.۱۲ RunBillingNow_Base — مدیر

بدون مدل ورودی. پردازش دوره‌ای دیتابیس را همین حالا اجرا می‌کند. ID پاسخ تعداد تنظیمات پردازش‌شده است؛ شناسه سند نیست. این اکشن به‌تنهایی مسیر ارسال پیامک JobManager را اجرا نمی‌کند. برای تست پیام‌ها، خود job در Hangfire اجرا شود.

### ۱۱.۱۳ GetBillingReconciliationReport_Base — مدیر

بدون مدل ورودی. پاسخ ListResultObject از BillingReconciliationIssue است. issueTypeهای فعلی:

- gateway_verified_not_posted: داده درگاه موفق ولی سابقه مالی نهایی نشده است.
- payment_total_mismatch: جمع سوابق موفق با PayedAmount پرداخت فرق دارد.
- missing_billing_payment_ledger: BillingPayment تأیید شده ولی گردش payment ندارد.
- payment_over_allocated: تخصیص فعال از مبلغ BillingPayment بیشتر است.

گزارش مغایرت ابزار تشخیص است، نه اصلاح خودکار و نه حسابرسی کامل تمام ناسازگاری‌های ممکن. انتظار و مقدار واقعی را همراه شناسه‌ها به مدیر نشان دهید.

### ۱۱.۱۴ RequestBillingRefund_Base

```json
{
  "billingPaymentID": 51,
  "amount": 50000,
  "requestID": "efba0bf0-0506-41be-af3c-dbc786040aa1",
  "reason": "درخواست بازپرداخت اعتبار استفاده‌نشده"
}
```

پاسخ ID بازپرداخت است. BillingPayment باید confirmed باشد و مبلغ از اعتبار آزاد قابل بازپرداخت بیشتر نباشد. اعتبار در pending رزرو می‌شود و دیگر قابل مصرف نیست. این فرمان مربوط به اعتبار Billing است، نه بازپرداخت هزینه رزرو مشتری یا قیمت اشتراک.

### ۱۱.۱۵ ResolveBillingRefund_Base — مدیر

```json
{
  "billingRefundID": 61,
  "confirm": true,
  "bankReference": "REAL-BANK-REFERENCE",
  "reason": "وجه خارج از سامانه به حساب صاحب اعتبار برگشت داده شد"
}
```

ابتدا بازپرداخت واقعی را از مسیر بانکی انجام دهید، سپس مرجع واقعی را ثبت کنید. API انتقال وجه نمی‌کند. برای confirm=true کد بانکی اجباری و در درخواست‌های confirmed یکتا است. برای رد، confirm=false و reason معنادار لازم است؛ اعتبار رزروشده آزاد می‌شود. تعیین تکلیف معکوس پس از نهایی‌شدن رد می‌شود. پاسخ ID بازپرداخت است.

## ۱۲. مرجع همه اکشن‌های خواندن

همه مسیرهای جدول زیر POST و دارای نقش‌های 2،3،4 و کنترل مالکیت هستند. ورودی لیست GetBillingListRequestBody و ورودی تکی {"id": عدد} است. پاسخ‌ها به ترتیب ListResultObject و RowResultObject از VM همان موجودیت‌اند.

| کنترلر | اکشن لیست | اکشن تکی |
| --- | --- | --- |
| SubscriptionPlan | GetAllSubscriptionPlans_Base | GetSubscriptionPlanById_Base |
| BusinessSubscription | GetAllBusinessSubscriptions_Base | GetBusinessSubscriptionById_Base |
| BillingAccount | GetAllBillingAccounts_Base | GetBillingAccountById_Base |
| BusinessBillingSetting | GetAllBusinessBillingSettings_Base | GetBusinessBillingSettingById_Base |
| WorkerBillingSetting | GetAllWorkerBillingSettings_Base | GetWorkerBillingSettingById_Base |
| BookingPlatformFee | GetAllBookingPlatformFees_Base | GetBookingPlatformFeeById_Base |
| BillingTransaction | GetAllBillingTransactions_Base | GetBillingTransactionById_Base |
| BillingInvoice | GetAllBillingInvoices_Base | GetBillingInvoiceById_Base |
| BillingInvoiceItem | GetAllBillingInvoiceItems_Base | GetBillingInvoiceItemById_Base |
| BillingPayment | GetAllBillingPayments_Base | GetBillingPaymentById_Base |
| BillingAllocation | GetAllBillingAllocations_Base | GetBillingAllocationById_Base |
| BillingRefund | GetAllBillingRefunds_Base | GetBillingRefundById_Base |
| BillingSettingHistory | GetAllBillingSettingHistories_Base | GetBillingSettingHistoryById_Base |

نمونه مسیر کامل: /BillingInvoice/GetAllBillingInvoices_Base.

### مدل مشترک لیست

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "sortQuery": "createDate-desc,id-desc",
  "searchText": "",
  "billingAccountID": 0,
  "businessID": 0,
  "workerID": 0,
  "paymentID": 0,
  "bookingID": 0,
  "billingInvoiceID": 0,
  "billingPaymentID": 0,
  "status": null,
  "fromDate": null,
  "toDate": null
}
```

pageIndex از ۱، pageSize بین صفر تا ۵۰۰؛ صفر یعنی همه رکوردهای مجاز بدون صفحه‌بندی. فیلتر شناسه با صفر اعمال نمی‌شود. شناسه مثبت فقط وقتی مجاز است که همان فیلد مستقیماً در مدل مقصد وجود داشته باشد؛ فیلترها join خودکار ایجاد نمی‌کنند.

مثلاً برای InvoiceItem از billingInvoiceID و برای Allocation از billingPaymentID استفاده کنید؛ billingAccountID در این دو مدل فیلد مستقیم نیست و ارسال مثبت آن خطا می‌دهد. برای Subscription فیلتر businessID و paymentID موجود است، نه subscriptionPlanID در مدل مشترک فیلتر. برای فیلتر دلخواهی که در قرارداد نیست، قابلیت جدید لازم است.

status فقط برای موجودیت دارای Status قابل استفاده است و مقایسه مستقیم دارد؛ مقادیر استاندارد همان سند را ارسال کنید. fromDate و toDate بر CreateDate اعمال می‌شوند، نه تاریخ رزرو، سررسید یا AccruedAt؛ toDate انحصاری است. searchText فقط Description را جست‌وجو می‌کند، نه نام پلن یا نام متخصص.

SortQuery به SortBy فعلی پروژه می‌رود و محدود به چند ستون ثابت نشده است؛ از نام فیلد ذخیره‌شده معتبر همان موجودیت استفاده کنید. RemainingAmount و AllocatedAmount و ReversedAmount محاسباتی‌اند و مرتب‌سازی SQL مستقیم روی آن‌ها پشتیبانی نمی‌شود. برای ورودی اشتباه، احتمال پاسخ خطا وجود دارد.

### مسیرهای مرتبط Payment و PaymentHistory

| مسیر | ورودی اصلی |
| --- | --- |
| /Payment/GetAllPayments یا GetAllPayments_Base | مدل لیست پرداخت |
| /Payment/GetPaymentById یا GetPaymentById_Base | id مربوط به Payment |
| /Payment/RequestPayment | paymentID |
| /Payment/VerifyPayment | GET با PayId مربوط به PaymentHistory و داده درگاه |
| /PaymentHistory/GetAllPaymentHistories_Base | paymentId، bookingId و صفحه‌بندی |
| /PaymentHistory/GetPaymentHistoryById_Base | id مربوط به PaymentHistory |

برای گرفتن پرداخت‌های جاری:

```json
{
  "pageIndex": 1,
  "pageSize": 20,
  "searchText": "",
  "sortQuery": "createDate-desc",
  "customerId": null,
  "bookingId": 0,
  "paymentIncludes": 0,
  "paymentStatus": "",
  "paymentLevel": 0,
  "fromDate": null,
  "toDate": null
}
```

paymentIncludes=0 فیلتر PaymentFinished=false، مقدار 1 فیلتر true و سایر مقادیر در منطق فعلی بدون این فیلتر هستند. این قرارداد قدیمی را با purpose اشتباه نگیرید؛ فیلتر purpose در مدل فعلی لیست پرداخت وجود ندارد. نقش غیرمدیر از customerId ارسالی برای دسترسی به مشتری دیگر استفاده نمی‌کند و دامنه مالکیت خودش اعمال می‌شود.

اکشن‌های Add/Edit/Delete PaymentHistory برای درج دستی موفقیت مالی قابل استفاده نیستند و پاسخ خطای کنترل‌شده می‌دهند. همچنین Payment مالی جدید یا پرداختی که سابقه تلاش درگاه/پرداخت دارد، از مسیر ویرایش عمومی قابل بازنویسی دلخواه نیست.

## ۱۳. راهنمای اجرایی فرانت‌اند

### صفحه مالی پروفایل

با انتخاب پروفایل، GetBillingAccountReport_Base و GetWorkerBillingContext_Base را بخوانید. کارت‌های بدهی، اعتبار قابل استفاده و اعتبار رزروشده را جدا نمایش دهید. هنگام SwitchProfile داده‌های cache را با WorkerID/BusinessID تفکیک یا پاک کنید؛ یک پروفایل دیگر همان شخص لزوماً همان BillingAccount را ندارد.

### صفحه خرید اشتراک

پلن‌های قابل مشاهده را بگیرید، قیمت اشتراک و کارمزد هر نوبت و نسخه قرارداد را نشان دهید. بعد از انتخاب، یک UUID برای همان اقدام بسازید؛ PurchaseSubscription و سپس RequestPayment را اجرا کنید. تا Verify موفق، اشتراک را قابل استفاده نشان ندهید. تمدید صف‌شده را با startsAt آینده نمایش دهید.

### صفحه تنظیمات سالن

تنظیمات کسب‌وکار، متخصص‌ها و استثناهای پرداخت‌کننده را جدا نمایش دهید. حالت «پیروی از تنظیم سالن» معادل feePayerOverride=null است. کنار متخصص مسئول پرداخت، وضعیت رضایت و نسخه قرارداد را نشان دهید. تغییر تنظیمات سالن نباید UI را به این برداشت برساند که همه متخصص‌ها قرارداد را پذیرفته‌اند.

### صفحه شارژ

billingAccountID را از گزارش بگیرید؛ مبلغ ریالی معتبر، UUID، CreateBillingPayment بدون invoice و سپس RequestPayment. بعد از بازگشت، گزارش و BillingPayment را تازه کنید. «درخواست شارژ ایجاد شد» با «حساب شارژ شد» دو حالت متفاوت‌اند.

### صفحه صورتحساب

لیست صورتحساب حساب را با status و remainingAmount نشان دهید. برای جزئیات، InvoiceItemها را با invoiceID بخوانید و در صورت نیاز کارمزد و رزرو را جدا واکشی کنید؛ GET سربرگ لیست nested همه آیتم‌ها را خودکار اضافه نمی‌کند. پرداخت را با CreateBillingPayment همراه invoiceID آغاز کنید و شاخه id=0 را حتماً پوشش دهید.

### صفحه گردش و کارمزد

BillingTransaction برای گردش و BookingPlatformFee برای علت بدهی است. مبلغ گردش را با معنی علامت نمایش دهید. برای ردیابی مصرف پرداخت به Allocation مراجعه کنید؛ جمع‌زدن گردش و کارمزد با هم، دوباره‌شماری ایجاد می‌کند. کارمزد reversed را مخفی نکنید؛ با برچسب برگشت‌خورده نشان دهید.

### صفحه بازپرداخت

ابتدا BillingPaymentهای confirmed را نمایش دهید و ID واقعی آن را به فرم بدهید. مبلغ آزاد را سرور نهایی کنترل می‌کند؛ گزارش کلی به‌تنهایی تضمین آزادبودن همان پرداخت مشخص نیست. درخواست pending، confirmed و rejected را جدا نشان دهید. پنل مدیر باید ثبت کد واقعی بانک یا دلیل رد را اجباری کند.

### خطا، retry و کش

- همه کلیدهای کش مالی باید شناسه پروفایل یا حساب داشته باشند.
- در retry خرید/شارژ/درخواست بازپرداخت همان requestID قبلی استفاده شود.
- دکمه فرمان در زمان درخواست غیرفعال شود، اما محافظ اصلی سرور است.
- errorMessage و logId احتمالی به کاربر/پشتیبانی ارائه شود؛ جزئیات خام دیتابیس در UI بازنشر گسترده نشود.
- بعد از هر فرمان مالی، پاسخ GET مرجع به‌روز شود؛ صرفاً مانده را روی کلاینت کم و زیاد نکنید.
- لغو یا بستن مرورگر اثبات شکست پرداخت نیست؛ قبل از ساخت اقدام جدید، سابقه بررسی شود.

## ۱۴. پیام‌ها، Settings و Hangfire

JobManager.ProcessPlatformBilling هر روز ساعت ۰۸:۰۰ با منطقه زمانی ایران، jobID=process-platform-billing اجرا می‌شود. DisableConcurrentExecution با زمان ۶۰۰ ثانیه نیز روی آن هست. پردازش مالی دیتابیس سپس ارسال اعلان صورتحساب باز و یادآوری پایان اشتراک انجام می‌شود.

قالب از Settings با GetSettingRowAsync(0,key) خوانده می‌شود. تنها در صورت موفق بودن خواندن، IsActive=true و Value غیرخالی ارسال انجام می‌شود. جایگذاری از MakeMessageOnPattern فعلی پروژه است و نام متغیرها باید عیناً درست باشد.

| Setting key | متغیرهای قابل جایگذاری |
| --- | --- |
| BillingInvoiceMessage | #workername، #invoiceid، #amount، #currency، #duedate |
| SubscriptionExpiryMessage | #workername، #planname، #enddate |

قالب پیشنهادی صورتحساب:

```text
#workername عزیز، مانده صورتحساب شماره #invoiceid برابر #amount #currency است. سررسید: #duedate
```

قالب پیشنهادی پایان اشتراک:

```text
#workername عزیز، اشتراک #planname شما در تاریخ #enddate پایان می‌یابد. برای تمدید به پنل مراجعه کنید.
```

تاریخ‌های متن با ToShamsiString ساخته می‌شوند. پیام برای Person صاحب Worker حساب یا کسب‌وکار ارسال می‌شود، نه مشتری رزرو. صورت‌حساب پرداخت‌نشده با مانده مثبت، روزانه یادآوری می‌شود و کلید جلوگیری از تکرار آن invoice+روز UTC است؛ یادآوری اشتراک در بازه هفت روز آخر و با کلید مخصوص همان اشتراک است.

نتیجه ارسال در SMSMessages و متن در Notifications ثبت می‌شود. شکست ارسال باعث exception و امکان retry Hangfire می‌شود. جلوگیری از تکرار بر اساس سابقه ارسال موفق است؛ در فاصله ارسال خارجی و ذخیره دیتابیس، خرابی می‌تواند باعث تکرار پیام شود. بنابراین این مسیر تضمین exactly-once پیامک ندارد.

برای تست فوری مالی فقط RunBillingNow_Base را بزنید. برای تست همراه پیامک، در Dashboard Hangfire همان recurring job را Trigger کنید؛ ابتدا شماره مقصد، فعال‌بودن template، مانده واقعی صورتحساب یا تاریخ پایان اشتراک و تنظیمات ارائه‌دهنده پیامک را کنترل کنید. اجرای آزمایشی روی داده واقعی ممکن است پیام واقعی بفرستد.

تعریف recurring job به‌تنهایی کافی نیست؛ سرویس Hangfire Server باید در حال اجرا باشد و هاست متوقف یا sleep نباشد. دسترسی production به /hangfire باید محدود به مدیر باقی بماند.

## ۱۵. پرمیشن‌ها و نقش‌ها

RoleIDهای فعلی: Customer=1، Worker=2، WorkShop=3، Admin=4. فرمان‌های Billing و GETهای جداول مالی برای 2/3/4 هستند؛ فرمان پلن، برگشت دستی کارمزد، اجرای دستی، مغایرت و تعیین تکلیف بازپرداخت مدیرمحورند. Payment GET و RequestPayment و خواندن PaymentHistory برای 1/2/3/4 تعریف شده‌اند و مالکیت نیز اعمال می‌شود.

PermissionKeyهای اکشن‌ها با الگوی controller.action کوچک نوشته می‌شوند؛ مثلاً billing.purchasesubscription_base و billinginvoice.getallbillinginvoices_base. واردکردن فایل JSON مجوزها در سازوکار فعلی پروژه می‌تواند تعریف اکشن‌ها را تأمین کند؛ نسبت نقش/کاربر و cache دسترسی هم باید با همان سازوکار پروژه هماهنگ باشد. درج JSON به‌تنهایی اثبات نمی‌کند هر نقش به آن‌ها دسترسی دارد.

در نسخه فعلی PermissionBootstrapper از کامپایل کنار گذاشته شده و فراخوانی startup آن کامنت است. ثبت خودکار جدیدی توسط این راهنما اضافه نشده است. طبق اجرای قبلی پیاده‌سازی، مجوزها و ارتباط نقش‌های مربوط در دیتابیس اصلی ثبت شده بودند؛ برگرداندن کد Bootstrapper آن رکوردها را پاک نکرد. در این بازسازی وضعیت زنده جداول مجوز دوباره استعلام نشده است.

مجوزهای اپلیکیشن با GRANT مجوز SQL روی schema/table متفاوت‌اند. جابه‌جایی جدول به dbo جای PermissionCenter را نمی‌گیرد و نیازهای SQL کاربر اتصال نیز جدا بررسی می‌شود.

## ۱۶. پیاده‌سازی فنی و مدیریت خطا

| فایل یا ناحیه | مسئولیت |
| --- | --- |
| NobatPlusDATA/Domain/Billing.cs | موجودیت‌ها، PaymentPurposes و مدل گزارش |
| NobatPlusDATA/Domain/GatewayStorage.cs | مدل ذخیره‌سازی داخلی درگاه |
| NobatPlusDATA/DataLayer/BillingModelConfiguration.cs | روابط، ایندکس‌ها و محدودیت‌ها |
| NobatPlusDATA/DataLayer/Services/BillingRep.cs | فرمان‌های تراکنشی و قواعد مالی |
| NobatPlusDATA/DataLayer/Services/BillingReadRep.cs | خواندن scoped، فیلتر و صفحه‌بندی |
| NobatPlusAPI/Models/Billing/BillingRequestBodies.cs | مدل‌های ورودی فرمان‌ها |
| NobatPlusAPI/ViewModels/Billing | قرارداد خروجی جداول مالی |
| NobatPlusAPI/Controllers/BillingController.cs | اکشن‌های فرمان و گزارش |
| NobatPlusAPI/Tools/DatabaseGatewayStorage.cs | پیاده‌سازی IStorage پایدار Parbad |
| NobatPlusAPI/Tools/JobManager.cs | پردازش زمان‌بندی و ارسال پیام |
| NobatPlusAPI/Tools/MappingProfile.cs | نگاشت مدل‌ها و ViewModelها |
| NobatPlusAPI/Program.cs | DI، درگاه پایدار و زمان‌بندی |

نوشتن مالی در WriteAsync زیر تراکنش Serializable و قفل دیتابیسی sp_getapplock با نام Nobatix:Billing انجام می‌شود. قفل، transaction-owned و مهلت انتظار آن ۱۵ ثانیه است. این قفل بین چند نمونه API و job نیز مشترک است؛ درخواست شبکه درگاه خارج از قفل انجام می‌شود.

اگر تراکنش موجود رزرو فعال باشد، سرویس از همان استفاده می‌کند. خطاهای کنترل‌شده به Status=false و ErrorMessage تبدیل می‌شوند؛ در خطا ChangeTracker پاک می‌شود تا تغییر برگشت‌خورده هنگام ذخیره بعدی ناخواسته باقی نماند. نتیجه ناموفق مالی در مسیر رزرو باید موجب rollback رزرو شود.

Getها نیز خطا را در ResultObject برمی‌گردانند. Middleware فعلی پروژه برای ثبت خطا و logId استفاده می‌شود. هیچ Middleware قادر به تضمین ذخیره خطا هنگام قطع کامل دیتابیس یا خرابی فرایند نیست؛ مراقبت عملیاتی و لاگ مستقل میزبان همچنان لازم است.

داده‌های Parbad دیگر صرفاً MemoryCache نیستند؛ Program از DatabaseGatewayStorage با lifetime Scoped استفاده می‌کند. پس restart برنامه نباید به‌تنهایی سوابق درگاه را از حافظه پاک کند. این موضوع جای backup دیتابیس یا کنترل مغایرت را نمی‌گیرد.

## ۱۷. راه‌اندازی، دیتابیس و وضعیت فعلی

این بازسازی مستندات هیچ migration اجرا نکرد، schema جابه‌جا نکرد، مبلغ یا مجوز جدید ثبت نکرد و به درگاه یا پیامک واقعی وصل نشد. توصیف جدول‌ها از مدل فعلی کد است؛ برای ادعای یکسان‌بودن دیتابیس زنده با کد باید بررسی مستقل انجام شود.

در اجرای قبلی، migrationهای AddPlatformBillingArchitecture و CompleteBillingRefundsAndConsent با موفقیت روی دیتابیس اصلی اعمال شده بودند. اما در فایل‌های فعلی پوشه NobatPlusDATA/Migrations فقط NobatPlusContextModelSnapshot.cs موجود است؛ فایل‌های migration قبلی در این بازسازی وجود ندارند. در نتیجه اجرای پروژه روی دیتابیس خالی یا اتکا به update-database برای بازسازی کامل، بدون بازیابی تاریخچه migration قابل تضمین نیست. فایل‌های حذف‌شده را از Git/backup بازیابی یا برنامه baseline کنترل‌شده تهیه کنید؛ این راهنما migration جایگزین نمی‌سازد.

نگاشت‌های مالی فعلی schema صریح dbo تعیین نکرده‌اند. اگر جدول‌های جدید هنوز در schema پیش‌فرض کاربر SQL باشند یا دستی به dbo منتقل شوند، تطابق mapping، default schema کاربر، تاریخچه migration و SQL permission باید با دیتابیس واقعی کنترل شود. کوئری انتقال schema که قبلاً ارائه شده بود در این عملیات اجرا نشده و اجرای آن توسط کاربر مفروض نیست.

### چک‌لیست پیش از انتشار

1. backup قابل بازیابی از دیتابیس تهیه و schema و ایندکس‌ها را کنترل کنید.
2. تاریخچه migration را با سورس و __EFMigrationsHistory همسان کنید.
3. connection string را در محل امن و بدون commit نگهداری کنید.
4. درگاه فعال، MerchantId واقعی و sandbox/production درست تنظیم شوند.
5. HTTPS و callback عمومی قابل دسترس از درگاه باشند؛ Host و Scheme پشت proxy درست تنظیم شوند.
6. Hangfire storage و Server و timezone ایران و دسترسی امن Dashboard کنترل شوند.
7. پرمیشن‌ها و ارتباط نقش‌ها و کنترل مالکیت با کاربران واقعی تست شوند.
8. پلن فعال و تنظیم کسب‌وکار و رضایت لازم ثبت شوند؛ قیمت‌های مثال را وارد production نکنید.
9. Settings پیام‌ها فقط پس از تأیید قالب، شماره و هزینه ارسال فعال شوند.
10. یک چرخه واقعی کم‌ریسک درگاه و برگشت و قطع ارتباط، با مجوز مسئول مالی تست شود.

## ۱۸. سناریوهای پذیرش و عیب‌یابی

| آزمون | انتظار |
| --- | --- |
| خرید تکراری با همان UUID | همان Payment بدون اشتراک تکراری |
| همان UUID با پلن دیگر | خطای درخواست ناسازگار |
| درخواست درگاه هم‌زمان | جلوگیری از تلاش فعال تکراری |
| callback موفق تکراری | بدون ثبت شارژ یا تمدید دوباره |
| جعل Status callback | بدون تأیید صرفاً با پارامتر ورودی |
| تمدید پیش از پایان | شروع پس از آخرین اشتراک پرداخت‌شده |
| تکمیل رزرو بدون اشتراک در وصول فعال | خطا و عدم نهایی‌شدن تغییر مالی/رزرو |
| Prepaid بدون اعتبار | خطای اعتبار کافی |
| مسئولیت متخصص بدون رضایت نسخه جاری | خطای قرارداد |
| صدور صورتحساب | بدون گردش بدهی دوباره |
| صورتحساب پوشش‌داده‌شده با اعتبار | موفقیت با ID=0 و بدون درگاه |
| برگشت کارمزد | یک reversal و آزادشدن تخصیص‌ها |
| بازپرداخت pending | رزرو و عدم مصرف همان وجه |
| رد بازپرداخت | آزادشدن رزرو بدون refund ledger |
| تأیید بازپرداخت | مرجع واقعی، گردش مثبت و وضعیت confirmed |
| دسترسی حساب متخصص دیگر | رد حتی برای والد سالن بدون مالکیت مجاز |
| Setting غیرفعال | عدم ارسال همان پیام |

اگر اعتبار پس از بازگشت زیاد نشده، ابتدا PaymentHistory.paymentStatus و gatewayRequestState، سپس BillingPayment.status و گردش payment را بررسی کنید. اگر اشتراک دیده می‌شود ولی قابل استفاده نیست، بازه startsAt/endsAt و BusinessID و Snapshot را بررسی کنید. اگر مانده صورتحساب با مبلغ سربرگ فرق دارد، از remainingAmount و reversal/allocation استفاده کنید. برای 403، نقش فعال، PermissionCenter، ارتباط نقش، PersonID مالک و cache مجوز بررسی شود.

این راهنما بر اساس بازخوانی سورس تهیه شده است؛ بازسازی مستندات به معنی اجرای دوباره همه تست‌ها نیست. پیش از انتشار، سناریوهای بالا در محیط تست تکرار شوند. تست درگاه خارجی و پیامک واقعی باید جداگانه ثبت و تأیید شوند.

## ۱۹. پیوست قرارداد فیلدها

پیوست بعدی از مدل‌های فعلی پروژه استخراج می‌شود و برای جلوگیری از جاماندن فیلدها همراه این راهنما نگهداری می‌شود. C# nullable مانند long? و DateTime? یعنی امکان null؛ فیلد مرجع بدون ? را صرفاً از روی آن به‌عنوان تضمین عدم null در همه داده‌های تاریخی تلقی نکنید. API به‌صورت camelCase سریال می‌شود؛ نام‌های C# در پیوست با نوشتار اصلی آمده‌اند.

تمام VMهای مالی از BaseEntity ارث می‌برند: ID، CreateDate nullable، UpdateDate nullable، Description nullable و IsActive. در پیوست، نمونه رکوردها و ورودی‌ها عیناً به قرارداد موجود متصل‌اند؛ قابلیت ناموجود از متن استنتاج نشود.


### ۱۹.۱ فیلدهای کامل موجودیت‌ها و گزارش‌های مالی

#### SubscriptionPlan

علاوه بر فیلدهای مشترک BaseEntity.

| فیلد | نوع C# | مقدار پیش‌فرض صریح |
| --- | --- | --- |
| Name | string | "" |
| DurationMonths | int | — |
| Price | decimal | — |
| BookingFeeAmount | decimal | — |
| CurrencyCode | string | "IRR" |
| ContractVersion | string | "1" |

#### BusinessSubscription

علاوه بر فیلدهای مشترک BaseEntity.

| فیلد | نوع C# | مقدار پیش‌فرض صریح |
| --- | --- | --- |
| BusinessID | long | — |
| SubscriptionPlanID | long | — |
| PaymentID | long | — |
| RequestID | Guid | — |
| PlanNameSnapshot | string | "" |
| DurationMonthsSnapshot | int | — |
| PriceSnapshot | decimal | — |
| BookingFeeAmountSnapshot | decimal | — |
| CurrencyCode | string | "IRR" |
| ContractVersionSnapshot | string | "" |
| Status | string | "pending" |
| StartsAt | DateTime? | — |
| EndsAt | DateTime? | — |

#### BillingAccount

علاوه بر فیلدهای مشترک BaseEntity.

| فیلد | نوع C# | مقدار پیش‌فرض صریح |
| --- | --- | --- |
| WorkerID | long | — |
| OwnerType | string | "Worker" |
| CurrencyCode | string | "IRR" |

#### BusinessBillingSetting

علاوه بر فیلدهای مشترک BaseEntity.

| فیلد | نوع C# | مقدار پیش‌فرض صریح |
| --- | --- | --- |
| BusinessID | long | — |
| DefaultFeePayer | string | "Business" |
| CollectionMode | string | "Hybrid" |
| InvoiceCycle | string | "Monthly" |
| IsBookingFeeEnabled | bool | true |
| CreditLimit | decimal | — |
| DueDays | int | 7 |
| EffectiveFrom | DateTime | — |

#### WorkerBillingSetting

علاوه بر فیلدهای مشترک BaseEntity.

| فیلد | نوع C# | مقدار پیش‌فرض صریح |
| --- | --- | --- |
| BusinessID | long | — |
| WorkerID | long | — |
| FeePayerOverride | string? | — |
| AcceptedByPersonID | long? | — |
| AcceptedAt | DateTime? | — |
| AcceptedContractVersion | string? | — |

#### BookingPlatformFee

علاوه بر فیلدهای مشترک BaseEntity.

| فیلد | نوع C# | مقدار پیش‌فرض صریح |
| --- | --- | --- |
| BookingID | long | — |
| BusinessID | long | — |
| WorkerID | long | — |
| BillingAccountID | long | — |
| BusinessSubscriptionID | long | — |
| FeeAmount | decimal | — |
| CurrencyCode | string | "IRR" |
| FeePayerSnapshot | string | "" |
| CollectionModeSnapshot | string | "" |
| ContractVersionSnapshot | string | "" |
| Status | string | "accrued" |
| AccruedAt | DateTime | — |
| ReversedAt | DateTime? | — |

#### BillingTransaction

علاوه بر فیلدهای مشترک BaseEntity.

| فیلد | نوع C# | مقدار پیش‌فرض صریح |
| --- | --- | --- |
| BillingAccountID | long | — |
| Amount | decimal | — |
| TransactionType | string | "" |
| SourceType | string | "" |
| SourceID | long | — |
| OccurredAt | DateTime | — |

#### BillingInvoice

علاوه بر فیلدهای مشترک BaseEntity.

| فیلد | نوع C# | مقدار پیش‌فرض صریح |
| --- | --- | --- |
| RemainingAmount | decimal | — |
| AllocatedAmount | decimal | — |
| ReversedAmount | decimal | — |
| BillingAccountID | long | — |
| PeriodStart | DateTime | — |
| PeriodEnd | DateTime | — |
| IssueDate | DateTime | — |
| DueDate | DateTime | — |
| Status | string | "open" |
| CurrencyCode | string | "IRR" |
| TotalAmount | decimal | — |

#### BillingInvoiceItem

علاوه بر فیلدهای مشترک BaseEntity.

| فیلد | نوع C# | مقدار پیش‌فرض صریح |
| --- | --- | --- |
| BillingInvoiceID | long | — |
| BookingPlatformFeeID | long | — |
| Amount | decimal | — |

#### BillingPayment

علاوه بر فیلدهای مشترک BaseEntity.

| فیلد | نوع C# | مقدار پیش‌فرض صریح |
| --- | --- | --- |
| BillingAccountID | long | — |
| PaymentID | long | — |
| BillingInvoiceID | long? | — |
| RequestID | Guid | — |
| Amount | decimal | — |
| CurrencyCode | string | "IRR" |
| Status | string | "pending" |
| ConfirmedAt | DateTime? | — |

#### BillingAllocation

علاوه بر فیلدهای مشترک BaseEntity.

| فیلد | نوع C# | مقدار پیش‌فرض صریح |
| --- | --- | --- |
| BillingPaymentID | long | — |
| BookingPlatformFeeID | long | — |
| Amount | decimal | — |
| AllocatedAt | DateTime | — |
| ReversedAt | DateTime? | — |

#### BillingRefund

علاوه بر فیلدهای مشترک BaseEntity.

| فیلد | نوع C# | مقدار پیش‌فرض صریح |
| --- | --- | --- |
| BillingPaymentID | long | — |
| RequestID | Guid | — |
| Amount | decimal | — |
| Status | string | "pending" |
| RequestedByPersonID | long | — |
| ResolvedByPersonID | long? | — |
| BankReference | string? | — |
| ResolvedAt | DateTime? | — |

#### BillingSettingHistory

علاوه بر فیلدهای مشترک BaseEntity.

| فیلد | نوع C# | مقدار پیش‌فرض صریح |
| --- | --- | --- |
| BusinessID | long | — |
| WorkerID | long? | — |
| ActorPersonID | long | — |
| BeforeJson | string | "" |
| AfterJson | string | "" |
| OccurredAt | DateTime | — |

#### BillingAccountReport

| فیلد | نوع C# | مقدار پیش‌فرض صریح |
| --- | --- | --- |
| BillingAccountID | long | — |
| WorkerID | long | — |
| CurrencyCode | string | "IRR" |
| NetBalance | decimal | — |
| AmountDue | decimal | — |
| AvailableCredit | decimal | — |
| OpenFees | decimal | — |
| TotalFees | decimal | — |
| ReversedFees | decimal | — |
| ConfirmedPayments | decimal | — |
| UnallocatedCredit | decimal | — |
| ReservedCredit | decimal | — |
| RefundedAmount | decimal | — |
| ReconciliationDifference | decimal | — |
| IsReconciled | bool | — |

#### BillingReconciliationIssue

| فیلد | نوع C# | مقدار پیش‌فرض صریح |
| --- | --- | --- |
| IssueType | string | "" |
| PaymentID | long? | — |
| PaymentHistoryID | long? | — |
| BillingAccountID | long? | — |
| ExpectedAmount | decimal | — |
| ActualAmount | decimal | — |

#### WorkerBillingContext

| فیلد | نوع C# | مقدار پیش‌فرض صریح |
| --- | --- | --- |
| WorkerID | long | — |
| BusinessID | long | — |
| IsBookingFeeEnabled | bool | — |
| HasActiveSubscription | bool | — |
| BookingFeeAmount | decimal | — |
| CurrencyCode | string | "IRR" |
| ContractVersion | string? | — |
| FeePayer | string | "Business" |
| CollectionMode | string | "Hybrid" |
| RequiresWorkerConsent | bool | — |
| HasWorkerConsent | bool | — |

#### GatewayPaymentRecord

| فیلد | نوع C# | مقدار پیش‌فرض صریح |
| --- | --- | --- |
| ID | long | — |
| TrackingNumber | long | — |
| Amount | decimal | — |
| Token | string | "" |
| TransactionCode | string? | — |
| GatewayName | string | "" |
| GatewayAccountName | string? | — |
| IsCompleted | bool | — |
| IsPaid | bool | — |

#### GatewayTransactionRecord

| فیلد | نوع C# | مقدار پیش‌فرض صریح |
| --- | --- | --- |
| ID | long | — |
| GatewayPaymentRecordID | long | — |
| Amount | decimal | — |
| Type | int | — |
| IsSucceed | bool | — |
| Message | string? | — |
| AdditionalData | string? | — |

### ۱۹.۲ فیلدهای خروجی Payment و PaymentHistory

این بخش برای تفاوت فیلدهای قدیمی پرداخت خدمات با Billing درج شده است. فیلدهای محاسباتی قیمت خدمات و تخفیف در مسیر خدمات باقی مانده‌اند؛ از آن‌ها برای محاسبه کارمزد پلتفرم استفاده نکنید.

#### PaymentVM

| فیلد | نوع C# |
| --- | --- |
| Purpose | string |
| CurrencyCode | string |
| OwnerPersonID | long? |
| WorkerID | long |
| CustomerID | long |
| WorkerName | string |
| WorkshopName | string |
| CustomerName | string |
| AllPaymentAmount | decimal |
| DepositAmount | decimal |
| PayedAmount | decimal |
| RemainAmount | decimal |
| TotalServiceAmount | decimal |
| DiscountedServiceAmount | decimal |
| PlatformAmount | decimal |
| VatAmount | decimal |
| WorkerAmount | decimal |
| PaymentDate | DateTime |
| PaymentStatus | string |
| PaymentFinished | bool |
| PaymentLevel | int |
| Bookings | List<PaymentBookingItemVM> |

#### PaymentBookingItemVM

| فیلد | نوع C# |
| --- | --- |
| BookingID | long |
| BookingDate | DateTime |
| WorkerID | long |
| CustomerID | long |
| WorkerName | string |
| WorkshopName | string |
| PaymentItems | List<PaymentItemVM> |

#### PaymentHistoryVM

| فیلد | نوع C# |
| --- | --- |
| PaymentID | long |
| Purpose | string |
| CurrencyCode | string |
| GatewayRequestState | string |
| RequestExpiresAt | DateTime? |
| GatewayUrl | string? |
| BookingIDs | List<long> |
| WorkerID | long |
| CustomerID | long |
| WorkerName | string |
| WorkshopName | string |
| CustomerName | string |
| AllPaymentAmount | decimal |
| DepositAmount | decimal |
| Amount | decimal |
| TotalServiceAmount | decimal |
| PlatformAmount | decimal |
| WorkerAmount | decimal |
| PaymentDate | DateTime |
| PaymentMethod | int |
| PaymentStatus | bool |
| TransactionCode | string? |
| TrackingNumber | string? |
| GatewayName | string? |
| GatewayMessage | string? |
| PaymentToken | string? |


#### PaymentItemVM

این مدل محتوای هر عضو bookings[].paymentItems در PaymentVM است و از BaseEntity ارث نمی‌برد.

| فیلد | نوع C# | معنی |
| --- | --- | --- |
| WorkerServiceAmount | decimal | مبلغ خدمت در جزئیات پرداخت |
| DiscountAmount | decimal | مبلغ تخفیف |
| PriceAfterDiscount | decimal | مبلغ بعد از تخفیف |
| DiscountPercent | int | درصد تخفیف |
| WorkerServicePriceVariantID | long? | قیمت متغیر انتخاب‌شده در صورت وجود |
| AppliedOptionSummary | string | شرح گزینه‌های انتخاب‌شده |
| AppliedOptionValueIDs | List<long> | شناسه مقدار گزینه‌ها |
| BookingID | long | رزرو مرتبط با این آیتم |
| WorkerID | long | ارائه‌دهنده خدمت |
| ServiceManagementID | long | خدمت مرتبط |
| ServiceTitle | string | عنوان خدمت |
| WorkerName | string | نام ارائه‌دهنده |
| WorkshopName | string? | نام سالن در صورت وجود |

برای پرداخت خدمت، مسیر نمایش جزئیات Payment -> bookings[] -> paymentItems[] است. قیمت‌ها و تخفیف‌های آیتم را از همین خروجی بخوانید و با نرخ روز پلن اشتراک جایگزین نکنید. برای داده قدیمیِ فاقد گزینه، null یا لیست خالی را تحمل کنید. PaymentDetailVM مستقل نیز همین قیمت‌ها و گزینه‌ها را همراه ID مشترک، PaymentID، BookingID، WorkerServiceID و اطلاعات سربرگ پرداخت برمی‌گرداند؛ همه جزئیات آن در PaymentItemVM تکرار نشده‌اند.

## ۲۰. پیوست مجوزهای دقیق اکشن‌ها

این داده‌ها از نام اکشن‌های فعلی کنترلرها استخراج شده‌اند. JSON هر گروه، نمونه قابل استفاده در قالب ثبت مجوز موجود پروژه است؛ اجرای واردکننده و تخصیص نقش‌ها باید با ابزار فعلی پروژه انجام شود. این فایل مستند هیچ مجوزی به دیتابیس اضافه نمی‌کند. HasAuth=true یعنی احراز هویت لازم است؛ به‌تنهایی معادل اعطای دسترسی به همه کاربران نیست.

### Billing

```json
[
  {
    "ActionName": "SaveSubscriptionPlan_Base",
    "PermissionKey": "billing.savesubscriptionplan_base",
    "HasAuth": true
  },
  {
    "ActionName": "SaveBusinessBillingSettings_Base",
    "PermissionKey": "billing.savebusinessbillingsettings_base",
    "HasAuth": true
  },
  {
    "ActionName": "SaveWorkerBillingSettings_Base",
    "PermissionKey": "billing.saveworkerbillingsettings_base",
    "HasAuth": true
  },
  {
    "ActionName": "AcceptWorkerBillingLiability_Base",
    "PermissionKey": "billing.acceptworkerbillingliability_base",
    "HasAuth": true
  },
  {
    "ActionName": "GetWorkerBillingContext_Base",
    "PermissionKey": "billing.getworkerbillingcontext_base",
    "HasAuth": true
  },
  {
    "ActionName": "PurchaseSubscription_Base",
    "PermissionKey": "billing.purchasesubscription_base",
    "HasAuth": true
  },
  {
    "ActionName": "CreateBillingPayment_Base",
    "PermissionKey": "billing.createbillingpayment_base",
    "HasAuth": true
  },
  {
    "ActionName": "GetBillingAccountReport_Base",
    "PermissionKey": "billing.getbillingaccountreport_base",
    "HasAuth": true
  },
  {
    "ActionName": "IssueBillingInvoice_Base",
    "PermissionKey": "billing.issuebillinginvoice_base",
    "HasAuth": true
  },
  {
    "ActionName": "ReverseBookingPlatformFee_Base",
    "PermissionKey": "billing.reversebookingplatformfee_base",
    "HasAuth": true
  },
  {
    "ActionName": "RunBillingNow_Base",
    "PermissionKey": "billing.runbillingnow_base",
    "HasAuth": true
  },
  {
    "ActionName": "GetBillingReconciliationReport_Base",
    "PermissionKey": "billing.getbillingreconciliationreport_base",
    "HasAuth": true
  },
  {
    "ActionName": "RequestBillingRefund_Base",
    "PermissionKey": "billing.requestbillingrefund_base",
    "HasAuth": true
  },
  {
    "ActionName": "ResolveBillingRefund_Base",
    "PermissionKey": "billing.resolvebillingrefund_base",
    "HasAuth": true
  }
]
```

### SubscriptionPlan

```json
[
  {
    "ActionName": "GetAllSubscriptionPlans_Base",
    "PermissionKey": "subscriptionplan.getallsubscriptionplans_base",
    "HasAuth": true
  },
  {
    "ActionName": "GetSubscriptionPlanById_Base",
    "PermissionKey": "subscriptionplan.getsubscriptionplanbyid_base",
    "HasAuth": true
  }
]
```

### BusinessSubscription

```json
[
  {
    "ActionName": "GetAllBusinessSubscriptions_Base",
    "PermissionKey": "businesssubscription.getallbusinesssubscriptions_base",
    "HasAuth": true
  },
  {
    "ActionName": "GetBusinessSubscriptionById_Base",
    "PermissionKey": "businesssubscription.getbusinesssubscriptionbyid_base",
    "HasAuth": true
  }
]
```

### BillingAccount

```json
[
  {
    "ActionName": "GetAllBillingAccounts_Base",
    "PermissionKey": "billingaccount.getallbillingaccounts_base",
    "HasAuth": true
  },
  {
    "ActionName": "GetBillingAccountById_Base",
    "PermissionKey": "billingaccount.getbillingaccountbyid_base",
    "HasAuth": true
  }
]
```

### BusinessBillingSetting

```json
[
  {
    "ActionName": "GetAllBusinessBillingSettings_Base",
    "PermissionKey": "businessbillingsetting.getallbusinessbillingsettings_base",
    "HasAuth": true
  },
  {
    "ActionName": "GetBusinessBillingSettingById_Base",
    "PermissionKey": "businessbillingsetting.getbusinessbillingsettingbyid_base",
    "HasAuth": true
  }
]
```

### WorkerBillingSetting

```json
[
  {
    "ActionName": "GetAllWorkerBillingSettings_Base",
    "PermissionKey": "workerbillingsetting.getallworkerbillingsettings_base",
    "HasAuth": true
  },
  {
    "ActionName": "GetWorkerBillingSettingById_Base",
    "PermissionKey": "workerbillingsetting.getworkerbillingsettingbyid_base",
    "HasAuth": true
  }
]
```

### BookingPlatformFee

```json
[
  {
    "ActionName": "GetAllBookingPlatformFees_Base",
    "PermissionKey": "bookingplatformfee.getallbookingplatformfees_base",
    "HasAuth": true
  },
  {
    "ActionName": "GetBookingPlatformFeeById_Base",
    "PermissionKey": "bookingplatformfee.getbookingplatformfeebyid_base",
    "HasAuth": true
  }
]
```

### BillingTransaction

```json
[
  {
    "ActionName": "GetAllBillingTransactions_Base",
    "PermissionKey": "billingtransaction.getallbillingtransactions_base",
    "HasAuth": true
  },
  {
    "ActionName": "GetBillingTransactionById_Base",
    "PermissionKey": "billingtransaction.getbillingtransactionbyid_base",
    "HasAuth": true
  }
]
```

### BillingInvoice

```json
[
  {
    "ActionName": "GetAllBillingInvoices_Base",
    "PermissionKey": "billinginvoice.getallbillinginvoices_base",
    "HasAuth": true
  },
  {
    "ActionName": "GetBillingInvoiceById_Base",
    "PermissionKey": "billinginvoice.getbillinginvoicebyid_base",
    "HasAuth": true
  }
]
```

### BillingInvoiceItem

```json
[
  {
    "ActionName": "GetAllBillingInvoiceItems_Base",
    "PermissionKey": "billinginvoiceitem.getallbillinginvoiceitems_base",
    "HasAuth": true
  },
  {
    "ActionName": "GetBillingInvoiceItemById_Base",
    "PermissionKey": "billinginvoiceitem.getbillinginvoiceitembyid_base",
    "HasAuth": true
  }
]
```

### BillingPayment

```json
[
  {
    "ActionName": "GetAllBillingPayments_Base",
    "PermissionKey": "billingpayment.getallbillingpayments_base",
    "HasAuth": true
  },
  {
    "ActionName": "GetBillingPaymentById_Base",
    "PermissionKey": "billingpayment.getbillingpaymentbyid_base",
    "HasAuth": true
  }
]
```

### BillingAllocation

```json
[
  {
    "ActionName": "GetAllBillingAllocations_Base",
    "PermissionKey": "billingallocation.getallbillingallocations_base",
    "HasAuth": true
  },
  {
    "ActionName": "GetBillingAllocationById_Base",
    "PermissionKey": "billingallocation.getbillingallocationbyid_base",
    "HasAuth": true
  }
]
```

### BillingRefund

```json
[
  {
    "ActionName": "GetAllBillingRefunds_Base",
    "PermissionKey": "billingrefund.getallbillingrefunds_base",
    "HasAuth": true
  },
  {
    "ActionName": "GetBillingRefundById_Base",
    "PermissionKey": "billingrefund.getbillingrefundbyid_base",
    "HasAuth": true
  }
]
```

### BillingSettingHistory

```json
[
  {
    "ActionName": "GetAllBillingSettingHistories_Base",
    "PermissionKey": "billingsettinghistory.getallbillingsettinghistories_base",
    "HasAuth": true
  },
  {
    "ActionName": "GetBillingSettingHistoryById_Base",
    "PermissionKey": "billingsettinghistory.getbillingsettinghistorybyid_base",
    "HasAuth": true
  }
]
```

### مجوزهای پرداخت مرتبط

برای Payment.GetAllPayments، Payment.GetAllPayments_Base، Payment.GetPaymentById، Payment.GetPaymentById_Base، Payment.RequestPayment و دو اکشن خواندن PaymentHistory نیز مجوز نقش‌های مورد نیاز باید وجود داشته باشد. VerifyPayment برگشت عمومی درگاه است و نباید با احراز هویت اجباری callback مسدود شود. نام PermissionKey مطابق همان الگوی controller.action است؛ تعریف دوباره مجوز موجود به‌جای تخصیص نقش توصیه نمی‌شود.

## ۲۱. نمونه گزارش و الگوی فراخوانی فرانت

### گزارش آموزشی حساب بدهکار

```json
{
  "status": true,
  "errorMessage": "",
  "result": {
    "billingAccountID": 21,
    "workerID": 10144,
    "currencyCode": "IRR",
    "netBalance": 4000,
    "amountDue": 4000,
    "availableCredit": 0,
    "openFees": 4000,
    "totalFees": 24000,
    "reversedFees": 0,
    "confirmedPayments": 20000,
    "unallocatedCredit": 0,
    "reservedCredit": 0,
    "refundedAmount": 0,
    "reconciliationDifference": 0,
    "isReconciled": true
  }
}
```

این نمونه با تخصیص ۲۰٬۰۰۰ ریال به کارمزدهای ۲۴٬۰۰۰ ریالی در حالت Hybrid سازگار است. شناسه‌ها فرضی‌اند. در Postpaid نسبت مانده خالص و تخصیص می‌تواند متفاوت باشد.

### توالی پیشنهادی بدون محاسبه مالی در کلاینت

```text
Choose active profile
  -> POST Billing/GetBillingAccountReport_Base
  -> POST Billing/GetWorkerBillingContext_Base
Choose plan
  -> POST Billing/PurchaseSubscription_Base (stable requestID)
  -> read response.id as PaymentID
  -> POST Payment/RequestPayment
  -> navigate to result.payGatewayUrl
Gateway callback
  -> GET Payment/VerifyPayment (gateway-supplied parameters)
  -> reload Payment + PaymentHistory + BusinessSubscription
Top up
  -> POST Billing/CreateBillingPayment_Base (invoiceID=null)
  -> RequestPayment -> VerifyPayment -> reload account
Pay invoice
  -> POST Billing/CreateBillingPayment_Base (invoiceID>0)
  -> if status && id==0: refresh, no gateway
  -> if status && id>0: RequestPayment -> VerifyPayment -> refresh
```

### زمان و نمایش تاریخ

بخش Billing برای بازه اشتراک، دوره صورتحساب و رخداد دفتر از UTC استفاده می‌کند؛ ورودی تاریخ این بخش را ISO 8601 با Z ارسال کنید. جداول قدیمی Payment و PaymentHistory در بخش‌هایی DateTime.Now دارند؛ همه تاریخ‌های تاریخی پروژه را به‌صورت یکسان UTC فرض نکنید. تبدیل شمسی برای نمایش است و نباید تاریخ API را به رشته شمسی تبدیل کند. درباره تاریخ‌های بدون offset، قرارداد محیط و timezone سرور باید مشخص باشد. برنامه زمان‌بندی job ساعت ایران را مستقل از timezone میزبان انتخاب می‌کند.

### بازتولید PDF

متن این راهنما در financial-guide.fa.md و خروجی در financial-guide.fa.pdf است. اسکریپت export-financial-guide.ps1 کنار آن، با Chrome نصب‌شده PDF فارسی را بدون وابستگی به وب یا ارسال متن به سرویس خارجی تولید می‌کند. پس از تغییر کد، توضیحات و پیوست مدل‌ها باید بازبینی شوند؛ اجرای اسکریپت فقط ظاهر و PDF را از Markdown موجود می‌سازد و صحت کسب‌وکاری متن را خودکار اثبات نمی‌کند.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\docs\export-financial-guide.ps1
```

برای نگهداری، هر دو فایل PDF و Markdown را همراه سورس در کنترل نسخه یا نسخه پشتیبان نگه دارید. PDF برای ارائه به همکار و Markdown برای به‌روزرسانی بعدی است.



