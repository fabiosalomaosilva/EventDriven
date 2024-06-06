using BillingService.Data;
using BillingService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BillingService.Services;

public class RabbitMqEventSubscriber(IConnection connection, IServiceScopeFactory scopeFactory) : IEventSubscriber
{
    public void Subscribe()
    {
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: "orders", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, deliverEventArgs) =>
        {
            var body = deliverEventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var order = JsonSerializer.Deserialize<Order>(message);

            if (order == null) return;

            // Create a scope to resolve scoped services
            using var scope = scopeFactory.CreateScope();
            // Create a context instance to interact with the database
            var context = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductName = order.ProductName,
                Quantity = order.Quantity,
                Price = order.Price,
                CreatedAt = DateTime.UtcNow
            };

            context.Invoices.Add(invoice);
            await context.SaveChangesAsync();
            Console.WriteLine($"Invoice generated: {JsonSerializer.Serialize(invoice)}");
        };

        channel.BasicConsume(queue: "orders", autoAck: true, consumer: consumer);

        Console.WriteLine("Press [enter] to exit.");
        Console.ReadLine();
    }
}