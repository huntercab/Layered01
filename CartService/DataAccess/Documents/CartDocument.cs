using System;
using System.Collections.Generic;
using System.Text;

namespace CartService.DataAccess.Documents
{
    public class CartDocument
    {
        public Guid Id { get; set; }
        public List<CartItemDocument> Items { get; set; } = new();
    }
}
