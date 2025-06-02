using System;
using UnityEngine;

namespace ProjectRaid.EditorTools
{
    /// <summary>
    /// 특정 필드가 지정한 값일 때만 이 필드를 인스펙터에 표시
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string targetFieldName;
        public object[] values;

        public ShowIfAttribute(string targetFieldName, params object[] values)
        {
            this.targetFieldName = targetFieldName;
            this.values = values;
        }
    }
}