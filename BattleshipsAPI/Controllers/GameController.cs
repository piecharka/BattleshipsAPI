using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BattleshipsAPI
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public GameController()
        {
            _connectionFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage(string queueName,[FromBody] Move move)
        {
            Console.WriteLine(queueName);
            Console.WriteLine(move.X);
            Console.WriteLine(move.Y);
            try
            {
                _channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(move.ToString());

                _channel.BasicPublish(exchange: "",
                                     routingKey: queueName,
                                     basicProperties: null,
                                     body: body);

                return Ok("Message sent successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet("receive")]
        public async Task<IActionResult> ReceiveMoves()
        {
            try
            {
                _channel.QueueDeclare(queue: "moves",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    Console.WriteLine("Received move: " + json);

                    // Wysyłanie odpowiedzi JSON do klienta
                    HttpContext.Response.ContentType = "application/json";
                    HttpContext.Response.WriteAsync(json);
                };

                _channel.BasicConsume(queue: "moves",
                                     autoAck: true,
                                     consumer: consumer);

                return Ok("Listening for moves...");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

    }
}
