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
            string expected = VNodeSnapshot.Serialize(expectedNode);
            string actual = VNodeSnapshot.Serialize(actualNode);
            if (expected == actual)
            {
                return new Result { Pass = true, Diff = string.Empty, Expected = expected, Actual = actual };
            }
            string diff = VNodeSnapshot.Diff(expectedNode, actualNode);
            return new Result { Pass = false, Diff = diff, Expected = expected, Actual = actual };
        }

        public static void AssertEqual(VirtualNode expected, VirtualNode actual, Action<string> logger = null)
        {
            var res = Compare(expected, actual);
            if (!res.Pass)
            {
                logger?.Invoke("Snapshot mismatch:\n" + res.Diff);
#if UNITY_EDITOR
                UnityEngine.Debug.LogError("Snapshot mismatch:\n" + res.Diff);
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
