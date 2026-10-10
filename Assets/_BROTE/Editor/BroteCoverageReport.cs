using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

public static class BroteCoverageReport
{
    [MenuItem("BROTE/Tests/Export Functional Coverage")]
    public static void Export()
    {
        var output=new StringBuilder("# Functional test coverage\n\nInventory is source-based, not measured percentage coverage. Presence/compilation does not mean execution. Unity Test Runner XML is execution evidence.\n\n");
        foreach(string file in Directory.GetFiles("Assets/_BROTE/Tests","*.cs",SearchOption.AllDirectories))
        {
            string source=File.ReadAllText(file);output.Append("## ").Append(file).Append("\n\n");
            foreach(Match method in Regex.Matches(source,@"public\s+(?:void|IEnumerator)\s+(\w+)\s*\("))
                if(method.Groups[1].Value!="Cleanup"&&method.Groups[1].Value!="Setup")output.Append("- ").Append(method.Groups[1].Value).Append("\n");
        }
        output.Append("\nGaps/manual: input devices, native collisions/boundary sliding, Game View art/layout, audio perception and player build. Camera/YSort/pause/UI helpers may have logic tests; visual behavior still requires Play.\n");
        const string path="Assets/_BROTE/Documentation/FunctionalTestCoverage.md";Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,output.ToString());AssetDatabase.ImportAsset(path);
    }
}
