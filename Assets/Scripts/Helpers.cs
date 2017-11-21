using UnityEngine;

public static class Helpers {

    public class Tags
    {
        public const string CameraFollowTarget = "CameraLookTarget";
        public const string CameraPositionPivot = "CameraPositionPivot";
        public const string MovingPlatform = "MovingPlatform";
        public const string Spell = "Spell";
        public const string Player = "Player";
        public const string PlayerHUD = "PlayerHUD";
    }

    public static GameObject FindObjectInChildren(this GameObject gameObject, string gameObjectName)
    {
        Transform[] children = gameObject.GetComponentsInChildren<Transform>(true);
        foreach (Transform item in children)
        {
            if (item.name == gameObjectName)
            {
                return item.gameObject;
            }
        }

        return null;
    }

    public static GameObject FindTaggedObjectInChildren(this GameObject gameObject, string tag)
    {
        Transform[] children = gameObject.GetComponentsInChildren<Transform>(true);
        foreach (Transform item in children)
        {
            if (item.tag == tag)
            {
                return item.gameObject;
            }
        }

        return null;
    }

    public static void DebugDirectionRay(this Transform transform)
    {
        Debug.DrawRay(transform.position, transform.forward, Color.red, 0.1f);
    }
}
