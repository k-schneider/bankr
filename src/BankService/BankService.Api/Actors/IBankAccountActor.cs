

public interface IBankAccountActor : IActor
{
    Task<decimal> Balance(CancellationToken cancellationToken = default);
    Task Deposit(decimal amount, CancellationToken cancellationToken = default);
    Task Withdrawl(decimal amount, CancellationToken cancellationToken = default);
}