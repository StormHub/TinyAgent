using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace TinyAgents.SemanticKernel.Assistants;

internal static class DependencyInjection
{
    public static IServiceCollection AddAssistant(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(nameof(AssistantOptions));
        if (section.Exists())
        {
            services.AddOptions<AssistantOptions>()
                .BindConfiguration(nameof(AssistantOptions))
                .ValidateDataAnnotations();
            
            services.AddTransient<AssistantAgentBuilder>(provider =>
            {
                var builder = provider.GetRequiredService<IKernelBuilder>();
                var options = provider.GetRequiredService<IOptions<AssistantOptions>>();
            
                return new AssistantAgentBuilder(builder, options);
            });
        }
        else
        {
            services.AddTransient<IAssistantAgentBuilder>(provider =>
            {
                var builder = provider.GetRequiredService<IKernelBuilder>();
                return new AssistantAgentBuilder(builder, default);
            });
        }

        return services;
    }
}