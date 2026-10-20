using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Attachments;

// All attachments of one task, oldest first, with the uploader loaded.
public class AttachmentsByTaskSpecification : BaseSpecification<Attachment>
{
    public AttachmentsByTaskSpecification(int taskId) : base(a => a.TaskItemId == taskId)
    {
        AddInclude(a => a.UploadedBy!);
        AddOrderBy(a => a.UploadedAt);
    }
}
