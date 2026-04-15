using UnityEngine;

namespace PPGPerformancePlusMod
{
    public class Mod
    {
        private static RuntimeController controller;

        public static void OnLoad()
        {
            EnsureController();
        }

        public static void Main()
        {
            EnsureController();

            if (controller != null)
            {
                controller.NotifyCatalogRefresh();
            }
        }

        public static void OnUnload()
        {
            if (controller != null)
            {
                controller.ShutdownRuntime();
                Object.Destroy(controller.gameObject);
                controller = null;
            }
        }

        private static void EnsureController()
        {
            if (controller != null)
            {
                return;
            }

            var existing = Object.FindObjectOfType<RuntimeController>();
            if (existing != null)
            {
                controller = existing;
                return;
            }

            var host = new GameObject("PPGPerformancePlus.Runtime");
            Object.DontDestroyOnLoad(host);
            controller = host.AddComponent<RuntimeController>();
        }
    }
}
