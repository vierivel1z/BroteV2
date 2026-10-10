using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class BroteOfficialSFXImportTests
{
    [Test] public void FiveOfficialClipsResolveWithShortStereoPCMPreloadedSettings()
    {
        var config=Resources.Load<ScriptableObject>("Audio/BroteSFXClips");Assert.IsNotNull(config);
        string[] names={"Plant","SeedlingReady","Growth","Upgrade","SectorRestored"};
        string[] fields={"plant","seedlingReady","growth","upgrade","sectorRestored"};
        for(int i=0;i<names.Length;i++)
        {
            string path="Assets/_BROTE/Audio/SFX/SFX_"+names[i]+".wav";
            var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);Assert.IsNotNull(clip);
            Assert.AreSame(clip,config.GetType().GetField(fields[i]).GetValue(config));
            Assert.AreEqual(2,clip.channels);Assert.AreEqual(48000,clip.frequency);
            var importer=(AudioImporter)AssetImporter.GetAtPath(path);var settings=importer.defaultSampleSettings;
            Assert.AreEqual(AudioClipLoadType.DecompressOnLoad,settings.loadType);
            Assert.AreEqual(AudioCompressionFormat.PCM,settings.compressionFormat);
            Assert.AreEqual(AudioSampleRateSetting.PreserveSampleRate,settings.sampleRateSetting);
            Assert.IsTrue(settings.preloadAudioData);Assert.IsFalse(importer.loadInBackground);Assert.IsFalse(importer.forceToMono);
        }
        Assert.AreEqual(AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_BROTE/Audio/SFX/SFX_Growth.wav"),
            config.GetType().GetField("growth").GetValue(config));
    }
}
