using UnityEngine;

namespace ProjectRaid.Extensions
{
    public static class ComponentExtensions
    {
        /// <summary>
        /// 자신에게 컴포넌트가 있으면 반환하고, 없으면 추가 후 반환
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
            return component;
        }

        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            return component.gameObject.GetOrAddComponent<T>();
        }

        /// <summary>
        /// 자식에서 컴포넌트를 찾아 반환 (실패 시 null)
        /// </summary>
        public static bool TryGetComponentInChildren<T>(this Component component, out T result) where T : Component
        {
            result = component.GetComponentInChildren<T>();
            return result != null;
        }

        /// <summary>
        /// 부모에서 컴포넌트를 찾아 반환 (실패 시 null)
        /// </summary>
        public static bool TryGetComponentInParent<T>(this Component component, out T result) where T : Component
        {
            result = component.GetComponentInParent<T>();
            return result != null;
        }

        public static bool TryGetInterfaceInParent<TInterface>(this Component component, out TInterface result) where TInterface : class
        {
            Transform current = component.transform;
            while (current != null)
            {
                // 해당 오브젝트에 붙은 모든 컴포넌트를 가져와서
                foreach (var comp in current.GetComponents<Component>())
                {
                    // 인터페이스로 캐스트 시도
                    if (comp is TInterface iface)
                    {
                        result = iface;
                        return true;
                    }
                }
                current = current.parent;
            }
            result = null;
            return false;
        }
    }
}
