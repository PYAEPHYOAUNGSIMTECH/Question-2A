using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using producer.Models;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace producer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaskController : ControllerBase
    {

        [HttpPost]
        public async void Post([FromBody] Models.Task task)
        {
            var factory = new ConnectionFactory()
            {
                //HostName = "localhost" , 
                //Port = 30724
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))
            };

            Console.WriteLine(factory.HostName + ":" + factory.Port);
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                

                using (var client = new HttpClient())
                {
                    //Models.Task p = new Models.Task { email = "eve.holt@reqres.in", password = "cityslicka",task = "Any Task" };
                    client.BaseAddress = new Uri("https://reqres.in/api/login");
                    var response = client.PostAsJsonAsync("api/person", task).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        //get token later
                        channel.QueueDeclare(queue: "taskQueue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                        string message = task.email;

                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish(exchange: "",
                                             routingKey: "taskQueue",
                                             basicProperties: null,
                                             body: body);

                    }
                    else
                        Console.Write("Error");
                }
            }
            
        }
        }
       

 }


