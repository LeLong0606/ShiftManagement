using AutoMapper;
using ShiftManagement.DTOs.Sams;
using ShiftManagement.Models.Sams;
using ShiftManagement.Models.Sams.Views;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ShiftManagement.Profiles
{
    public class SamsProfile : Profile
    {
        public SamsProfile()
        {
            CreateMap<Team, TeamDto>().ReverseMap();
            CreateMap<Employee, EmployeeDto>().ReverseMap();
            CreateMap<Position, PositionDto>().ReverseMap();
            CreateMap<ShiftBase, ShiftBaseDto>()
                .ForMember(d => d.StartTime, o => o.MapFrom(s => s.StartTime.HasValue ? s.StartTime.Value.ToString(@"hh\:mm") : null))
                .ForMember(d => d.EndTime, o => o.MapFrom(s => s.EndTime.HasValue ? s.EndTime.Value.ToString(@"hh\:mm") : null))
                .ReverseMap()
                .ForMember(d => d.StartTime, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.StartTime) ? (TimeSpan?)null : TimeSpan.Parse(s.StartTime!)))
                .ForMember(d => d.EndTime, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.EndTime) ? (TimeSpan?)null : TimeSpan.Parse(s.EndTime!)));

            CreateMap<TeamShiftAlias, TeamShiftAliasDto>().ReverseMap();
            CreateMap<TeamShiftOverride, TeamShiftOverrideDto>()
                .ForMember(d => d.StartTime, o => o.MapFrom(s => s.StartTime.HasValue ? s.StartTime.Value.ToString(@"hh\:mm") : null))
                .ForMember(d => d.EndTime, o => o.MapFrom(s => s.EndTime.HasValue ? s.EndTime.Value.ToString(@"hh\:mm") : null))
                .ReverseMap()
                .ForMember(d => d.StartTime, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.StartTime) ? (TimeSpan?)null : TimeSpan.Parse(s.StartTime!)))
                .ForMember(d => d.EndTime, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.EndTime) ? (TimeSpan?)null : TimeSpan.Parse(s.EndTime!)));

            CreateMap<RosterPeriod, RosterPeriodDto>().ReverseMap();
            CreateMap<RosterEntry, RosterEntryDto>().ReverseMap();

            CreateMap<VRosterForExport, RosterEntryViewDto>();
            CreateMap<TimesheetBatch, TimesheetBatchDto>().ReverseMap();
            CreateMap<TimesheetEntry, TimesheetEntryDto>().ReverseMap();
            CreateMap<VTimesheetForExport, VTimesheetForExportDto>();

            CreateMap<ExportTemplate, ExportTemplateDto>().ReverseMap();
            CreateMap<TemplateFieldMap, TemplateFieldMapDto>().ReverseMap();
            CreateMap<TeamTemplateBinding, TeamTemplateBindingDto>().ReverseMap();
            CreateMap<ExportRun, ExportRunDto>().ReverseMap();

            CreateMap<CalendarDate, CalendarDateDto>().ReverseMap();
            CreateMap<TeamSettings, TeamSettingsDto>().ReverseMap();
        }
    }
}