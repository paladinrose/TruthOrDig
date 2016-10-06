using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
public class SpriteGeometryEditor : AssetPostprocessor {
    static SpriteGeometryEditor e;
    public static SpriteGeometryEditor SGEditor
    {
        get
        {
            if (e == null) { e = new SpriteGeometryEditor(); }
            return e;
        }
    }


    List<SpriteGeometryRegistry> registered = new List<SpriteGeometryRegistry>();
    public void RegisterSpriteForNewGeometry(Sprite s, Vector2[] p, ushort[] t)
    {
        registered.Add(new SpriteGeometryRegistry(s, t, p));
    }
	public void OnPostProcessSprites(Texture2D texture, Sprite[] sprites)
    {
        for(int i = sprites.Length-1; i >=0; i--)
        {
            Debug.Log("Have sprite " + i);
            for (int j = registered.Count - 1; j >= 0; j--)
            {
                Debug.Log("Have registered " + j);
                if (registered[j].s == sprites[i])
                {
                    Debug.Log("Applying new geometry to " + i + "," + j);
                    registered[j].s.OverrideGeometry(registered[j].points, registered[j].tris);
                    registered.RemoveAt(j);
                    break;
                }
            }
        }
    }
}
public class SpriteGeometryRegistry
{
    public Sprite s;
    public ushort[] tris;
    public Vector2[] points;
    public SpriteGeometryRegistry(Sprite ss, ushort[]trix, Vector2[] p)
    {
        s = ss;
        tris = trix;
        points = p;
    }
}