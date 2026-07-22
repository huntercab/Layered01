using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Authorization
{
    public static class StoreRoles
    {
        public const string Manager = "Manager";
        public const string StoreCustomer = "StoreCustomer";
    }

    public static class StorePolicies
    {
        public const string Read = "Store.Read";
        public const string Create = "Store.Create";
        public const string Update = "Store.Update";
        public const string Delete = "Store.Delete";
        public const string CartAccess = "Cart.Access";
    }
}
