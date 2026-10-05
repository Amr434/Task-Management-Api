using MediatR;
using Task_Management.Application.Features.Users.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Users.Queries;

// Everyone, including inactive users, with a CanManage flag per user
// computed for the person asking. Admins and the Super Admin only.
public class GetManagedUsersQuery : IRequest<Result<IEnumerable<ManagedUserDto>>>
{
    public int ActorId { get; set; }

    public GetManagedUsersQuery(int actorId)
    {
        ActorId = actorId;
    }
}

public class GetManagedUsersQueryHandler : IRequestHandler<GetManagedUsersQuery, Result<IEnumerable<ManagedUserDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetManagedUsersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<ManagedUserDto>>> Handle(GetManagedUsersQuery request, CancellationToken cancellationToken)
    {
        var actorResult = await UserManagementRules.LoadActiveManagerAsync(_unitOfWork, request.ActorId);
        if (actorResult.IsFailure)
        {
            return Result.Failure<IEnumerable<ManagedUserDto>>(actorResult.Error);
        }
        var actor = actorResult.Value;

        var users = await _unitOfWork.Repository<User>().ListAllAsync();
        var dtos = users
            .OrderByDescending(u => u.Role)
            .ThenBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => UserManagementRules.ToManagedDto(u, actor))
            .ToList();

        return Result.Success<IEnumerable<ManagedUserDto>>(dtos);
    }
}
