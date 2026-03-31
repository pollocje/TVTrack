namespace TVTrack.Models
{
    public class ErrorViewModel
    {
        // Simple view model for error pages, has the request ID and error type
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
