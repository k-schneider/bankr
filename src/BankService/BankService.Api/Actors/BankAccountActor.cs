namespace Bankr.BankService.Api.Actors;

public class BankAccountActor : Actor, IBankAccountActor
{
    public BankAccountActor(ActorHost host)
        : base(host) { }

    public Task Deposit(decimal amount, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Withdraw(decimal amount, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}