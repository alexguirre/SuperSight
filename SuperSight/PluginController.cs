namespace SuperSight
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Collections.Generic;

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

        public List<ISight> Sights { get; private set; } = new List<ISight>();

        public event CleanUpEventHandler CleanUp;

        private PluginController()
        {
            Fiber = new GameFiber(UpdateLoop, "SuperSight - PluginController::UpdateLoop Fiber");
        }

        public void Init()
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException($"{nameof(PluginController)} is already initialized, can't call {nameof(Init)} again.");
            }

            RegisterSights();

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
            for (int i = 0; i < Sights.Count; i++)
            {
                ISight s = Sights[i];

                if (s.IsActive)
                {
                    s.OnActiveUpdate();

                    if (s.MustBeDeactivated)
                    {
                        s.IsActive = false;
                    }
                }
                else
                {
                    if (s.MustBeActivated)
                    {
                        s.IsActive = true;
                    }
                }
            }
        }

        // creates an instance of every class that inherits from ISight contained in this assembly and adds it to the Sights list
        private void RegisterSights()
        {
            Sights.Clear();

            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && !t.IsInterface && typeof(ISight).IsAssignableFrom(t));

            foreach (Type type in types)
            {
                ISight s = (ISight)Activator.CreateInstance(type);

                Sights.Add(s);
            }
        }
    }
}
