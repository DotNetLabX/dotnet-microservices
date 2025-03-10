using Blocks.Domain;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using System.Data.Common;

namespace Blocks.EntityFrameworkCore;

public class TransactionalDispatchDomainEventsInterceptor : SaveChangesInterceptor, IAsyncDisposable
{
		private readonly TransactionOptions _transactionOptions;
		private readonly IDomainEventPublisher _eventPublisher;
		private readonly TransactionProvider _transactionProvider;
		private DbTransaction? _transaction;

		public TransactionalDispatchDomainEventsInterceptor(TransactionProvider transactionProvider, IOptions<TransactionOptions> transactionOptions, IDomainEventPublisher eventPublisher)
				=> (_transactionProvider, _transactionOptions, _eventPublisher) = (transactionProvider, transactionOptions.Value, eventPublisher);

		/// <summary>
		/// use this method to automatically start a transaction right before saving
		/// </summary>
		public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
		{
				if (_transactionOptions.UseSingleTransaction
						&& eventData.Context is not null 
						&& eventData.Context.Database.CurrentTransaction is null
						&& _transaction is null)
				{
						_transaction =  await _transactionProvider.BeginTransactionAsync(ct);
						await eventData.Context.Database.UseTransactionAsync(_transaction, ct);
				}

				return await base.SavingChangesAsync(eventData, result, ct);
		}

		/// <summary>
		/// dispacth domain events imediatly after Saving.
		/// if you opted to have a transaction, the transaction is commited after dispatching the event, so then you can save data in your event handlers in the same transaction
		/// </summary>
		public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken ct = default)
		{
				result = await base.SavedChangesAsync(eventData, result, ct);

				if(eventData.Context is not null)
						await eventData.Context.DispatchDomainEventsAsync(_eventPublisher, ct);

				if (_transaction != null)
						await _transaction.CommitAsync(ct);

				return result;
		}

		/// <summary>
		/// if you opted to have a transaction, rollback the transaction in case of failure
		/// </summary>
		public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
		{
				if (_transaction != null)
						await _transaction.RollbackAsync(cancellationToken);

				await base.SaveChangesFailedAsync(eventData, cancellationToken);
		}

		public async ValueTask DisposeAsync()
		{
				if (_transaction != null)
				{
						await _transaction.DisposeAsync();
						_transaction = null;
				}
		}
}
