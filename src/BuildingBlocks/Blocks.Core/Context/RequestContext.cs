namespace Blocks.Core.Context;

public class RequestContext
{
		public string? CorrelationId { get; set; }
		public DateTime StartedOn { get; set; } = DateTime.UtcNow;
		public string? RemoteIp { get; set; }

		public bool IsUpload { get; set; } = false;
		public bool IsDownload { get; set; } = false;
		public bool IsFileTransfer => IsUpload || IsDownload;

}
