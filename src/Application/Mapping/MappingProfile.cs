using AutoMapper;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.DTOs.Employee;
using TaskManagement.Application.DTOs.Notification;
using TaskManagement.Application.DTOs.Task;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Mapping;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<User, EmployeeLookupDto>(); // Simple mapping for dropdowns

        CreateMap<User, EmployeeDto>()
            .ForMember(d => d.TotalTasks, o => o.MapFrom(s => s.AssignedTasks.Count))
            .ForMember(d => d.CompletedTasks, o => o.MapFrom(s =>
                s.AssignedTasks.Count(t => t.Status == Domain.Enums.TaskStatus.Completed)))
            .ForMember(d => d.PendingTasks, o => o.MapFrom(s =>
                s.AssignedTasks.Count(t => t.Status != Domain.Enums.TaskStatus.Completed)));

        CreateMap<TaskItem, TaskDto>()
            .ForMember(d => d.AssignedToName, o => o.MapFrom(s => s.AssignedTo.FullName))
            .ForMember(d => d.CreatedByName, o => o.MapFrom(s => s.CreatedBy.FullName));

        CreateMap<Notification, NotificationDto>();
    }
}
