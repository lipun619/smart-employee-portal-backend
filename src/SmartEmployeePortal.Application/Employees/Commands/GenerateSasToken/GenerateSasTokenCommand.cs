using MediatR;
using SmartEmployeePortal.Application.Employees.DTOs;

namespace SmartEmployeePortal.Application.Employees.Commands.GenerateSasToken;

public record GenerateSasTokenCommand(Guid EmployeeId, string FileExtension) : IRequest<SasTokenDto>;
