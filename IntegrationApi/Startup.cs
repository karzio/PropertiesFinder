using System;
using System.Threading.Tasks;
using DatabaseConnection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IntegrationApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public class RequestLoggingMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly ILogger _logger;

            public RequestLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
            {
                _next = next;
                _logger = loggerFactory.CreateLogger<RequestLoggingMiddleware>();
            }

            public async Task Invoke(HttpContext context)
            {
                if (context.Request.Headers.ContainsKey("X-Request-ID"))
                {
                    using (DatabaseContext databaseContext = new DatabaseContext())
                    {
                        Log log = new Log()
                        {
                            Created = DateTime.Now,
                            LogContent = context.Request.Headers["X-Request-ID"]
                        };
                        databaseContext.Logs.Add(log);

                        await databaseContext.SaveChangesAsync();
                    }
                }

                await _next(context);
            }

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
