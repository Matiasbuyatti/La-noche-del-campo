using System.Collections.Generic;
using UnityEngine;

// Cada panel pide el cursor libre. Si hay al menos uno, se libera.
public static class GameCursor
{
    private static readonly HashSet<object> owners = new HashSet<object>();

    public static void SetFree(object owner, bool free)
    {
        if (free)
            owners.Add(owner);
        else
            owners.Remove(owner);

        Apply();
    }

    public static void Clear()
    {
        owners.Clear();
        Apply();
    }

    private static void Apply()
    {
        bool free = owners.Count > 0;

        Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = free;
    }
}
