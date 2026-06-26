using AutoMapper;
using Domain;
using Microsoft.AspNetCore.JsonPatch.Adapters;
using MTPermissionCenter.EFCore.Entities;
using NobatPlusAPI.ViewModels;
using NobatPlusAPI.ViewModels;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.ViewModels;



namespace NobatPlusAPI.Tools
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap(typeof(ListResultObject<>), typeof(ListResultObject<>));
            CreateMap(typeof(RowResultObject<>), typeof(RowResultObject<>));
            CreateMap(typeof(BitResultObject), typeof(BitResultObject));

            CreateMap<RateHistoryDTO, RateHistoryVM>()
.ForMember(dest => dest.SalonName, opt => opt.MapFrom(src => src.Stylist.StylistName))
.ForMember(dest => dest.StylistName, opt => opt.MapFrom(src => src.Stylist.Person.FirstName + " " + src.Stylist.Person.LastName))
.ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Person.FirstName + " " + src.Customer.Person.LastName))
.ForMember(dest => dest.RateQuestionText, opt => opt.MapFrom(src => src.RateQuestion.RateQuestionText))
;

            CreateMap<BookingServiceOptionValueDTO, BookingSelectedServiceOptionValueVM>();
            CreateMap<BookingServiceSelectionDTO, BookingSelectedServiceVM>();

            CreateMap<BookingDTO, BookingVM>()
           .ForMember(dest => dest.SalonName, opt => opt.MapFrom(src => src.Stylist.StylistName))
           .ForMember(dest => dest.StylistName, opt => opt.MapFrom(src => src.Stylist.Person.FirstName + " " + src.Stylist.Person.LastName))
           .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Person.FirstName + " " + src.Customer.Person.LastName))
           .ForMember(dest => dest.CustomerPhoneNumber, opt => opt.MapFrom(src => src.Customer.Person.PhoneNumber))
           ;

            CreateMap<Review, ReviewVM>()
         .ForMember(dest => dest.SalonName, opt => opt.MapFrom(src => src.Booking.Stylist.StylistName))
         .ForMember(dest => dest.StylistName, opt => opt.MapFrom(src => src.Booking.Stylist.Person.FirstName + " " + src.Booking.Stylist.Person.LastName))
         .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.IsPrivate ? src.Customer.Person.FirstName + " " + src.Customer.Person.LastName : "ناشناس"))
         ;

            CreateMap<SocialNetwork, SocialNetworkVM>()
          .ForMember(dest => dest.SalonName, opt => opt.MapFrom(src => src.Stylist.StylistName))
          .ForMember(dest => dest.StylistName, opt => opt.MapFrom(src => src.Stylist.Person.FirstName + " " + src.Stylist.Person.LastName))
          ;

            CreateMap<CheckAvailability,CheckAvailabilityVM>()
          .ForMember(dest => dest.SalonName, opt => opt.MapFrom(src => src.Stylist.StylistName))
          .ForMember(dest => dest.StylistName, opt => opt.MapFrom(src => src.Stylist.Person.FirstName + " " + src.Stylist.Person.LastName))
          ;

            CreateMap<WorkTime, WorkTimeVM>()
       .ForMember(dest => dest.SalonName, opt => opt.MapFrom(src => src.Stylist.StylistName))
       .ForMember(dest => dest.StylistRestTime, opt => opt.MapFrom(src => src.Stylist.RestTime))
       .ForMember(dest => dest.StylistName, opt => opt.MapFrom(src => src.Stylist.Person.FirstName + " " + src.Stylist.Person.LastName))
       ;
            CreateMap<StylistPacific, StylistPacificVM>()
      .ForMember(dest => dest.SalonName, opt => opt.MapFrom(src => src.Stylist.StylistName))
      .ForMember(dest => dest.StylistName, opt => opt.MapFrom(src => src.Stylist.Person.FirstName + " " + src.Stylist.Person.LastName))
      ;

            CreateMap<BookingService, BookingServiceVM>()
          .ForMember(dest => dest.SalonName, opt => opt.MapFrom(src => src.Booking.Stylist.StylistName))
          .ForMember(dest => dest.StylistID, opt => opt.MapFrom(src => src.Booking.Stylist.Person.ID))
          .ForMember(dest => dest.CustomerID, opt => opt.MapFrom(src => src.Booking.Customer.Person.ID))
          .ForMember(dest => dest.ServiceTitle, opt => opt.MapFrom(src => src.ServiceManagement.ServiceName))
          .ForMember(dest => dest.StylistName, opt => opt.MapFrom(src => src.Booking.Stylist.Person.FirstName + " " + src.Booking.Stylist.Person.LastName))
          .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Booking.Customer.Person.FirstName + " " + src.Booking.Customer.Person.LastName))
          ;

            CreateMap<PaymentBooking, PaymentBookingVM>()
          .ForMember(dest => dest.BookingDate, opt => opt.MapFrom(src => src.Booking.BookingDate))
          .ForMember(dest => dest.CustomerID, opt => opt.MapFrom(src => src.Booking.CustomerID))
          .ForMember(dest => dest.StylistID, opt => opt.MapFrom(src => src.Booking.StylistID))
          .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Booking.Customer.Person.FirstName + " " + src.Booking.Customer.Person.LastName))
          .ForMember(dest => dest.StylistName, opt => opt.MapFrom(src => src.Booking.Stylist.Person.FirstName + " " + src.Booking.Stylist.Person.LastName))
          .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.Payment.PaymentStatus))
          .ForMember(dest => dest.AllPaymentAmount, opt => opt.MapFrom(src => src.Payment.AllPaymentAmount))
          ;

            CreateMap<BookingServiceOptionValue, BookingServiceOptionValueVM>()
          .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.BookingService.ServiceManagement.ServiceName))
          .ForMember(dest => dest.ServiceOptionID, opt => opt.MapFrom(src => src.ServiceOptionValue.ServiceOptionID))
          .ForMember(dest => dest.OptionName, opt => opt.MapFrom(src => src.ServiceOptionValue.ServiceOption.OptionName))
          .ForMember(dest => dest.ValueName, opt => opt.MapFrom(src => src.ServiceOptionValue.ValueName))
          ;

            CreateMap<StylistServicePriceVariantOptionValue, StylistServicePriceVariantOptionValueVM>()
          .ForMember(dest => dest.StylistID, opt => opt.MapFrom(src => src.StylistServicePriceVariant.StylistID))
          .ForMember(dest => dest.ServiceManagementID, opt => opt.MapFrom(src => src.StylistServicePriceVariant.ServiceManagementID))
          .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.StylistServicePriceVariant.Price))
          .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.StylistServicePriceVariant.Duration))
          .ForMember(dest => dest.ServiceOptionID, opt => opt.MapFrom(src => src.ServiceOptionValue.ServiceOptionID))
          .ForMember(dest => dest.OptionName, opt => opt.MapFrom(src => src.ServiceOptionValue.ServiceOption.OptionName))
          .ForMember(dest => dest.ValueName, opt => opt.MapFrom(src => src.ServiceOptionValue.ValueName))
          ;

            CreateMap<PaymentDetailOptionValue, PaymentDetailOptionValueVM>()
          .ForMember(dest => dest.PaymentID, opt => opt.MapFrom(src => src.PaymentDetail.PaymentID))
          .ForMember(dest => dest.BookingID, opt => opt.MapFrom(src => src.PaymentDetail.BookingID))
          .ForMember(dest => dest.ServiceManagementID, opt => opt.MapFrom(src => src.PaymentDetail.ServiceManagementID))
          .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.PaymentDetail.ServiceManagement.ServiceName))
          .ForMember(dest => dest.ServiceOptionID, opt => opt.MapFrom(src => src.ServiceOptionValue.ServiceOptionID))
          .ForMember(dest => dest.OptionName, opt => opt.MapFrom(src => src.ServiceOptionValue.ServiceOption.OptionName))
          .ForMember(dest => dest.ValueName, opt => opt.MapFrom(src => src.ServiceOptionValue.ValueName))
          ;

            CreateMap<Payment, PaymentVM>()
         .ForMember(dest => dest.SalonName, opt => opt.MapFrom(src =>
            src.PaymentBookings != null && src.PaymentBookings.Any()
                ? src.PaymentBookings.First().Booking.Stylist.StylistName
                : ""))
         .ForMember(dest => dest.StylistID, opt => opt.MapFrom(src =>
            src.PaymentBookings != null && src.PaymentBookings.Any()
                ? src.PaymentBookings.First().Booking.StylistID
                : 0))
         .ForMember(dest => dest.CustomerID, opt => opt.MapFrom(src =>
            src.PaymentBookings != null && src.PaymentBookings.Any()
                ? src.PaymentBookings.First().Booking.CustomerID
                : 0))
         .ForMember(dest => dest.StylistName, opt => opt.MapFrom(src =>
            src.PaymentBookings != null && src.PaymentBookings.Any()
                ? src.PaymentBookings.First().Booking.Stylist.Person.FirstName + " " + src.PaymentBookings.First().Booking.Stylist.Person.LastName
                : ""))
         .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
            src.PaymentBookings != null && src.PaymentBookings.Any()
                ? src.PaymentBookings.First().Booking.Customer.Person.FirstName + " " + src.PaymentBookings.First().Booking.Customer.Person.LastName
                : ""))
         .ForMember(dest => dest.Bookings,
        opt => opt.MapFrom(src => (src.PaymentBookings != null && src.PaymentBookings.Any()
            ? src.PaymentBookings.Select(pb => new PaymentBookingItemVM()
            {
                BookingID = pb.BookingID,
                BookingDate = pb.Booking.BookingDate,
                StylistID = pb.Booking.StylistID,
                CustomerID = pb.Booking.CustomerID,
                SalonName = pb.Booking.Stylist.StylistName,
                StylistName = $"{pb.Booking.Stylist.Person.FirstName} {pb.Booking.Stylist.Person.LastName}",
                PaymentItems = src.PaymentDetails
                    .Where(ss => ss.BookingID == pb.BookingID)
                    .Select(ss => new PaymentItemVM()
                    {
                        BookingID = ss.BookingID,
                        DiscountAmount = ss.DiscountAmount,
                        PriceAfterDiscount = ss.StylistServiceAmount - ss.DiscountAmount,
                        StylistID = ss.StylistID,
                        DiscountPercent = ss.DiscountPercent,
                        StylistServicePriceVariantID = ss.StylistServicePriceVariantID,
                        AppliedOptionSummary = ss.AppliedOptionSummary,
                        AppliedOptionValueIDs = ss.OptionValues.Select(x => x.ServiceOptionValueID).ToList(),
                        ServiceManagementID = ss.ServiceManagementID,
                        StylistServiceAmount = ss.StylistServiceAmount,
                        SalonName = ss.Stylist.StylistName,
                        StylistName = $"{ss.Stylist.Person.FirstName} {ss.Stylist.Person.LastName}",
                        ServiceTitle = ss.ServiceManagement.ServiceName,
                    }).ToList()
            }).ToList()
            : new List<PaymentBookingItemVM>())));

            ;

            CreateMap<PaymentHistory, PaymentHistoryVM>()
         .ForMember(dest => dest.BookingIDs, opt => opt.MapFrom(src => src.Payment.PaymentBookings.Select(x => x.BookingID).ToList()))
         .ForMember(dest => dest.SalonName, opt => opt.MapFrom(src => src.Payment.PaymentBookings.Select(x => x.Booking.Stylist.StylistName).FirstOrDefault()))
         .ForMember(dest => dest.StylistID, opt => opt.MapFrom(src => src.Payment.PaymentBookings.Select(x => x.Booking.Stylist.PersonID).FirstOrDefault()))
         .ForMember(dest => dest.CustomerID, opt => opt.MapFrom(src => src.Payment.PaymentBookings.Select(x => x.Booking.Customer.PersonID).FirstOrDefault()))
         .ForMember(dest => dest.StylistName, opt => opt.MapFrom(src => src.Payment.PaymentBookings.Select(x => x.Booking.Stylist.Person.FirstName + " " + x.Booking.Stylist.Person.LastName).FirstOrDefault()))
         .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Payment.PaymentBookings.Select(x => x.Booking.Customer.Person.FirstName + " " + x.Booking.Customer.Person.LastName).FirstOrDefault()))
         .ForMember(dest => dest.AllPaymentAmount, opt => opt.MapFrom(src => src.Payment.AllPaymentAmount))
         .ForMember(dest => dest.DepositAmount, opt => opt.MapFrom(src => src.Payment.DepositAmount))
         .ForMember(dest => dest.PlatformAmount, opt => opt.MapFrom(src => src.Payment.PlarformAmount))
         .ForMember(dest => dest.StylistAmount, opt => opt.MapFrom(src => src.Payment.StylistAmount))
         .ForMember(dest => dest.TotalServiceAmount, opt => opt.MapFrom(src => src.Payment.TotalServiceAmount))
         ;

            CreateMap<CustomerDiscount, CustomerDiscountVM>()
          .ForMember(dest => dest.SalonName, opt => opt.MapFrom(src => src.Stylist.StylistName))
          .ForMember(dest => dest.DiscountCode, opt => opt.MapFrom(src => src.Discount.DiscountCode))
          .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.Discount.DiscountAmount))
          .ForMember(dest => dest.StylistName, opt => opt.MapFrom(src => src.Stylist.Person.FirstName + " " + src.Stylist.Person.LastName))
          .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Person.FirstName + " " + src.Customer.Person.LastName))
          ;

            CreateMap<DiscountAssignment, DiscountAssignmentVM>()
       .ForMember(dest => dest.SalonName, opt => opt.MapFrom(src => src.Stylist.StylistName))
       .ForMember(dest => dest.DiscountCode, opt => opt.MapFrom(src => src.Discount.DiscountCode))
       .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.Discount.DiscountAmount))
       .ForMember(dest => dest.StylistName, opt => opt.MapFrom(src => src.Stylist.Person.FirstName + " " + src.Stylist.Person.LastName))
       .ForMember(dest => dest.AdminFullName, opt => opt.MapFrom(src => src.Admin.Person.FirstName + " " + src.Admin.Person.LastName))
       ;
            CreateMap<Discount, DiscountVM>()
       .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.StartDate <= DateTime.Now && src.EndDate >= DateTime.Now))
       .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.EndDate < DateTime.Now))
       .ForMember(dest => dest.AssignmentCount, opt => opt.MapFrom(src => src.DiscountAssignments != null ? src.DiscountAssignments.Count(x => x.AdminId.HasValue || x.StylistId.HasValue) : 0))
       .ForMember(dest => dest.CustomerCount, opt => opt.MapFrom(src => src.CustomerDiscounts != null ? src.CustomerDiscounts.Count : 0))
       .ForMember(dest => dest.ServiceCount, opt => opt.MapFrom(src => src.ServiceDiscounts != null ? src.ServiceDiscounts.Count : 0))
       .ForMember(dest => dest.StylistIds, opt => opt.MapFrom(src => src.DiscountAssignments != null ? src.DiscountAssignments.Where(x => x.StylistId.HasValue).Select(x => x.StylistId.GetValueOrDefault()).ToList() : new List<long>()))
       .ForMember(dest => dest.CustomerIds, opt => opt.MapFrom(src => src.CustomerDiscounts != null ? src.CustomerDiscounts.Select(x => x.CustomerId).ToList() : new List<long>()))
       .ForMember(dest => dest.ServiceIds, opt => opt.MapFrom(src => src.ServiceDiscounts != null ? src.ServiceDiscounts.Select(x => x.ServiceManagementId).ToList() : new List<long>()));
            CreateMap<ServiceDiscount, ServiceDiscountVM>()
     .ForMember(dest => dest.SalonName, opt => opt.MapFrom(src => src.Stylist.StylistName))
     .ForMember(dest => dest.DiscountCode, opt => opt.MapFrom(src => src.Discount.DiscountCode))
     .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.Discount.DiscountAmount))
     .ForMember(dest => dest.StylistName, opt => opt.MapFrom(src => src.Stylist.Person.FirstName + " " + src.Stylist.Person.LastName))
     .ForMember(dest => dest.AdminFullName, opt => opt.MapFrom(src => src.Admin.Person.FirstName + " " + src.Admin.Person.LastName))
     .ForMember(dest => dest.ServiceTitle, opt => opt.MapFrom(src => src.ServiceManagement.ServiceName))
     ;
            CreateMap<ServiceOption, ServiceOptionVM>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.ServiceManagement.ServiceName));
            CreateMap<ServiceOptionValue, ServiceOptionValueVM>()
                .ForMember(dest => dest.OptionName, opt => opt.MapFrom(src => src.ServiceOption.OptionName));
            CreateMap<StylistServicePriceVariant, StylistServicePriceVariantVM>()
                .ForMember(dest => dest.OptionValueIDs, opt => opt.MapFrom(src =>
                    src.OptionValues.Select(x => x.ServiceOptionValueID).ToList()))
                .ForMember(dest => dest.OptionSummary, opt => opt.MapFrom(src =>
                    string.Join("، ", src.OptionValues
                        .OrderBy(x => x.ServiceOptionValue.ServiceOption.SortOrder)
                        .ThenBy(x => x.ServiceOptionValue.SortOrder)
                        .Select(x => x.ServiceOptionValue.ServiceOption.OptionName + ": " + x.ServiceOptionValue.ValueName))));
            CreateMap<StylistServicePriceVariantPriceDto, StylistServicePriceVariantPriceVM>();
            CreateMap<StylistServiceWithDiscountDto, StylistServiceVM>()
 
  ;

            CreateMap<StylistDTO, StylistVM>()
