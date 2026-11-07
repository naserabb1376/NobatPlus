using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer;
using NobatPlusDATA.Domain;
using Parbad.Storage.Abstractions;
using GatewayPayment = Parbad.Storage.Abstractions.Models.Payment;
using GatewayTransaction = Parbad.Storage.Abstractions.Models.Transaction;
using GatewayTransactionType = Parbad.Storage.Abstractions.Models.TransactionType;

namespace NobatPlusAPI.Tools;

public sealed class DatabaseGatewayStorage : IStorage
{
    private readonly NobatPlusContext _context;
    public DatabaseGatewayStorage(NobatPlusContext context) => _context = context;

    public IQueryable<GatewayPayment> Payments => _context.GatewayPaymentRecords.AsNoTracking().Select(x => new GatewayPayment
    {
        Id = x.ID, TrackingNumber = x.TrackingNumber, Amount = x.Amount, Token = x.Token,
        TransactionCode = x.TransactionCode, GatewayName = x.GatewayName, GatewayAccountName = x.GatewayAccountName,
        IsCompleted = x.IsCompleted, IsPaid = x.IsPaid
    });

    public IQueryable<GatewayTransaction> Transactions => _context.GatewayTransactionRecords.AsNoTracking().Select(x => new GatewayTransaction
    {
        Id = x.ID, PaymentId = x.GatewayPaymentRecordID, Amount = x.Amount, Type = (GatewayTransactionType)x.Type,
        IsSucceed = x.IsSucceed, Message = x.Message, AdditionalData = x.AdditionalData
    });

    public async Task CreatePaymentAsync(GatewayPayment payment, CancellationToken cancellationToken = default)
    {
        var row = new GatewayPaymentRecord();
        Copy(payment, row);
        _context.GatewayPaymentRecords.Add(row);
        await _context.SaveChangesAsync(cancellationToken);
        payment.Id = row.ID;
    }

    public async Task UpdatePaymentAsync(GatewayPayment payment, CancellationToken cancellationToken = default)
    {
        var row = await _context.GatewayPaymentRecords.SingleAsync(x => x.ID == payment.Id, cancellationToken);
        // A delayed failed callback must never downgrade an already verified gateway payment.
        if (row.IsPaid && !payment.IsPaid) return;
        Copy(payment, row);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void Copy(GatewayPayment source, GatewayPaymentRecord target)
    {
        target.TrackingNumber = source.TrackingNumber; target.Amount = source.Amount; target.Token = source.Token;
        target.TransactionCode = source.TransactionCode; target.GatewayName = source.GatewayName;
        target.GatewayAccountName = source.GatewayAccountName; target.IsCompleted = source.IsCompleted; target.IsPaid = source.IsPaid;
    }

    public async Task DeletePaymentAsync(GatewayPayment payment, CancellationToken cancellationToken = default)
    {
        // Keep the gateway audit trail. Parbad can still query completed records.
        await Task.CompletedTask;
    }

    public async Task CreateTransactionAsync(GatewayTransaction transaction, CancellationToken cancellationToken = default)
    {
        var row = new GatewayTransactionRecord();
        Copy(transaction, row);
        _context.GatewayTransactionRecords.Add(row);
        await _context.SaveChangesAsync(cancellationToken);
        transaction.Id = row.ID;
    }

    public async Task UpdateTransactionAsync(GatewayTransaction transaction, CancellationToken cancellationToken = default)
    {
        var row = await _context.GatewayTransactionRecords.SingleAsync(x => x.ID == transaction.Id, cancellationToken);
        Copy(transaction, row);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static void Copy(GatewayTransaction source, GatewayTransactionRecord target)
    {
        target.GatewayPaymentRecordID = source.PaymentId; target.Amount = source.Amount; target.Type = (int)source.Type;
        target.IsSucceed = source.IsSucceed; target.Message = source.Message; target.AdditionalData = source.AdditionalData;
    }

    public Task DeleteTransactionAsync(GatewayTransaction transaction, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<GatewayPayment> GetPaymentByTrackingNumberAsync(long trackingNumber, CancellationToken cancellationToken = default) => Payments.SingleOrDefaultAsync(x => x.TrackingNumber == trackingNumber, cancellationToken)!;
    public Task<GatewayPayment> GetPaymentByTokenAsync(string paymentToken, CancellationToken cancellationToken = default) => Payments.SingleOrDefaultAsync(x => x.Token == paymentToken, cancellationToken)!;
    public Task<bool> DoesPaymentExistAsync(long trackingNumber, CancellationToken cancellationToken = default) => Payments.AnyAsync(x => x.TrackingNumber == trackingNumber, cancellationToken);
    public Task<bool> DoesPaymentExistAsync(string paymentToken, CancellationToken cancellationToken = default) => Payments.AnyAsync(x => x.Token == paymentToken, cancellationToken);
    public Task<List<GatewayTransaction>> GetTransactionsAsync(GatewayPayment payment, CancellationToken cancellationToken = default) => Transactions.Where(x => x.PaymentId == payment.Id).ToListAsync(cancellationToken);
}
