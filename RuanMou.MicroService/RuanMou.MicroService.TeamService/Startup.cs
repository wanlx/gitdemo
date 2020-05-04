using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RuanMou.MicroService.TeamService.Context;
using RuanMou.MicroService.TeamService.Repositories;
using RuanMou.MicroService.TeamService.Services;

namespace RuanMou.MicroService.TeamService
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
            // 1、注册上下文到IOC容器
            services.AddDbContext<TeamContext>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });
            // 2、注册团队service
            services.AddScoped<ITeamService, TeamServiceImpl>();

            // 3、注册团队仓储
            services.AddScoped<ITeamRepository, TeamRepository>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            // 注册团队服务
            // 1、创建consul客户端连接
            var consulClient = new ConsulClient(configuration =>
            {
                //1.1 建立客户端和服务端连接
                configuration.Address = new Uri("http://127.0.0.1:8500");
            });
           
            // 2、创建consul服务注册对象
            var registration = new AgentServiceRegistration()
            {
                ID = Guid.NewGuid().ToString(),
                Name = "teamservice",
                Address = "https://localhost",
                Port = 5004,
                Check = new AgentServiceCheck
                {
                    // 3.1、consul健康检查超时间
                    Timeout = TimeSpan.FromSeconds(10),
                    // 3.2、服务停止5秒后注销服务
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                    // 3.3、consul健康检查地址
                    HTTP = "https://localhost:5004/HealthCheck",
                    // 3.4 consul健康检查间隔时间
                    Interval = TimeSpan.FromSeconds(3),
                }
            };

            // 1、不好扩展(集群配置)
            // 2、不好维护
            // 3、完全和业务无关
            // 封装
            
            // 3、注册服务
            consulClient.Agent.ServiceRegister(registration).Wait();

            Console.WriteLine("consul注册成功");



            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
