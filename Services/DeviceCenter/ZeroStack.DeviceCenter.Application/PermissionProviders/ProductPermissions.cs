﻿namespace ZeroStack.DeviceCenter.Application.PermissionProviders
{
    public static class ProductPermissions
    {
        public const string GroupName = "ProductManager";

        public static class Products
        {
            public const string Default = GroupName + ".Products";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }
    }
}
