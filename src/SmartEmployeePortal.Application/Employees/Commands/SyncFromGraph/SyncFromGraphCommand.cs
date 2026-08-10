using MediatR;
using SmartEmployeePortal.Application.Employees.DTOs;

namespace SmartEmployeePortal.Application.Employees.Commands.SyncFromGraph;

public record SyncFromGraphCommand(Guid EmployeeId) : IRequest<GraphSyncResultDto>;
