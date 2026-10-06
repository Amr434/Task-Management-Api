using AutoMapper;
using MediatR;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Domain.Interfaces;

namespace Task_Management.Application.Features.Tasks;

// Implemented by commands whose change concerns someone who may not be one of
// the task's assignees (e.g. the person just unassigned), so they're told too.
public interface ITaskChangeRecipients
{
    IEnumerable<int> AlsoNotify { get; }
}

// Runs after every command: whatever task-history entries it saved become live
// notifications. Keeping this in one place means any change that shows up in
// a task's history also notifies people — handlers only record history.
public class TaskChangeNotificationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ITaskNotifier _notifier;

    public TaskChangeNotificationBehavior(IUnitOfWork unitOfWork, IMapper mapper, ITaskNotifier notifier)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifier = notifier;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        var saved = _unitOfWork.TakeSavedActivities();
        if (saved.Count > 0)
        {
            var also = (request as ITaskChangeRecipients)?.AlsoNotify ?? Enumerable.Empty<int>();
            await TaskNotifications.SendAsync(_unitOfWork, _mapper, _notifier, saved, also);
        }

        return response;
    }
}
