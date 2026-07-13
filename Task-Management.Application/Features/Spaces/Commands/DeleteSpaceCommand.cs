using MediatR;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Spaces.Commands;

public class DeleteSpaceCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public DeleteSpaceCommand(int id, int userId)
    {
        Id = id;
        UserId = userId;
    }
}

public class DeleteSpaceCommandHandler : IRequestHandler<DeleteSpaceCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSpaceCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteSpaceCommand request, CancellationToken cancellationToken)
    {
        var space = await _unitOfWork.Repository<Space>().GetByIdAsync(request.Id);

        if (space == null)
        {
            return Result.Failure<bool>(new Error("Space.NotFound", "Space not found."));
        }

        if (space.OwnerId.HasValue && space.OwnerId != request.UserId)
        {
            return Result.Failure<bool>(new Error("Space.NotOwner", "Only the space owner can delete this space."));
        }

        _unitOfWork.Repository<Space>().Delete(space);
        await _unitOfWork.CompleteAsync();

        return Result.Success(true);
    }
}
