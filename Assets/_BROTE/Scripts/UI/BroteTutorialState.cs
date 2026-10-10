using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class BroteTutorialState : MonoBehaviour
{
    public bool Completed { get; private set; }
    private PlayerInteraction player;
    private SeedlingInventory inventory;
    private Label guide;
    private bool ownsGuide;
    private float finishUntil;
    private bool sawPlant;
    public void RestoreCompleted(bool completed)
    {
        Completed = completed;
        sawPlant = FindObjectsByType<PlantingSpot>().Any(x => x.gameObject.scene == gameObject.scene && x.State == PlantingSpot.SpotState.PLANTED);
    }
    private void Start()
    {
        player = FindObjectsByType<PlayerInteraction>().FirstOrDefault(x => x.gameObject.scene == gameObject.scene);
        inventory = player != null ? player.GetComponent<SeedlingInventory>() : null;
        var hud = FindObjectsByType<BroteHUD>().FirstOrDefault(x => x.gameObject.scene == gameObject.scene);
        if (hud == null || player == null) { enabled = false; return; }
        if(GetComponent<BroteRestorationObjectives>()!=null)guide=hud.ObjectiveDescription;
        else
        {
            ownsGuide=true;
            guide = new Label(); guide.pickingMode = PickingMode.Ignore;
            guide.style.position = Position.Absolute; guide.style.top = 18; guide.style.left = Length.Percent(25); guide.style.right = Length.Percent(25);
            guide.style.whiteSpace = WhiteSpace.Normal; guide.style.unityTextAlign = TextAnchor.MiddleCenter;
            guide.style.fontSize = 18; guide.style.color = Color.white; guide.style.backgroundColor = new Color(.06f,.12f,.09f,.9f);
            guide.style.paddingTop = guide.style.paddingBottom = 8;
            hud.GetComponent<UIDocument>().rootVisualElement.Add(guide);
        }
        PlantingSpot.Planted += OnPlanted; QuenualGrowth.Matured += OnMatured;
        sawPlant = FindObjectsByType<PlantingSpot>().Any(x => x.gameObject.scene == gameObject.scene && x.State == PlantingSpot.SpotState.PLANTED);
    }
    private void OnPlanted(PlantingSpot spot) { if (spot.gameObject.scene == gameObject.scene) sawPlant = true; }
    private void OnMatured(QuenualGrowth tree)
    {
        if (Completed || BroteSaveSystem.IsLoading || tree.gameObject.scene != gameObject.scene) return;
        Completed = true; finishUntil = Time.unscaledTime + 5f;
        GetComponent<BroteSaveSystem>()?.RequestSave();
    }
    private void Update()
    {
        if (guide == null) return;
        var objectives=GetComponent<BroteRestorationObjectives>();
        if(objectives!=null)return; // BroteHUD owns the permanent objective presentation.
        guide.style.display = BrotePauseMenu.IsPaused || (Completed && Time.unscaledTime >= finishUntil) ? DisplayStyle.None : DisplayStyle.Flex;
        guide.text = Completed ? "Cada queñual ayuda a recuperar el valle" : sawPlant ? "Observa cómo crece el queñual" : inventory != null && inventory.CurrentSeedlings > 0 ?
            player.CurrentInteractable is PlantingSpot spot && player.IsWithinRange(spot) ? "Presiona E para plantar" : "Busca una zona de plantación" : "Recoge una plántula en el vivero";
    }
    private void OnDestroy() { PlantingSpot.Planted -= OnPlanted; QuenualGrowth.Matured -= OnMatured; if(ownsGuide)guide?.RemoveFromHierarchy(); }
}
