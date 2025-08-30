using System;

namespace Sych.ShareAssets.Runtime.Tools
{
    internal static class ReflectionUtils
    {
        public static bool TryToCreateInstance<T>(string typeName, string assemblyName, out T instance) where T : class
        {
            instance = default;
            var type = Type.GetType($"{assemblyName}.{typeName}, {assemblyName}");
            if (type == null)
                return false;

            instance = Activator.CreateInstance(type) as T;
            return instance != null;
        }

        public static T CreateInstance<T>(string typeName, string assemblyName) where T : class
        {
            var type = Type.GetType($"{assemblyName}.{typeName}, {assemblyName}");
            if (type == null)
                throw new Exception($"Type {typeName} not found");
            return Activator.CreateInstance(type) as T;
        }

        public static bool IsTypeExists(string typeName, string assemblyName) => Type.GetType($"{assemblyName}.{typeName}, {assemblyName}") != null;
    }
}