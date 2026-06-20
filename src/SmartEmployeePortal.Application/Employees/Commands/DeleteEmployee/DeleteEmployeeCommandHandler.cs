using MediatR;
using SmartEmployeePortal.Application.Common.Exceptions;
using SmartEmployeePortal.Domain.Interfaces;

namespace SmartEmployeePortal.Application.Employees.Commands.DeleteEmployee;

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEmployeeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(
        DeleteEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Employee), request.Id);

        // Calls the domain method — soft delete marks IsDeleted = true
        // EF Core global query filter will exclude it from all future queries
        employee.SoftDelete();

        _unitOfWork.Employees.Update(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
