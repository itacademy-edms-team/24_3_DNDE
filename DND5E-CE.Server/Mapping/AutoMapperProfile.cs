using AutoMapper;
using DND5E_CE.Server.DTO.App;
using DND5E_CE.Server.Models.App;

namespace DND5E_CE.Server.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() 
        {
            CreateMap<CharacterModel, CharacterListDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Sheet1.Name))
                .ForMember(dest => dest.Class, opt => opt.MapFrom(src => src.Sheet1.Class))
                .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Sheet1.Level));

            CreateMap<CharacterModel, CharacterDto>()
                .ForMember(dest => dest.Sheet1, opt => opt.MapFrom(src => src.Sheet1))
                .ForMember(dest => dest.Sheet2, opt => opt.MapFrom(src => src.Sheet2))
                .ForMember(dest => dest.Sheet3, opt => opt.MapFrom(src => src.Sheet3));

            CreateMap<Sheet1Model, Sheet1Dto>();
            CreateMap<Sheet2Model, Sheet2Dto>();
            CreateMap<Sheet3Model, Sheet3Dto>()
                .ForMember(dest => dest.SpellBondAbility, opt => opt.MapFrom(src => src.SpellBondAbility ?? "none"))
                .ForMember(dest => dest.RemainingSpellSlotsLevel1, opt => opt.MapFrom(src => src.RemainingSpellSlotsLevel1))
                .ForMember(dest => dest.RemainingSpellSlotsLevel2, opt => opt.MapFrom(src => src.RemainingSpellSlotsLevel2))
                .ForMember(dest => dest.RemainingSpellSlotsLevel3, opt => opt.MapFrom(src => src.RemainingSpellSlotsLevel3))
                .ForMember(dest => dest.RemainingSpellSlotsLevel4, opt => opt.MapFrom(src => src.RemainingSpellSlotsLevel4))
                .ForMember(dest => dest.RemainingSpellSlotsLevel5, opt => opt.MapFrom(src => src.RemainingSpellSlotsLevel5))
                .ForMember(dest => dest.RemainingSpellSlotsLevel6, opt => opt.MapFrom(src => src.RemainingSpellSlotsLevel6))
                .ForMember(dest => dest.RemainingSpellSlotsLevel7, opt => opt.MapFrom(src => src.RemainingSpellSlotsLevel7))
                .ForMember(dest => dest.RemainingSpellSlotsLevel8, opt => opt.MapFrom(src => src.RemainingSpellSlotsLevel8))
                .ForMember(dest => dest.RemainingSpellSlotsLevel9, opt => opt.MapFrom(src => src.RemainingSpellSlotsLevel9));
        }
    }
}
