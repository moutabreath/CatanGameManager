using CatanGameManager.CommonObjects.Config;
using CatanGameManager.Core;
using CatanGameManager.Interfaces;
using CatanGameManager.Interfaces.PersistanceInterfaces;
using CatanGamePersistence.MongoDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatanGameManager.API
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
            CatanManagerConfig riscoInfraPortalConfig = new CatanManagerConfig();
            Configuration.GetSection("RiscoInfraPortalConfig").Bind(riscoInfraPortalConfig);

            services.AddTransient<ICatanUserPersist, CatanUserMongoPersist>();
            services.AddTransient<ICatanGamePersist, CatanGameMongoPersist>();
            services.AddTransient<ICatanUserBusinessLogic, CatanUserBusinessLogic>();
            services.AddTransient<ICatanGameBusinessLogic, CatanGameBusinessLogic>();

            services.AddSingleton<IConfiguration>(Configuration);
            services.Configure<CatanManagerConfig>(options => Configuration.GetSection("CatanHelperConfig").Bind(options));

            // Add service and create Policy with options
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });
            
          
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
