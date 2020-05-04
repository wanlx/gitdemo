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
            // 1��ע�������ĵ�IOC����
            services.AddDbContext<TeamContext>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });
            // 2��ע���Ŷ�service
            services.AddScoped<ITeamService, TeamServiceImpl>();

            // 3��ע���ŶӲִ�
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

            // ע���Ŷӷ���
            // 1������consul�ͻ�������
            var consulClient = new ConsulClient(configuration =>
            {
                //1.1 �����ͻ��˺ͷ��������
                configuration.Address = new Uri("http://127.0.0.1:8500");
            });
           
            // 2������consul����ע�����
            var registration = new AgentServiceRegistration()
            {
                ID = Guid.NewGuid().ToString(),
                Name = "teamservice",
                Address = "https://localhost",
                Port = 5004,
                Check = new AgentServiceCheck
                {
                    // 3.1��consul������鳬ʱ��
                    Timeout = TimeSpan.FromSeconds(10),
                    // 3.2������ֹͣ5���ע������
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                    // 3.3��consul��������ַ
                    HTTP = "https://localhost:5004/HealthCheck",
                    // 3.4 consul���������ʱ��
                    Interval = TimeSpan.FromSeconds(3),
                }
            };

            // 1��������չ(��Ⱥ����)
            // 2������ά��
            // 3����ȫ��ҵ���޹�
            // ��װ
            
            // 3��ע�����
            consulClient.Agent.ServiceRegister(registration).Wait();

            Console.WriteLine("consulע��ɹ�");



            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
