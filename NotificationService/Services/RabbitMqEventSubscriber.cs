using NotificationService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace NotificationService.Services;

public class RabbitMqEventSubscriber : IEventSubscriber
{
    private readonly IConnection _connection;
    private readonly IEmailService _emailService;

    public RabbitMqEventSubscriber(IConnection connection, IEmailService emailService)
    {
        _connection = connection;
        _emailService = emailService;
    }

    public void Subscribe()
    {
        using var channel = _connection.CreateModel();
        channel.QueueDeclare(queue: "invoices", durable: false, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var invoice = JsonSerializer.Deserialize<Invoice>(message);

            if (invoice != null)
            {
                var emailBody = $@"
                            <h1>Fatura Gerada</h1>
                            <p>Id do Pedido: {invoice.OrderId}</p>
                            <p>Produto: {invoice.ProductName}</p>
                            <p>Quantidade: {invoice.Quantity}</p>
                            <p>Preço: {invoice.Price:C}</p>
                            <p>Total: {invoice.Total:C}</p>
                            <p>Data: {invoice.CreatedAt}</p>";

                await _emailService.SendEmailAsync("fabio@arquivarnet.com.br", "Sua Fatura", emailBody);
                Console.WriteLine($"Email enviado para a fatura: {JsonSerializer.Serialize(invoice)}");
            }
        };

        channel.BasicConsume(queue: "invoices", autoAck: true, consumer: consumer);

        Console.WriteLine("Pressione [enter] para sair.");
        Console.ReadLine();
    }
}