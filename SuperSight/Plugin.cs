namespace SuperSight
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Collections.Generic;

    using Rage;
    
    internal static class Plugin
    {
        public const string PluginName = "SuperSight";
        public const string ResourcesFolder = "Plugins\\" + PluginName + "\\";

        public static Settings Settings { get; private set; }
        public static List<ISight> Sights { get; private set; } = new List<ISight>();
        
        private static void Main()
        {
            while (Game.IsLoading)
                GameFiber.Sleep(1000);

            if (!System.IO.Directory.Exists(ResourcesFolder))
                System.IO.Directory.CreateDirectory(ResourcesFolder);

            Settings = new Settings();

            RegisterSights();

            while (true)
            {
                GameFiber.Yield();

                Update();
            }
        }

        private static void Update()
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
        private static void RegisterSights()
        {
            Sights.Clear();

            IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && !t.IsInterface && typeof(ISight).IsAssignableFrom(t));

            foreach (Type type in types)
            {
                Game.LogTrivial($"Creating ISight instance of type '{type.Name}'...");
                ISight s = (ISight)Activator.CreateInstance(type);
                Sights.Add(s);
            }
        }

        private static void OnUnload(bool isTerminating)
        {
            for (int i = 0; i < Sights.Count; i++)
            {
                Sights[i]?.Dispose();
            }
            Sights.Clear();
            Sights = null;
        }
    }
}
