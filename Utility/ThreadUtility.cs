using System.Threading;
using UnityEngine;

public static class ThreadUtility
{
    private static int mainThreadId;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        mainThreadId = Thread.CurrentThread.ManagedThreadId;
    }

    public static bool IsMainThread()
    {
        return Thread.CurrentThread.ManagedThreadId == mainThreadId;
    }
}
