using UnityEngine;
using UnityEngine.UIElements;

// Read-only artistic HUD. UI Toolkit requires no EventSystem for these read-only elements.
public sealed class BroteHUD : MonoBehaviour
{
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private RestorationManager restorationManager;
    [SerializeField] private PanelSettings panelSettings;
    [SerializeField] private SeedlingInventory seedlingInventory;
    [SerializeField] private AudioClip restorationCompleteClip;
    [SerializeField] private GameplayHUDSkin skin;
    private BroteIncrementalSession session;
    private VisualElement hudPanel,progressRoot,progressClip,feedbackPanel;
    private Label objectiveTitle,objectiveDescription,objectiveProgress;
    public Label ObjectiveDescription => objectiveDescription;
    private static readonly Color Ink=new Color(.23f,.15f,.09f);
    private PanelSettings ownedPanel;
    private UIDocument document;
    private Label prompt;
    private Label percentage;
    private Label seedlings;
    private Label minka;
    private Label increment;
    private float incrementUntil;
    private int pendingMilestoneAudio;
    private bool incrementVisible;
    private readonly BroteRestorationFeedbackState progressFeedback=new BroteRestorationFeedbackState();
    [SerializeField] private AudioClip milestoneClip;
    private Label milestone;
    private bool milestoneVisible;
    private float milestoneUntil;
    private string contextualMessage;
    private float contextUntil;
    private int previousSeedlings;
    private bool seedlingVisualInitialized;
    private float seedlingPulseAge = 1f;
    private VisualElement fill;
    private string displayedPrompt;
    private float targetRestoration;
    private float progressPulseAge = 1f;
    private bool restorationVisualInitialized;
    private static readonly Color RestingFillColor = new Color(0.35f, 0.75f, 0.4f);
    private static readonly Color PulseFillColor = new Color(0.53f, 0.86f, 0.55f);

    private void OnEnable()
    {
        var clips=Resources.Load<BroteSFXClips>("Audio/BroteSFXClips");
        if(restorationCompleteClip==null && clips!=null)restorationCompleteClip=clips.sectorRestored;
        if (document == null)
        {
            document = GetComponent<UIDocument>();
            if (document == null) document = gameObject.AddComponent<UIDocument>();
        }
        if (panelSettings == null && ownedPanel == null)
        {
            ownedPanel = ScriptableObject.CreateInstance<PanelSettings>();
            ownedPanel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            ownedPanel.referenceResolution = new Vector2Int(1280, 720);
        }
        document.panelSettings = panelSettings != null ? panelSettings : ownedPanel;
        document.sortingOrder = 10;
        restorationVisualInitialized = false;
        progressPulseAge = 1f;
        seedlingVisualInitialized = false;
        seedlingPulseAge = 1f;
        contextualMessage = null;
        milestoneVisible = false;
        incrementVisible=false;pendingMilestoneAudio=0;
        if(skin==null)skin=Resources.Load<GameplayHUDSkin>("UI/GameplayHUDSkin");
        session=System.Array.Find(FindObjectsByType<BroteIncrementalSession>(),x=>x.gameObject.scene==gameObject.scene);
        BuildUI();
        if (playerInteraction != null) playerInteraction.FeedbackRequested += ShowContext;
        if (restorationManager != null)
        {
            restorationManager.RestorationChanged += RefreshRestoration;
            RefreshRestoration(restorationManager.RestorationPercentage);
        }
        else RefreshRestoration(0f);
        if (seedlingInventory != null)
        {
            seedlingInventory.SeedlingsChanged += RefreshSeedlings;
            RefreshSeedlings(seedlingInventory.CurrentSeedlings);
        }
        else RefreshSeedlings(0);
        RefreshPrompt();
        if(Application.isPlaying){BrotePauseMenu.Attach(this);AmbientZone.AttachSector(this);}
    }

