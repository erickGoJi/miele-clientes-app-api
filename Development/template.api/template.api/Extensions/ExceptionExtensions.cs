using Microsoft.AspNetCore.Builder;
using template.api.Handler;

namespace template.api.Extensions
{
    public static class ExceptionExtensions
    {
        public static void ConfigureGlobalException(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandler>();
        }
    }
}
