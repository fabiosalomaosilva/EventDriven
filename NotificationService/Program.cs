using NotificationService.Services;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);


// Configurando o EmailService
builder.Services.AddSingleton<IEmailService, EmailService>();

// Configurando RabbitMQ
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory() { HostName = "localhost" };
    return factory.CreateConnection();
});
builder.Services.AddSingleton<IEventSubscriber, RabbitMqEventSubscriber>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Iniciar o serviço de assinatura de eventos
var subscriber = app.Services.GetRequiredService<IEventSubscriber>();
subscriber.Subscribe();

app.Run();
