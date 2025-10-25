using System;

namespace ReactiveUITK.Bench
{
    public enum BenchOutputTarget
    {
        Auto,
        Editor,
        Runtime
    }

    [Serializable]
    public struct BenchEnvOverrides
    {
        public bool?  isEditor;
        public bool?  isDevelopmentBuild;
        public string productName;
        public string platform;
        public string graphicsDevice;
        public string deviceModel;
        public string deviceName;
        public int?   screenWidth;
        public int?   screenHeight;
        public int?   systemMemoryMB;
    }
}
