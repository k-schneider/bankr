using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Bankr.BankService.Api.Actors;

public class BankAccountActor : Actor, IBankAccountActor
{
    private readonly EventStoreClient _eventStore;
    private string _streamName = string.Empty;
    private decimal _balance;
    private long _version = -1;

    public BankAccountActor(ActorHost host, EventStoreClient eventStore)
        : base(host)
    {
        _eventStore = eventStore;
    }

    protected override async Task OnActivateAsync()
    {
        _streamName = $"{GetType().Name}-{this.Id}";

        // Hydrate
        var readResult = _eventStore.ReadStreamAsync(
            Direction.Forwards,
            _streamName,
            StreamPosition.Start);

        if (await readResult.ReadState == ReadState.StreamNotFound)
        {
            return;
        }

        var events = await readResult.ToListAsync();

        foreach (var @event in events)
        {
            switch (DeserializeEvent(@event.Event))
            {
                case Deposited deposited:
                    Apply(deposited);
                    break;
                case Withdrawn withdrawn:
                    Apply(withdrawn);
                    break;
            }
        }
    }

    public async Task Deposit(decimal amount, CancellationToken cancellationToken = default)
    {
        var @event = new Deposited { Amount = amount };
        Apply(@event);
        await RaiseEvent(@event, cancellationToken);
    }

    public async Task Withdrawl(decimal amount, CancellationToken cancellationToken = default)
    {
        var @event = new Withdrawn { Amount = amount };
        Apply(@event);
        await RaiseEvent(@event, cancellationToken);
    }

    public Task<decimal> Balance(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_balance);
    }

    private async Task RaiseEvent(BankAccountEvent @event, CancellationToken cancellationToken = default)
    {
        var eventData = ToEventData(Guid.NewGuid(), @event, new Dictionary<string, object>());

        await _eventStore.AppendToStreamAsync(
            _streamName,
            _version == 0 ? StreamRevision.None : StreamRevision.FromInt64(_version - 1),
            new[] { eventData },
            cancellationToken: cancellationToken);
    }

    private void Apply(Deposited @event)
    {
        _balance += @event.Amount;
        _version++;
    }

    private void Apply(Withdrawn @event)
    {
        _balance -= @event.Amount;
        _version++;
    }

    private static EventData ToEventData(Guid eventId, object @event, IDictionary<string, object> headers)
    {
        var data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));

        var eventHeaders = new Dictionary<string, object>(headers)
        {
            {
                "EventClrType", @event.GetType().AssemblyQualifiedName!
            }
        };
        var metadata = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(eventHeaders));
        var typeName = @event.GetType().Name;

        return new EventData(Uuid.FromGuid(eventId), typeName, data, metadata);
    }

    private static BankAccountEvent DeserializeEvent(EventRecord eventRecord)
    {
        var metadata = JsonSerializer.Deserialize<JsonObject>(
            Encoding.UTF8.GetString(eventRecord.Metadata.ToArray()))!;

        var eventClrType = metadata["EventClrType"]!.GetValue<string>();

        return (BankAccountEvent)JsonSerializer
            .Deserialize(
                Encoding.UTF8.GetString(eventRecord.Data.ToArray()),
                Type.GetType(eventClrType)!)!;
    }
}

public abstract record BankAccountEvent
{
    public decimal Amount { get; set; }
}

public record Deposited : BankAccountEvent { }

public record Withdrawn : BankAccountEvent { }