using MediatR;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications;

namespace Task_Management.Application.Features.Projects.Commands;

public class DeleteProjectCommand : IRequest<Result<bool>>
{
    public int ProjectId { get; set; }

    public DeleteProjectCommand(int projectId)
    {
        ProjectId = projectId;
    }
}

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProjectCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Repository<Project>().GetByIdAsync(request.ProjectId);

        if (project == null)
        {
            return Result.Failure<bool>(new Error("Project.NotFound", $"Project with Id {request.ProjectId} was not found."));
        }

        var invitations = await _unitOfWork.Repository<Invitation>().ListAsync(
            new BaseSpecification<Invitation>(i => i.ProjectId == project.Id));

        foreach (var inv in invitations)
        {
            _unitOfWork.Repository<Invitation>().Delete(inv);
        }

        _unitOfWork.Repository<Project>().Delete(project);
        await _unitOfWork.CompleteAsync();

        return Result.Success(true);
    }
}