.ForMember(dest => dest.JobTypeTitle, opt => opt.MapFrom(src => src.JobType.JobTitle))
.ForMember(dest => dest.PersonFirstName, opt => opt.MapFrom(src => src.Person.FirstName))
.ForMember(dest => dest.PersonLastName, opt => opt.MapFrom(src => src.Person.LastName))
.ForMember(dest => dest.PersonNationalCode, opt => opt.MapFrom(src => src.Person.NaCode))
.ForMember(dest => dest.PersonPhoneNumber, opt => opt.MapFrom(src => src.Person.PhoneNumber))
.ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Person.IsActive))
.ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Person.Gender))
.ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Person.Email))
.ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.Person.DateOfBirth))
.ForMember(dest => dest.AddressLocationHorizentalPoint, opt => opt.MapFrom(src => src.Person.Address.AddressLocationHorizentalPoint))
.ForMember(dest => dest.AddressLocationVerticalPoint, opt => opt.MapFrom(src => src.Person.Address.AddressLocationVerticalPoint))
.ForMember(dest => dest.AddressStreet, opt => opt.MapFrom(src => src.Person.Address.AddressStreet))
.ForMember(dest => dest.AddressPostalCode, opt => opt.MapFrom(src => src.Person.Address.AddressPostalCode))
  .ForMember(dest => dest.ServiceNames,
        opt => opt.MapFrom(src => src.StylistServices
            .Where(x => x.ServiceManagement != null &&
                        !string.IsNullOrWhiteSpace(x.ServiceManagement.ServiceName))
            .Select(x => x.ServiceManagement.ServiceName)
            .Distinct()
            .ToList()));
            ;
            CreateMap<Address, AddressVM>()
                          .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City.CityName))
