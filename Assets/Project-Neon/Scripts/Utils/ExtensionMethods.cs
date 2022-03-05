using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    //rect transfrom extenstion from this source: https://orbcreation.com/cgi-bin/orbcreation/page.pl?1099
    public static void SetSize(this RectTransform trans, Vector2 newSize)
    {
        Vector2 oldSize = trans.rect.size;
        Vector2 deltaSize = newSize - oldSize;
        trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
        trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
    }

    public static void AlignUp(this Transform trans, Vector3 newUp)
    {
        Quaternion newRot = Quaternion.FromToRotation(trans.up, newUp) * trans.rotation;
        trans.rotation = newRot;
    }
}
