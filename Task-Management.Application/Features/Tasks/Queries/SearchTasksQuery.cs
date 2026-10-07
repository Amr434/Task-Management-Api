using System.Text.RegularExpressions;
using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Tasks.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Tasks;

namespace Task_Management.Application.Features.Tasks.Queries;

// Search box: finds tasks by number ("2", "#2", "task 2") or by name, but only
// among tasks the user can access. Order: exact number match, then tasks
// assigned to the user, then the rest (newest first).
public class SearchTasksQuery : IRequest<Result<IEnumerable<TaskItemDto>>>
{
    public int UserId { get; set; }
    public string Query { get; set; }

    public SearchTasksQuery(int userId, string query)
    {
        UserId = userId;
        Query = query;
    }
}

public class SearchTasksQueryHandler : IRequestHandler<SearchTasksQuery, Result<IEnumerable<TaskItemDto>>>
{
    private const int MaxResults = 20;
    private static readonly Regex TaskNumber = new(@"^(?:task\s*)?#?\s*(\d+)$", RegexOptions.IgnoreCase);

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SearchTasksQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<TaskItemDto>>> Handle(SearchTasksQuery request, CancellationToken cancellationToken)
    {
        var text = (request.Query ?? string.Empty).Trim();
        if (text.Length == 0)
        {
            return Result.Success<IEnumerable<TaskItemDto>>(new List<TaskItemDto>());
        }
        if (text.Length > 100)
        {
            text = text[..100];
        }

        int? taskId = null;
        var match = TaskNumber.Match(text);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var id))
        {
            taskId = id;
        }

        var tasks = await _unitOfWork.Repository<TaskItem>()
            .ListAsync(new SearchAccessibleTasksSpecification(request.UserId, text, taskId, MaxResults));

        var ordered = tasks
            .OrderByDescending(t => taskId != null && t.Id == taskId)
            .ThenByDescending(t => t.Assignees.Any(a => a.Id == request.UserId))
            .ThenByDescending(t => t.Id);

        return Result.Success(_mapper.Map<IEnumerable<TaskItemDto>>(ordered));
    }
}
