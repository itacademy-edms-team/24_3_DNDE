    using AutoMapper;
    using DND5E_CE.Server.DTO.App;
    using DND5E_CE.Server.DTO.App.Sheet1;
    using DND5E_CE.Server.DTO.App.Sheet2;
    using DND5E_CE.Server.DTO.App.Sheet3;
    using DND5E_CE.Server.Models.App;
    using DND5E_CE.Server.Models.App.Sheet1;

    namespace DND5E_CE.Server.Mapping
    {
        public class AutoMapperProfile : Profile
        {
            public AutoMapperProfile()
            {
                // CharacterModel -> CharacterSelectItemDto
                CreateMap<CharacterModel, CharacterSelectItemDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Sheet1.Name))
                    .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Sheet1.Level))
                    .ForMember(dest => dest.Class, opt => opt.MapFrom(src => src.Sheet1.Class));

                // CharacterModel -> CharacterDto
                CreateMap<CharacterModel, CharacterDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.Sheet1, opt => opt.MapFrom(src => src.Sheet1))
                    .ForMember(dest => dest.Sheet2, opt => opt.MapFrom(src => src.Sheet2))
                    .ForMember(dest => dest.Sheet3, opt => opt.MapFrom(src => src.Sheet3));

                // Sheet DTOs
                CreateMap<Sheet1Model, Sheet1Dto>();
                CreateMap<Sheet2Model, Sheet2Dto>();
                CreateMap<Sheet3Model, Sheet3Dto>();
                CreateMap<AbilityModel, AbilityDto>();
                CreateMap<AbilitySaveThrowModel, AbilitySaveThrowDto>();
                CreateMap<SkillModel, SkillDto>();
                CreateMap<ToolModel, ToolDto>();
                CreateMap<OtherToolModel, OtherToolDto>();
                CreateMap<HitPointModel, HitPointDto>();
                CreateMap<HitDiceModel, HitDiceDto>();
                CreateMap<DeathSaveThrowModel, DeathSaveThrowDto>();
                CreateMap<AttackModel, AttackDto>();
                CreateMap<GlobalDamageModifierModel, GlobalDamageModifierDto>();
                CreateMap<InventoryGoldModel, InventoryGoldDto>();
                CreateMap<InventoryItemModel, InventoryItemDto>();
                CreateMap<ClassResourceModel, ClassResourceDto>();

            // Sheet1CreateDto -> Sheet1Model
            CreateMap<Sheet1CreateDto, Sheet1Model>();

            }
        }
    }