    private void BuildUI()
    {
        VisualElement root=document.rootVisualElement;
        root.Clear();root.name="gameplay-hud-root";
        root.style.display=DisplayStyle.Flex;root.pickingMode=PickingMode.Ignore;
        root.style.position=Position.Absolute;root.style.left=root.style.right=root.style.top=root.style.bottom=0;
        hudPanel=new VisualElement{name="gameplay-hud-panel",pickingMode=PickingMode.Ignore};
        hudPanel.style.position=Position.Absolute;hudPanel.style.top=12;
        NurseryUISkin.Apply(hudPanel,skin!=null?skin.mainPanel:null,new Color(.91f,.83f,.63f,.96f));
        FitArtwork(hudPanel);root.Add(hudPanel);
        objectiveTitle=HUDLabel("objective-title","OBJETIVO 1/6",16,14.5f,21,18.5f,13);
        objectiveDescription=HUDLabel("objective-description",BroteRestorationObjectives.Titles[0],16,7.6f,41,30.5f,22);
        objectiveProgress=HUDLabel("objective-progress","Progreso: 0 / 1",13,7.6f,64,30.5f,12);
        Icon("objective-icon",skin!=null?skin.iconObjective:null,11.4f,20.5f,3);
        HUDLabel("restoration-title","SALUD DEL VALLE",12,46,34,20.7f,9);
        percentage=HUDLabel("restoration-percentage","0%",18,46,43,20.7f,12);
        Icon("restoration-icon",skin!=null?skin.iconRestoration:null,42.5f,35,3);
        progressRoot=new VisualElement{name="RestorationProgressRoot",pickingMode=PickingMode.Ignore};
        Place(progressRoot,43.6f,58.2f,23.6f,16.2f);hudPanel.Add(progressRoot);
        var background=new VisualElement{name="ProgressBackground",pickingMode=PickingMode.Ignore};
        Place(background,0,0,100,100);progressRoot.Add(background); // Track is painted into MainPanel.
        progressClip=new VisualElement{name="ProgressClip",pickingMode=PickingMode.Ignore};
        Place(progressClip,0,0,0,100);progressClip.style.overflow=Overflow.Hidden;progressRoot.Add(progressClip);
        fill=new VisualElement{name="GameplayHUD_ProgressFill",pickingMode=PickingMode.Ignore};
        fill.style.position=Position.Absolute;fill.style.left=fill.style.top=fill.style.bottom=0;
        NurseryUISkin.Apply(fill,skin!=null?skin.progressFill:null,RestingFillColor);
        FitArtwork(fill);progressClip.Add(fill);
        progressRoot.RegisterCallback<GeometryChangedEvent>(_=>ResizeProgressArtwork());
        // Cover only the inner baked medallion; preserve the surrounding official frame.
        ResourceIcon("seedling-icon",skin!=null?skin.iconSeedling:null,73.4f);
        ResourceIcon("minka-icon",skin!=null?skin.iconMinka:null,86.6f);
        seedlings=HUDLabel("hud-seedlings","0 / 5",18,72.4f,64,10.3f,16);
        seedlings.tooltip="Plántulas / capacidad";
        minka=HUDLabel("hud-minka","0",18,85.8f,64,10.3f,16);minka.tooltip="Minka disponible";
        feedbackPanel=new VisualElement{name="hud-temporary-feedback",pickingMode=PickingMode.Ignore};
        feedbackPanel.style.position=Position.Absolute;feedbackPanel.style.bottom=96;
        feedbackPanel.style.left=Length.Percent(25);feedbackPanel.style.right=Length.Percent(25);
        feedbackPanel.style.alignItems=Align.Center;root.Add(feedbackPanel);
        increment=MakeLabel("",16);increment.style.color=new Color(.6f,.9f,.6f);
        increment.style.display=DisplayStyle.None;feedbackPanel.Add(increment);
        milestone=MakeLabel("",16);milestone.style.color=new Color(.66f,.9f,.64f);
        milestone.style.display=DisplayStyle.None;feedbackPanel.Add(milestone);
        prompt=MakeLabel("",20);prompt.name="hud-interaction-prompt";
        prompt.style.position=Position.Absolute;prompt.style.bottom=24;
        prompt.style.left=Length.Percent(24);prompt.style.right=Length.Percent(24);
        prompt.style.unityTextAlign=TextAnchor.MiddleCenter;
        prompt.style.paddingTop=prompt.style.paddingBottom=10;
        prompt.style.backgroundColor=new Color(.06f,.12f,.09f,.92f);root.Add(prompt);
        displayedPrompt=null;
        root.RegisterCallback<GeometryChangedEvent>(_=>AdaptHUDLayout());
        AdaptHUDLayout();RefreshObjective();
    }
    private static void Place(VisualElement element,float x,float y,float width,float height)
    {
        element.style.position=Position.Absolute;element.style.left=Length.Percent(x);
        element.style.top=Length.Percent(y);element.style.width=Length.Percent(width);element.style.height=Length.Percent(height);
        element.style.minWidth=element.style.minHeight=0;
    }
    private static void FitArtwork(VisualElement element)
    {
        element.style.backgroundSize=new BackgroundSize(BackgroundSizeType.Contain);
        element.style.backgroundPositionX=new BackgroundPosition(BackgroundPositionKeyword.Center);
        element.style.backgroundPositionY=new BackgroundPosition(BackgroundPositionKeyword.Center);
        element.style.backgroundRepeat=new BackgroundRepeat(Repeat.NoRepeat,Repeat.NoRepeat);
    }
    private Label HUDLabel(string name,string text,int size,float x,float y,float width,float height)
    {
        var label=MakeLabel(text,size);label.name=name;label.style.color=Ink;
        label.style.unityTextAlign=TextAnchor.MiddleCenter;Place(label,x,y,width,height);
        hudPanel.Add(label);return label;
    }
    private void Icon(string name,Sprite sprite,float x,float y,float width)
    {
        var icon=new VisualElement{name=name,pickingMode=PickingMode.Ignore};
        Place(icon,x,y,width,width*1657/343);
        NurseryUISkin.Apply(icon,sprite,Color.clear);FitArtwork(icon);hudPanel.Add(icon);
    }
    private void ResourceIcon(string name,Sprite sprite,float x)
    {
        var backing=new VisualElement{name=name+"-backing",pickingMode=PickingMode.Ignore};
        Place(backing,x,27,6.9f,6.9f*1657/343);
        backing.style.backgroundColor=new Color(.86f,.73f,.48f);
        backing.style.borderTopLeftRadius=backing.style.borderTopRightRadius=backing.style.borderBottomLeftRadius=backing.style.borderBottomRightRadius=Length.Percent(50);
        hudPanel.Add(backing);
        Icon(name,sprite,x,27,6.9f);
    }
    private void AdaptHUDLayout()
    {
        var root=document.rootVisualElement;float width=root.resolvedStyle.width;
        if(float.IsNaN(width)||width<=0)return;
        float panelWidth=Mathf.Min(820,Mathf.Max(280,width-32));
        float ratio=skin!=null&&skin.mainPanel!=null?skin.mainPanel.rect.width/skin.mainPanel.rect.height:1657f/343;
        hudPanel.style.width=panelWidth;hudPanel.style.height=panelWidth/ratio;hudPanel.style.left=(width-panelWidth)*.5f;
        // The official atlas has 23 transparent pixels above the painted frame.
        // Align the painted edge at 12 units without cropping or changing the art.
        hudPanel.style.top=Mathf.Max(0,12-panelWidth/ratio*(23f/343));
        float scale=Mathf.Clamp(panelWidth/920,.78f,1);
        objectiveTitle.style.fontSize=16*scale;objectiveDescription.style.fontSize=16*scale;
        objectiveProgress.style.fontSize=13*scale;percentage.style.fontSize=18*scale;
        hudPanel.Q<Label>("restoration-title").style.fontSize=12*scale;
        seedlings.style.fontSize=minka.style.fontSize=18*scale;
        ResizeProgressArtwork();
    }
    private void ResizeProgressArtwork()
    {
        float width=progressRoot.resolvedStyle.width;
        if(!float.IsNaN(width))fill.style.width=Mathf.Max(0,width);
    }
    private void RefreshObjective()
    {
        var objectives=session!=null?session.Objectives:null;
        int claimed=objectives!=null?Mathf.Clamp(objectives.Claimed,0,6):0;
        objectiveTitle.text=$"OBJETIVO {Mathf.Min(claimed+1,6)}/6";
        objectiveDescription.text=claimed<6?BroteRestorationObjectives.Titles[claimed]:"Primer ciclo completado";
        bool observed=objectives!=null&&claimed<6&&(objectives.Observed&(1<<claimed))!=0;
        objectiveProgress.text=claimed<6?$"Progreso: {(observed?1:0)} / 1":"Completados: 6 / 6";
    }

