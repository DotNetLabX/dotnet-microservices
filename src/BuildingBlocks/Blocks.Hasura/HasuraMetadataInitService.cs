using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Blocks.Hasura;

public class HasuraMetadataInitService(
		HasuraMetadataService _hasuraMetadataService,
		ILogger<HasuraMetadataInitService> _logger,
		IOptions<HasuraOptions> hasuraOptions) : BackgroundService
{
		private readonly HasuraOptions _hasuraOptions = hasuraOptions.Value;
		InitState _state = InitState.Started;
		public InitState State => _state;

		public enum InitState
		{
				Started,
				TablesTracked,
				RelationsTracked,
				Failed,
				Succeeded
		}

		protected override async Task ExecuteAsync(CancellationToken ct)
		{
				if (string.IsNullOrWhiteSpace(_hasuraOptions.BaseUrl) || string.IsNullOrWhiteSpace(_hasuraOptions.AdminSecret))
				{
						_logger.LogWarning("Hasura BaseUrl/AdminSecret missing. Skipping Hasura metadata init.");
						return;
				}

				// simple retry loop (wait for Hasura to be up)
				const int maxAttempts = 5;
				for (int attempt = 1; attempt <= maxAttempts && !ct.IsCancellationRequested; attempt++)
				{
						try
						{
								if (_state == InitState.Started)
								{
										var tables = await _hasuraMetadataService.DiscoverTablesAsync(ct);
										await _hasuraMetadataService.TrackAllTablesAsync(tables, ct);
										_state = InitState.TablesTracked;
								}
								if (_state == InitState.TablesTracked)
								{
										var relationships = await _hasuraMetadataService.GetAllRelationshipsAsync(ct);
										await _hasuraMetadataService.TrackAllRelationshipsAsync(relationships, ct);
										_state = InitState.RelationsTracked;
								}

								_state = InitState.Succeeded;
								_logger.LogInformation("Hasura metadata init completed on attempt {Attempt}.", attempt);
								return;
						}
						catch (Exception ex)
						{
								if (attempt == maxAttempts)
								{
										_logger.LogError(ex, "Hasura metadata init failed after {Max} attempts.", maxAttempts);
										return;
								}
								_logger.LogWarning(ex, "Hasura metadata init attempt {Attempt}/{Max} failed. Retrying…", attempt, maxAttempts);
								await Task.Delay(TimeSpan.FromSeconds(5), ct);
						}
				}

				if (_state != InitState.Succeeded)
				{
						_state = InitState.Failed;
						throw new InvalidOperationException($"Hasura metadata init failed. State: {_state}");
				}
		}
}
