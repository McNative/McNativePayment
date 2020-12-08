using System;
using McNativePayment.Handlers;
using McNativePayment.Model;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace McNativePayment
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            services.AddDbContext<PaymentContext>(options => options.UseMySql(Environment.GetEnvironmentVariable("PRETRONIC_DATABASE")));

            services.AddAuthentication()
                .AddScheme<IssuerAuthenticationHandlerOptions, IssuerAuthenticationHandler>(IssuerAuthenticationHandler.AUTHENTICATION_SCHEMA, op => { });

            services.AddMvc(option => option.EnableEndpointRouting = false);
            services.AddOData();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseMvc(routeBuilder =>
            {
                routeBuilder.Select().Filter().Expand().MaxTop(250).OrderBy().Count();
                routeBuilder.EnableDependencyInjection();
                routeBuilder.MapODataServiceRoute("api", "", EdmModel.BuildEdmModel());
            });
        }
    }
}
