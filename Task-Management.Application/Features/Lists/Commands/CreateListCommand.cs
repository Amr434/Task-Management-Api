using AutoMapper;
using FluentValidation;
using MediatR;
using Task_Management.Application.Features.Lists.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Lists.Commands;

public class CreateListCommand : IRequest<Result<ListDto>>
{
    public CreateListDto ListDto { get; set; }

    public CreateListCommand(CreateListDto listDto)
    {
        ListDto = listDto;
    }
}

public class CreateListCommandValidator : AbstractValidator<CreateListCommand>
{
    public CreateListCommandValidator()
    {
        RuleFor(v => v.ListDto.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");
            
        RuleFor(v => v.ListDto.ProjectId)
            .GreaterThan(0).WithMessage("ProjectId must be valid.");
    }
}

public class CreateListCommandHandler : IRequestHandler<CreateListCommand, Result<ListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateListCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ListDto>> Handle(CreateListCommand request, CancellationToken cancellationToken)
    {
        var list = _mapper.Map<List>(request.ListDto);

        _unitOfWork.Repository<List>().Add(list);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<ListDto>(list);
        return Result.Success(dto);
    }
}
