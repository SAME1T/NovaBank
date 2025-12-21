using NovaBank.Application.Common.Results;
using NovaBank.Contracts.Transactions;

namespace NovaBank.Application.Transfers;

public interface ITransfersService
{
    Task<Result<TransferResponse>> TransferInternalAsync(TransferInternalRequest request, CancellationToken ct = default);
    Task<Result<TransferResponse>> TransferExternalAsync(TransferExternalRequest request, CancellationToken ct = default);
    Task<Result<ReverseTransferResponse>> ReverseTransferAsync(ReverseTransferRequest req, CancellationToken ct = default);
}

