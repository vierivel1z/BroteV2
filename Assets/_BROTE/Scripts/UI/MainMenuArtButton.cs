using System;
using UnityEngine;
using UnityEngine.UIElements;
// Keep UI Toolkit's click, keyboard navigation and disabled behavior.
public sealed class MainMenuArtButton : Button
{
    private readonly MainMenuUISkin skin;
    private readonly bool small;
    private bool hovered, pressed, focused;
    public Sprite DisplayedSprite { get; private set; }
    public MainMenuArtButton(string label, Action action, MainMenuUISkin art, bool compact=false) : base(action)
    {
        text=label;skin=art;small=compact;
        style.backgroundColor=Color.clear;
        style.borderLeftWidth=style.borderRightWidth=style.borderTopWidth=style.borderBottomWidth=0;
        style.paddingLeft=style.paddingRight=24;
        style.color=new Color(.19f,.12f,.06f);
        style.unityFontStyleAndWeight=FontStyle.Bold;style.unityTextAlign=TextAnchor.MiddleCenter;
        style.backgroundSize=new BackgroundSize(BackgroundSizeType.Contain);
        style.backgroundPositionX=new BackgroundPosition(BackgroundPositionKeyword.Center);
        style.backgroundPositionY=new BackgroundPosition(BackgroundPositionKeyword.Center);
        RegisterCallback<PointerEnterEvent>(_=>{hovered=true;RefreshState();});
        RegisterCallback<PointerLeaveEvent>(_=>{hovered=pressed=focused=false;RefreshState();});
        // Observe before Clickable consumes pointer events at the target.
        RegisterCallback<PointerDownEvent>(e=>{if(e.button==0){pressed=true;RefreshState();}},TrickleDown.TrickleDown);
        RegisterCallback<PointerUpEvent>(_=>{pressed=false;RefreshState();},TrickleDown.TrickleDown);
        RegisterCallback<PointerCaptureOutEvent>(_=>{pressed=false;RefreshState();});
        RegisterCallback<FocusEvent>(_=>{focused=true;RefreshState();});
        RegisterCallback<BlurEvent>(_=>{focused=pressed=false;RefreshState();});
        RefreshState();
    }
    public void RefreshState()
    {
        if(skin==null)return;
        DisplayedSprite=!enabledInHierarchy?skin.buttonDisabled:small?skin.buttonSmall:
            pressed?skin.buttonPressed:hovered||focused?skin.buttonHover:skin.buttonNormal;
        style.backgroundImage=new StyleBackground(DisplayedSprite);
        style.unityBackgroundImageTintColor=small&&pressed?new Color(.86f,.86f,.86f):Color.white;
    }
}

