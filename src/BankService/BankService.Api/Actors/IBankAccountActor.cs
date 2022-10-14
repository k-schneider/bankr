

public interface IBankAccountActor : IActor
{
    Task Deposit(decimal amount, CancellationToken cancellationToken = default);
    Task Withdraw(decimal amount, CancellationToken cancellationToken = default);
}