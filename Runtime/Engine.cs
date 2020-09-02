using UnityEngine;

namespace UniMob.UI
{
    internal static class Engine
    {
        public static bool IsApplicationQuiting { get; private set; }

        [RuntimeInitializeOnLoadMethod]
        static void Setup()
        {
            Application.quitting -= OnApplicationQuitting;
            Application.quitting += OnApplicationQuitting;
        }

        private static void OnApplicationQuitting()
        {
            IsApplicationQuiting = true;
        }
    }
}