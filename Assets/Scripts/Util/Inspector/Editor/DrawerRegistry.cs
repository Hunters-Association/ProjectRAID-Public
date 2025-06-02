using System;
using System.Collections.Generic;

namespace ProjectRaid.EditorTools
{
    public static class DrawerRegistry
    {
        private static readonly Dictionary<Type, NonSerializedFieldDrawerBase> drawers = new()
        {
            { typeof(ShowNonSerializedFieldAttribute), ShowNonSerializedFieldDrawer.Instance }
        };

        public static bool TryGetDrawer(Type attributeType, out NonSerializedFieldDrawerBase drawer)
        {
            return drawers.TryGetValue(attributeType, out drawer);
        }
    }
}
