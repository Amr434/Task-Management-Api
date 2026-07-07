using AutoMapper;
using FluentValidation;
using MediatR;
using Task_Management.Application.Features.Spaces.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Spaces.Commands;

public class CreateSpaceCommand : IRequest<Result<SpaceDto>>
{
    public CreateSpaceDto SpaceDto { get; set; }

    public CreateSpaceCommand(CreateSpaceDto spaceDto)
    {
        SpaceDto = spaceDto;
    }
}

public class CreateSpaceCommandValidator : AbstractValidator<CreateSpaceCommand>
{
    public CreateSpaceCommandValidator()
    {
        RuleFor(v => v.SpaceDto.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
    }
}

public class CreateSpaceCommandHandler : IRequestHandler<CreateSpaceCommand, Result<SpaceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSpaceCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<SpaceDto>> Handle(CreateSpaceCommand request, CancellationToken cancellationToken)
    {
        var space = _mapper.Map<Space>(request.SpaceDto);

        _unitOfWork.Repository<Space>().Add(space);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<SpaceDto>(space);
        return Result.Success(dto);
    }
}