    private static Label MakeLabel(string text, int size)
    {
        var label = new Label(text);
        label.pickingMode = PickingMode.Ignore;
        label.style.color = Color.white;
        label.style.fontSize = size;
        label.style.whiteSpace=WhiteSpace.Normal;
        label.style.unityFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return label;
    }

    // Interaction already selects its target each frame; restoration is event-driven.
    private void LateUpdate()
    {
        var economy=session!=null?session.Minka:BroteSaveSystem.Current?.GetComponent<BroteMinkaManager>();
        if(minka!=null)minka.text= economy!=null?economy.Minka.ToString():"0";
        RefreshObjective();
        bool blocked=(BrotePauseMenu.IsPaused&&!BrotePauseMenu.IsSectorRestoredVisible)||NurseryManagementPanel.IsOpen;
        hudPanel.style.display=feedbackPanel.style.display=blocked?DisplayStyle.None:DisplayStyle.Flex;
        RefreshPrompt();
        if(BrotePauseMenu.IsPaused){prompt.style.display=DisplayStyle.None;displayedPrompt=null;}
        AnimateRestoration();
        AnimateInventory();
        if(progressFeedback.TryTake(Time.unscaledTime,out float amount))
        {
            increment.text="+"+amount.ToString("0.#")+" Salud del Valle";
            increment.style.display=DisplayStyle.Flex;incrementVisible=true;incrementUntil=Time.unscaledTime+1f;
            if(pendingMilestoneAudio>0)
            {
                BroteAudioFeedback.PlayOptional(pendingMilestoneAudio==100?restorationCompleteClip:milestoneClip,playerInteraction!=null?playerInteraction.transform.position:transform.position,false,pendingMilestoneAudio==100?BroteAudioCategory.SFX:BroteAudioCategory.UI);
                pendingMilestoneAudio=0;
            }
        }
        if(incrementVisible&&Time.unscaledTime>=incrementUntil){incrementVisible=false;increment.style.display=DisplayStyle.None;}
        if (milestoneVisible && Time.unscaledTime >= milestoneUntil)
        {
            milestoneVisible = false;
            milestone.style.display = DisplayStyle.None;
        }
    }

