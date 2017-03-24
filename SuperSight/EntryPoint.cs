#define LSPDFR

using Rage;

namespace SuperSight
{
    using System;

#if NO_LSPDFR

    internal class EntryPoint
    {
        private static void Main()
        {
            PluginController.Instance.Init();

            GameFiber.Hibernate();
        }

        private static void OnUnload(bool isTerminating)
        {
            PluginController.Instance.PerformCleanUp(isTerminating);
        }
    }

#elif LSPDFR

    using LSPD_First_Response.Mod.API;

    internal class EntryPoint : Plugin
    {
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChanged;
        }

        private void OnOnDutyStateChanged(bool onDuty)
        {
            if (onDuty)
            {
                if (!PluginController.Instance.IsInitialized)
                {
                    PluginController.Instance.Init();
                }
                else
                {
                    PluginController.Instance.ResetAndInit();
                }
            }
            else
            {
                if (PluginController.Instance.IsInitialized)
                {
                    PluginController.Instance.Reset();
                }
            }
        }

        public override void Finally()
        {
            PluginController.Instance.PerformCleanUp(false);
        }
    }

#endif
}
