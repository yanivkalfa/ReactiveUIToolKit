using System;
using ReactiveUITK.Core.Util;

namespace ReactiveUITK.Core.Util
{
    public static class SnapshotAssert
    {
        public struct Result
        {
            public bool Pass;
            public string Diff;
            public string Expected;
            public string Actual;
        }

        public static Result Compare(VirtualNode expectedNode, VirtualNode actualNode)
        {
            string expectedSerialized = VNodeSnapshot.Serialize(expectedNode);
            string actualSerialized = VNodeSnapshot.Serialize(actualNode);
            if (expectedSerialized == actualSerialized)
            {
                return new Result
                {
                    Pass = true,
                    Diff = string.Empty,
                    Expected = expectedSerialized,
                    Actual = actualSerialized,
                };
            }
            string diffSnapshot = VNodeSnapshot.Diff(expectedNode, actualNode);
            return new Result
            {
                Pass = false,
                Diff = diffSnapshot,
                Expected = expectedSerialized,
                Actual = actualSerialized,
            };
        }

        public static void AssertEqual(
            VirtualNode expectedNode,
            VirtualNode actualNode,
            Action<string> logAction = null
        )
        {
            Result comparison = Compare(expectedNode, actualNode);
            if (!comparison.Pass)
            {
                logAction?.Invoke("Snapshot mismatch:\n" + comparison.Diff);
#if UNITY_EDITOR
                UnityEngine.Debug.LogError("Snapshot mismatch:\n" + comparison.Diff);
#endif
            }
            else
            {
#if UNITY_EDITOR
                UnityEngine.Debug.Log("Snapshot OK");
#endif
            }
        }
    }
}
