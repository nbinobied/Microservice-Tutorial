using CommandService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace CommandService.AsyncDataServices
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IEventProcessor _eventProcessor;
        private IConnection _connection;
        private IModel _channel;
        private string _queue;

        public MessageBusSubscriber(IConfiguration configuration,
                                        IEventProcessor eventProcessor)
        {
            _configuration = configuration;
            _eventProcessor = eventProcessor;

            InitialiseRabbitMQ();
        }

        private void InitialiseRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"])
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: "Trigger", type: ExchangeType.Fanout);
            _queue = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(
                queue: _queue,
                exchange: "Trigger",
                routingKey: "");

            Console.WriteLine("--> Listening on the Message Bus...");

            _connection.ConnectionShutdown += RabbitMQConnectionShutDown;
        }

        private void RabbitMQConnectionShutDown(object? sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> Connection Shutdown.");
        }

        public override void Dispose()
        {
            if (_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
            base.Dispose();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ModuleHandle, e) =>
            {
                Console.WriteLine("--> Event Received.");
                var body = e.Body;
                var message = Encoding.UTF8.GetString(body.ToArray());
                _eventProcessor.ProcessEvent(message);
            };
            _channel.BasicConsume(queue: _queue, autoAck: true, consumer = consumer);
            return Task.CompletedTask;
        }
    }
}