;

            CreateMap<Admin, AdminVM>()
.ForMember(dest => dest.PersonFullName, opt => opt.MapFrom(src => src.Person.FirstName + " " + src.Person.LastName))
           ;

            CreateMap<Register, RegisterVM>()
.ForMember(dest => dest.PersonFullName, opt => opt.MapFrom(src => src.Person.FirstName + " " + src.Person.LastName))
      ;

            CreateMap<Login, LoginVM>()
.ForMember(dest => dest.PersonFullName, opt => opt.MapFrom(src => src.Person.FirstName + " " + src.Person.LastName))
           ;

            CreateMap<CustomerDTO, CustomerVM>()
.ForMember(dest => dest.PersonFullName, opt => opt.MapFrom(src => src.Person.FirstName + " " + src.Person.LastName))
.ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Person.FirstName))
.ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Person.LastName))
.ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Person.PhoneNumber))
.ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Person.Email))
.ForMember(dest => dest.NaCode, opt => opt.MapFrom(src => src.Person.NaCode))
.ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Person.Gender))
.ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Person.IsActive))
.ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.Person.DateOfBirth))
.ForMember(dest => dest.AddressID, opt => opt.MapFrom(src => src.Person.AddressID))
.ForMember(dest => dest.CityID, opt => opt.MapFrom(src => src.Person.Address != null ? src.Person.Address.CityID : (long?)null))
.ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.Person.Address != null && src.Person.Address.City != null ? src.Person.Address.City.CityName : ""))
.ForMember(dest => dest.AddressStreet, opt => opt.MapFrom(src => src.Person.Address != null ? src.Person.Address.AddressStreet : ""))
.ForMember(dest => dest.AddressPostalCode, opt => opt.MapFrom(src => src.Person.Address != null ? src.Person.Address.AddressPostalCode : ""))
.ForMember(dest => dest.CustomerDescription, opt => opt.MapFrom(src => src.Description))
           ;
            CreateMap<City, CityVM>();
            CreateMap<Setting, SettingVM>()
