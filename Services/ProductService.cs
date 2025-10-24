using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

public class ProductService
{
    private readonly IConfiguration _configuration;
    private readonly string _rabbitHost;
    private readonly string _rabbitUser;
    private readonly string _rabbitPass;
    private readonly string _queueName;

    public ProductService(IConfiguration configuration)
    {
        _configuration = configuration;
        _rabbitHost = _configuration["RabbitMQ:Host"] ?? "localhost";
        _rabbitUser = _configuration["RabbitMQ:User"] ?? "guest";
        _rabbitPass = _configuration["RabbitMQ:Pass"] ?? "guest";
        _queueName = _configuration["RabbitMQ:Queue"] ?? "add-product-quantity";
    }

    // Envia id do produto e quantidade para uma fila RabbitMQ
    public async Task EnqueueProductQuantityAsync(int productId, int quantity)
    {
        await Task.Run(() =>
        {
            var factory = new ConnectionFactory()
            {
                HostName = _rabbitHost,
                UserName = _rabbitUser,
                Password = _rabbitPass,
                DispatchConsumersAsync = false
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var payload = new { ProductId = productId, Quantity = quantity };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));

            var props = channel.CreateBasicProperties();
            props.Persistent = true;

            channel.BasicPublish(exchange: "",
                                 routingKey: _queueName,
                                 basicProperties: props,
                                 body: body);
        });
    }
}