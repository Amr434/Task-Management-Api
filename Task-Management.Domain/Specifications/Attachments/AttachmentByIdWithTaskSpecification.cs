using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Attachments;

// One attachment with its task (for access checks) and uploader.
public class AttachmentByIdWithTaskSpecification : BaseSpecification<Attachment>
{
    public AttachmentByIdWithTaskSpecification(int attachmentId) : base(a => a.Id == attachmentId)
    {
        AddInclude(a => a.UploadedBy!);
        AddInclude(a => a.TaskItem!);
    }
}
