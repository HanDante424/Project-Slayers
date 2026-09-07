using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class SlayersCodeSnapshotTool
{
    private const string AssetsRoot = "Assets";
    private const string OutputDirectory = "AI_Context";

    private const string IndexFileName = "00_ProjectIndex.txt";
    private const string ScriptsFileName = "01_Scripts.txt";
    private const string ScenesPrefabsFileName = "02_Scenes_Prefabs.txt";
    private const string GameAssetsFileName = "03_GameAssets.txt";
    private const string ExternalAssetsFileName = "04_ExternalAssets.txt";

    // 외부 에셋 폴더
    private const string ExternalAssetsRoot = "Assets/UnityAsset/";

    // 코드/설정 계열
    private static readonly HashSet<string> ScriptExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".cs",
            ".inputactions",
            ".asmdef",
            ".asmref",
            ".json"
        };

    // 씬/프리팹
    private static readonly HashSet<string> ScenePrefabExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".unity",
            ".prefab"
        };

    // 애니메이션/Unity 설정 계열
    private static readonly HashSet<string> GameAssetExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".controller",
            ".anim",
            ".asset"
        };

    // 바이너리 파일은 내용 대신 경로/크기만 기록
    private static readonly HashSet<string> BinaryExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".psd",
            ".tga",
            ".bmp",
            ".gif",
            ".webp",

            ".wav",
            ".mp3",
            ".ogg",
            ".flac",
            ".aiff",
            ".aif",

            ".fbx",
            ".obj",
            ".blend",

            ".mp4",
            ".mov",
            ".avi",

            ".ttf",
            ".otf"
        };

    [MenuItem("Tools/Slayers/Export Project Snapshot")]
    public static void ExportProjectSnapshot()
    {
        string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
        string assetsFullPath = Path.Combine(projectRoot, AssetsRoot);
        string outputDirectoryFullPath = Path.Combine(projectRoot, OutputDirectory);

        if (!Directory.Exists(assetsFullPath))
        {
            EditorUtility.DisplayDialog(
                "Export Failed",
                $"Assets folder not found:\n{assetsFullPath}",
                "OK"
            );

            return;
        }

        Directory.CreateDirectory(outputDirectoryFullPath);

        string[] allFiles = Directory.GetFiles(
            assetsFullPath,
            "*",
            SearchOption.AllDirectories
        );

        Array.Sort(allFiles, StringComparer.OrdinalIgnoreCase);

        string snapshotId = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        string generatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        try
        {
            ExportProjectIndex(
                projectRoot,
                outputDirectoryFullPath,
                allFiles,
                snapshotId,
                generatedAt
            );

            ExportScripts(
                projectRoot,
                outputDirectoryFullPath,
                allFiles,
                snapshotId,
                generatedAt
            );

            ExportScenesAndPrefabs(
                projectRoot,
                outputDirectoryFullPath,
                allFiles,
                snapshotId,
                generatedAt
            );

            ExportGameAssets(
                projectRoot,
                outputDirectoryFullPath,
                allFiles,
                snapshotId,
                generatedAt
            );

            ExportExternalAssets(
                projectRoot,
                outputDirectoryFullPath,
                allFiles,
                snapshotId,
                generatedAt
            );

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Snapshot Export Complete",
                "Snapshot files created:\n\n" +
                $"{OutputDirectory}/{IndexFileName}\n" +
                $"{OutputDirectory}/{ScriptsFileName}\n" +
                $"{OutputDirectory}/{ScenesPrefabsFileName}\n" +
                $"{OutputDirectory}/{GameAssetsFileName}\n" +
                $"{OutputDirectory}/{ExternalAssetsFileName}",
                "OK"
            );

            Debug.Log(
                $"[Slayers Snapshot] Export complete. Snapshot ID: {snapshotId}"
            );
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);

            EditorUtility.DisplayDialog(
                "Export Failed",
                ex.Message,
                "OK"
            );
        }
    }

    // =========================================================
    // 00 - Project Index
    // =========================================================

    private static void ExportProjectIndex(
        string projectRoot,
        string outputDirectory,
        string[] allFiles,
        string snapshotId,
        string generatedAt)
    {
        StringBuilder builder = new StringBuilder(512 * 1024);

        WriteHeader(
            builder,
            snapshotId,
            generatedAt,
            "00_ProjectIndex"
        );

        builder.AppendLine("==================================================");
        builder.AppendLine("PROJECT SUMMARY");
        builder.AppendLine("==================================================");
        builder.AppendLine();

        builder.AppendLine($"Files Scanned : {allFiles.Length}");
        builder.AppendLine();

        WriteExtensionSummary(builder, allFiles);

        builder.AppendLine();
        builder.AppendLine("==================================================");
        builder.AppendLine("ASSET TREE");
        builder.AppendLine("==================================================");
        builder.AppendLine();

        foreach (string fullPath in allFiles)
        {
            string relativePath = GetRelativePath(projectRoot, fullPath);
            builder.AppendLine(relativePath);
        }

        WriteFile(
            Path.Combine(outputDirectory, IndexFileName),
            builder
        );
    }

    // =========================================================
    // 01 - Scripts
    // =========================================================

    private static void ExportScripts(
        string projectRoot,
        string outputDirectory,
        string[] allFiles,
        string snapshotId,
        string generatedAt)
    {
        StringBuilder builder = new StringBuilder(1024 * 1024);

        WriteHeader(
            builder,
            snapshotId,
            generatedAt,
            "01_Scripts"
        );

        string[] files = allFiles
            .Where(path =>
            {
                string relativePath = GetRelativePath(projectRoot, path);
                string extension = Path.GetExtension(path);

                if (IsExternalAsset(relativePath))
                    return false;

                // Assets/Scripts 아래는 모든 등록된 코드 확장자 포함
                if (relativePath.StartsWith(
                    "Assets/Scripts/",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return ScriptExtensions.Contains(extension);
                }

                // Input Actions는 Scripts 밖에 있어도 포함
                if (extension.Equals(
                    ".inputactions",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            })
            .ToArray();

        WriteFileSectionHeader(builder, files.Length);

        foreach (string file in files)
        {
            AppendTextFile(builder, projectRoot, file);
        }

        WriteFile(
            Path.Combine(outputDirectory, ScriptsFileName),
            builder
        );
    }

    // =========================================================
    // 02 - Scenes / Prefabs
    // =========================================================

    private static void ExportScenesAndPrefabs(
        string projectRoot,
        string outputDirectory,
        string[] allFiles,
        string snapshotId,
        string generatedAt)
    {
        StringBuilder builder = new StringBuilder(1024 * 1024);

        WriteHeader(
            builder,
            snapshotId,
            generatedAt,
            "02_Scenes_Prefabs"
        );

        string[] files = allFiles
            .Where(path =>
            {
                string relativePath = GetRelativePath(projectRoot, path);
                string extension = Path.GetExtension(path);

                return !IsExternalAsset(relativePath) &&
                       ScenePrefabExtensions.Contains(extension);
            })
            .ToArray();

        WriteFileSectionHeader(builder, files.Length);

        foreach (string file in files)
        {
            AppendTextFile(builder, projectRoot, file);
        }

        WriteFile(
            Path.Combine(outputDirectory, ScenesPrefabsFileName),
            builder
        );
    }

    // =========================================================
    // 03 - Game Assets
    // =========================================================

    private static void ExportGameAssets(
        string projectRoot,
        string outputDirectory,
        string[] allFiles,
        string snapshotId,
        string generatedAt)
    {
        StringBuilder builder = new StringBuilder(1024 * 1024);

        WriteHeader(
            builder,
            snapshotId,
            generatedAt,
            "03_GameAssets"
        );

        string[] textFiles = allFiles
            .Where(path =>
            {
                string relativePath = GetRelativePath(projectRoot, path);
                string extension = Path.GetExtension(path);

                return !IsExternalAsset(relativePath) &&
                       GameAssetExtensions.Contains(extension);
            })
            .ToArray();

        builder.AppendLine("==================================================");
        builder.AppendLine("TEXT GAME ASSETS");
        builder.AppendLine("==================================================");
        builder.AppendLine();
        builder.AppendLine($"Files: {textFiles.Length}");
        builder.AppendLine();

        foreach (string file in textFiles)
        {
            AppendTextFile(builder, projectRoot, file);
        }

        string[] binaryFiles = allFiles
            .Where(path =>
            {
                string relativePath = GetRelativePath(projectRoot, path);
                string extension = Path.GetExtension(path);

                return !IsExternalAsset(relativePath) &&
                       BinaryExtensions.Contains(extension);
            })
            .ToArray();

        builder.AppendLine();
        builder.AppendLine("==================================================");
        builder.AppendLine("BINARY GAME ASSETS");
        builder.AppendLine("==================================================");
        builder.AppendLine();
        builder.AppendLine(
            "Binary file contents are not included. " +
            "Only path and size are recorded."
        );
        builder.AppendLine();

        foreach (string file in binaryFiles)
        {
            FileInfo info = new FileInfo(file);

            builder.AppendLine(
                $"{GetRelativePath(projectRoot, file)} | {FormatFileSize(info.Length)}"
            );
        }

        WriteFile(
            Path.Combine(outputDirectory, GameAssetsFileName),
            builder
        );
    }

    // =========================================================
    // 04 - External Assets
    // =========================================================

    private static void ExportExternalAssets(
        string projectRoot,
        string outputDirectory,
        string[] allFiles,
        string snapshotId,
        string generatedAt)
    {
        StringBuilder builder = new StringBuilder(512 * 1024);

        WriteHeader(
            builder,
            snapshotId,
            generatedAt,
            "04_ExternalAssets"
        );

        builder.AppendLine(
            "External package contents are intentionally NOT embedded."
        );
        builder.AppendLine(
            "Only file paths and file sizes are recorded."
        );
        builder.AppendLine();

        string[] files = allFiles
            .Where(path =>
            {
                string relativePath = GetRelativePath(projectRoot, path);
                return IsExternalAsset(relativePath);
            })
            .ToArray();

        builder.AppendLine($"External Files: {files.Length}");
        builder.AppendLine();

        foreach (string file in files)
        {
            FileInfo info = new FileInfo(file);

            builder.AppendLine(
                $"{GetRelativePath(projectRoot, file)} | {FormatFileSize(info.Length)}"
            );
        }

        WriteFile(
            Path.Combine(outputDirectory, ExternalAssetsFileName),
            builder
        );
    }

    // =========================================================
    // Shared
    // =========================================================

    private static void WriteHeader(
        StringBuilder builder,
        string snapshotId,
        string generatedAt,
        string partName)
    {
        builder.AppendLine("==================================================");
        builder.AppendLine("SLAYERS - UNITY PROJECT SNAPSHOT");
        builder.AppendLine("==================================================");
        builder.AppendLine();

        builder.AppendLine($"Snapshot ID   : {snapshotId}");
        builder.AppendLine($"Generated At  : {generatedAt}");
        builder.AppendLine($"Unity Version : {Application.unityVersion}");
        builder.AppendLine($"Project Name  : {Application.productName}");
        builder.AppendLine($"Part          : {partName}");
        builder.AppendLine();
    }

    private static void WriteFileSectionHeader(
        StringBuilder builder,
        int fileCount)
    {
        builder.AppendLine("==================================================");
        builder.AppendLine("FILES");
        builder.AppendLine("==================================================");
        builder.AppendLine();

        builder.AppendLine($"Files Included: {fileCount}");
        builder.AppendLine();
    }

    private static void WriteExtensionSummary(
        StringBuilder builder,
        string[] allFiles)
    {
        builder.AppendLine("==================================================");
        builder.AppendLine("EXTENSION SUMMARY");
        builder.AppendLine("==================================================");
        builder.AppendLine();

        var groups = allFiles
            .GroupBy(path =>
            {
                string extension = Path.GetExtension(path);

                return string.IsNullOrEmpty(extension)
                    ? "(no extension)"
                    : extension.ToLowerInvariant();
            })
            .OrderBy(group => group.Key);

        foreach (var group in groups)
        {
            builder.AppendLine($"{group.Key} : {group.Count()}");
        }
    }

    private static void AppendTextFile(
        StringBuilder builder,
        string projectRoot,
        string fullPath)
    {
        string relativePath = GetRelativePath(projectRoot, fullPath);

        builder.AppendLine();
        builder.AppendLine(
            "--------------------------------------------------"
        );
        builder.AppendLine($"FILE: {relativePath}");
        builder.AppendLine(
            "--------------------------------------------------"
        );

        try
        {
            FileInfo info = new FileInfo(fullPath);

            builder.AppendLine($"Size: {FormatFileSize(info.Length)}");
            builder.AppendLine(
                $"Last Modified: {info.LastWriteTime:yyyy-MM-dd HH:mm:ss}"
            );
            builder.AppendLine();

            string content = File.ReadAllText(
                fullPath,
                Encoding.UTF8
            );

            builder.AppendLine(content);
        }
        catch (Exception ex)
        {
            builder.AppendLine(
                $"[READ ERROR] {ex.GetType().Name}: {ex.Message}"
            );
        }

        builder.AppendLine();
    }

    private static bool IsExternalAsset(string relativePath)
    {
        return relativePath.StartsWith(
            ExternalAssetsRoot,
            StringComparison.OrdinalIgnoreCase
        );
    }

    private static string GetRelativePath(
        string projectRoot,
        string fullPath)
    {
        string relativePath = Path.GetRelativePath(
            projectRoot,
            fullPath
        );

        return relativePath.Replace('\\', '/');
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes >= 1024L * 1024L)
        {
            return $"{bytes / (1024d * 1024d):0.00} MB";
        }

        if (bytes >= 1024L)
        {
            return $"{bytes / 1024d:0.00} KB";
        }

        return $"{bytes} B";
    }

    private static void WriteFile(
        string path,
        StringBuilder builder)
    {
        File.WriteAllText(
            path,
            builder.ToString(),
            new UTF8Encoding(false)
        );
    }
}