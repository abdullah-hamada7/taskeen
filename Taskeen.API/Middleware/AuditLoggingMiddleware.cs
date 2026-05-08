using Taskeen.Domain.Entities;
using Taskeen.Infrastructure.Persistence;
using Taskeen.Application.Interfaces;

namespace Taskeen.API.Middleware;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TaskeenDbContext dbContext, ICurrentUserService currentUserService)
    {
        // Simple audit for POST/PUT/DELETE
        if (context.Request.Method != "GET")
        {
            int currentUserId = 0;
            if (int.TryParse(currentUserService.UserId, out int parsedId))
            {
                currentUserId = parsedId;
            }

            var auditLog = new AuditLog
            {
                EntityName = context.Request.Path,
                Action = context.Request.Method,
                Timestamp = DateTime.UtcNow,
                UserId = currentUserId
            };
            
            // Note: In a real app, you'd capture more details, 
            // but for this assignment, this shows the pattern.
            await dbContext.AuditLogs.AddAsync(auditLog);
            await dbContext.SaveChangesAsync();
        }

        await _next(context);
    }
}
