using UnityEngine;

// Exactly six milestones; observations are retained if actions happen out of order.
[DisallowMultipleComponent]
public sealed class BroteRestorationObjectives : MonoBehaviour
{
    public static readonly string[] Titles={"Produce tu primera pl\u00e1ntula","Planta tu primer que\u00f1ual","Desarrolla tu primer que\u00f1ual","Prepara un nuevo terreno","Compra tu primera mejora","Desbloquea Expansion01"};
    public static readonly int[] Rewards={1,1,2,1,1,2};
    public int Observed { get; private set; }
    public int Claimed { get; private set; }
    public string CurrentText => Claimed<6 ? $"OBJETIVO {Claimed+1}/6\n{Titles[Claimed]}" : "Primer ciclo completado";
    public void Restore(int observed,int claimed) { Observed=observed; Claimed=claimed; }
    public void Observe(int index)
    {
        if(BroteSaveSystem.IsLoading || index<0 || index>=6) return;
        Observed |= 1<<index;
        while(Claimed<6 && (Observed & (1<<Claimed))!=0) { GetComponent<BroteMinkaManager>().Add(Rewards[Claimed]); Claimed++; }
    }
}
