using api.Models;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace api.Controllers;

[Authorize]
[ApiController]
[Route("api/tasks/{taskId}/comments")]
public class CommentsController : ControllerBase
{
    private readonly TaskService _taskService;
    private readonly IHubContext<CollaborationHub> _hubContext;

    public CommentsController(TaskService taskService, IHubContext<CollaborationHub> hubContext)
    {
        _taskService = taskService;
        _hubContext = hubContext;
    }

    // POST: api/tasks/{taskId}/comments
    [HttpPost]
    public async Task<IActionResult> AddComment(string taskId, [FromBody] AddCommentDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var tenantId = User.FindFirst("TenantId")?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
            return Unauthorized();

        var task = await _taskService.GetByTenantAndIdAsync(tenantId, taskId);
        if (task == null)
            return NotFound();

        var comment = new Comment
        {
            UserId = userId,
            Username = username ?? "Unknown",
            Text = dto.Text
        };

        task.Comments.Add(comment);

        // Add activity log entry
        var activity = new ActivityLog
        {
            UserId = userId,
            Username = username ?? "Unknown",
            ActivityType = "Commented",
            Description = $"Added a comment"
        };
        task.ActivityLog.Add(activity);

        await _taskService.UpdateAsync(taskId, task);

        // Broadcast comment added event
        await _hubContext.Clients.Group(tenantId).SendAsync("CommentAdded", new
        {
            taskId = taskId,
            comment = comment
        });

        // Also send to public share group if task is shared
        if (!string.IsNullOrEmpty(task.PublicShareId))
        {
            await _hubContext.Clients.Group(task.PublicShareId).SendAsync("CommentAdded", new
            {
                taskId = taskId,
                comment = comment
            });
        }

        return Ok(comment);
    }

    // GET: api/tasks/{taskId}/comments
    [HttpGet]
    public async Task<IActionResult> GetComments(string taskId)
    {
        var tenantId = User.FindFirst("TenantId")?.Value;
        if (string.IsNullOrEmpty(tenantId))
            return Unauthorized();

        var task = await _taskService.GetByTenantAndIdAsync(tenantId, taskId);
        if (task == null)
            return NotFound();

        return Ok(task.Comments);
    }

    // DELETE: api/tasks/{taskId}/comments/{commentId}
    [HttpDelete("{commentId}")]
    public async Task<IActionResult> DeleteComment(string taskId, string commentId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var tenantId = User.FindFirst("TenantId")?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
            return Unauthorized();

        var task = await _taskService.GetByTenantAndIdAsync(tenantId, taskId);
        if (task == null)
            return NotFound();

        var comment = task.Comments.FirstOrDefault(c => c.Id == commentId);
        if (comment == null)
            return NotFound();

        // Only allow user to delete their own comments or admin
        if (comment.UserId != userId)
            return Forbid();

        task.Comments.Remove(comment);

        // Add activity log entry
        var activity = new ActivityLog
        {
            UserId = userId,
            Username = username ?? "Unknown",
            ActivityType = "CommentDeleted",
            Description = $"Deleted a comment"
        };
        task.ActivityLog.Add(activity);

        await _taskService.UpdateAsync(taskId, task);

        // Broadcast comment deleted event
        await _hubContext.Clients.Group(tenantId).SendAsync("CommentDeleted", new
        {
            taskId = taskId,
            commentId = commentId
        });

        // Also send to public share group if task is shared
        if (!string.IsNullOrEmpty(task.PublicShareId))
        {
            await _hubContext.Clients.Group(task.PublicShareId).SendAsync("CommentDeleted", new
            {
                taskId = taskId,
                commentId = commentId
            });
        }

        return NoContent();
    }
}

public class AddCommentDto
{
    public string Text { get; set; } = string.Empty;
}
