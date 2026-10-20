using AutoMapper;
using FluentValidation;
using MediatR;
using Task_Management.Application.Features.Spaces.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Spaces.Commands;

public class UpdateSpaceCommand : IRequest<Result<SpaceDto>>
{
    public int Id { get; set; }
    public UpdateSpaceDto SpaceDto { get; set; }
    public int UserId { get; set; }

    public UpdateSpaceCommand(int id, UpdateSpaceDto spaceDto, int userId)
    {
        Id = id;
        SpaceDto = spaceDto;
        UserId = userId;
    }
}

public class UpdateSpaceCommandValidator : AbstractValidator<UpdateSpaceCommand>
{
    public UpdateSpaceCommandValidator()
    {
        RuleFor(v => v.SpaceDto.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
    }
}

public class UpdateSpaceCommandHandler : IRequestHandler<UpdateSpaceCommand, Result<SpaceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateSpaceCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<SpaceDto>> Handle(UpdateSpaceCommand request, CancellationToken cancellationToken)
    {
        var space = await _unitOfWork.Repository<Space>().GetByIdAsync(request.Id);

        if (space == null)
        {
            return Result.Failure<SpaceDto>(new Error("Space.NotFound", "Space not found."));
        }

        // Only the owner can edit the space itself (members can work inside it).
        if (space.OwnerId.HasValue && space.OwnerId != request.UserId)
        {
            return Result.Failure<SpaceDto>(new Error("Space.NotOwner", "Only the space owner can edit this space."));
        }

        space.Name = request.SpaceDto.Name;
        space.Description = request.SpaceDto.Description;
        space.Color = request.SpaceDto.Color;
        space.Icon = request.SpaceDto.Icon;

        _unitOfWork.Repository<Space>().Update(space);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<SpaceDto>(space);
        return Result.Success(dto);
    }
}
