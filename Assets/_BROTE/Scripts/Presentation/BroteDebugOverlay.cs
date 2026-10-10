using UnityEngine;
using UnityEngine.InputSystem;

public sealed class BroteDebugOverlay : MonoBehaviour
{
    private bool visible;
    private PlayerInteraction player;
    private SeedlingInventory inventory;
    private RestorationManager manager;
    private float fps;
    private QuenualGrowth[] trees=System.Array.Empty<QuenualGrowth>();
    private float refreshAt;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoaded;
        Attach();
#endif
    }
    private static void SceneLoaded(UnityEngine.SceneManagement.Scene scene,UnityEngine.SceneManagement.LoadSceneMode mode)=>Attach();
    private static void Attach()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        var hud=FindFirstObjectByType<BroteHUD>();
        if(hud!=null&&hud.GetComponent<BroteDebugOverlay>()==null)hud.gameObject.AddComponent<BroteDebugOverlay>();
#endif
    }
    private void Awake()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        player=FindFirstObjectByType<PlayerInteraction>();inventory=player!=null?player.GetComponent<SeedlingInventory>():null;manager=FindFirstObjectByType<RestorationManager>();
#else
        enabled=false;
#endif
    }
    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if(Keyboard.current!=null&&Keyboard.current.f3Key.wasPressedThisFrame)visible=!visible;
        if(!visible)return;
        fps=Mathf.Lerp(fps,1f/Mathf.Max(.001f,Time.unscaledDeltaTime),.1f);
        if(Time.unscaledTime>=refreshAt){trees=FindObjectsByType<QuenualGrowth>(FindObjectsSortMode.None);refreshAt=Time.unscaledTime+.5f;}
#endif
    }
    private void OnGUI()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if(!visible || NurseryManagementPanel.IsOpen)return;
        GUILayout.BeginArea(new Rect(12,12,350,Mathf.Min(Screen.height-24,450)),GUI.skin.box);
        GUILayout.Label("BROTE debug (F3)");GUILayout.Label("FPS "+fps.ToString("F0"));
        if(player!=null){GUILayout.Label("Suyay "+player.transform.position.ToString("F2"));var target=player.CurrentInteractable;GUILayout.Label("Target "+(target is Component c?c.name:"none"));}
        GUILayout.Label("Seedlings "+(inventory!=null?inventory.CurrentSeedlings.ToString():"missing"));GUILayout.Label("Restoration "+(manager!=null?manager.RestorationPercentage.ToString("F1"):"missing"));
        foreach(var tree in trees)if(tree!=null)GUILayout.Label(tree.name+" stage "+tree.Stage);
        GUILayout.EndArea();
#endif
    }
}
