using UnityEngine;

/// <summary>Optional presentation: creates nothing until an authored clip is assigned and requested.</summary>
public static class BroteAudioFeedback
{
#if UNITY_EDITOR
    // Read-only test observation of clip requests; voice/cooldown decisions remain unchanged.
    public static event System.Action<AudioClip,int,bool> PlaybackRequested;
#endif
    private static readonly AudioSource[] voices = new AudioSource[3];
    private static readonly bool[] growthVoice = new bool[3];
    private static readonly BroteAudioCategory[] categories=new BroteAudioCategory[3];
    private static GameObject root;
    private static float lastGrowth = float.NegativeInfinity;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        if (root != null) Object.Destroy(root);
        root = null; System.Array.Clear(voices,0,voices.Length); System.Array.Clear(growthVoice,0,growthVoice.Length);
        lastGrowth = float.NegativeInfinity;
    }
    public static void RefreshVolumes(){for(int i=0;i<voices.Length;i++)if(voices[i]!=null)voices[i].volume=.28f*BroteAudioManager.VolumeFor(categories[i]);}
    public static bool PlayOptional(AudioClip clip, Vector3 position, bool isGrowth = false, BroteAudioCategory category = BroteAudioCategory.SFX)
    {
        if (clip == null || !Application.isPlaying) return false;
#if UNITY_EDITOR
        if(PlaybackRequested!=null)
            foreach(System.Action<AudioClip,int,bool> observer in PlaybackRequested.GetInvocationList())
                try{observer(clip,(int)category,isGrowth);}catch(System.Exception exception){Debug.LogException(exception);}
#endif
        try
        {
            if (isGrowth && Time.unscaledTime - lastGrowth < 0.18f) return false;
            int activeGrowth = 0;
            for (int i=0;i<voices.Length;i++) if (voices[i]!=null && voices[i].isPlaying && growthVoice[i]) activeGrowth++;
            if (isGrowth && activeGrowth >= 2) return false;
            for (int i=0;i<voices.Length;i++)
            {
                if (voices[i]!=null && voices[i].isPlaying) continue;
                if (voices[i]==null)
                {
                    if (root==null) { root=new GameObject("BroteAudioFeedback"); root.hideFlags=HideFlags.DontSave; }
                    var child=new GameObject("OptionalVoice_"+i);child.transform.SetParent(root.transform,false);
                    voices[i]=child.AddComponent<AudioSource>();voices[i].playOnAwake=false;
                    voices[i].loop=false;voices[i].volume=0.28f;voices[i].spatialBlend=0.1f;
                    voices[i].minDistance=1f;voices[i].maxDistance=24f;
                }
                voices[i].transform.position=position;voices[i].clip=clip;growthVoice[i]=isGrowth;
                categories[i]=category;voices[i].volume=.28f*BroteAudioManager.VolumeFor(category);
                voices[i].Play();if(isGrowth)lastGrowth=Time.unscaledTime;return true;
            }
            return false;
        }
        catch (System.Exception exception) { Debug.LogException(exception); return false; }
    }
}
