using MediatR;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Lists.Commands;

public class DeleteListCommand : IRequest<Result<bool>>
{
    public int ListId { get; set; }

    public DeleteListCommand(int listId)
    {
        ListId = listId;
    }
}

public class DeleteListCommandHandler : IRequestHandler<DeleteListCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteListCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteListCommand request, CancellationToken cancellationToken)
    {
        var list = await _unitOfWork.Repository<List>().GetByIdAsync(request.ListId);

        if (list == null)
        {
            return Result.Failure<bool>(new Error("List.NotFound", $"List with Id {request.ListId} was not found."));
        }

        _unitOfWork.Repository<List>().Delete(list);
        await _unitOfWork.CompleteAsync();

        return Result.Success(true);
    }
}
