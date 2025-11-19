
using System;
using System.Collections.Generic;

namespace ReactiveUITK.Bench
{
    [Serializable]
    public class ScenarioDef
    {
        public string Name;
        public float DurationSec = 8f;
    }

    public static class BenchConfig
    {
        public static readonly List<ScenarioDef> Default = new()
        {
            new ScenarioDef { Name = "Smoke", DurationSec = 5f }, 
            new ScenarioDef { Name = "StaticScreen", DurationSec = 8f },
            new ScenarioDef { Name = "PropChurn_500", DurationSec = 10f },
            new ScenarioDef { Name = "ListReorder_200", DurationSec = 10f },
            new ScenarioDef { Name = "MountUnmount_50x20", DurationSec = 10f },
            new ScenarioDef { Name = "BigListManual_3000", DurationSec = 12f },
            new ScenarioDef { Name = "SharedDemo", DurationSec = 10f },
        };
    }
}
