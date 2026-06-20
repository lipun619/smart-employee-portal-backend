using MediatR;
using SmartEmployeePortal.Application.Common.Exceptions;
using SmartEmployeePortal.Domain.Interfaces;

namespace SmartEmployeePortal.Application.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEmployeeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(
        UpdateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Employee), request.Id);

        // Check email uniqueness — exclude the current employee from the check
        var emailExists = await _unitOfWork.Employees.EmailExistsAsync(
            request.Email, request.Id, cancellationToken);

        if (emailExists)
            throw new InvalidOperationException($"An employee with email '{request.Email}' already exists.");

        // Update fields — no Mapster here to keep changes explicit and auditable
        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;
        employee.PhoneNumber = request.PhoneNumber;
        employee.DateOfBirth = request.DateOfBirth;
        employee.HireDate = request.HireDate;
        employee.JobTitle = request.JobTitle;
        employee.Salary = request.Salary;
        employee.EmploymentStatus = request.EmploymentStatus;
        employee.Gender = request.Gender;
        employee.DepartmentId = request.DepartmentId;

        _unitOfWork.Employees.Update(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
