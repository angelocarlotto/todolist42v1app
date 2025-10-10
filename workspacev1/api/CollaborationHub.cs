using Microsoft.AspNetCore.SignalR;

namespace api
{
    // SignalR hub for real-time collaboration
    public class CollaborationHub : Hub {
        // Allow clients to join a group (e.g., userId)
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Broadcast task update to all users in the tenant group

        public async Task BroadcastTaskUpdate(object task)
        {
            // Expect task to have a tenantId property
            var tenantIdObj = task.GetType().GetProperty("TenantId")?.GetValue(task, null);
            var tenantId = tenantIdObj as string;
            if (!string.IsNullOrEmpty(tenantId))
            {
                await Clients.Group(tenantId).SendAsync("TaskUpdated", task);
            }
        }

        // Broadcast task creation
        public async Task BroadcastTaskCreated(object task)
        {
            var tenantIdObj = task.GetType().GetProperty("TenantId")?.GetValue(task, null);
            var tenantId = tenantIdObj as string;
            if (!string.IsNullOrEmpty(tenantId))
            {
                await Clients.Group(tenantId).SendAsync("TaskCreated", task);
            }
        }

        // Broadcast task deletion
        public async Task BroadcastTaskDeleted(string tenantId, string taskId)
        {
            if (!string.IsNullOrEmpty(tenantId))
            {
                await Clients.Group(tenantId).SendAsync("TaskDeleted", taskId);
            }
        }
    }
}
