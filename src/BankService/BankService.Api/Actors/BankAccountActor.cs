using System.Text;
using System.Text.Json;

namespace Bankr.BankService.Api.Actors;

public class BankAccountActor : Actor, IBankAccountActor
{
    private readonly EventStoreClient _eventStore;
    private string _streamName = string.Empty;
    private StreamPosition _streamPosition;
    private BankAccountState _state = new();

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
                    _state.Apply(deposited);
                    break;
                case Withdrawn withdrawn:
                    _state.Apply(withdrawn);
                    break;
            }
        }

        _streamPosition = readResult.LastStreamPosition.GetValueOrDefault();
    }

    public async Task Deposit(decimal amount, CancellationToken cancellationToken = default)
    {
        var @event = new Deposited { Amount = amount };
        _state.Apply(@event);
        await AppendEvent(@event, cancellationToken);
    }

    public async Task Withdrawl(decimal amount, CancellationToken cancellationToken = default)
    {
        var @event = new Withdrawn { Amount = amount };
        _state.Apply(@event);
        await AppendEvent(@event, cancellationToken);
    }

    public Task<decimal> Balance(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_state.Balance);
    }

    private async Task AppendEvent(BankAccountEvent @event, CancellationToken cancellationToken = default)
    {
        var data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
        var typeName = @event.GetType().Name;
        var eventData = new EventData(Uuid.NewUuid(), typeName, data);

        await _eventStore.AppendToStreamAsync(
            _streamName,
            _streamPosition == default ? StreamRevision.None : StreamRevision.FromStreamPosition(_streamPosition),
            new[] { eventData },
            cancellationToken: cancellationToken);

        _streamPosition.Next();
    }

    private static BankAccountEvent DeserializeEvent(EventRecord eventRecord)
    {
        return (BankAccountEvent)JsonSerializer
            .Deserialize(
                Encoding.UTF8.GetString(eventRecord.Data.ToArray()),
                Type.GetType($"{typeof(BankAccountEvent).Namespace}.{eventRecord.EventType}")!)!;
    }
}

public abstract record BankAccountEvent
{
    public decimal Amount { get; set; }
}

public record Deposited : BankAccountEvent { }

public record Withdrawn : BankAccountEvent { }

public class BankAccountState
{
    public decimal Balance { get; set; }
    public long Version { get; set; }

    public BankAccountState Apply(Deposited @event)
    {
        Balance += @event.Amount;
        Version++;
        return this;
    }

    public BankAccountState Apply(Withdrawn @event)
    {
        Balance -= @event.Amount;
        Version++;
        return this;
    }
}