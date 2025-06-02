using System;
using System.Reflection;

namespace ProjectRaid.Extensions
{
    public static class ReflectionExtensions
    {
        public static MemberInfo GetFieldOrProperty(this Type type, string name)
        {
            return (MemberInfo)type.GetProperty(name) ?? type.GetField(name);
        }

        public static object GetValue(this MemberInfo member, object target)
        {
            return member is PropertyInfo prop ? prop.GetValue(target) :
                   member is FieldInfo field ? field.GetValue(target) : null;
        }

        public static Type GetMemberType(this MemberInfo member)
        {
            return member is PropertyInfo prop ? prop.PropertyType :
                   member is FieldInfo field ? field.FieldType : null;
        }
    }
}