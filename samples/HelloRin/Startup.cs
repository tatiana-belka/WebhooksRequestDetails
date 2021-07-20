using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelloWebhook.Data;
using HelloWebhook.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Webhook.Core;
using Webhook.Extensions.EntityFrameworkCore;

namespace HelloWebhook
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                .AddWebhookMvcSupport();

            services.AddGrpc();

            


            
            services.AddDbContext<MyDbContext>(options =>
            {
                
                var conn = new SqliteConnection("Data Source=MyDatabase;Mode=Memory;Cache=Shared");
                conn.Open();

                options.UseSqlite(conn);
            });

            services.AddWebhook(options =>
            {
                options.RequestRecorder.RetentionMaxRequests = 100;
                options.RequestRecorder.Excludes.Add(request => request.Path.Value.EndsWith(".js") || request.Path.Value.EndsWith(".css") || request.Path.Value.EndsWith(".svg"));
            })
                
                .AddEntityFrameworkCoreDiagnostics()
                
                .AddBodyDataTransformer<WebhookCustomContentTypeTransformer>()
                // Optional: Use Redis as storage
                //.UseRedisStorage(options =>
                //{
                //    options.ConnectionConfiguration = "localhost:6379";
                //})
            ;
        }

        
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseWebhook();
                app.UseWebhookMvcSupport();
                app.UseDeveloperExceptionPage();
                app.UseWebhookDiagnosticsHandler();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
