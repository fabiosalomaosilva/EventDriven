namespace OrderService.Services;

public interface IEventPublisher
{
    void Publish<T>(T @event);
}