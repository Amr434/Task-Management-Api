namespace Task_Management.Domain.Enums;

// What a TaskActivity entry records. Values are persisted, so only append.
public enum TaskActivityType
{
    Created = 0,
    TitleChanged = 1,
    DescriptionChanged = 2,
    StatusChanged = 3,
    PriorityChanged = 4,
    DueDateChanged = 5,
    AssigneeAdded = 6,
    AssigneeRemoved = 7,
    TagAdded = 8,
    TagRemoved = 9,
    MovedToProject = 10,
    CommentAdded = 11,
    CommentDeleted = 12,
    CommentAssigned = 13,
    CommentUnassigned = 14,
    CommentResolved = 15,
    CommentReopened = 16,
    AttachmentAdded = 17,
    AttachmentDeleted = 18
}
