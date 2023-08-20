using PlatformService.Dtos;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;

            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPOrt"])
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(exchange: "Triiger", type: ExchangeType.Fanout);
                _connection.ConnectionShutdown += RabbitMQConnectionShutdown;
            }
            catch (Exception e)
            {
                Console.WriteLine($"--> Could not connect to the Message Bus: {e.Message}");
                throw;
            }
        }

        public void PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
        {
            var message = JsonSerializer.Serialize(platformPublishedDto);
            if (_connection.IsOpen)
            {
                Console.WriteLine($"--> RabbitMQ Connection Open, Sending Message...");
                SendMessage(message);
            }
            else
            {
                Console.WriteLine($"--> RabbitMQ Connection is Closed, Not Sending Message...");
            }
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "Trigger", routingKey: "", basicProperties: null, body: body);
            Console.WriteLine($"--> We have Sent {message}");
        }

        public void Dispose()
        {
            Console.WriteLine($"--> Message Bus Disposed.");
            if (_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
        }

        private void RabbitMQConnectionShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            Console.WriteLine($"--> RabbitMQ Connection Shutdown.");
        }
    }
}
