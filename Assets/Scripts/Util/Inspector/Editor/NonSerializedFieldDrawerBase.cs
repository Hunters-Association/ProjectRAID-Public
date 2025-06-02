using System.Reflection;
using UnityEngine.UIElements;

namespace ProjectRaid.EditorTools
{
    public abstract class NonSerializedFieldDrawerBase
    {
        public abstract Label DrawWithLabel(FieldInfo field, object target);
    }
}
