using System;
using McNativePayment.Handlers;
using McNativePayment.Model;
using McNativePayment.Services;
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
                options.AddPolicy("AllowAllOrigins", builder => builder.AllowAnyOrigin().AllowAnyHeader());
            });

            services.AddDbContext<PaymentContext>(options => options.UseMySql(Environment.GetEnvironmentVariable("PAYMENT_DATABASE")));
            services.AddDbContext<McNativeContext>(options => options.UseMySql(Environment.GetEnvironmentVariable("MCNATIVE_DATABASE")));

            services.AddAuthentication()
                .AddScheme<IssuerAuthenticationHandlerOptions, IssuerAuthenticationHandler>(IssuerAuthenticationHandler.AUTHENTICATION_SCHEMA, op => { });

            services.AddMvc(option => option.EnableEndpointRouting = false);
            services.AddOData();

            services.AddSingleton(new PayPalService(
                Environment.GetEnvironmentVariable("PAYPAL_URL_OAUTH")
                ,Environment.GetEnvironmentVariable("PAYPAL_URL_ORDER")
                ,Environment.GetEnvironmentVariable("PAYPAL_CLIENT_ID")
                ,Environment.GetEnvironmentVariable("PAYPAL_SECRET")
                ,Environment.GetEnvironmentVariable("PAYPAL_REDIRECT")));

            services.AddHostedService<OrderService>();
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
            app.UseCors("AllowAllOrigins");

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
