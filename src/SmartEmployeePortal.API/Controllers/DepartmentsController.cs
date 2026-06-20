using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartEmployeePortal.API.Common;
using SmartEmployeePortal.Application.Departments.DTOs;
using SmartEmployeePortal.Application.Departments.Queries.GetAllDepartments;

namespace SmartEmployeePortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DepartmentsController : ControllerBase
{
    private readonly ISender _mediator;

    public DepartmentsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DepartmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllDepartmentsQuery(), cancellationToken);
        return Ok(ApiResponse<IEnumerable<DepartmentDto>>.Ok(result));
    }
}
