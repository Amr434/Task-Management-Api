using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Users.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Users.Queries;

public class GetAllUsersQuery : IRequest<Result<IEnumerable<UserDto>>> { }

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<IEnumerable<UserDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllUsersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        // The people you can invite to a space or list: active accounts only.
        var users = (await _unitOfWork.Repository<User>().ListAllAsync()).Where(u => u.IsActive);
        var dtos = _mapper.Map<IEnumerable<UserDto>>(users);
        return Result.Success(dtos);
    }
}
