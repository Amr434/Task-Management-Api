using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task_Management.Application.Features.Dashboards.Commands;
using Task_Management.Application.Features.Dashboards.DTOs;
using Task_Management.Application.Features.Dashboards.Queries;

namespace Task_Management.Api.Controllers;

// Dashboards are for Admins and the Super Admin only. A Member gets 403 on
// every endpoint here, whatever the frontend shows.
[Authorize(Roles = "Admin,SuperAdmin")]
public class DashboardsController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DashboardDto>>> GetDashboards()
    {
        var result = await Mediator.Send(new GetAllDashboardsQuery(CurrentUserId));
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<DashboardDto>> CreateDashboard([FromBody] CreateDashboardDto dto)
    {
        var result = await Mediator.Send(new CreateDashboardCommand(dto, CurrentUserId));
        return HandleResult(result);
    }

    [HttpGet("{id}/summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary(int id)
    {
        var result = await Mediator.Send(new GetDashboardSummaryQuery(id, CurrentUserId));
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDashboard(int id)
    {
        var result = await Mediator.Send(new DeleteDashboardCommand(id, CurrentUserId));
        return HandleResult(result);
    }

    [HttpPut("{id}/name")]
    public async Task<ActionResult> RenameDashboard(int id, [FromBody] RenameDashboardDto dto)
    {
        var result = await Mediator.Send(new RenameDashboardCommand(id, dto.Name, CurrentUserId));
        return HandleResult(result);
    }
}
