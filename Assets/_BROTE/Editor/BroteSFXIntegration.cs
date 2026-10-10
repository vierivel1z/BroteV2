using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BroteSFXIntegration
{
    [MenuItem("BROTE/Audio/Connect Official SFX")]
    public static void Connect()
    {
        string[] names={"Plant","SeedlingReady","Growth","Upgrade","SectorRestored"};
        string[] fields={"plant","seedlingReady","growth","upgrade","sectorRestored"};
        const string asset="Assets/_BROTE/Resources/Audio/BroteSFXClips.asset";
        var config=AssetDatabase.LoadAssetAtPath<BroteSFXClips>(asset);
        if(config==null){config=ScriptableObject.CreateInstance<BroteSFXClips>();AssetDatabase.CreateAsset(config,asset);}
        var report=new System.Text.StringBuilder();
        for(int i=0;i<names.Length;i++)
        {
            string path="Assets/_BROTE/Audio/SFX/SFX_"+names[i]+".wav";
            var importer=AssetImporter.GetAtPath(path) as AudioImporter;
            if(importer==null)
            {
                typeof(BroteSFXClips).GetField(fields[i]).SetValue(config,null);
                report.AppendLine("MISSING "+path+"; hook left silent.");continue;
            }
            var settings=importer.defaultSampleSettings;
            settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;
            settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;settings.preloadAudioData=true;
            importer.defaultSampleSettings=settings;importer.loadInBackground=false;importer.forceToMono=false;
            importer.SaveAndReimport();
            var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            typeof(BroteSFXClips).GetField(fields[i]).SetValue(config,clip);
            if(clip==null)throw new InvalidOperationException("AudioClip failed to import: "+path);
            report.AppendLine(path+" -> "+fields[i]+" channels="+clip.channels+" frequency="+clip.frequency+" seconds="+clip.length);
        }
        EditorUtility.SetDirty(config);AssetDatabase.SaveAssets();Debug.Log(report.ToString());
        var args=Environment.GetCommandLineArgs();int arg=Array.IndexOf(args,"-sfxReport");
        if(arg>=0 && arg+1<args.Length)File.WriteAllText(args[arg+1],report.ToString());
    }
}
