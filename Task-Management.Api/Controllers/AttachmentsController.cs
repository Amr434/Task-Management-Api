using Microsoft.AspNetCore.Mvc;
using Task_Management.Application.Features.Attachments.Commands;
using Task_Management.Application.Features.Attachments.Queries;

namespace Task_Management.Api.Controllers;

public class AttachmentsController : BaseApiController
{
    // Streams the file back; access is checked against the attachment's
    // task/project, so anyone the task is shared with can download it.
    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var result = await Mediator.Send(new DownloadAttachmentQuery(id, CurrentUserId));
        if (result.IsFailure)
        {
            return result.Error.Code.Contains("NotFound", StringComparison.OrdinalIgnoreCase)
                ? NotFound(result.Error)
                : BadRequest(result.Error);
        }
        var file = result.Value;
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await Mediator.Send(new DeleteAttachmentCommand(id, CurrentUserId));
        return HandleResult(result);
    }
}
