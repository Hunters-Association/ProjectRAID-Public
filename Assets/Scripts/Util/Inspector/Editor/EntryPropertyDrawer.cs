using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(UIConfig.Entry), true)]
public class EntryPropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty prop)
    {
        var prefabProp = prop.FindPropertyRelative(nameof(UIConfig.Entry.prefab));
        var persistProp = prop.FindPropertyRelative(nameof(UIConfig.Entry.isPersistent));

        // 컨테이너
        var row = new VisualElement();
        row.AddToClassList("foldout-row");
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;
        row.style.marginBottom = 2;

        // 타입 라벨
        var typeLabel = new Label();
        typeLabel.style.minWidth = 0;
        typeLabel.style.flexShrink = 1;
        typeLabel.style.flexGrow = 0;
        typeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        typeLabel.style.whiteSpace = WhiteSpace.NoWrap;
        typeLabel.style.textOverflow = TextOverflow.Ellipsis;
        typeLabel.style.overflow = Overflow.Hidden;
        row.Add(typeLabel);

        var spacer = new VisualElement();
        spacer.style.flexGrow = 1;
        row.Add(spacer);

        // Prefab 필드 (ObjectField)
        var objField = new ObjectField
        {
            allowSceneObjects = false,
            bindingPath = prefabProp.propertyPath
        };
        objField.style.minWidth = 120;
        objField.style.maxWidth = 120;
        objField.style.flexShrink = 0;
        objField.style.flexGrow = 0;
        row.Add(objField);

        // Persist 토글
        var toggle = new Toggle
        {
            label = "Persist",
            bindingPath = persistProp.propertyPath
        };
        toggle.style.unityFontStyleAndWeight = FontStyle.Normal;
        toggle.style.marginLeft = 12;
        toggle.style.marginRight = 12;
        toggle.style.flexShrink = 0;
        toggle.style.flexGrow = 0;
        row.Add(toggle);

        // Prefab 변경 → 라벨 갱신
        void RefreshLabel()
        {
            var ui = prefabProp.objectReferenceValue as BaseUI;
            typeLabel.text = ui ? ui.GetType().Name : "None";
        }
        RefreshLabel();

        objField.RegisterValueChangedCallback(_ => RefreshLabel());

        return row;
    }

    // public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    //     => EditorGUIUtility.singleLineHeight;

    // public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
    // {
    //     var prefabProp   = prop.FindPropertyRelative("prefab");
    //     var persistProp  = prop.FindPropertyRelative("isPersistent");

    //     // 1) 제목을 프리팹 / 타입 이름으로
    //     string title = "None";
    //     if (prefabProp.objectReferenceValue != null)
    //     {
    //         var ui = (BaseUI)prefabProp.objectReferenceValue;
    //         title = ui.GetType().Name;
    //     }

    //     // 2) Foldout 헤더 그리기
    //     rect = EditorGUI.PrefixLabel(rect, new GUIContent(title));

    //     // 3) Prefab 필드
    //     float w = rect.width * 0.7f;
    //     Rect prefabRect  = new(rect.x, rect.y, w, rect.height);
    //     EditorGUI.PropertyField(prefabRect, prefabProp, GUIContent.none);

    //     // 4) Persistent 토글
    //     Rect toggleRect  = new(rect.x + w + 4, rect.y, rect.width - w - 4, rect.height);
    //     persistProp.boolValue = EditorGUI.ToggleLeft(toggleRect, "Persist", persistProp.boolValue);
    // }
}
