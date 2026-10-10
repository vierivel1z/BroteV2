using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// Both nursery tabs share the existing modal input context.
public sealed class NurseryManagementPanel : MonoBehaviour
{
    private static NurseryManagementPanel current;
    public static bool IsOpen => current!=null && current.open;
    public static int ClosedFrame { get; private set; } = -1;
    private bool open, upgradesVisible;
    private BroteIncrementalSession session;
    private VisualElement root, shell, safeArea, nursery, upgrades, heading, productionInfo, contentViewport;
    private Button productionTab, upgradesTab;
    private NurseryUISkin skin;
    private VisualTreeAsset upgradeCardTemplate;
    private VisualElement progressTrack, progressInterior, progressClip, progressFill;
    private PanelSettings ownedPanelSettings;
    private ScrollView productionScroll, upgradeScroll;
    private Label title;
    private Button closeButton;
    private Label percentage, clickEffect, automatic, fullMessage, help;
    private ProgressBar progress;
    private Label inventory, income;
    private Button prepare;
    private readonly bool[] buyHeld=new bool[5];
    private readonly Button[] buy=new Button[5];
    private readonly Label[] effects=new Label[5], levels=new Label[5], costs=new Label[5];
    private float pulseUntil;
    private bool primaryHeld;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() { current=null; ClosedFrame=-1; }
    public static void Attach(BroteHUD hud,BroteIncrementalSession owner)
    {
        var go=new GameObject("NurseryManagementPanel"); go.transform.SetParent(owner.transform,false);
        var panel=go.AddComponent<NurseryManagementPanel>(); panel.session=owner; panel.Build(hud.GetComponent<UIDocument>().panelSettings); current=panel;
    }
    public static void OpenFor(PlayerInteraction player)
    {
        if(current==null || current.session.Player!=player || BrotePauseMenu.IsPaused) return;
        current.open=true; current.upgradesVisible=false; current.Refresh(); current.root.style.display=DisplayStyle.Flex;
    }
    public void Close() { open=false; primaryHeld=false; ClosedFrame=Time.frameCount; root.style.display=DisplayStyle.None; }
    public void ToggleView() { if(!open)return;upgradesVisible=!upgradesVisible;Refresh(); }
    public void HandleNavigation(bool tab,bool escape)
    { if(!open || BrotePauseMenu.IsPaused)return;if(escape)Close();else if(tab)ToggleView(); }
    private void Build(PanelSettings settings)
    {
        // Use physical UI pixels for this sidebar without changing the HUD's shared scale.
        ownedPanelSettings=Instantiate(settings);
        ownedPanelSettings.scaleMode=PanelScaleMode.ConstantPixelSize;ownedPanelSettings.scale=1;
        var doc=gameObject.AddComponent<UIDocument>();doc.panelSettings=ownedPanelSettings;doc.sortingOrder=20;
        skin=Resources.Load<NurseryUISkin>("UI/NurseryUISkin");
        upgradeCardTemplate=Resources.Load<VisualTreeAsset>("UI/NurseryUpgradeCard");
        root=doc.rootVisualElement;root.name="nursery-modal";
        var baseStyles=Resources.Load<StyleSheet>("UI/NurseryPanel");
        if(baseStyles!=null)root.styleSheets.Add(baseStyles);
        if(skin!=null && skin.additionalStyles!=null)root.styleSheets.Add(skin.additionalStyles);
        root.style.position=Position.Absolute;root.style.left=root.style.right=root.style.top=root.style.bottom=0;
        root.style.alignItems=Align.FlexEnd;root.style.justifyContent=Justify.Center;
        root.style.backgroundColor=Color.clear;root.style.paddingRight=24;root.style.display=DisplayStyle.None;
        root.pickingMode=PickingMode.Ignore;
        shell=new VisualElement{name="nursery-shell"};shell.AddToClassList("nursery-panel");
        shell.style.width=410;shell.style.height=Length.Percent(86);shell.style.minWidth=0;shell.style.minHeight=0;shell.style.flexShrink=0;
        shell.style.unityFont=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");shell.style.flexDirection=FlexDirection.Column;
        shell.style.paddingLeft=shell.style.paddingRight=40;shell.style.paddingTop=48;shell.style.paddingBottom=42;
        ApplyFrame(shell,skin!=null?skin.panelBackground:null,new Color(.06f,.12f,.09f,.98f),skin!=null?skin.panelSliceScale:1,skin!=null?skin.panelSliceBorders:Vector4.zero);
        root.Add(shell);
        safeArea=new VisualElement{name="nursery-safe-area"};safeArea.style.flexGrow=1;safeArea.style.minHeight=0;safeArea.style.flexDirection=FlexDirection.Column;shell.Add(safeArea);
        heading=new VisualElement{name="nursery-heading"};heading.AddToClassList("nursery-fixed-header");heading.style.flexShrink=0;safeArea.Add(heading);
        title=Text("GESTIONAR VIVERO",22);title.name="nursery-title";title.style.paddingRight=38;heading.Add(title);
        var resources=new VisualElement{name="nursery-resources"};resources.style.flexDirection=FlexDirection.Row;resources.style.flexWrap=Wrap.Wrap;heading.Add(resources);
        income=Text("",17);income.name="nursery-minka";inventory=Text("",17);inventory.name="nursery-inventory";
        resources.Add(ResourceSlot("minka-icon",skin!=null?skin.iconMinka:null,income));
        resources.Add(ResourceSlot("seedling-icon",skin!=null?skin.iconSeedling:null,inventory));
        var tabs=new VisualElement{name="nursery-tabs"};tabs.AddToClassList("nursery-fixed-tabs");tabs.style.flexDirection=FlexDirection.Row;tabs.style.justifyContent=Justify.SpaceBetween;tabs.style.flexShrink=0;safeArea.Add(tabs);
        productionTab=MakeButton("PLÁNTULAS",()=>{upgradesVisible=false;Refresh();});productionTab.name="production-tab";productionTab.AddToClassList("nursery-tab");
        upgradesTab=MakeButton("MEJORAS",()=>{upgradesVisible=true;Refresh();});upgradesTab.name="upgrades-tab";upgradesTab.AddToClassList("nursery-tab");
        productionTab.style.width=upgradesTab.style.width=Length.Percent(48);tabs.Add(productionTab);tabs.Add(upgradesTab);
        contentViewport=new VisualElement{name="nursery-content-viewport"};
        contentViewport.style.flexGrow=1;contentViewport.style.flexShrink=1;contentViewport.style.minHeight=0;
        contentViewport.style.overflow=Overflow.Hidden;safeArea.Add(contentViewport);
        productionScroll=CreateContentScroll("production-view");nursery=productionScroll;
        nursery.style.flexGrow=1;nursery.style.flexShrink=1;nursery.style.minHeight=0;contentViewport.Add(nursery);
        productionScroll.horizontalScrollerVisibility=ScrollerVisibility.Hidden;
        var productionTitle=Text("PREPARACIÓN DE PLÁNTULA",17);productionTitle.name="production-title";nursery.Add(productionTitle);
        BuildProgress();
        percentage=Text("0%",18);percentage.name="production-percentage";percentage.style.unityTextAlign=TextAnchor.MiddleCenter;nursery.Add(percentage);
        prepare=MakeButton("PREPARAR PLÁNTULA",()=>{session.Click();pulseUntil=Time.unscaledTime+.18f;Refresh();});prepare.name="prepare-seedling";prepare.AddToClassList("nursery-produce-button");nursery.Add(prepare);
        prepare.RegisterCallback<PointerDownEvent>(evt=>{if(evt.button==0 && prepare.enabledInHierarchy){primaryHeld=true;Refresh();}},TrickleDown.TrickleDown);
        prepare.RegisterCallback<PointerUpEvent>(evt=>{if(evt.button==0){primaryHeld=false;Refresh();}},TrickleDown.TrickleDown);
        prepare.RegisterCallback<PointerCancelEvent>(_=>{primaryHeld=false;Refresh();});
        prepare.RegisterCallback<PointerCaptureOutEvent>(_=>{primaryHeld=false;Refresh();});
        prepare.RegisterCallback<PointerLeaveEvent>(_=>{primaryHeld=false;Refresh();});
        productionInfo=new VisualElement{name="production-info"};productionInfo.style.flexShrink=0;nursery.Add(productionInfo);
        clickEffect=Text("",16);productionInfo.Add(clickEffect);automatic=Text("",15);productionInfo.Add(automatic);
        fullMessage=Text("INVENTARIO LLENO",15);fullMessage.name="inventory-full";fullMessage.style.color=new Color(.55f,.2f,.08f);nursery.Add(fullMessage);


        upgrades=new VisualElement{name="upgrade-view"};upgrades.style.flexGrow=1;upgrades.style.flexShrink=1;upgrades.style.minHeight=0;contentViewport.Add(upgrades);
        upgradeScroll=CreateContentScroll("upgrade-list");upgradeScroll.style.flexGrow=1;
        upgradeScroll.style.flexShrink=1;upgradeScroll.style.minHeight=0;
        upgradeScroll.horizontalScrollerVisibility=ScrollerVisibility.Hidden;
        upgradeScroll.contentContainer.style.flexDirection=FlexDirection.Column;upgrades.Add(upgradeScroll);
        string[] titles={"CLICK POWER","RIEGO","CAPACIDAD","PREPARACI\u00d3N DEL SUELO","EXPANSI\u00d3N"};
        for(int i=0;i<5;i++)
        {
            var kind=(BroteUpgrade)i;var card=CreateUpgradeCard(i,titles[i]);
            buy[i].clicked+=()=>{session.Upgrades.TryBuy(kind);Refresh();};upgradeScroll.Add(card);
        }
        closeButton=MakeButton("",Close);closeButton.name="nursery-close";closeButton.tooltip="Cerrar vivero (ESC)";closeButton.AddToClassList("nursery-close-button");
        closeButton.style.position=Position.Absolute;closeButton.style.right=0;closeButton.style.top=0;
        closeButton.style.width=closeButton.style.height=closeButton.style.minWidth=closeButton.style.minHeight=32;
        closeButton.style.marginTop=closeButton.style.marginBottom=0;
        closeButton.style.borderLeftWidth=closeButton.style.borderRightWidth=closeButton.style.borderTopWidth=closeButton.style.borderBottomWidth=0;
        heading.Add(closeButton);NurseryUISkin.Apply(closeButton,skin!=null?skin.closeButtonBackground:null,new Color(.16f,.3f,.19f));
        FitSprite(closeButton);if(skin==null || skin.closeButtonBackground==null)closeButton.text="×";
        help=Text("TAB: Cambiar pesta\u00f1a · ESC: Cerrar",12);help.name="nursery-help";safeArea.Add(help);
        root.RegisterCallback<GeometryChangedEvent>(_=>AdaptLayout());
        AdaptLayout();
    }
    private static ScrollView CreateContentScroll(string name)
    {
        var scroll=new ScrollView(ScrollViewMode.Vertical){name=name};
        scroll.touchScrollBehavior=ScrollView.TouchScrollBehavior.Clamped;
        scroll.style.overflow=Overflow.Hidden;
        // Explicit clipping also works when the shared PanelSettings has no theme sheet.
        scroll.contentViewport.style.overflow=Overflow.Hidden;
        scroll.contentViewport.style.minHeight=0;scroll.contentViewport.style.minWidth=0;
        scroll.contentViewport.style.flexGrow=1;scroll.contentViewport.style.flexShrink=1;
        var scrollBody=scroll.contentViewport.parent;
        scrollBody.style.flexDirection=FlexDirection.Row;scrollBody.style.flexGrow=1;
        scrollBody.style.flexShrink=1;scrollBody.style.minHeight=0;
        scroll.verticalScroller.style.width=12;scroll.verticalScroller.style.flexShrink=0;
        scroll.contentContainer.style.flexShrink=0;
        scroll.horizontalScrollerVisibility=ScrollerVisibility.Hidden;
        return scroll;
    }
    private void BuildProgress()
    {
        progress=new ProgressBar{name="production-progress",lowValue=0,highValue=100};
        progress.style.height=38;progress.style.minHeight=38;progress.style.flexShrink=0;progress.style.backgroundColor=Color.clear;
        progress.style.marginBottom=4;nursery.Add(progress);
        var nativeTrack=progress.Q<VisualElement>(className:"unity-progress-bar__background");
        if(nativeTrack!=null)nativeTrack.style.display=DisplayStyle.None;
        progress.style.paddingLeft=progress.style.paddingRight=progress.style.paddingTop=progress.style.paddingBottom=0;
        progress.style.borderLeftWidth=progress.style.borderRightWidth=progress.style.borderTopWidth=progress.style.borderBottomWidth=0;
        progressTrack=new VisualElement{name="production-progress-track",pickingMode=PickingMode.Ignore};
        progressTrack.style.position=Position.Absolute;progressTrack.style.left=progressTrack.style.right=progressTrack.style.top=progressTrack.style.bottom=0;
        progress.Add(progressTrack);
        progressInterior=new VisualElement{name="production-progress-interior",pickingMode=PickingMode.Ignore};
        progressInterior.style.position=Position.Absolute;
        Vector4 inset=skin!=null && skin.progressBackground!=null?skin.progressInsets:Vector4.zero;
        progressInterior.style.left=inset.x;progressInterior.style.top=inset.y;
        progressInterior.style.right=inset.z;progressInterior.style.bottom=inset.w;progressTrack.Add(progressInterior);
        progressClip=new VisualElement{name="production-progress-clip",pickingMode=PickingMode.Ignore};
        progressClip.style.position=Position.Absolute;progressClip.style.left=progressClip.style.top=progressClip.style.bottom=0;
        progressClip.style.right=StyleKeyword.Auto;progressClip.style.minWidth=0;
        progressClip.style.overflow=Overflow.Hidden;progressClip.style.width=Length.Percent(0);progressInterior.Add(progressClip);
        progressFill=new VisualElement{name="production-progress-fill",pickingMode=PickingMode.Ignore};
        progressFill.style.position=Position.Absolute;progressFill.style.left=progressFill.style.top=progressFill.style.bottom=0;
        progressFill.style.right=StyleKeyword.Auto;progressFill.style.minWidth=0;progressClip.Add(progressFill);
        progressInterior.RegisterCallback<GeometryChangedEvent>(_=>ResizeProgressArtwork());
        NurseryUISkin.Apply(progressTrack,skin!=null?skin.progressBackground:null,new Color(.17f,.23f,.18f));
        NurseryUISkin.Apply(progressFill,skin!=null?skin.progressFill:null,new Color(.42f,.74f,.43f));
    }
    private void ResizeProgressArtwork()
    {
        // Artwork always spans the full interior; only the parent's visible width changes.
        float width=progressInterior.resolvedStyle.width;
        if(!float.IsNaN(width))progressFill.style.width=Mathf.Max(0,width);
    }
    private void AdaptLayout()
    {
        float width=root.resolvedStyle.width,height=root.resolvedStyle.height;
        bool compact=height>0 && height<650;bool narrow=width>0 && width<1000;
        root.style.paddingRight=narrow?16:24;
        shell.style.width=narrow?Mathf.Max(200,Mathf.Min(380,width*.42f)):Mathf.Clamp(width*.32f,390,430);
        shell.style.height=Length.Percent(height>0 && height<450?88:86);
        bool framed=skin!=null && skin.panelBackground!=null;
        shell.style.paddingLeft=shell.style.paddingRight=framed?(compact?54:58):(compact?18:24);
        shell.style.paddingTop=framed?(compact?32:38):(compact?12:20);
        shell.style.paddingBottom=framed?(compact?32:38):(compact?12:20);
        title.style.fontSize=compact?18:22;title.style.minHeight=compact?21:25;
        income.style.fontSize=compact?14:17;income.style.minHeight=compact?17:20;
        inventory.style.fontSize=compact?14:17;inventory.style.minHeight=compact?17:20;
        productionTab.style.height=upgradesTab.style.height=compact?58:72;
        productionTab.style.minHeight=upgradesTab.style.minHeight=compact?58:72;
        productionTab.style.fontSize=upgradesTab.style.fontSize=compact?12:14;
        productionTab.style.paddingTop=upgradesTab.style.paddingTop=compact?14:20;
        prepare.style.height=compact?44:54;prepare.style.minHeight=compact?44:54;prepare.style.fontSize=compact?16:18;
        clickEffect.style.fontSize=compact?14:16;automatic.style.fontSize=compact?13:15;
        help.style.display=compact?DisplayStyle.None:DisplayStyle.Flex;
        closeButton.style.height=closeButton.style.minHeight=32;
    }
    private static void ApplyFrame(VisualElement element,Sprite sprite,Color placeholder,float scale,Vector4 fallback)
    {
        NurseryUISkin.Apply(element,sprite,placeholder);
        Vector4 border=sprite!=null?(sprite.border!=Vector4.zero?sprite.border:fallback):Vector4.zero;
        element.style.unitySliceLeft=Mathf.RoundToInt(border.x);element.style.unitySliceBottom=Mathf.RoundToInt(border.y);
        element.style.unitySliceRight=Mathf.RoundToInt(border.z);element.style.unitySliceTop=Mathf.RoundToInt(border.w);
        element.style.unitySliceScale=scale;
    }
    // Shared external template; only data and callbacks differ among the five cards.
    private VisualElement CreateUpgradeCard(int index,string title)
    {
        VisualElement card;
        if(upgradeCardTemplate!=null)card=upgradeCardTemplate.CloneTree().Q<VisualElement>("upgrade-card");
        else
        {
            card=new VisualElement();
            foreach(string name in new[]{"upgrade-title","upgrade-level","upgrade-effect","upgrade-cost"})card.Add(new Label{name=name});
            card.Add(new Button{name="upgrade-buy"});
        }
        card.name="upgrade-card-"+index;
        var icon=card.Q<VisualElement>("upgrade-icon");
        if(icon==null){icon=new VisualElement{name="upgrade-icon"};card.Insert(0,icon);}
        icon.style.width=icon.style.height=36;icon.style.flexShrink=0;icon.style.marginBottom=4;
        Sprite iconSprite=skin==null?null:new[]{skin.iconSoilPreparation,skin.iconIrrigation,skin.iconCapacity,skin.iconSoilPreparation,skin.iconExpansion}[index];
        NurseryUISkin.Apply(icon,iconSprite,new Color(.35f,.4f,.28f,.2f));FitSprite(icon);
        card.AddToClassList("nursery-upgrade-card");card.style.flexDirection=FlexDirection.Column;card.style.flexShrink=0;
        card.style.minHeight=166;card.style.paddingLeft=card.style.paddingRight=skin!=null && skin.upgradeCardBackground!=null?48:20;card.style.paddingTop=card.style.paddingBottom=skin!=null && skin.upgradeCardBackground!=null?38:16;card.style.marginBottom=10;
        ApplyFrame(card,skin!=null?skin.upgradeCardBackground:null,new Color(.1f,.19f,.12f),skin!=null?skin.cardSliceScale:1,skin!=null?skin.cardSliceBorders:Vector4.zero);
        var titleLabel=card.Q<Label>("upgrade-title");titleLabel.text=title;StyleText(titleLabel,17);
        levels[index]=card.Q<Label>("upgrade-level");StyleText(levels[index],14);
        effects[index]=card.Q<Label>("upgrade-effect");StyleText(effects[index],14);
        costs[index]=card.Q<Label>("upgrade-cost");StyleText(costs[index],14);
        buy[index]=card.Q<Button>("upgrade-buy");StyleButton(buy[index]);
        buy[index].style.marginTop=buy[index].style.marginBottom=4;
        buy[index].style.borderLeftWidth=buy[index].style.borderRightWidth=buy[index].style.borderTopWidth=buy[index].style.borderBottomWidth=0;
        buy[index].style.unityTextAlign=TextAnchor.MiddleCenter;
        buy[index].style.whiteSpace=WhiteSpace.NoWrap;buy[index].style.overflow=Overflow.Visible;
        FitSprite(buy[index]);
        buy[index].RegisterCallback<GeometryChangedEvent>(_=>LayoutBuyButton(buy[index]));
        buy[index].RegisterCallback<PointerDownEvent>(e=>{if(e.button==0&&buy[index].enabledInHierarchy){buyHeld[index]=true;Refresh();}},TrickleDown.TrickleDown);
        buy[index].RegisterCallback<PointerUpEvent>(_=>{buyHeld[index]=false;Refresh();},TrickleDown.TrickleDown);
        buy[index].RegisterCallback<PointerLeaveEvent>(_=>{buyHeld[index]=false;Refresh();});
        buy[index].RegisterCallback<PointerCaptureOutEvent>(_=>{buyHeld[index]=false;Refresh();});
        if(skin!=null && skin.upgradeCardBackground!=null)
            foreach(var label in card.Query<Label>().ToList())label.style.color=new Color(.23f,.15f,.09f);
        NurseryUISkin.Apply(buy[index],skin!=null?skin.buyButtonBackground:null,new Color(.16f,.3f,.19f));
        return card;
    }
    private void LayoutBuyButton(Button button)
    {
        float width=button.resolvedStyle.width;
        if(float.IsNaN(width)||width<=0)return;
        // This sprite includes a medallion above the text plate: never squeeze it to 32px.
        float aspect=skin!=null&&skin.buyAvailable!=null?skin.buyAvailable.rect.width/skin.buyAvailable.rect.height:1.74f;
        float height=Mathf.Min(126,width/aspect);
        float drawnWidth=Mathf.Min(width,height*aspect);
        button.style.height=button.style.minHeight=height;
        button.style.paddingLeft=button.style.paddingRight=(width-drawnWidth)*.5f+drawnWidth*.18f;
        button.style.paddingTop=height*.46f;button.style.paddingBottom=height*.16f;
        button.style.fontSize=width<190?13:14;
    }
    private void StyleText(Label l,int size)
    {
        l.style.fontSize=size;l.style.color=skin!=null && skin.panelBackground!=null?new Color(.23f,.15f,.09f):Color.white;l.style.whiteSpace=WhiteSpace.Normal;
        l.style.flexShrink=0;l.style.minHeight=size+3;l.style.marginBottom=4;
    }
    private Label Text(string text,int size)
    {
        var l=new Label(text);StyleText(l,size);return l;
    }
    private static Button MakeButton(string text,Action action)
    {var b=new Button(action){text=text};StyleButton(b);return b;}
    private static void StyleButton(Button b)
    {b.AddToClassList("nursery-button");b.style.height=38;b.style.minHeight=38;b.style.flexShrink=0;b.style.marginTop=6;b.style.marginBottom=6;b.style.fontSize=16;b.style.color=Color.white;b.style.backgroundColor=new Color(.16f,.3f,.19f);b.style.whiteSpace=WhiteSpace.Normal;}
    private void Update()
    {
        var keyboard=Keyboard.current;
        if(keyboard!=null && !BrotePauseMenu.IsPaused)
        {
            if(open && keyboard.escapeKey.wasPressedThisFrame) HandleNavigation(false,true);
            else if(keyboard.tabKey.wasPressedThisFrame)
            {
                if(open) HandleNavigation(true,false);
                else
                {
                    var source=UnityEngine.Object.FindObjectsByType<NurserySeedlingSource>().FirstOrDefault(x=>x.gameObject.scene==session.gameObject.scene);
                    if(source!=null && session.Player.IsWithinRange(source)) OpenFor(session.Player);
                }
            }
        }
        if(open)
        {
            root.style.display=BrotePauseMenu.IsPaused ? DisplayStyle.None : DisplayStyle.Flex;Refresh();
            prepare.style.scale=new Scale(Vector3.one*(Time.unscaledTime<pulseUntil ? 1.025f : 1f));
        }
    }
    private void Refresh()
    {
        var inv=session.Production.Inventory;var up=session.Upgrades;
        inventory.text=$"{inv.CurrentSeedlings} / {inv.MaxSeedlings}";inventory.tooltip="Pl\u00e1ntulas / capacidad";
        clickEffect.text=$"+{up.Value(BroteUpgrade.ClickPower)}% por click";automatic.text=$"Producci\u00f3n autom\u00e1tica: {up.Value(BroteUpgrade.Riego)}% / segundo";
        progress.value=(float)session.Production.Progress;progress.title="";progressClip.style.width=Length.Percent(Mathf.Clamp(progress.value,0,100));percentage.text=$"{session.Production.Progress:0.#}%";
        bool full=inv.CurrentSeedlings>=inv.MaxSeedlings;prepare.SetEnabled(!full && !BrotePauseMenu.IsPaused && Time.timeScale>0);
        if(upgradesVisible || !prepare.enabledSelf)primaryHeld=false;
        NurseryUISkin.Apply(prepare,PrimarySprite(),full?new Color(.22f,.27f,.23f):new Color(.28f,.53f,.3f));prepare.style.color=skin!=null && PrimarySprite()!=null?new Color(.23f,.15f,.09f):Color.white;prepare.text="PREPARAR PL\u00c1NTULA";fullMessage.style.display=full?DisplayStyle.Flex:DisplayStyle.None;
        income.text=$"{session.Minka.Minka}";income.tooltip=$"Minka: +{session.DevelopedCount*.6f:0.#}/min";
        upgrades.style.display=upgradesVisible ? DisplayStyle.Flex : DisplayStyle.None;
        nursery.style.display=upgradesVisible ? DisplayStyle.None : DisplayStyle.Flex;
        SetTabAppearance(productionTab,!upgradesVisible);SetTabAppearance(upgradesTab,upgradesVisible);
        for(int i=0;i<5;i++)
        {
            var kind=(BroteUpgrade)i;int level=up.Level(kind),cost=up.NextCost(kind);string suffix=i==0 ? "%/click" : i==1 ? "%/s" : i==2 ? " pl\u00e1ntulas" : i==3 ? " acciones" : "";
            string value=i==4 ? new[]{"Initial","Expansion01","Expansion02"}[level] : up.Value(kind)+suffix;
            string next=cost<0 ? "" : i==4 ? new[]{"Initial","Expansion01","Expansion02"}[level+1] : BroteUpgradeManager.Values[i][level+1]+suffix;
            levels[i].text=$"Nivel actual: {level}";effects[i].text=cost<0?value:$"{value} \u2192 {next}";costs[i].text=cost<0?"Nivel m\u00e1ximo":$"Coste: {cost} Minka";
            bool affordable=cost>=0 && session.Minka.Minka>=cost;
            buy[i].text=cost<0 ? "M\u00c1XIMO" : affordable ? (i==4?"Desbloquear":"Mejorar") : "Sin Minka";
            buy[i].SetEnabled(affordable && !BrotePauseMenu.IsPaused && Time.timeScale>0);
            Sprite buySprite=skin==null?null:cost<0?skin.buyMax:buy[i].enabledSelf?skin.buyAvailable:skin.buyDisabled;
            NurseryUISkin.Apply(buy[i],buySprite!=null?buySprite:skin!=null?skin.buyButtonBackground:null,new Color(.16f,.3f,.19f));
            buy[i].style.color=buySprite!=null?new Color(.23f,.15f,.09f):Color.white;
            if(!buy[i].enabledSelf)buyHeld[i]=false;
            buy[i].style.unityBackgroundImageTintColor=buyHeld[i]?new Color(.9f,.9f,.9f):Color.white;
        }
    }
    private Sprite PrimarySprite()
    {
        if(skin==null)return null;
        Sprite sprite=!prepare.enabledSelf?skin.primaryDisabled:primaryHeld?skin.primaryPressed:skin.primaryNormal;
        return sprite!=null?sprite:skin.produceButtonBackground;
    }
    private static void FitSprite(VisualElement element)
    {
        element.style.backgroundSize=new BackgroundSize(BackgroundSizeType.Contain);
        element.style.backgroundPositionX=new BackgroundPosition(BackgroundPositionKeyword.Center);
        element.style.backgroundPositionY=new BackgroundPosition(BackgroundPositionKeyword.Center);
        element.style.backgroundRepeat=new BackgroundRepeat(Repeat.NoRepeat,Repeat.NoRepeat);
    }
    private static VisualElement ResourceSlot(string name,Sprite sprite,Label label)
    {
        var row=new VisualElement{name=name+"-row"};row.style.flexDirection=FlexDirection.Row;row.style.maxWidth=Length.Percent(100);row.style.flexShrink=1;row.style.marginRight=12;row.style.alignItems=Align.Center;label.style.flexShrink=1;label.style.minWidth=0;
        var icon=new VisualElement{name=name};icon.style.width=icon.style.height=28;icon.style.flexShrink=0;icon.style.marginRight=4;
        NurseryUISkin.Apply(icon,sprite,Color.clear);FitSprite(icon);row.Add(icon);row.Add(label);return row;
    }
    private void SetTabAppearance(Button tab,bool active)
    {
        tab.EnableInClassList("nursery-tab-active",active);tab.EnableInClassList("nursery-tab-inactive",!active);
        Sprite sprite=null;
        if(skin!=null)
        {
            sprite=tab==productionTab?(active?skin.productionTabActive:skin.productionTabInactive):(active?skin.upgradesTabActive:skin.upgradesTabInactive);
            if(sprite==null)sprite=active?skin.tabActive:skin.tabInactive;
        }
        NurseryUISkin.Apply(tab,sprite,active?new Color(.28f,.53f,.3f):new Color(.16f,.3f,.19f));
        tab.style.color=sprite!=null?new Color(.23f,.15f,.09f):Color.white;
    }
    private void OnDisable() { if(open) Close(); }
    private void OnDestroy() { if(current==this) current=null;if(ownedPanelSettings!=null)Destroy(ownedPanelSettings); }
}
