namespace Bankr.BankService.Api.Endpoints.BankAccounts;

/// <summary>
/// Get balance for a specific bank account.
/// </summary>
public class GetBalanceEndpoint : Endpoint<GetBalanceQuery>
{
    private readonly IActorProxyFactory _actorProxyFactory;

    public GetBalanceEndpoint(IActorProxyFactory actorProxyFactory)
    {
        _actorProxyFactory = actorProxyFactory;
    }

    public override void Configure()
    {
        Get("/bank-accounts/{bankAccountId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetBalanceQuery query, CancellationToken cancellationToken)
    {
        var bankAccount = _actorProxyFactory.CreateActorProxy<IBankAccountActor>(
            new ActorId(query.BankAccountId),
            nameof(BankAccountActor));

        var balance = await bankAccount.Balance(cancellationToken);

        await SendOkAsync(
            new BalanceDto
            {
                Balance = balance
            },
            cancellationToken);
    }
}

public class GetBalanceSummary: Summary<GetBalanceEndpoint>
{
    public GetBalanceSummary()
    {
        Response<BalanceDto>(200, "balance information", example: new BalanceDto
        {
            Balance = 100m
        });
        Response(404, "account not found");
        Response<InternalErrorResponse>(500, "server error");
        ExampleRequest = new GetBalanceQuery
        {
            BankAccountId = "123"
        };
    }
}

/// <summary>
/// Get bank account query.
/// </summary>
public class GetBalanceQuery
{
    /// <summary>
    /// ID of the bank account.
    /// </summary>
    public string BankAccountId { get; set; } = string.Empty;
}

/// <summary>
/// Balance information for a specific bank account.
/// </summary>
public class BalanceDto
{
    /// <summary>
    /// Current balance of the bank account.
    /// </summary>
    public decimal Balance { get; set; }
}