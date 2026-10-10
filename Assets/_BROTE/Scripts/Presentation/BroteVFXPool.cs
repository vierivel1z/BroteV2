using System.Collections.Generic;
using UnityEngine;

/// <summary>Only three reusable effects, not a generic pooling framework.</summary>
public static class BroteVFXPool
{
    private static readonly List<BroteGrowthVFX> effects=new List<BroteGrowthVFX>(3);
    private static readonly BroteGrowthVFX[] presets=new BroteGrowthVFX[3];
    private static GameObject root;
    public static int ActiveCount { get { int count=0;foreach(var effect in effects)if(effect!=null&&effect.Active)count++;return count; } }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset(){foreach(var effect in effects)if(effect!=null)Object.Destroy(effect.gameObject);effects.Clear();System.Array.Clear(presets,0,3);if(root!=null)Object.Destroy(root);root=null;}
    public static BroteGrowthVFX Preset(int stage)
    {
        if(stage<0||stage>2)return null;
        if(presets[stage]==null){var prefab=Resources.Load<GameObject>("VFX/"+(stage==0?"PlantingVFX":stage==1?"GrowthVFX":"MatureVFX"));presets[stage]=prefab!=null?prefab.GetComponent<BroteGrowthVFX>():null;}
        return presets[stage];
    }
    public static BroteGrowthVFX Acquire(int stage,Transform owner)
    {
        effects.RemoveAll(effect=>effect==null);
        foreach(var effect in effects)if(!effect.Active){effect.transform.SetParent(owner,false);effect.gameObject.SetActive(true);return effect;}
        if(effects.Count>=3)return null;
        var preset=Preset(stage);if(preset==null)return null;
        var created=Object.Instantiate(preset,owner,false);effects.Add(created);return created;
    }
    public static void Release(BroteGrowthVFX effect)
    {
        if(effect==null)return;
        effect.Stop();effect.gameObject.SetActive(false);
        if(root==null){root=new GameObject("BroteVFXPool");root.hideFlags=HideFlags.DontSave;}
        effect.transform.SetParent(root.transform,false);
    }
}
