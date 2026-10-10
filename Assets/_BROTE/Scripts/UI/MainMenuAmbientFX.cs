using UnityEngine;
using UnityEngine.UIElements;
// Fixed elements owned by this menu; no recurring Instantiate/Destroy.
public sealed class MainMenuAmbientFX
{
    private sealed class Mote
    {
        public VisualElement visual;
        public float phase,duration,delay,lane,size,angle;
        public bool wind;
    }
    private readonly Mote[] motes;
    private readonly System.Random random=new System.Random(804);
    public int ElementCount=>motes.Length;
    public MainMenuAmbientFX(VisualElement layer,MainMenuUISkin skin)
    {
        var list=new System.Collections.Generic.List<Mote>();
        if(skin!=null)
        {
            Add(list,layer,skin.leaves,5,false,false);
            Add(list,layer,skin.clusters,1,false,true);
            Add(list,layer,skin.winds,2,true,false);
        }
        motes=list.ToArray();
    }
    private float Range(float min,float max)=>min+(float)random.NextDouble()*(max-min);
    private void Add(System.Collections.Generic.List<Mote> list,VisualElement layer,Sprite[] sprites,int count,bool wind,bool cluster)
    {
        if(sprites==null||sprites.Length==0)return;
        for(int i=0;i<count;i++)
        {
            var sprite=sprites[i%sprites.Length];if(sprite==null)continue;
            var visual=new VisualElement{name=wind?"MenuWind_"+i:cluster?"MenuCluster_"+i:"MenuLeaf_"+i,pickingMode=PickingMode.Ignore};
            visual.style.position=Position.Absolute;visual.style.backgroundImage=new StyleBackground(sprite);
            visual.style.backgroundSize=new BackgroundSize(BackgroundSizeType.Contain);layer.Add(visual);
            list.Add(new Mote{visual=visual,wind=wind,phase=Range(0,1),duration=wind?Range(22,36):Range(35,56),
                lane=Range(.12f,.72f),size=wind?Range(130,190):cluster?36:Range(17,27),angle=Range(-25,25)});
        }
    }
    public void Tick(float delta,float width,float height)
    {
        if(width<=0||height<=0)return;
        foreach(var mote in motes)
        {
            if(mote.delay>0){mote.delay-=delta;mote.visual.style.opacity=0;continue;}
            mote.phase+=delta/mote.duration;
            if(mote.phase>1){mote.phase=0;mote.delay=Range(3,12);mote.lane=Range(.12f,.72f);}
            // Behind UI and restricted to the left half, away from logo/buttons.
            mote.visual.style.left=Mathf.Lerp(-mote.size,width*.51f+mote.size,mote.phase);
            mote.visual.style.top=height*mote.lane+height*.1f*mote.phase;
            mote.visual.style.width=mote.size;mote.visual.style.height=mote.size;
            mote.visual.style.rotate=new Rotate(new Angle(mote.angle+(mote.wind?0:Mathf.Sin(mote.phase*6)*18),AngleUnit.Degree));
            mote.visual.style.opacity=Mathf.Sin(Mathf.PI*mote.phase)*(mote.wind?.09f:.32f);
        }
    }
}

