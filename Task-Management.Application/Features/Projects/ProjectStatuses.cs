using System.Text.Json;
using Task_Management.Application.Features.Projects.DTOs;

namespace Task_Management.Application.Features.Projects;

// (De)serialises Project.CustomStatuses. Ids 0-2 are the built-in statuses,
// so custom ones always start at 3.
public static class ProjectStatuses
{
    public const int FirstCustomId = 3;

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static List<ProjectStatusDto> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<ProjectStatusDto>();
        try
        {
            return JsonSerializer.Deserialize<List<ProjectStatusDto>>(json, Options) ?? new List<ProjectStatusDto>();
        }
        catch (JsonException)
        {
            return new List<ProjectStatusDto>();
        }
    }

    public static string? Serialize(IReadOnlyCollection<ProjectStatusDto> statuses) =>
        statuses.Count == 0 ? null : JsonSerializer.Serialize(statuses, Options);
}
