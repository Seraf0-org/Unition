using UnityEngine;
using UnityEditor;
using System.IO;

namespace Unition.Editor
{
    public class PackageExporter
    {
        [MenuItem("Unition/Build .unitypackage")]
        public static void BuildPackage()
        {
            // パッケージのルートディレクトリ（現在のプロジェクト構成に依存）
            // UnitionはPackages/manifest.jsonで "file:../../Unition" として参照されている想定
            // またはVoidriveプロジェクトの外にある "c:\Users\kouty\Desktop\UnityProjects\Unition"
            
            string sourcePath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Unition"));
            if (!Directory.Exists(sourcePath))
            {
                // Unitionリポジトリ自体を開いている場合
                sourcePath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                
                // それでもない場合はエラー
                if (!File.Exists(Path.Combine(sourcePath, "package.json")))
                {
                    Debug.LogError($"[Unition] Could not find Unition package root at {sourcePath}");
                    return;
                }
            }

            string tempDir = "Assets/Temp_Unition_Build";
            string exportPath = "Unition.unitypackage";

            try
            {
                EditorUtility.DisplayProgressBar("Building Unition Package", "Copying files...", 0.2f);

                // 1. 一時ディレクトリ作成
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                // 2. Unitionのファイルを一時ディレクトリにコピー（Runtime, Editor, package.json等）
                CopyDirectory(Path.Combine(sourcePath, "Runtime"), Path.Combine(tempDir, "Runtime"));
                CopyDirectory(Path.Combine(sourcePath, "Editor"), Path.Combine(tempDir, "Editor"));
                CopyDirectory(Path.Combine(sourcePath, "Samples~"), Path.Combine(tempDir, "Samples")); // Samples~ -> Samples
                File.Copy(Path.Combine(sourcePath, "package.json"), Path.Combine(tempDir, "package.json"));
                File.Copy(Path.Combine(sourcePath, "README.md"), Path.Combine(tempDir, "README.md"));
                File.Copy(Path.Combine(sourcePath, "LICENSE"), Path.Combine(tempDir, "LICENSE"));
                File.Copy(Path.Combine(sourcePath, "CHANGELOG.md"), Path.Combine(tempDir, "CHANGELOG.md"));
                
                // 日本語READMEがあればコピー
                if (File.Exists(Path.Combine(sourcePath, "README_JP.md")))
                    File.Copy(Path.Combine(sourcePath, "README_JP.md"), Path.Combine(tempDir, "README_JP.md"));

                AssetDatabase.Refresh();
                
                EditorUtility.DisplayProgressBar("Building Unition Package", "Exporting...", 0.7f);

                // 3. エクスポート
                AssetDatabase.ExportPackage(
                    tempDir,
                    exportPath,
                    ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies
                );

                Debug.Log($"[Unition] Package built successfully: {Path.GetFullPath(exportPath)}");
                EditorUtility.RevealInFinder(exportPath);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Unition] Build failed: {e.Message}");
            }
            finally
            {
                // 4. 後始末
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                    File.Delete(tempDir + ".meta");
                }
                
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
            }
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            if (!Directory.Exists(sourceDir)) return;
            
            Directory.CreateDirectory(destDir);
            
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                if (file.EndsWith(".meta")) continue; // metaファイルは除外（新規生成させる）
                
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile);
            }
            
            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }
    }
}
