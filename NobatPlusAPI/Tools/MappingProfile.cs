using AITechWebAPI.ViewModels;
using AutoMapper;
using Domain;
using Microsoft.AspNetCore.JsonPatch.Adapters;
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

            CreateMap<BookingDTO, BookingVM>()
           .ForMember(dest => dest.SalonName, opt => opt.MapFrom(src => src.Stylist.StylistName))
           .ForMember(dest => dest.StylistName, opt => opt.MapFrom(src => src.Stylist.Person.FirstName + " " + src.Stylist.Person.LastName))
           .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Person.FirstName + " " + src.Customer.Person.LastName))
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

            CreateMap<Payment, PaymentVM>()
         .ForMember(dest => dest.SalonName, opt => opt.MapFrom(src => src.Booking.Stylist.StylistName))
         .ForMember(dest => dest.StylistID, opt => opt.MapFrom(src => src.Booking.Stylist.Person.ID))
         .ForMember(dest => dest.CustomerID, opt => opt.MapFrom(src => src.Booking.Customer.Person.ID))
         .ForMember(dest => dest.StylistName, opt => opt.MapFrom(src => src.Booking.Stylist.Person.FirstName + " " + src.Booking.Stylist.Person.LastName))
         .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Booking.Customer.Person.FirstName + " " + src.Booking.Customer.Person.LastName))
         ;

            CreateMap<PaymentHistory, PaymentHistoryVM>()
         .ForMember(dest => dest.SalonName, opt => opt.MapFrom(src => src.Booking.Stylist.StylistName))
         .ForMember(dest => dest.StylistID, opt => opt.MapFrom(src => src.Booking.Stylist.PersonID))
         .ForMember(dest => dest.CustomerID, opt => opt.MapFrom(src => src.Booking.Customer.PersonID))
         .ForMember(dest => dest.StylistName, opt => opt.MapFrom(src => src.Booking.Stylist.Person.FirstName + " " + src.Booking.Stylist.Person.LastName))
         .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Booking.Customer.Person.FirstName + " " + src.Booking.Customer.Person.LastName))
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
            CreateMap<ServiceDiscount, ServiceDiscountVM>()
     .ForMember(dest => dest.SalonName, opt => opt.MapFrom(src => src.Stylist.StylistName))
     .ForMember(dest => dest.DiscountCode, opt => opt.MapFrom(src => src.Discount.DiscountCode))
     .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.Discount.DiscountAmount))
     .ForMember(dest => dest.StylistName, opt => opt.MapFrom(src => src.Stylist.Person.FirstName + " " + src.Stylist.Person.LastName))
     .ForMember(dest => dest.AdminFullName, opt => opt.MapFrom(src => src.Admin.Person.FirstName + " " + src.Admin.Person.LastName))
     .ForMember(dest => dest.ServiceTitle, opt => opt.MapFrom(src => src.ServiceManagement.ServiceName))
     ;
            CreateMap<StylistService, StylistServiceVM>()
  .ForMember(dest => dest.SalonName, opt => opt.MapFrom(src => src.Stylist.StylistName))
  .ForMember(dest => dest.StylistName, opt => opt.MapFrom(src => src.Stylist.Person.FirstName + " " + src.Stylist.Person.LastName))
  .ForMember(dest => dest.ServiceTitle, opt => opt.MapFrom(src => src.ServiceManagement.ServiceName))
  ;

            CreateMap<StylistDTO, StylistVM>()
.ForMember(dest => dest.JobTypeTitle, opt => opt.MapFrom(src => src.JobType.JobTitle))
.ForMember(dest => dest.PersonFirstName, opt => opt.MapFrom(src => src.Person.FirstName))
.ForMember(dest => dest.PersonLastName, opt => opt.MapFrom(src => src.Person.LastName))
.ForMember(dest => dest.PersonNationalCode, opt => opt.MapFrom(src => src.Person.NaCode))
.ForMember(dest => dest.PersonPhoneNumber, opt => opt.MapFrom(src => src.Person.PhoneNumber))
  .ForMember(dest => dest.ServiceNames,
        opt => opt.MapFrom(src => src.StylistServices
            .Select(ss => ss.ServiceManagement.ServiceName).ToList()));
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
           ;
            CreateMap<City, CityVM>();

            CreateMap<Notification, NotificationVM>()
.ForMember(dest => dest.PersonFullName, opt => opt.MapFrom(src => src.Person.FirstName + " " + src.Person.LastName))
          ;


            CreateMap<SMSMessage, SMSMessageVM>()
.ForMember(dest => dest.PersonFullName, opt => opt.MapFrom(src => src.Person.FirstName + " " + src.Person.LastName))
          ;
        }
    }

}