    private void AnimateRestoration()
    {
        if (fill == null) return;
        // No visual lag: clip and text both represent the manager's exact current total.
        progressClip.style.width=Length.Percent(targetRestoration);
        if (progressPulseAge < 1f)
        {
            progressPulseAge = Mathf.Min(1f, progressPulseAge + Time.unscaledDeltaTime / 0.4f);
            if(skin==null||skin.progressFill==null)
                fill.style.backgroundColor=Color.Lerp(RestingFillColor,PulseFillColor,BroteTween.Pulse(progressPulseAge,1f));
        }
    }
    private void AnimateInventory()
    {
        if (seedlings == null || seedlingPulseAge >= 1f) return;
        seedlingPulseAge = Mathf.Min(1f, seedlingPulseAge + Time.unscaledDeltaTime / 0.35f);
        seedlings.style.color = Color.Lerp(Ink, new Color(0.23f, 0.46f, 0.17f), BroteTween.Pulse(seedlingPulseAge, 1f));
    }
    private void ShowContext(string message)
    {
        contextualMessage = message; contextUntil = Time.unscaledTime + (message == "Que\u00f1ual plantado" ? 0.9f : 1f);
        RefreshPrompt();
    }

    private void RefreshPrompt()
    {
        IInteractable target = playerInteraction != null ? playerInteraction.CurrentInteractable : null;
        // Interface references do not use Unity's destroyed-object null comparison.
        bool available = target != null && (!(target is Object obj) || obj != null) && playerInteraction.IsWithinRange(target);
        bool contextActive = contextualMessage != null && Time.unscaledTime < contextUntil;
        string text = contextActive ? contextualMessage : available ? target.InteractionPrompt : "";
        available |= contextActive;
        if (text == displayedPrompt) return;
        displayedPrompt = text;
        prompt.text = text;
        prompt.style.display = available ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void RefreshRestoration(float value)
    {
        value = Mathf.Clamp(value, 0f, 100f);
        if (BroteSaveSystem.IsLoading) restorationVisualInitialized = false;
        if (restorationVisualInitialized)
        {
            int highest=progressFeedback.Observe(value,Time.unscaledTime);
            if(highest==100 && !BroteSaveSystem.IsLoading)
                GetComponentInChildren<BrotePauseMenu>(true)?.ShowSectorRestored();
            if (highest > 0)
            {
                milestone.text=BroteRestorationFeedbackState.MilestoneText(highest);
                milestone.style.display = DisplayStyle.Flex;
                milestoneVisible = true; milestoneUntil = Time.unscaledTime + 2f;
                pendingMilestoneAudio=highest;
            }
        }
        if (!restorationVisualInitialized)
        {
            progressFeedback.Sync(value);
            progressClip.style.width = Length.Percent(value);
            restorationVisualInitialized = true;
        }
        else if (value > targetRestoration) progressPulseAge = 0f;
        targetRestoration = value;
        progressClip.style.width=Length.Percent(value);
        // Text and clip report the same gameplay total without interpolation.
        percentage.text = $"{value:0.#}%";
    }

    private void RefreshSeedlings(int value)
    {
        int maximum = seedlingInventory != null ? seedlingInventory.MaxSeedlings : 5;
        if (!BroteSaveSystem.IsLoading && seedlingVisualInitialized && value > previousSeedlings) seedlingPulseAge = 0f;
        previousSeedlings = value; seedlingVisualInitialized = true;
        seedlings.text = $"{value} / {maximum}";
    }

    private void OnDisable()
    {
        if (playerInteraction != null) playerInteraction.FeedbackRequested -= ShowContext;
        if (restorationManager != null) restorationManager.RestorationChanged -= RefreshRestoration;
        if (seedlingInventory != null) seedlingInventory.SeedlingsChanged -= RefreshSeedlings;
        var pause=GetComponentInChildren<BrotePauseMenu>(true);if(pause!=null)pause.enabled=false;
        if (document != null) document.rootVisualElement.style.display = DisplayStyle.None;
    }

    private void OnDestroy()
    {
        if (ownedPanel != null) Destroy(ownedPanel);
    }
}
