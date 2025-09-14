using AutoMapper;
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

            

        }
    }

}
