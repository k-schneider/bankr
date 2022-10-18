namespace Bankr.BankService.Api.Endpoints.BankAccount;

/// <summary>
/// Deposit money into a bank account.
/// </summary>
public class DepositEndpoint : Endpoint<DepositCommand>
{
    private readonly IActorProxyFactory _actorProxyFactory;

    public DepositEndpoint(IActorProxyFactory actorProxyFactory)
    {
        _actorProxyFactory = actorProxyFactory;
    }

    public override void Configure()
    {
        Post("/bank-accounts/{bankAccountId}/deposit");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DepositCommand command, CancellationToken cancellationToken)
    {
        var bankAccount = _actorProxyFactory.CreateActorProxy<IBankAccountActor>(
            new ActorId(command.BankAccountId),
            nameof(BankAccountActor));

        await bankAccount.Deposit(
            command.Amount,
            cancellationToken);

        await SendNoContentAsync(cancellationToken);
    }
}

public class DepositSummary: Summary<DepositEndpoint>
{
    public DepositSummary()
    {
        Response(204, "deposit successful");
        Response(404, "account not found");
        Response<InternalErrorResponse>(500, "server error");
        ExampleRequest = new DepositCommand
        {
            Amount = 50m
        };
    }
}

/// <summary>
/// The deposit command.
/// </summary>
public class DepositCommand
{
    /// <summary>
    /// ID of the bank account to deposit to.
    /// </summary>
    public string BankAccountId { get; set; } = string.Empty;

    /// <summary>
    /// Amount to deposit.
    /// </summary>
    public decimal Amount { get; set; }
}