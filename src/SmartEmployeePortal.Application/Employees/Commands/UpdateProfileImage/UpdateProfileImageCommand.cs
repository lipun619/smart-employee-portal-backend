using MediatR;

namespace SmartEmployeePortal.Application.Employees.Commands.UpdateProfileImage;

public record UpdateProfileImageCommand(Guid EmployeeId, string BlobUrl) : IRequest;
