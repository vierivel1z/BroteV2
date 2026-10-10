using UnityEngine;

public enum BroteAudioCategory { UI, SFX, Ambience }
public sealed class BroteAudioManager : MonoBehaviour
{
    private const string MasterKey="BROTE.MasterVolume",SfxKey="BROTE.SFXVolume";
    [SerializeField] private AudioClip upgradeClip;
    private BroteUpgradeManager[] upgradeSources=System.Array.Empty<BroteUpgradeManager>();
    private static float master=1f,sfx=1f;
    private static bool focused=true,loaded;
    [SerializeField] private bool muteWhenUnfocused=true;
    public static float MasterVolume { get { Load();return master; } }
    public static float SFXVolume { get { Load();return sfx; } }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset(){loaded=false;focused=true;}
    private static void Load(){if(loaded)return;master=Clamp(PlayerPrefs.GetFloat(MasterKey,1f));sfx=Clamp(PlayerPrefs.GetFloat(SfxKey,1f));loaded=true;}
    private static float Clamp(float value)=>float.IsNaN(value)||float.IsInfinity(value)?1f:Mathf.Clamp01(value);
    public static void SetMasterVolume(float value){Load();master=Clamp(value);PlayerPrefs.SetFloat(MasterKey,master);PlayerPrefs.Save();}
    public static void SetSFXVolume(float value){Load();sfx=Clamp(value);PlayerPrefs.SetFloat(SfxKey,sfx);PlayerPrefs.Save();}
    public static float VolumeFor(BroteAudioCategory category){Load();return focused?master*(category==BroteAudioCategory.SFX?sfx:1f):0f;}
    public static BroteAudioManager Attach(GameObject owner){var component=owner.GetComponent<BroteAudioManager>();return component!=null?component:owner.AddComponent<BroteAudioManager>();}
    private void OnEnable()
    {
        Load();focused=!muteWhenUnfocused||Application.isFocused;
        var clips=Resources.Load<BroteSFXClips>("Audio/BroteSFXClips");
        if(upgradeClip==null && clips!=null)upgradeClip=clips.upgrade;
        BindUpgradeAudio();
    }
    private void Start()=>BindUpgradeAudio();
    private void BindUpgradeAudio()
    {
        foreach(var source in upgradeSources)if(source!=null)source.Purchased-=OnUpgradePurchased;
        upgradeSources=System.Array.FindAll(FindObjectsByType<BroteUpgradeManager>(),x=>x.gameObject.scene==gameObject.scene);
        foreach(var source in upgradeSources)source.Purchased+=OnUpgradePurchased;
    }
    private void OnUpgradePurchased(BroteUpgrade kind)
    {
        if(!BroteSaveSystem.IsLoading)BroteAudioFeedback.PlayOptional(upgradeClip,transform.position,false,BroteAudioCategory.SFX);
    }
    private void OnApplicationFocus(bool hasFocus){focused=!muteWhenUnfocused||hasFocus;}
    private void OnDisable()
    {
        foreach(var source in upgradeSources)if(source!=null)source.Purchased-=OnUpgradePurchased;
        upgradeSources=System.Array.Empty<BroteUpgradeManager>();focused=true;
    }
    private void LateUpdate()=>BroteAudioFeedback.RefreshVolumes();
}
