namespace Bankr.BankService.Api.Endpoints.BankAccount;

/// <summary>
/// Deposit money into a bank account.
/// </summary>
public class DepositEndpoint : Endpoint<DepositCommand>
{
    public override void Configure()
    {
        Post("/bank-accounts/{bankAccountId}/deposit");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DepositCommand command, CancellationToken cancellationToken)
    {
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