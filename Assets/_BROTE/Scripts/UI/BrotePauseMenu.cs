using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public sealed class BrotePauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }
    private float previousScale=1f;
    private bool ownsPause;
    private VisualElement overlay,confirm,pausePanel,restoredPanel;
    public static bool IsSectorRestoredVisible { get; private set; }
    private bool restoredShown;
    private Label saveMessage;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset(){IsPaused=false;IsSectorRestoredVisible=false;}
    public static void Attach(BroteHUD hud)
    {
        BroteAudioManager.Attach(hud.gameObject);
        var existing=hud.GetComponentInChildren<BrotePauseMenu>(true);
        if(existing!=null){existing.enabled=true;var doc=existing.GetComponent<UIDocument>();if(doc!=null){doc.enabled=false;doc.enabled=true;}return;}
        var root=new GameObject("BrotePauseMenu");root.transform.SetParent(hud.transform,false);root.AddComponent<BrotePauseMenu>();
    }
    private void Awake()
    {
        var parent=GetComponentInParent<BroteHUD>();var source=parent!=null?parent.GetComponent<UIDocument>():null;
        if(source==null||source.panelSettings==null){enabled=false;return;}
        var doc=gameObject.AddComponent<UIDocument>();doc.panelSettings=source.panelSettings;doc.sortingOrder=30;
        overlay=doc.rootVisualElement;overlay.style.position=Position.Absolute;overlay.style.left=overlay.style.right=overlay.style.top=overlay.style.bottom=0;
        overlay.style.backgroundColor=new Color(.04f,.09f,.06f,.85f);overlay.style.alignItems=Align.Center;overlay.style.justifyContent=Justify.Center;
        var panel=new VisualElement();pausePanel=panel;panel.style.width=360;panel.style.maxWidth=Length.Percent(94);panel.style.paddingLeft=panel.style.paddingRight=20;panel.style.paddingTop=panel.style.paddingBottom=20;panel.style.backgroundColor=new Color(.08f,.16f,.1f);overlay.Add(panel);
        AddLabel(panel,"BROTE - Pausa",24);AddLabel(panel,"WASD / flechas - Movimiento\nE - Interacci\u00f3n\nESC - Pausa / continuar",17);
        AddButton(panel,"Continuar",()=>SetPaused(false));saveMessage=new Label();saveMessage.style.color=Color.white;panel.Add(saveMessage);
        AddButton(panel,"Guardar",()=> { var save=BroteSaveSystem.Current;saveMessage.text=save!=null&&save.Save()?"Progreso guardado":"No se pudo guardar"; });AddButton(panel,"Nueva partida",()=>confirm.style.display=DisplayStyle.Flex);
        confirm=new VisualElement();confirm.style.display=DisplayStyle.None;panel.Add(confirm);AddLabel(confirm,"Se borrar\u00e1 el progreso guardado. \u00bfReiniciar?",16);
        AddButton(confirm,"S\u00ed, reiniciar",Restart);AddButton(confirm,"Cancelar",()=>confirm.style.display=DisplayStyle.None);
        AddLabel(panel,"Volumen",18);AddVolume(panel,"Master",BroteAudioManager.MasterVolume,BroteAudioManager.SetMasterVolume);AddVolume(panel,"SFX",BroteAudioManager.SFXVolume,BroteAudioManager.SetSFXVolume);
        AddButton(panel,Application.isEditor?"Salir de Play":"Salir al escritorio",Quit);BuildRestoredPanel();overlay.style.display=DisplayStyle.None;
    }
    private static void AddLabel(VisualElement parent,string text,int size){var label=new Label(text);label.style.fontSize=size;label.style.color=Color.white;label.style.whiteSpace=WhiteSpace.Normal;label.style.marginBottom=10;parent.Add(label);}
    private static void AddButton(VisualElement parent,string text,System.Action action){var button=new Button(action){text=text};button.style.height=32;button.style.marginBottom=6;parent.Add(button);}
    private static void AddVolume(VisualElement parent,string title,float value,System.Action<float> setter){var slider=new Slider(title,0f,1f){value=value};slider.RegisterValueChangedCallback(e=>setter(e.newValue));slider.style.color=Color.white;slider.style.marginBottom=10;parent.Add(slider);}
    private void Update(){
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if(!IsPaused&&!NurseryManagementPanel.IsOpen&&Keyboard.current!=null&&Keyboard.current.kKey.wasPressedThisFrame)TryDebugRestoration();
#endif
        if(IsSectorRestoredVisible)return;
        if(NurseryManagementPanel.IsOpen || NurseryManagementPanel.ClosedFrame==Time.frameCount)return;if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame)SetPaused(!ownsPause);}
    public void SetPaused(bool paused)
    {
        if(overlay==null||paused==ownsPause)return;
        if(paused){previousScale=Time.timeScale;Time.timeScale=0f;ownsPause=true;IsPaused=true;confirm.style.display=DisplayStyle.None;overlay.style.display=DisplayStyle.Flex;}
        else{Time.timeScale=previousScale;ownsPause=false;IsPaused=false;IsSectorRestoredVisible=false;overlay.style.display=DisplayStyle.None;pausePanel.style.display=DisplayStyle.Flex;if(restoredPanel!=null)restoredPanel.style.display=DisplayStyle.None;}
    }
    private void BuildRestoredPanel()
    {
        var art=Resources.Load<MainMenuUISkin>("UI/MainMenuUISkin");
        restoredPanel=new VisualElement{name="sector-restored-panel"};
        restoredPanel.style.width=480;restoredPanel.style.maxWidth=Length.Percent(94);
        restoredPanel.style.paddingLeft=restoredPanel.style.paddingRight=36;
        restoredPanel.style.paddingTop=restoredPanel.style.paddingBottom=32;
        restoredPanel.style.alignItems=Align.Center;
        NurseryUISkin.Apply(restoredPanel,art!=null?art.panelBackground:null,new Color(.91f,.83f,.63f));
        restoredPanel.style.unitySliceLeft=restoredPanel.style.unitySliceRight=120;
        restoredPanel.style.unitySliceTop=restoredPanel.style.unitySliceBottom=70;
        restoredPanel.style.unitySliceScale=.5f;overlay.Add(restoredPanel);
        foreach(var pair in new[]{new[]{"SECTOR RESTAURADO","24"},new[]{"Salud del Valle: 100%","21"},new[]{"Los queñuales devuelven vida al valle. Gracias por cuidar Acopalca.","18"}})
        {
            var label=new Label(pair[0]);label.style.fontSize=int.Parse(pair[1]);
            label.style.color=new Color(.23f,.15f,.09f);label.style.whiteSpace=WhiteSpace.Normal;
            label.style.unityTextAlign=TextAnchor.MiddleCenter;label.style.width=Length.Percent(100);
            label.style.marginBottom=16;restoredPanel.Add(label);
        }
        var keep=new MainMenuArtButton("SEGUIR JUGANDO",ContinueAfterRestoration,art,true){name="sector-continue"};
        var menu=new MainMenuArtButton("VOLVER AL MENÚ",ReturnToMainMenu,art,true){name="sector-main-menu"};
        foreach(var button in new[]{keep,menu})
        {
            button.style.width=300;button.style.maxWidth=Length.Percent(100);button.style.height=64;
            button.style.fontSize=16;button.style.marginBottom=8;restoredPanel.Add(button);
        }
        restoredPanel.style.display=DisplayStyle.None;
    }
    public void ShowSectorRestored()
    {
        if(restoredShown||restoredPanel==null)return;
        restoredShown=true;SetPaused(true);IsSectorRestoredVisible=true;
        pausePanel.style.display=DisplayStyle.None;restoredPanel.style.display=DisplayStyle.Flex;
        restoredPanel.Q<Button>("sector-continue").Focus();
    }
    public void ContinueAfterRestoration()=>SetPaused(false);
    public void ReturnToMainMenu()
    {
        BroteSaveSystem.Current?.Save();SetPaused(false);
        SceneManager.LoadSceneAsync("Assets/_BROTE/Scenes/UI/MainMenu.unity",LoadSceneMode.Single);
    }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public bool TryDebugRestoration()
    {
        if((!Application.isEditor&&!Debug.isDebugBuild)||IsPaused||NurseryManagementPanel.IsOpen||BroteSaveSystem.IsLoading)return false;
        var manager=System.Array.Find(FindObjectsByType<RestorationManager>(),x=>x.gameObject.scene==gameObject.scene);
        if(manager==null||manager.RestorationPercentage>=100)return false;
        BroteSaveSystem.Current?.BeginRestorationDebugPreview();
        manager.AddRestoration(manager.MaxRestoration-manager.CurrentRestoration);
        return true;
    }
#endif
    private void Restart()
    {
        Scene scene=gameObject.scene;
        if(string.IsNullOrEmpty(scene.path))return;
        if (!BroteSaveSystem.DeleteConfirmedSave()) return;
        SetPaused(false);SceneManager.LoadSceneAsync(scene.path,LoadSceneMode.Single);
    }
    private void Quit()
    {
        SetPaused(false);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying=false;
#else
        Application.Quit();
#endif
    }
    private void OnDisable(){if(ownsPause)SetPaused(false);}
}
