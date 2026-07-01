namespace CartService.API.Contracts
{
    public sealed class ImageRequest
    {
        public string Url { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
    }
}
