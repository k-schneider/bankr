namespace Bankr.BankService.Api.Endpoints.BankAccount;

/// <summary>
/// Withdraw money from a bank account.
/// </summary>
public class WithdrawlEndpoint : Endpoint<WithdrawlCommand>
{
    private readonly IActorProxyFactory _actorProxyFactory;

    public WithdrawlEndpoint(IActorProxyFactory actorProxyFactory)
    {
        _actorProxyFactory = actorProxyFactory;
    }

    public override void Configure()
    {
        Post("/bank-accounts/{bankAccountId}/withdrawl");
        AllowAnonymous();
    }

    public override async Task HandleAsync(WithdrawlCommand command, CancellationToken cancellationToken)
    {
        var bankAccount = _actorProxyFactory.CreateActorProxy<IBankAccountActor>(
            new ActorId(command.BankAccountId),
            nameof(BankAccountActor));

        await bankAccount.Withdrawl(
            command.Amount,
            cancellationToken);

        await SendNoContentAsync(cancellationToken);
    }
}

public class WithdrawlSummary: Summary<WithdrawlEndpoint>
{
    public WithdrawlSummary()
    {
        Response(204, "withdrawl successful");
        Response(404, "account not found");
        Response<InternalErrorResponse>(500, "server error");
        ExampleRequest = new WithdrawlCommand
        {
            Amount = 50m
        };
    }
}

/// <summary>
/// The withdrawl command.
/// </summary>
public class WithdrawlCommand
{
    /// <summary>
    /// ID of the bank account to deposit to.
    /// </summary>
    public string BankAccountId { get; set; } = string.Empty;

    /// <summary>
    /// Amount to withdraw.
    /// </summary>
    public decimal Amount { get; set; }
}