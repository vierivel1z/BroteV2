using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Explicit editor action only; never called during scene startup or included in a player.
[InitializeOnLoad]
public static class BroteLocalSaveDebug
{
    private const string ChoiceKey="BROTE.EditorPlayChoice",PathKey="BROTE.EditorPlaySavePath";
    private static string PreferenceKey=>"BROTE.DirectAcopalcaClean."+Application.dataPath;
    static BroteLocalSaveDebug(){EditorApplication.playModeStateChanged-=OnPlayMode;EditorApplication.playModeStateChanged+=OnPlayMode;}
    [MenuItem("BROTE/Debug/Direct Acopalca Play = Clean Test")]
    private static void ToggleDirectClean()=>EditorPrefs.SetBool(PreferenceKey,!EditorPrefs.GetBool(PreferenceKey,true));
    [MenuItem("BROTE/Debug/Direct Acopalca Play = Clean Test",true)]
    private static bool ValidateDirectClean(){Menu.SetChecked("BROTE/Debug/Direct Acopalca Play = Clean Test",EditorPrefs.GetBool(PreferenceKey,true));return !EditorApplication.isPlayingOrWillChangePlaymode;}
    [MenuItem("BROTE/Debug/Start Clean Play")]
    public static void StartCleanPlay()=>StartPlay(1);
    [MenuItem("BROTE/Debug/Play With Current Save")]
    public static void PlayWithCurrentSave()=>StartPlay(2);
    [MenuItem("BROTE/Debug/Play From Main Menu")]
    public static void PlayFromMainMenu()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        EditorSceneManager.OpenScene("Assets/_BROTE/Scenes/UI/MainMenu.unity",OpenSceneMode.Single);
        SessionState.SetInt(ChoiceKey,2);EditorApplication.isPlaying=true;
    }
    private static void StartPlay(int choice)
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        EditorSceneManager.OpenScene("Assets/_BROTE/Scenes/Acopalca/Acopalca.unity",OpenSceneMode.Single);
        SessionState.SetInt(ChoiceKey,choice);EditorApplication.isPlaying=true;
    }
    private static void OnPlayMode(PlayModeStateChange state)
    {
        if(Application.isBatchMode && !SessionState.GetBool("BROTE.Debug.ValidateCleanPlay",false))return; // Native tests retain their own isolated save paths.
        if(state==PlayModeStateChange.ExitingEditMode)
        {
            int choice=SessionState.GetInt(ChoiceKey,0);SessionState.SetInt(ChoiceKey,0);
            bool clean=EditorSceneManager.GetActiveScene().name=="Acopalca" && (choice==1 || choice==0 && EditorPrefs.GetBool(PreferenceKey,true));
            string path=clean?Path.GetFullPath(Path.Combine(Application.dataPath,"../.brote-work/editor-clean-saves/"+Guid.NewGuid().ToString("N")+".json")):"";
            SessionState.SetString(PathKey,path);BroteSaveSystem.ConfigureEditorSavePath(path);
        }
        // Clear only after all scene teardown/quit saves have completed.
        if(state==PlayModeStateChange.EnteredEditMode){SessionState.EraseString(PathKey);BroteSaveSystem.ConfigureEditorSavePath(null);}
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ConfigurePlaySaveAfterReload()
    {
        if(Application.isBatchMode && !SessionState.GetBool("BROTE.Debug.ValidateCleanPlay",false))return;
        string path=SessionState.GetString(PathKey,"");BroteSaveSystem.ConfigureEditorSavePath(path);
        if(!string.IsNullOrEmpty(path))Debug.Log("BROTE: Clean Test. Save aislado: "+path+". El save local real no se carga ni modifica.");
    }

    [MenuItem("BROTE/Debug/Clear Local Save")]
    public static void ClearLocalSave()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorUtility.DisplayDialog("BROTE", "Sal de Play antes de limpiar el save local.", "Aceptar"); return; }
        string path=BroteSaveSystem.SavePath;
        if(!File.Exists(path)){EditorUtility.DisplayDialog("BROTE", "No existe un save local.", "Aceptar");return;}
        if(!EditorUtility.DisplayDialog("Limpiar save local", "Se conservará una copia en .brote-work/local-save-backups. El próximo Play directo en Acopalca comenzará limpio.\n\n"+path, "Crear copia y limpiar", "Cancelar"))return;
        try
        {
            string directory=Path.GetFullPath(Path.Combine(Application.dataPath,"../.brote-work/local-save-backups"));
            Directory.CreateDirectory(directory);
            File.Copy(path,Path.Combine(directory,"brote-sector01-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff")+".json"));
            if(!BroteSaveSystem.DeleteConfirmedSave())throw new IOException("No se pudo borrar el guardado.");
            Debug.Log("BROTE: save local respaldado y limpiado explícitamente. Nueva partida lista para Play.");
        }
        catch(Exception e){EditorUtility.DisplayDialog("BROTE: no se pudo limpiar",e.Message,"Aceptar");}
    }
    [MenuItem("BROTE/Debug/Clear Local Save",true)]
    private static bool CanClear()=>!EditorApplication.isPlayingOrWillChangePlaymode;

    public static void AuditInitialState()
    {
        var scene=EditorSceneManager.OpenScene("Assets/_BROTE/Scenes/Acopalca/Acopalca.unity",OpenSceneMode.Additive);
        try
        {
            var roots=scene.GetRootGameObjects();var spots=roots.SelectMany(x=>x.GetComponentsInChildren<PlantingSpot>(true)).ToArray();
            int trees=roots.Sum(x=>x.GetComponentsInChildren<QuenualGrowth>(true).Length);
            string report="Authored spots="+spots.Length+"; authored QuenualGrowth="+trees+"; IDs="+string.Join(",",spots.Select(x=>x.StableId).OrderBy(x=>x));
            string output=Environment.GetCommandLineArgs().SkipWhile(x=>x!="-initialStateReport").Skip(1).First();
            File.WriteAllText(output,report);Debug.Log(report);
            if(spots.Length!=10 || trees!=0)throw new InvalidOperationException("Unexpected authored initial state");
        }
        finally{EditorSceneManager.CloseScene(scene,true);}
    }
}
