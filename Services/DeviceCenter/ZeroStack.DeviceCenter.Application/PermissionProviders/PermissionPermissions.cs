﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroStack.DeviceCenter.Application.PermissionProviders
{
    public static class PermissionPermissions
    {
        public const string GroupName = "PermissionManager";

        public static class Permissions
        {
            public const string Default = GroupName + ".Permissions";
            public const string Get = Default + ".Get";
            public const string Edit = Default + ".Edit";
        }
    }
}
