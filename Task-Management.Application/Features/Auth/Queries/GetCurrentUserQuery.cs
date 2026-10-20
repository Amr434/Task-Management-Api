using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Users.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Auth.Queries;

public class GetCurrentUserQuery : IRequest<Result<UserDto>>
{
    public int UserId { get; set; }

    public GetCurrentUserQuery(int userId)
    {
        UserId = userId;
    }
}

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCurrentUserQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId);
        if (user is null)
        {
            return Result.Failure<UserDto>(new Error("Auth.UserNotFound", "User not found."));
        }

        return Result.Success(_mapper.Map<UserDto>(user));
    }
}
