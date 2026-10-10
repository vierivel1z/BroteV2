using System;

/// <summary>Presentation-only aggregation; never changes restoration.</summary>
public sealed class BroteRestorationFeedbackState
{
    private static readonly int[] marks={20,40,50,100};
    private int seen;
    private float previous,pending,readyAt;
    public void Sync(float value)
    {
        previous=value;pending=0f;
        for(int i=0;i<marks.Length;i++)if(value>=marks[i])seen|=1<<i;
    }
    public int Observe(float value,float now)
    {
        if(float.IsNaN(value)||float.IsInfinity(value))return 0;
        float increase=Math.Max(0f,value-previous);
        if(increase>0f){if(pending==0f)readyAt=now+.15f;pending+=increase;}
        int highest=0;
        for(int i=0;i<marks.Length;i++)if(value>=marks[i]&&(seen&(1<<i))==0){seen|=1<<i;highest=marks[i];}
        previous=value;return highest;
    }
    public bool TryTake(float now,out float amount)
    {
        amount=0f;if(pending==0f||now<readyAt)return false;
        amount=pending;pending=0f;return true;
    }
    public static string MilestoneText(int mark)=>mark==20?"Primeros signos de recuperaci\u00f3n":mark==40?"La vegetaci\u00f3n se fortalece":mark==50?"El valle recupera vida":mark==100?"Sector restaurado":"";
}
