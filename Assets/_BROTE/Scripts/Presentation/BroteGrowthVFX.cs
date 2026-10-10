using UnityEngine;
public sealed class BroteGrowthVFX : MonoBehaviour
{
    public BroteVFXSettings settings=new BroteVFXSettings();
    public bool Active {get;private set;}
    private ParticleSystem dust,leaves,pulse;
    private Material particleMaterial;
    private int sortingLayerID;
    private readonly Material[] ownedMaterials=new Material[3];
    public void Play(BroteVFXSettings preset,Material material,Sprite dustSprite,Sprite leafSprite,Sprite pulseSprite,Color dustColor,Color leafColor,Color pulseColor,int layer,float crownHeight)
    {
        settings=preset;particleMaterial=material;sortingLayerID=layer;Active=true;
        if(dust==null)dust=CreateSystem("PlantDust",0,dustSprite,dustColor);
        if(leaves==null)leaves=CreateSystem("GrowthLeaves",1,leafSprite,leafColor);
        if(pulse==null)pulse=CreateSystem("RestorationPulse",2,pulseSprite,pulseColor);
        dust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);leaves.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);pulse.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        // Dust and the mature accent are mutually exclusive, using the same cleared system.
        bool sparkOnly = settings.dustCount <= 0 && settings.sparkCount > 0 && settings.sparkSprite != null;
        ConfigureSprites(dust, 0, sparkOnly ? null : settings.dustSprites,
            sparkOnly ? settings.sparkSprite : dustSprite, !sparkOnly);
        ConfigureSprites(leaves, 1, settings.leafSprites, leafSprite, true);
        ConfigureSprites(pulse, 2, null, settings.pulseSprite != null ? settings.pulseSprite : pulseSprite, false);
        var dustMain=dust.main;dustMain.startColor=dustColor;var leafMain=leaves.main;leafMain.startColor=leafColor;var pulseMain=pulse.main;pulseMain.startColor=pulseColor;
        var dustRenderer=dust.GetComponent<ParticleSystemRenderer>();dustRenderer.sortingLayerID=layer;dustRenderer.sortingOrder=sparkOnly?0:2;leaves.GetComponent<ParticleSystemRenderer>().sortingLayerID=layer;pulse.GetComponent<ParticleSystemRenderer>().sortingLayerID=layer;
        dust.Play();leaves.Play();pulse.Play();
        if(settings.dustCount>0)EmitScatter(dust,Mathf.Clamp(settings.dustCount,0,10),.1f,.22f,.3f,.08f);
        if(sparkOnly)EmitSpark(crownHeight);
        EmitScatter(leaves,Mathf.Clamp(settings.leafCount,0,10),settings.leafMinSize,settings.leafMaxSize,Mathf.Clamp(settings.leafLifetime,.2f,.46f),crownHeight);
        if(settings.emitPulse)
        {
            var emission=new ParticleSystem.EmitParams();emission.position=transform.position+Vector3.up*.12f;
            emission.startSize=Mathf.Clamp(settings.pulseSize,.3f,1.2f);emission.startLifetime=Mathf.Clamp(settings.pulseLifetime,.2f,.46f);emission.velocity=Vector3.zero;pulse.Emit(emission,1);
        }
    }
    public void Stop(){Active=false;if(dust!=null)dust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);if(leaves!=null)leaves.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);if(pulse!=null)pulse.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
    private void OnDisable()=>Stop();
    private void OnDestroy(){foreach(var material in ownedMaterials)if(material!=null)Destroy(material);}
    private void ConfigureSprites(ParticleSystem system, int kind, Sprite[] sprites, Sprite fallbackSprite, bool randomVariant)
    {
        var sheet = system.textureSheetAnimation;
        while (sheet.spriteCount > 0) sheet.RemoveSprite(sheet.spriteCount - 1);
        Sprite first = null;
        if (sprites != null)
        {
            foreach (var sprite in sprites)
            {
                if (sprite == null) continue;
                // Sprites mode requires one shared texture; skip incompatible entries safely.
                if (first != null && sprite.texture != first.texture) continue;
                if (first == null) first = sprite;
                sheet.AddSprite(sprite);
            }
        }
        if (first == null)
        {
            first = fallbackSprite != null ? fallbackSprite : QuenualGrowthFeedback.GetProceduralSprite(kind);
            sheet.AddSprite(first);
        }
        sheet.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
        sheet.startFrame = randomVariant ? new ParticleSystem.MinMaxCurve(0f, 1f) : new ParticleSystem.MinMaxCurve(0f);
        sheet.cycleCount = 1;
        ownedMaterials[kind].mainTexture = first.texture;
    }

    private void EmitSpark(float crownHeight)
    {
        for (int i = 0; i < Mathf.Clamp(settings.sparkCount, 0, 2); i++)
        {
            var emission = new ParticleSystem.EmitParams();
            emission.position = transform.position + new Vector3(i == 0 ? -.08f : .08f, crownHeight, 0f);
            emission.velocity = new Vector3(i == 0 ? -.04f : .04f, .12f, 0f);
            emission.startSize = .075f;
            emission.startLifetime = .24f;
            emission.startColor = new Color(1f, 1f, 1f, .28f);
            dust.Emit(emission, 1);
        }
    }

    private void EmitScatter(ParticleSystem system, int count, float minSize, float maxSize,
        float lifetime, float height)
    {
        // Local deterministic variation: does not consume gameplay's Random state.
        float phase = (GetEntityId().GetHashCode() & 255) * 0.13f;
        for (int i = 0; i < count; i++)
        {
            float angle = phase + i * 2.399963f;
            var emission = new ParticleSystem.EmitParams();
            emission.position = transform.position + new Vector3(Mathf.Sin(angle) * 0.2f, height, 0f);
            emission.velocity = new Vector3(Mathf.Sin(angle) * 0.6f, 0.25f + Mathf.Abs(Mathf.Cos(angle)) * 0.5f, 0f) * Mathf.Clamp(settings.velocityMultiplier,.2f,1.2f);
            emission.startLifetime = lifetime;
            emission.startSize = Mathf.Lerp(minSize, maxSize, (i % 3) / 2f);
            emission.rotation = angle * Mathf.Rad2Deg;
            system.Emit(emission, 1);
        }
    }

    private ParticleSystem CreateSystem(string name, int kind, Sprite sprite, Color tint)
    {
        var child = new GameObject(name);
        child.transform.SetParent(transform, false);
        var system = child.AddComponent<ParticleSystem>();
        system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = system.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.48f;
        main.startSpeed = 0f;
        main.startColor = tint;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 12;
        main.gravityModifier = kind == 1 ? 0.08f : 0f;
        main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
        var automaticEmission = system.emission;
        automaticEmission.enabled = false;
        var shape = system.shape;
        shape.enabled = false;
        var color = system.colorOverLifetime;
        color.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.12f), new GradientAlphaKey(0f, 1f) });
        color.color = gradient;
        var size = system.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, kind == 2 ? 0.6f : 1f, 1f, kind == 2 ? 1.5f : 0.7f));
        var rotation = system.rotationOverLifetime;
        rotation.enabled = kind == 1;
        rotation.z = 1.2f;
        if (sprite == null) sprite = QuenualGrowthFeedback.GetProceduralSprite(kind);
        var sheet = system.textureSheetAnimation;
        sheet.enabled = true;
        sheet.mode = ParticleSystemAnimationMode.Sprites;
        sheet.AddSprite(sprite);
        sheet.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
        sheet.cycleCount = 1;
        var renderer = system.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        Material material = particleMaterial != null ? new Material(particleMaterial) : new Material(Shader.Find("Sprites/Default"));
        material.mainTexture = sprite.texture;
        ownedMaterials[kind] = material;
        renderer.sharedMaterial = material;
        renderer.sortingLayerID = sortingLayerID;
        renderer.sortingOrder = kind == 2 ? 0 : 2;
        return system;
    }

}
