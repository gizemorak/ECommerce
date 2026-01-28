
namespace OrderDomain;

public class Idempotency
{
    public Guid Key { get; set; }

    public EventType EventType { get; set; }
    public DateTime Created { get; set; }
}