.ForMember(dest => dest.ParentKey, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Key : ""))
.ForMember(dest => dest.ChildrenCount, opt => opt.MapFrom(src => src.Children != null ? src.Children.Count : 0));
            CreateMap<Person, PersonVM>();
            CreateMap<ServiceManagementDTO, ServiceManagementVM>();
            CreateMap<MTPermissionCenter_Permission, PermissionVM>();


            CreateMap<Notification, NotificationVM>()
.ForMember(dest => dest.PersonFullName, opt => opt.MapFrom(src => src.Person.FirstName + " " + src.Person.LastName))
          ;


            CreateMap<SMSMessage, SMSMessageVM>()
.ForMember(dest => dest.PersonFullName, opt => opt.MapFrom(src => src.Person.FirstName + " " + src.Person.LastName))
          ;

            CreateMap<MTPermissionCenter_PermissionRole, PermissionRoleVM>()
.ForMember(dest => dest.PermissionName, opt => opt.MapFrom(src => src.Permission.Name))
.ForMember(dest => dest.RouteName, opt => opt.MapFrom(src => src.Permission.Routename))
.ForMember(dest => dest.PermissionType, opt => opt.MapFrom(src => src.Permission.PermissionType));

            CreateMap<MTPermissionCenter_UserPermission, UserPermissionVM>()
.ForMember(dest => dest.PermissionName, opt => opt.MapFrom(src => src.Permission.Name))
.ForMember(dest => dest.RouteName, opt => opt.MapFrom(src => src.Permission.Routename))
.ForMember(dest => dest.PermissionType, opt => opt.MapFrom(src => src.Permission.PermissionType));
        }
    }

}
