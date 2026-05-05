using System;
using System.Collections.Generic;
using System.Text;

namespace CartService.Domain.ValueObjects
{
    public class ImageInfo
    {
        public string Url { get; }
        public string AltText { get; }

        private ImageInfo(string  url, string altText)
        {
            Url = url;
            AltText = altText; 
        }

        public static ImageInfo Create(string url, string altText)
        {
            if(string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Image Url is required", nameof(url));

            if(!Uri.TryCreate(url, UriKind.Absolute, out _))
                throw new ArgumentException("Invalid Image Url", nameof(url));

            return new ImageInfo(url.Trim(), altText?.Trim() ?? string.Empty);
        }

    }
}
