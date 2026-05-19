using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoAI.Application.Abstractions;
using TodoAI.Infrastructure.Persistence;
using TodoAI.Infrastructure.Repositories;

namespace TodoAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=todoai.db";

        services.AddDbContext<TodoAiDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<ITodoItemRepository, TodoItemRepository>();

        return services;
    }
}
