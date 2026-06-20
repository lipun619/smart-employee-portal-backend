using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartEmployeePortal.API.Common;
using SmartEmployeePortal.Application.Employees.Commands.CreateEmployee;
using SmartEmployeePortal.Application.Employees.Commands.DeleteEmployee;
using SmartEmployeePortal.Application.Employees.Commands.UpdateEmployee;
using SmartEmployeePortal.Application.Employees.DTOs;
using SmartEmployeePortal.Application.Employees.Queries.GetAllEmployees;
using SmartEmployeePortal.Application.Employees.Queries.GetEmployeeById;
using SmartEmployeePortal.Application.Employees.Queries.GetEmployeesPaginated;

namespace SmartEmployeePortal.API.Controllers;

/// <summary>
/// REST API controller for Employee CRUD operations.
///
/// Every action:
/// 1. Creates a MediatR Command or Query
/// 2. Sends it via ISender (MediatR dispatches to the correct handler)
/// 3. Wraps the result in ApiResponse<T> for consistent shape
///
/// The controller has ZERO business logic — it's purely HTTP concerns.
/// Validation, business rules, and DB access all happen in MediatR handlers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmployeesController : ControllerBase
{
    private readonly ISender _mediator;

    public EmployeesController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get paginated list of employees with optional search.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10)</param>
    /// <param name="searchTerm">Optional search term (name, email, job title)</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedEmployeesDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetEmployeesPaginatedQuery(pageNumber, pageSize, searchTerm),
            cancellationToken);

        return Ok(ApiResponse<PaginatedEmployeesDto>.Ok(result));
    }

    /// <summary>
    /// Get a single employee by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetEmployeeByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<EmployeeDto>.Ok(result));
    }

    /// <summary>
    /// Create a new employee.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateEmployeeCommand(
            dto.FirstName,
            dto.LastName,
            dto.Email,
            dto.PhoneNumber,
            dto.DateOfBirth,
            dto.HireDate,
            dto.JobTitle,
            dto.Salary,
            dto.EmploymentStatus,
            dto.Gender,
            dto.DepartmentId);

        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            ApiResponse<EmployeeDto>.Ok(result, "Employee created successfully."));
    }

    /// <summary>
    /// Update an existing employee.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateEmployeeDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateEmployeeCommand(
            id,
            dto.FirstName,
            dto.LastName,
            dto.Email,
            dto.PhoneNumber,
            dto.DateOfBirth,
            dto.HireDate,
            dto.JobTitle,
            dto.Salary,
            dto.EmploymentStatus,
            dto.Gender,
            dto.DepartmentId);

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Soft-delete an employee (marks as deleted, not physically removed).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new DeleteEmployeeCommand(id), cancellationToken);
        return NoContent();
    }
}
