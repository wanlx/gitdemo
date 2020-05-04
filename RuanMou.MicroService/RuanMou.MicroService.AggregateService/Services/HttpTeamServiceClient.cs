using Consul;
using Newtonsoft.Json;
using RuanMou.MicroService.TeamService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RuanMou.MicroService.AggregateService.Services
{
    /// <summary>
    /// 服务调用实现
    /// </summary>
    public class HttpTeamServiceClient : ITeamServiceClient
    {
        
        private readonly IHttpClientFactory httpClientFactory;
        
        public HttpTeamServiceClient(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<IList<Team>> GetTeams()
        {

            // 开始消费
            // 1、创建consul客户端连接
            var consulClient = new ConsulClient(configuration =>
            {
                //1.1 建立客户端和服务端连接
                configuration.Address = new Uri("http://127.0.0.1:8500");
            });
            // 2、consul查询服务,根据具体的服务名称查询
            var queryResult = await consulClient.Catalog.Service("teamservice");

            // 3、将服务进行拼接
            var list = new List<string>();
            foreach (var service in queryResult.Response)
            {
                list.Add(service.ServiceAddress + ":" + service.ServicePort );
            }


            // 1、建立请求
            HttpClient httpClient = httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.GetAsync(list[0]+"/Teams");

            // 1.1json转换成对象
            IList<Team> teams = null;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string json = await response.Content.ReadAsStringAsync();
                
                teams = JsonConvert.DeserializeObject<List<Team>>(json);
            }
            
            return teams;
        }
    }
}
