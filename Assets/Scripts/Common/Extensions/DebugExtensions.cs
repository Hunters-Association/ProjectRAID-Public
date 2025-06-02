using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ProjectRaid.Extensions
{
#if UNITY_EDITOR
    public static class DebugExtensions
    {
        public static void Log(object message, Object context = null, [CallerFilePath] string file = "")
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            if (context != null)
                Debug.LogError($"<color=#ffffff><b>[{fileName}]</b> {message}</color>", context);
            else
                Debug.Log($"<color=#ffffff><b>[{fileName}]</b> {message}</color>");
        }

        public static void LogWarning(object message, Object context = null, [CallerFilePath] string file = "")
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            if (context != null)
                Debug.LogError($"<color=#ffd700><b>[{fileName}]</b> {message}</color>", context);
            else
                Debug.LogWarning($"<color=#ffd700><b>[{fileName}]</b> {message}</color>");
        }

        public static void LogError(object message, Object context = null, [CallerFilePath] string file = "")
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            if (context != null)
                Debug.LogError($"<color=#dc143c><b>[{fileName}]</b> {message}</color>", context);
            else
                Debug.LogError($"<color=#dc143c><b>[{fileName}]</b> {message}</color>");
        }
    }
#endif
}
