using AutoMapper;
using FluentValidation;
using MediatR;
using Task_Management.Application.Features.Tags.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Tags.Commands;

public class CreateTagCommand : IRequest<Result<TagDto>>
{
    public CreateTagDto TagDto { get; set; }

    public CreateTagCommand(CreateTagDto tagDto)
    {
        TagDto = tagDto;
    }
}

public class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagCommandValidator()
    {
        RuleFor(v => v.TagDto.Name).NotEmpty().MaximumLength(30);
        RuleFor(v => v.TagDto.ColorHex).NotEmpty().MaximumLength(7);
    }
}

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, Result<TagDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateTagCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<TagDto>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var name = request.TagDto.Name.Trim();

        // Tag names are unique (case-insensitive): reuse an existing tag rather than
        // creating a duplicate, so there are never two tags with the same name.
        var existing = (await _unitOfWork.Repository<Tag>().ListAllAsync())
            .FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            return Result.Success(_mapper.Map<TagDto>(existing));
        }

        var tag = _mapper.Map<Tag>(request.TagDto);
        tag.Name = name;

        _unitOfWork.Repository<Tag>().Add(tag);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<TagDto>(tag);
        return Result.Success(dto);
    }
}
