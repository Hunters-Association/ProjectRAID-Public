using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectRaid.EditorTools
{
    /// <summary>
    /// MonoBehaviour / ScriptableObject 공통 Foldout + ShowIf 인스펙터
    /// </summary>
    [CustomEditor(typeof(UnityEngine.Object), true)]
    [CanEditMultipleObjects]
    public class FoldoutGroupEditor : Editor
    {
        [SerializeField] private StyleSheet editorStyleSheet;

        private readonly Dictionary<string, List<VisualElement>> grouped = new();
        private readonly Dictionary<string, FoldoutGroupAttribute> groupAttrs = new();
        private readonly List<VisualElement> unGrouped = new();
        private readonly Dictionary<FieldInfo, Label> runtimeLabels = new();

        private VisualElement cachedRoot;

        // 리플렉션 캐싱: 대상 타입별 필드 정보 캐시
        private static readonly Dictionary<Type, FieldInfo[]> fieldCache = new();

        // Refresh 호출 스케줄링 플래그 (Debounce)
        private bool refreshScheduled = false;
        private bool isRefreshing = false;
        
        #region LIFECYCLE
        private void OnEnable() => EditorApplication.update += UpdateRuntimeLabels;
        private void OnDisable() => EditorApplication.update -= UpdateRuntimeLabels;
        #endregion

        public override VisualElement CreateInspectorGUI()
        {
            if (target is not MonoBehaviour && target is not ScriptableObject)
                return base.CreateInspectorGUI();
                
            var root = new VisualElement();
            cachedRoot = root;

            if (editorStyleSheet != null) root.styleSheets.Add(editorStyleSheet);

            Refresh(root);
            return root;
        }

        /// <summary>
        /// Inspector 재구성 + 바인딩 + 이벤트 등록
        /// </summary>
        private void Refresh(VisualElement root)
        {
            if (isRefreshing) return;
            isRefreshing = true;
            refreshScheduled = false;

            serializedObject.Update();
            RebuildInspector(root);
            root.Bind(serializedObject);
            RegisterShowIfTargetWatchers(root);

            isRefreshing = false;
        }

        /// <summary>
        /// Inspector UI 재구성
        /// - 단일 객체: 기존 커스텀 그룹화 및 ShowIf 처리
        /// - 다중 객체: 혼합된(값이 통일되지 않은) 옵션은 렌더링하지 않음
        /// </summary>
        private void RebuildInspector(VisualElement root)
        {
            root.Clear();
            grouped.Clear();
            groupAttrs.Clear();
            unGrouped.Clear();
            runtimeLabels.Clear();

            // 첫 번째 "Script" 필드는 항상 표시
            var iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            var scriptField = new PropertyField(iterator.Copy()) { name = "Script" };
            scriptField.SetEnabled(false);
            root.Add(scriptField);

            Type targetType = target.GetType();
            if (!fieldCache.TryGetValue(targetType, out FieldInfo[] fields))
            {
                fields = targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                fieldCache[targetType] = fields;
            }

            FoldoutGroupAttribute currentGroup = null;

            // SerializedProperty 반복자를 통해 기본 필드 순회
            while (iterator.NextVisible(false))
            {
                if (iterator.name == "m_Script")
                    continue;

                var fieldInfo = Array.Find(fields, f => f.Name == iterator.name);
                var fGroup = fieldInfo?.GetCustomAttribute<FoldoutGroupAttribute>();
                var showIf = fieldInfo?.GetCustomAttribute<ShowIfAttribute>();

                // ShowIf 조건 검사
                if (showIf != null && !IsShowIfMatched(showIf))
                    continue;

                // 혼합 값이 있는 경우 IMGUIContainer를 사용해 기본 EditorGUI로 표시
                if (serializedObject.isEditingMultipleObjects && iterator.hasMultipleDifferentValues)
                {
                    var mixedProperty = iterator.Copy();
                    IMGUIContainer mixedField = new(() =>
                    {
                        // 혼합 상태 표시 활성화
                        EditorGUI.showMixedValue = true;
                        EditorGUILayout.PropertyField(mixedProperty, true);
                        EditorGUI.showMixedValue = false;
                    })
                    {
                        name = iterator.name
                    };

                    // 그룹핑 여부에 따라 추가
                    if (fGroup != null)
                    {
                        currentGroup = fGroup;
                        AddToGroup(fGroup.groupName, fGroup, mixedField);
                    }
                    else if (currentGroup != null && currentGroup.groupAllFieldsUntilNext)
                    {
                        AddToGroup(currentGroup.groupName, currentGroup, mixedField);
                    }
                    else
                    {
                        unGrouped.Add(mixedField);
                        currentGroup = null;
                    }
                    continue;
                }

                var propField = new PropertyField(iterator.Copy())
                {
                    name = iterator.name
                };

                if (fGroup != null)
                {
                    currentGroup = fGroup;
                    AddToGroup(fGroup.groupName, fGroup, propField);
                }
                else if (currentGroup != null && currentGroup.groupAllFieldsUntilNext)
                {
                    AddToGroup(currentGroup.groupName, currentGroup, propField);
                }
                else
                {
                    unGrouped.Add(propField);
                    currentGroup = null;
                }
            }

            // 비 직렬화 필드 처리 ([ShowNonSerializedField])
            FoldoutGroupAttribute currentNSGroup = null;
            foreach (var field in fields)
            {
                if (serializedObject.isEditingMultipleObjects && !HasUniformValue(field))
                    continue;

                foreach (var attr in field.GetCustomAttributes(true))
                {
                    if (attr is Attribute a && DrawerRegistry.TryGetDrawer(a.GetType(), out var drawer))
                    {
                        var container = new VisualElement();
                        var label = drawer.DrawWithLabel(field, target);
                        runtimeLabels[field] = label;
                        container.Add(label);

                        var gAttr = field.GetCustomAttribute<FoldoutGroupAttribute>();
                        if (gAttr != null)
                        {
                            currentNSGroup = gAttr;
                            AddToGroup(gAttr.groupName, gAttr, container);
                        }
                        else if (currentNSGroup != null && currentNSGroup.groupAllFieldsUntilNext)
                        {
                            AddToGroup(currentNSGroup.groupName, currentNSGroup, container);
                        }
                        else
                        {
                            unGrouped.Add(container);
                            currentNSGroup = null;
                        }
                    }
                }
            }

            // Foldout 그룹별로 UI 렌더링 (다중 선택 시에는 그룹을 모두 열어서 표시)
            foreach (var gName in grouped.Keys)
            {
                var attr = groupAttrs[gName];
                bool foldoutState = serializedObject.isEditingMultipleObjects || !attr.closedByDefault;
                var foldout = new Foldout { text = gName, value = foldoutState };
                Color color = attr.colorEnum.HasValue
                    ? (Color)typeof(ExtendedColors).GetField(attr.colorEnum.Value.ToString())?.GetValue(null)
                    : ExtendedColors.GetColorAt(attr.colorIndex);

                foldout.style.borderLeftWidth = 3;
                foldout.style.borderRightWidth = 0;
                foldout.style.borderLeftColor = color;
                foldout.style.marginBottom = 6;
                foldout.style.paddingLeft = 0;
                foldout.style.paddingRight = 0;

                foreach (var element in grouped[gName])
                    foldout.Add(element);
                root.Add(foldout);
            }

            // 그룹에 속하지 않는 나머지 요소 렌더링
            foreach (var element in unGrouped)
                root.Add(element);
        }

        /// <summary>
        /// ShowIf 조건 처리:
        /// 다중 객체 편집 중일 때 혼합 옵션 기본 UI로 표시
        /// </summary>
        private bool IsShowIfMatched(ShowIfAttribute attr)
        {
            var prop = serializedObject.FindProperty(attr.targetFieldName);
            if (prop == null)
                return false;

            if (serializedObject.isEditingMultipleObjects && prop.hasMultipleDifferentValues)
                return false;

            foreach (var v in attr.values)
            {
                switch (prop.propertyType)
                {
                    case SerializedPropertyType.Enum:
                    case SerializedPropertyType.Integer:
                        {
                            int propVal = prop.intValue;
                            int targetVal = Convert.ToInt32(v);

                            Type enumType = target.GetType()
                                .GetField(attr.targetFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                ?.FieldType;

                            if (enumType != null && enumType.IsDefined(typeof(FlagsAttribute), false))
                            {
                                if ((propVal & targetVal) == targetVal)
                                    return true;
                            }
                            else
                            {
                                // 일반 enum 비교
                                if (propVal == targetVal)
                                    return true;
                            }
                            break;
                        }
                    case SerializedPropertyType.Boolean:
                        if (prop.boolValue.Equals(v)) return true;
                        break;
                    case SerializedPropertyType.String:
                        if (prop.stringValue.Equals(v)) return true;
                        break;
                }
            }
            return false;
        }

        /// <summary>
        /// ShowIf 관련 대상 필드에 ChangeEvent 등록 (이벤트를 Debounce 방식으로 처리)
        /// </summary>
        private void RegisterShowIfTargetWatchers(VisualElement root)
        {
            var type = target.GetType();
            if (!fieldCache.TryGetValue(type, out FieldInfo[] fields))
            {
                fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                fieldCache[type] = fields;
            }

            HashSet<string> targetNames = new();
            foreach (var f in fields)
            {
                var s = f.GetCustomAttribute<ShowIfAttribute>();
                if (s != null) targetNames.Add(s.targetFieldName);
            }

            foreach (string name in targetNames)
            {
                var ui = root.Q<PropertyField>(name);
                if (ui == null)
                    continue;

                // 기존 이벤트 제거
                ui.UnregisterCallback<ChangeEvent<int>>(OnAnyValueChanged);
                ui.UnregisterCallback<ChangeEvent<bool>>(OnAnyValueChanged);
                ui.UnregisterCallback<ChangeEvent<string>>(OnAnyValueChanged);

                ui.RegisterCallback<ChangeEvent<int>>(OnAnyValueChanged);
                ui.RegisterCallback<ChangeEvent<bool>>(OnAnyValueChanged);
                ui.RegisterCallback<ChangeEvent<string>>(OnAnyValueChanged);
            }
        }

        private void OnAnyValueChanged(EventBase evt)
        {
            if (refreshScheduled) return;

            refreshScheduled = true;
            EditorApplication.delayCall += DelayedRefresh;
        }

        private void DelayedRefresh()
        {
            if (target != null && cachedRoot != null) Refresh(cachedRoot); 
            refreshScheduled = false;
        }

        /// <summary>
        /// 런타임 라벨 업데이트
        /// </summary>
        private void UpdateRuntimeLabels()
        {
            if (runtimeLabels.Count == 0)
                return;

            foreach (var kv in runtimeLabels)
            {
                var value = kv.Key.GetValue(target);
                kv.Value.text = $"{ObjectNames.NicifyVariableName(kv.Key.Name)}: {value ?? "null"}";
            }
        }

        /// <summary>
        /// 그룹에 요소 추가
        /// </summary>
        private void AddToGroup(string name, FoldoutGroupAttribute attr, VisualElement el)
        {
            if (!grouped.ContainsKey(name))
            {
                grouped[name] = new List<VisualElement>();
                groupAttrs[name] = attr;
            }
            grouped[name].Add(el);
        }

        /// <summary>
        /// 다중 객체 편집 시, 비 직렬화 필드의 값이 모든 대상에서 동일한지 검사
        /// </summary>
        private bool HasUniformValue(FieldInfo field)
        {
            var targets = serializedObject.targetObjects;
            if (targets.Length == 0)
                return true;

            object firstValue = field.GetValue(targets[0]);
            foreach (var obj in targets)
            {
                object val = field.GetValue(obj);
                if (!Equals(val, firstValue))
                    return false;
            }
            return true;
        }
    }
}
