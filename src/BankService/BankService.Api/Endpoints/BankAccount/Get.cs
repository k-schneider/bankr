namespace Bankr.BankService.Api.Endpoints.BankAccount;

/// <summary>
/// Get information about a specific bank account.
/// </summary>
public class GetEndpoint : Endpoint<GetQuery>
{
    public override void Configure()
    {
        Get("/bank-accounts/{bankAccountId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetQuery query, CancellationToken cancellationToken)
    {
        await SendAsync(new BankAccountDto {
            Id = query.BankAccountId,
            Balance = 123456.78m
        }, 200, cancellationToken);
    }
}

public class GetSummary: Summary<GetEndpoint>
{
    public GetSummary()
    {
        Response<BankAccountDto>(200, "bank account information", example: new BankAccountDto
        {
            Id = "123",
            Balance = 100m
        });
        Response(404, "account not found");
        Response<InternalErrorResponse>(500, "server error");
        ExampleRequest = new GetQuery
        {
            BankAccountId = "123"
        };
    }
}

/// <summary>
/// Get bank account query.
/// </summary>
public class GetQuery
{
    /// <summary>
    /// ID of the bank account.
    /// </summary>
    public string BankAccountId { get; set; } = string.Empty;
}

/// <summary>
/// A bank account.
/// </summary>
public class BankAccountDto
{
    /// <summary>
    /// ID of the bank account.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Current balance of the bank account.
    /// </summary>
    public decimal Balance { get; set; }
}