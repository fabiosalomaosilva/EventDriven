using BillingService.Data;
using BillingService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BillingService.Services
{
    public class RabbitMqEventSubscriber(IConnection connection, IServiceScopeFactory scopeFactory) : IEventSubscriber
    {
        public void Subscribe()
        {
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "orders", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var order = JsonSerializer.Deserialize<Order>(message);

                if (order == null) return;
                using var scope = scopeFactory.CreateScope();
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

                try
                {
                    context.Invoices.Add(invoice);
                    await context.SaveChangesAsync();

                    // Publicar evento de fatura no RabbitMQ
                    var invoiceEvent = JsonSerializer.Serialize(invoice);
                    var invoiceBody = Encoding.UTF8.GetBytes(invoiceEvent);
                    channel.QueueDeclare(queue: "invoices", durable: false, exclusive: false, autoDelete: false, arguments: null);
                    channel.BasicPublish(exchange: "", routingKey: "invoices", basicProperties: null, body: invoiceBody);
                    Console.WriteLine($"Fatura salva e evento publicado: {invoiceEvent}");
                }
                catch (Exception ex)
                {
                    // Publicar evento de compensação
                    var compensationEvent = JsonSerializer.Serialize(new { OrderId = order.Id });
                    var compensationBody = Encoding.UTF8.GetBytes(compensationEvent);
                    channel.QueueDeclare(queue: "compensations", durable: false, exclusive: false, autoDelete: false, arguments: null);
                    channel.BasicPublish(exchange: "", routingKey: "compensations", basicProperties: null, body: compensationBody);
                    Console.WriteLine($"Erro ao salvar fatura, evento de compensação publicado: {compensationEvent}");
                }
            };

            channel.BasicConsume(queue: "orders", autoAck: true, consumer: consumer);

            Console.WriteLine("Pressione [enter] para sair.");
            Console.ReadLine();
        }
    }
}
