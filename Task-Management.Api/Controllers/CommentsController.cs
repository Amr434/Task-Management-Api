using Microsoft.AspNetCore.Mvc;
using Task_Management.Application.Features.Comments.Commands;
using Task_Management.Application.Features.Comments.DTOs;
using Task_Management.Application.Features.Comments.Queries;

namespace Task_Management.Api.Controllers;

public class CommentsController : BaseApiController
{
    // The current user's action items across all tasks.
    [HttpGet("assigned")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetAssignedComments()
    {
        var result = await Mediator.Send(new GetAssignedCommentsQuery(CurrentUserId));
        return HandleResult(result);
    }

    // Replies: all comments on tasks in projects shared with the current user (plus comments assigned to them), newest first.
    [HttpGet("replies")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetReplies()
    {
        var result = await Mediator.Send(new GetRepliesQuery(CurrentUserId));
        return HandleResult(result);
    }

    // The number on the Replies badge.
    [HttpGet("replies/unread-count")]
    public async Task<ActionResult<int>> GetUnreadRepliesCount()
    {
        var result = await Mediator.Send(new GetUnreadRepliesCountQuery(CurrentUserId));
        return HandleResult(result);
    }

    // "Mark all as read" on the Replies page.
    [HttpPost("replies/read")]
    public async Task<ActionResult<int>> MarkAllRepliesRead()
    {
        var result = await Mediator.Send(new MarkRepliesReadCommand(CurrentUserId));
        return HandleResult(result);
    }

    // One reply opened (from Replies or a notification).
    [HttpPost("{id}/read")]
    public async Task<ActionResult<int>> MarkCommentRead(int id)
    {
        var result = await Mediator.Send(new MarkRepliesReadCommand(CurrentUserId, commentId: id));
        return HandleResult(result);
    }

    [HttpPut("{id}/assignee/{userId}")]
    public async Task<ActionResult<CommentDto>> AssignComment(int id, int userId)
    {
        var result = await Mediator.Send(new SetCommentAssigneeCommand(id, CurrentUserId, userId));
        return HandleResult(result);
    }

    [HttpDelete("{id}/assignee")]
    public async Task<ActionResult<CommentDto>> UnassignComment(int id)
    {
        var result = await Mediator.Send(new SetCommentAssigneeCommand(id, CurrentUserId, null));
        return HandleResult(result);
    }

    [HttpPost("{id}/resolve")]
    public async Task<ActionResult<CommentDto>> ResolveComment(int id)
    {
        var result = await Mediator.Send(new ResolveCommentCommand(id, CurrentUserId, true));
        return HandleResult(result);
    }

    [HttpPost("{id}/reopen")]
    public async Task<ActionResult<CommentDto>> ReopenComment(int id)
    {
        var result = await Mediator.Send(new ResolveCommentCommand(id, CurrentUserId, false));
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteComment(int id)
    {
        var result = await Mediator.Send(new DeleteCommentCommand(id, CurrentUserId));
        return HandleResult(result);
    }
}
