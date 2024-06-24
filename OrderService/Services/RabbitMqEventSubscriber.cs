using OrderService.Data;
using OrderService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace OrderService.Services;

public class RabbitMqEventSubscriber(IConnection connection, IServiceScopeFactory scopeFactory) : IEventSubscriber
{
    public void Subscribe()
    {
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: "compensations", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var compensation = JsonSerializer.Deserialize<Compensation>(message);

            if (compensation == null) return;
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            var order = await context.Orders.FindAsync(compensation.OrderId);

            if (order == null) return;
            context.Orders.Remove(order);
            await context.SaveChangesAsync();
            Console.WriteLine($"Pedido compensado: {JsonSerializer.Serialize(order)}");
        };

        channel.BasicConsume(queue: "compensations", autoAck: true, consumer: consumer);

        Console.WriteLine("Pressione [enter] para sair.");
        Console.ReadLine();
    }
}