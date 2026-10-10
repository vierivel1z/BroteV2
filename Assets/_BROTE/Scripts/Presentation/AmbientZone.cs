using UnityEngine;

public sealed class AmbientZone : MonoBehaviour
{
    [SerializeField] private AudioClip wind,birds,river,leaves;
    [SerializeField,Range(0f,1f)] private float gain=.22f;
    [SerializeField,Range(0f,1f)] private float spatialBlend;
    [SerializeField,Min(1f)] private float maxDistance=35f;
    [SerializeField] private Vector4 layerGains=new Vector4(1f,.5f,1f,.35f);
    private bool paused;
    private readonly AudioSource[] voices=new AudioSource[4];
    private float refreshAt;
    private void OnEnable()
    {
        if(!Application.isPlaying)return;
        var clips=Resources.Load<BroteAmbienceClips>("Audio/BroteAmbienceClips");
        if(clips!=null)
        {
            if(spatialBlend>0){if(river==null)river=clips.river;}
            else {if(wind==null)wind=clips.wind;if(birds==null)birds=clips.birds;if(leaves==null)leaves=clips.leaves;}
        }
        paused=BrotePauseMenu.IsPaused || Time.timeScale<=0;Refresh();
    }
    private void Refresh()
    {
        for(int i=0;i<4;i++)
        {
            AudioClip clip=i==0?wind:i==1?birds:i==2?river:leaves;
            if(clip==null){if(voices[i]!=null){voices[i].Stop();voices[i].clip=null;}continue;}
            if(voices[i]==null){var child=new GameObject("Ambience_"+i);child.transform.SetParent(transform,false);voices[i]=child.AddComponent<AudioSource>();voices[i].playOnAwake=false;voices[i].loop=true;}
            voices[i].spatialBlend=spatialBlend;voices[i].minDistance=2f;voices[i].maxDistance=maxDistance;
            voices[i].volume=gain*Mathf.Clamp01(layerGains[i])*BroteAudioManager.VolumeFor(BroteAudioCategory.Ambience);
            if(voices[i].clip!=clip){voices[i].Stop();voices[i].clip=clip;}
            if(!paused && !voices[i].isPlaying)voices[i].Play();
        }
    }
    private void Update()
    {
        bool shouldPause=BrotePauseMenu.IsPaused || Time.timeScale<=0;
        if(shouldPause!=paused)
        {
            paused=shouldPause;
            foreach(var voice in voices)if(voice!=null){if(paused)voice.Pause();else voice.UnPause();}
        }
        if(Time.unscaledTime>=refreshAt){Refresh();refreshAt=Time.unscaledTime+.5f;}
        for(int i=0;i<voices.Length;i++)if(voices[i]!=null)
            voices[i].volume=gain*Mathf.Clamp01(layerGains[i])*BroteAudioManager.VolumeFor(BroteAudioCategory.Ambience);
    }
    private void OnDisable(){foreach(var voice in voices)if(voice!=null)voice.Stop();}
    public static void AttachSector(BroteHUD hud)
    {
        if(hud.gameObject.scene.name!="Acopalca")return;
        var world=GameObject.Find("World");if(world==null||world.transform.Find("Audio")!=null)return;
        var root=new GameObject("Audio");root.transform.SetParent(world.transform,false);
        foreach(string name in new[]{"Sector01Ambience","DistantRiverAudio"})
        {
            var prefab=Resources.Load<GameObject>("Audio/"+name);
            if(prefab!=null)Instantiate(prefab,root.transform,false);
        }
    }
}
