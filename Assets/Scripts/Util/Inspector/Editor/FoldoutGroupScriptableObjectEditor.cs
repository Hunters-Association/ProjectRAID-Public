// using System;
// using System.Collections.Generic;
// using System.Reflection;
// using UnityEditor;
// using UnityEditor.UIElements;
// using UnityEngine;
// using UnityEngine.UIElements;

// namespace ProjectRaid.EditorTools
// {
//     /// <summary>
//     /// ScriptableObject 전용 FoldoutGroup 기반 커스텀 인스펙터
//     /// </summary>
//     [CustomEditor(typeof(ScriptableObject), true)]
//     [CanEditMultipleObjects]
//     public class FoldoutGroupScriptableObjectEditor : Editor
//     {
//         [SerializeField] private StyleSheet editorStyleSheet;

//         private readonly Dictionary<string, List<VisualElement>> groupedElements = new();
//         private readonly Dictionary<string, FoldoutGroupAttribute> groupAttributes = new();
//         private readonly List<VisualElement> ungroupedElements = new();

//         public override VisualElement CreateInspectorGUI()
//         {
//             groupedElements.Clear();
//             groupAttributes.Clear();
//             ungroupedElements.Clear();

//             var root = new VisualElement();

//             // 스타일 적용
//             if (editorStyleSheet != null)
//             {
//                 root.styleSheets.Add(editorStyleSheet);
//             }

//             // "Script" 필드 표시 (readonly)
//             var iterator = serializedObject.GetIterator();
//             iterator.NextVisible(true);
//             var scriptField = new PropertyField(iterator.Copy()) { name = "Script" };
//             scriptField.SetEnabled(false);
//             root.Add(scriptField);

//             // 리플렉션으로 필드 읽기
//             var targetType = target.GetType();
//             var fields = targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

//             FoldoutGroupAttribute currentGroup = null;

//             foreach (var field in fields)
//             {
//                 if (field.Name == "m_Script") continue;

//                 var prop = serializedObject.FindProperty(field.Name);
//                 if (prop == null) continue;

//                 var groupAttr = field.GetCustomAttribute<FoldoutGroupAttribute>();
//                 var showIfAttr = field.GetCustomAttribute<ShowIfAttribute>();

//                 if (showIfAttr != null)
//                 {
//                     var targetField = targetType.GetField(showIfAttr.targetFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
//                     if (targetField != null)
//                     {
//                         object actualValue = targetField.GetValue(target);
//                         bool match = false;

//                         foreach (var v in showIfAttr.values)
//                         {
//                             if (actualValue.Equals(v))
//                             {
//                                 match = true;
//                                 break;
//                             }
//                         }

//                         if (!match)
//                             continue;
//                     }
//                 }

//                 var propField = new PropertyField(prop);

//                 if (groupAttr != null)
//                 {
//                     currentGroup = groupAttr;
//                     AddToGroup(groupAttr.groupName, groupAttr, propField);
//                 }
//                 else
//                 {
//                     if (currentGroup != null && currentGroup.groupAllFieldsUntilNext)
//                     {
//                         AddToGroup(currentGroup.groupName, currentGroup, propField);
//                     }
//                     else
//                     {
//                         currentGroup = null;
//                         ungroupedElements.Add(propField);
//                     }
//                 }
//             }

//             // Foldout 그리기
//             foreach (var groupName in groupedElements.Keys)
//             {
//                 var attr = groupAttributes[groupName];
//                 var foldout = new Foldout
//                 {
//                     text = groupName,
//                     value = !attr.closedByDefault
//                 };

//                 Color color = attr.colorEnum.HasValue
//                     ? (Color)typeof(ExtendedColors).GetField(attr.colorEnum.Value.ToString())?.GetValue(null)
//                     : ExtendedColors.GetColorAt(attr.colorIndex);

//                 foldout.style.borderLeftWidth = 3;
//                 foldout.style.borderLeftColor = color;
//                 foldout.style.marginBottom = 6;
//                 foldout.style.paddingLeft = 0;

//                 foreach (var element in groupedElements[groupName])
//                 {
//                     foldout.Add(element);
//                 }

//                 root.Add(foldout);
//             }

//             foreach (var element in ungroupedElements)
//             {
//                 root.Add(element);
//             }

//             return root;
//         }

//         private void AddToGroup(string name, FoldoutGroupAttribute attr, VisualElement element)
//         {
//             if (!groupedElements.ContainsKey(name))
//             {
//                 groupedElements[name] = new List<VisualElement>();
//                 groupAttributes[name] = attr;
//             }
//             groupedElements[name].Add(element);
//         }
//     }
// }
