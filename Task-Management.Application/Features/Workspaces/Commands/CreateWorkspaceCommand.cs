using AutoMapper;
using FluentValidation;
using MediatR;
using Task_Management.Application.Features.Workspaces.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Workspaces.Commands;

public class CreateWorkspaceCommand : IRequest<Result<WorkspaceDto>>
{
    public CreateWorkspaceDto WorkspaceDto { get; set; }

    public CreateWorkspaceCommand(CreateWorkspaceDto workspaceDto)
    {
        WorkspaceDto = workspaceDto;
    }
}

public class CreateWorkspaceCommandValidator : AbstractValidator<CreateWorkspaceCommand>
{
    public CreateWorkspaceCommandValidator()
    {
        RuleFor(v => v.WorkspaceDto.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
    }
}

public class CreateWorkspaceCommandHandler : IRequestHandler<CreateWorkspaceCommand, Result<WorkspaceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateWorkspaceCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<WorkspaceDto>> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var workspace = _mapper.Map<Workspace>(request.WorkspaceDto);

        _unitOfWork.Repository<Workspace>().Add(workspace);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<WorkspaceDto>(workspace);
        return Result.Success(dto);
    }
}
