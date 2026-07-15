using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Contracts
{
    public sealed record ProductImageIntegrationModel(
        string Url,
        string AltText);
}
