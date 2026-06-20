using Mapster;
using MediatR;
using SmartEmployeePortal.Application.Common.Exceptions;
using SmartEmployeePortal.Application.Employees.DTOs;
using SmartEmployeePortal.Domain.Entities;
using SmartEmployeePortal.Domain.Interfaces;

namespace SmartEmployeePortal.Application.Employees.Commands.CreateEmployee;

/// <summary>
/// Handler for CreateEmployeeCommand.
/// Responsibilities:
/// 1. Check email uniqueness (business rule)
/// 2. Map command → entity
/// 3. Persist via repository
/// 4. Return DTO to caller
///
/// Note: Validation has ALREADY run via ValidationBehavior before we get here.
/// </summary>
public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateEmployeeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeDto> Handle(
        CreateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        // Business rule: email must be unique across all employees
        var emailExists = await _unitOfWork.Employees.EmailExistsAsync(
            request.Email, cancellationToken: cancellationToken);

        if (emailExists)
            throw new InvalidOperationException($"An employee with email '{request.Email}' already exists.");

        var employee = new Employee
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            HireDate = request.HireDate,
            JobTitle = request.JobTitle,
            Salary = request.Salary,
            EmploymentStatus = request.EmploymentStatus,
            Gender = request.Gender,
            DepartmentId = request.DepartmentId
        };

        await _unitOfWork.Employees.AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapster adapts Employee entity → EmployeeDto automatically
        return employee.Adapt<EmployeeDto>();
    }
}
