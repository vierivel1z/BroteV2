using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BroteAmbienceIntegration
{
    [MenuItem("BROTE/Audio/Connect Official Ambience")]
    public static void Connect()
    {
        const string target="Assets/_BROTE/Resources/Audio/BroteAmbienceClips.asset";
        var config=AssetDatabase.LoadAssetAtPath<BroteAmbienceClips>(target);
        if(config==null){config=ScriptableObject.CreateInstance<BroteAmbienceClips>();AssetDatabase.CreateAsset(config,target);}
        var report=new System.Text.StringBuilder();
        string[] names={"Wind","Birds","River","GrassRustle"},fields={"wind","birds","river","leaves"};
        for(int i=0;i<names.Length;i++)
        {
            string path="Assets/_BROTE/Audio/Ambient/Ambience_"+names[i]+".wav";
            if(!File.Exists(path))path="Assets/_BROTE/Audio/Ambience/Ambience_"+names[i]+".wav";
            var importer=AssetImporter.GetAtPath(path) as AudioImporter;
            if(importer==null){typeof(BroteAmbienceClips).GetField(fields[i]).SetValue(config,null);report.AppendLine("MISSING "+names[i]);continue;}
            var settings=importer.defaultSampleSettings;
            settings.loadType=AudioClipLoadType.Streaming;settings.compressionFormat=AudioCompressionFormat.Vorbis;
            settings.quality=.7f;settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;settings.preloadAudioData=false;
            importer.defaultSampleSettings=settings;importer.forceToMono=false;importer.loadInBackground=true;importer.SaveAndReimport();
            var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);if(clip==null)throw new InvalidOperationException(path);
            typeof(BroteAmbienceClips).GetField(fields[i]).SetValue(config,clip);
            report.AppendLine(path+" -> "+fields[i]+" channels="+clip.channels+" seconds="+clip.length);
        }
        EditorUtility.SetDirty(config);AssetDatabase.SaveAssets();Debug.Log(report);
        var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-ambienceReport");
        if(index>=0)File.WriteAllText(args[index+1],report.ToString());
    }
}
