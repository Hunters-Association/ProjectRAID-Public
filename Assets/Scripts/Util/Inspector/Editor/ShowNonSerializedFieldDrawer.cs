using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace ProjectRaid.EditorTools
{
    public class ShowNonSerializedFieldDrawer : NonSerializedFieldDrawerBase
    {
        public static readonly ShowNonSerializedFieldDrawer Instance = new();

        public override Label DrawWithLabel(FieldInfo field, object target)
        {
            // 다중 객체 편집 감지를 위해 Selection API 활용 (또는 인스펙터에서 전달받은 SerializedObject 체크)
            bool isMultiEdit = Selection.objects.Length > 1;

            object value = field.GetValue(target);
            string displayValue = value != null ? value.ToString() : "null";

            if (isMultiEdit)
            {
                // 여러 대상에 대해 값이 다를 수 있으므로 혼합된 값 표시
                displayValue = "[Mixed]";
            }

            string labelName = ObjectNames.NicifyVariableName(field.Name);
            return new Label($"{labelName}: {displayValue}")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Normal,
                    color = new StyleColor(Color.gray),
                    marginBottom = 2,
                    marginTop = 2,
                    marginLeft = 4
                }
            };
        }
    }
}