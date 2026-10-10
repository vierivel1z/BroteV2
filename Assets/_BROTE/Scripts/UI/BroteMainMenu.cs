using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public sealed class BroteMainMenu : MonoBehaviour
{
    [SerializeField] private Sprite landscape; // Retained serialized reference for old scenes.
    [SerializeField] private MainMenuUISkin skin;
    private PanelSettings panelSettings;
    private VisualElement root,page,dialog,ambientLayer;
    private MainMenuAmbientFX ambient;
    private bool loading;
    private void Start()
    {
        Time.timeScale=1f;BroteAudioManager.Attach(gameObject);
        if(skin==null)skin=Resources.Load<MainMenuUISkin>("UI/MainMenuUISkin");
        panelSettings=ScriptableObject.CreateInstance<PanelSettings>();
        panelSettings.scaleMode=PanelScaleMode.ScaleWithScreenSize;
        panelSettings.referenceResolution=new Vector2Int(1280,720);
        var document=gameObject.AddComponent<UIDocument>();document.panelSettings=panelSettings;
        root=document.rootVisualElement;root.name="MainMenuRoot";
        Stretch(root);root.style.overflow=Overflow.Hidden;
        var background=new VisualElement{name="MainMenuBackground",pickingMode=PickingMode.Ignore};
        Stretch(background);
        background.style.backgroundImage=new StyleBackground(skin!=null&&skin.background!=null?skin.background:landscape);
        background.style.backgroundSize=new BackgroundSize(BackgroundSizeType.Cover);
        background.style.backgroundPositionX=new BackgroundPosition(BackgroundPositionKeyword.Center);
        background.style.backgroundPositionY=new BackgroundPosition(BackgroundPositionKeyword.Center);
        root.Add(background);
        ambientLayer=new VisualElement{name="MainMenuAmbientFX",pickingMode=PickingMode.Ignore};
        Stretch(ambientLayer);ambientLayer.style.overflow=Overflow.Hidden;root.Add(ambientLayer);
        ambient=new MainMenuAmbientFX(ambientLayer,skin);
        page=new VisualElement{name="MainMenuHome"};page.style.position=Position.Absolute;root.Add(page);
        dialog=new VisualElement{name="MainMenuDialog"};dialog.style.position=Position.Absolute;
        dialog.style.paddingLeft=dialog.style.paddingRight=38;
        dialog.style.paddingTop=dialog.style.paddingBottom=32;
        dialog.style.justifyContent=Justify.Center;
        if(skin!=null)
        {
            dialog.style.backgroundImage=new StyleBackground(skin.panelBackground);
            dialog.style.unitySliceLeft=dialog.style.unitySliceRight=120;
            dialog.style.unitySliceTop=dialog.style.unitySliceBottom=70;
            dialog.style.unitySliceScale=.5f;
        }
        root.Add(dialog);
        root.RegisterCallback<GeometryChangedEvent>(_=>Layout(root.contentRect.width,root.contentRect.height));
        Home();
    }
    private static void Stretch(VisualElement element)
    {
        element.style.position=Position.Absolute;
        element.style.left=element.style.right=element.style.top=element.style.bottom=0;
    }
    private void Layout(float width,float height)
    {
        if(width<=0||height<=0)return;
        float column=Mathf.Min(350,width*.43f);
        float buttonHeight=Mathf.Min(column*.34f,height*.145f);
        float logoHeight=Mathf.Min(column*.48f,height*.22f);
        float gap=Mathf.Min(12,height*.018f);
        float total=logoHeight+gap+4*(buttonHeight+gap);
        page.style.width=column;page.style.right=width*.055f;
        page.style.top=Mathf.Max(12,(height-total)*.5f);
        var logo=page.Q<VisualElement>("MainMenuLogo");
        if(logo!=null){logo.style.height=logoHeight;logo.style.marginBottom=gap;}
        foreach(var button in page.Query<MainMenuArtButton>().ToList())
        {
            button.style.height=buttonHeight;button.style.marginBottom=gap;
            button.style.fontSize=Mathf.Clamp(column*.056f,12,20);
        }
        float panelWidth=Mathf.Min(540,width-24),panelHeight=Mathf.Min(390,height-24);
        dialog.style.width=panelWidth;dialog.style.height=panelHeight;
        dialog.style.left=(width-panelWidth)*.5f;dialog.style.top=(height-panelHeight)*.5f;
    }
    private void Update()
    {
        if(root!=null&&ambient!=null)ambient.Tick(Time.unscaledDeltaTime,root.contentRect.width,root.contentRect.height);
    }
    private void Label(string text,int size=18)
    {
        var label=new Label(text);label.style.color=new Color(.2f,.13f,.07f);
        label.style.fontSize=size;label.style.whiteSpace=WhiteSpace.Normal;
        label.style.unityTextAlign=TextAnchor.MiddleCenter;label.style.marginBottom=14;dialog.Add(label);
    }
    private MainMenuArtButton Button(VisualElement parent,string text,System.Action action,bool small=false)
    {
        var button=new MainMenuArtButton(text,action,skin,small);
        button.name=text;button.style.height=small?58:110;button.style.marginBottom=8;
        button.style.flexShrink=0;button.style.fontSize=small?17:20;
        if(small){button.style.width=220;button.style.maxWidth=Length.Percent(100);button.style.alignSelf=Align.Center;}
        parent.Add(button);return button;
    }
    private void Home()
    {
        page.Clear();dialog.Clear();dialog.style.display=DisplayStyle.None;page.style.display=DisplayStyle.Flex;
        var logo=new VisualElement{name="MainMenuLogo",pickingMode=PickingMode.Ignore};
        if(skin!=null&&skin.logo!=null)logo.style.backgroundImage=new StyleBackground(skin.logo);
        logo.style.backgroundSize=new BackgroundSize(BackgroundSizeType.Contain);
        logo.style.backgroundPositionX=new BackgroundPosition(BackgroundPositionKeyword.Center);
        logo.style.backgroundPositionY=new BackgroundPosition(BackgroundPositionKeyword.Center);
        page.Add(logo);
        Button(page,"NUEVA PARTIDA",ConfirmNew);
        var continueButton=Button(page,"CONTINUAR",Continue);
        continueButton.SetEnabled(BroteSaveSystem.HasValidSave());continueButton.RefreshState();
        Button(page,"OPCIONES",Options);Button(page,"SALIR",Exit);
        Layout(root.contentRect.width,root.contentRect.height);
    }
    private void ShowDialog()
    {
        page.style.display=DisplayStyle.None;dialog.style.display=DisplayStyle.Flex;dialog.Clear();
    }
    private void Controls()
    {
        ShowDialog();Label("Controles",24);
        Label("WASD / flechas — movimiento\nE — interacción\nESC — pausa");
        Button(dialog,"VOLVER",Options,true);
    }
    private void Slider(string name,float value,System.Action<float> setter)
    {
        var slider=new Slider(name,0,1){value=value};slider.style.color=new Color(.2f,.13f,.07f);
        slider.style.marginBottom=16;slider.RegisterValueChangedCallback(e=>setter(e.newValue));dialog.Add(slider);
    }
    private void Options()
    {
        ShowDialog();Label("Audio",24);
        Slider("Master",BroteAudioManager.MasterVolume,BroteAudioManager.SetMasterVolume);
        Slider("SFX",BroteAudioManager.SFXVolume,BroteAudioManager.SetSFXVolume);
        Button(dialog,"CONTROLES",Controls,true);Button(dialog,"VOLVER",Home,true);
    }
    private void ConfirmNew()
    {
        if(!BroteSaveSystem.HasValidSave()){StartNewGame();return;}
        ShowDialog();Label("¿Comenzar una nueva partida?",24);
        Label("Se borrará el progreso guardado de Sector_01.");
        Button(dialog,"CONFIRMAR",StartNewGame,true);Button(dialog,"CANCELAR",Home,true);
    }
    private void StartNewGame()
    {
        if(BroteSaveSystem.DeleteConfirmedSave())Play();
        else {ShowDialog();Label("No se pudo borrar el guardado.");Button(dialog,"VOLVER",Home,true);}
    }
    private void Continue()
    {
        if(BroteSaveSystem.HasValidSave())Play();else Home();
    }
    private void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying=false;
#else
        Application.Quit();
#endif
    }
    private void Play()
    {
        if(loading)return;loading=true;ShowDialog();Label("Cargando Acopalca…",24);
        SceneManager.LoadSceneAsync("Assets/_BROTE/Scenes/Acopalca/Acopalca.unity",LoadSceneMode.Single);
    }
    private void OnDestroy(){if(panelSettings!=null)Destroy(panelSettings);}
}

