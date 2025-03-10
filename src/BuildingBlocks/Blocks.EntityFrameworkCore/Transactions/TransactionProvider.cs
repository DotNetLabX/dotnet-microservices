using System.Data.Common;
using System.Data;

namespace Blocks.EntityFrameworkCore;

public class TransactionProvider(DbConnection _dbConnection): IDisposable
{
		private DbTransaction? _transaction;
		public async Task<DbTransaction> GetCurrentTransaction(CancellationToken ct = default)
		{
				if(_transaction is null)
						_transaction = await BeginTransactionAsync(ct);
				return _transaction;
		}

		public async Task<DbTransaction> BeginTransactionAsync(CancellationToken ct = default)
		{
				if (_transaction is not null)
						return _transaction!;

				if (_dbConnection.State == ConnectionState.Closed)
						await _dbConnection.OpenAsync(ct);

				_transaction = await _dbConnection.BeginTransactionAsync(ct);
				return _transaction;
		}

		public async Task CommitTransactionAsync(CancellationToken ct = default)
		{
				if (_transaction == null)
						throw new InvalidOperationException("No transaction has been started.");

				await _transaction.CommitAsync(ct);
		}

		public async Task RollbackTransactionAsync(CancellationToken ct = default)
		{
				if (_transaction == null)
						throw new InvalidOperationException("No transaction has been started.");

				await _transaction.RollbackAsync(ct);
		}

		public async ValueTask DisposeAsync()
		{
				if (_transaction != null)
				{
						await _transaction.DisposeAsync();
						_transaction = null;
				}

				if (_dbConnection.State == ConnectionState.Open)
						await _dbConnection.CloseAsync();
		}

		public void Dispose()
		{
				if (_transaction != null)
				{
						_transaction.Dispose();
						_transaction = null;
				}

				if (_dbConnection.State == ConnectionState.Open)
						_dbConnection.Close();

				_dbConnection.Dispose();
		}
}
