namespace SuperSight
{
    using System;

    using Rage;

    internal class PluginController
    {
        public delegate void CleanUpEventHandler(bool isTerminating);

        private static PluginController instance;
        public static PluginController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PluginController();
                }

                return instance;
            }
        }


        public bool IsInitialized { get; private set; }
        public GameFiber Fiber { get; private set; }

        public event CleanUpEventHandler CleanUp;

        private PluginController()
        {
            Fiber = new GameFiber(UpdateLoop, "Helicopter Camera - PluginController::UpdateLoop Fiber");
        }

        public void Init()
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException($"{nameof(PluginController)} is already initialized, can't call {nameof(Init)} again.");
            }

            Fiber.Start();

            IsInitialized = true;
        }

        public void Reset()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException($"{nameof(PluginController)} isn't initialized, can't call {nameof(Reset)}.");
            }

            Fiber?.Abort();
            Fiber = new GameFiber(UpdateLoop, "Helicopter Camera - PluginController::UpdateLoop Fiber");

            IsInitialized = false;
        }

        public void ResetAndInit()
        {
            Reset();
            Init();
        }

        public void PerformCleanUp(bool isTerminating)
        {
            Fiber?.Abort();
            Fiber = null;

            CleanUp?.Invoke(isTerminating);
        }

        private void UpdateLoop()
        {
            while (true)
            {
                GameFiber.Yield();

                Update();
            }
        }

        private void Update()
        {
            
        }
    }
}
