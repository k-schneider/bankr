namespace Bankr.BankService.Api.Endpoints.BankAccount;

/// <summary>
/// Withdraw money from a bank account.
/// </summary>
public class WithdrawEndpoint : Endpoint<WithdrawCommand>
{
    public override void Configure()
    {
        Post("/bank-accounts/{bankAccountId}/withdraw");
        AllowAnonymous();
    }

    public override async Task HandleAsync(WithdrawCommand command, CancellationToken cancellationToken)
    {
        await SendNoContentAsync(cancellationToken);
    }
}

public class WithdrawSummary: Summary<WithdrawEndpoint>
{
    public WithdrawSummary()
    {
        Response(204, "withdraw successful");
        Response(404, "account not found");
        Response<InternalErrorResponse>(500, "server error");
        ExampleRequest = new WithdrawCommand
        {
            Amount = 50m
        };
    }
}

/// <summary>
/// The withdraw command.
/// </summary>
public class WithdrawCommand
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