#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Callbacks;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public partial class AssetManager
{
    public const string _c_prefix_AddressableFolder = "AB_";

    private static bool _CheckAssetBundleTargetDir(string path_File)
    {
        return path_File.StartsWith("Assets/01_Resources") 
               && path_File.Contains(_c_prefix_AddressableFolder)
               && AssetDatabase.IsValidFolder(path_File);
    }

    public static string FindAddressableGroupNameInPath(string path)
    {
        /* "AB_" 키워드를 소유한 폴더명 찾기 */
        var nIdx_Start = path.IndexOf(_c_prefix_AddressableFolder);
        if (nIdx_Start < 0)
            return null;

        var nIdx_Last = path.IndexOf('/', nIdx_Start);
        if (nIdx_Last <= nIdx_Start)
            nIdx_Last = path.Length;

        /* GroupName, AddrKey 확정 */
        return path.Substring(nIdx_Start, nIdx_Last - nIdx_Start);
    }
    
    private static void _RegistAddress(string path, AddressableAssetSettings settings)
    {
        /* GroupName, AddrKey 확정 */
        var groupName = FindAddressableGroupNameInPath(path);
        if (groupName == null)
            return;
        
        var groupKey = groupName.Split("_")[1];

        /* Get Or Create Group */
        var group = settings.FindGroup(groupName);
        if (group == null)
        {
            settings.AddLabel(groupKey);

            group = settings.CreateGroup(groupName, 
                false, 
                true, 
                true, 
                null, 
                typeof(ContentUpdateGroupSchema), 
                typeof(BundledAssetGroupSchema));

            var lBuildPath = settings.profileSettings.GetProfileDataByName("Remote.BuildPath");
            var lLoadPath = settings.profileSettings.GetProfileDataByName("Remote.LoadPath");

            var lBundledAssetGroupSchema = group.GetSchema<BundledAssetGroupSchema>();
            lBundledAssetGroupSchema.Timeout = 60;
            lBundledAssetGroupSchema.RetryCount = 3;
            lBundledAssetGroupSchema.BuildPath.SetVariableById(settings, lBuildPath.Id);
            lBundledAssetGroupSchema.LoadPath.SetVariableById(settings, lLoadPath.Id);
            lBundledAssetGroupSchema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
        }
        var lGuid = AssetDatabase.AssetPathToGUID(path);
        
        /* 기존 Entry 정리 */
        var entry = settings.FindAssetEntry(lGuid);
        var prvGroup = entry == null ? null : entry.parentGroup;

        /* Create */
        entry = settings.CreateOrMoveEntry(lGuid, group, false, true);
        entry.SetAddress(groupKey);
        entry.SetLabel(groupKey, true);

        if (prvGroup != null && (prvGroup.entries == null || prvGroup.entries.Count == 0))
            settings.RemoveGroup(prvGroup);
    }
    
    private static void _RemoveEmptyGroups(AddressableAssetSettings pSettings)
    {
        for (int i = pSettings.groups.Count - 1; i >= 0; --i)
        {
            var lGroup = pSettings.groups[i];
            if (lGroup == null)
            {
                pSettings.groups.RemoveAt(i);
                continue;
            }

            if (string.IsNullOrWhiteSpace(lGroup.Name) == false)
            {
                switch (lGroup.Name)
                {
                    case "Built In Data":
                    case "Default Local Group":
                    case "Duplicate Asset Isolation":
                        continue;
                }
            }

            if (lGroup.entries.Count > 0)
                continue;

            pSettings.RemoveGroup(lGroup);
        }
    }

    [MenuItem("Nori Tools/Addressable/Update Addressable Asset Groups", false, 100)]
    private static void _UpdateAddressableAssetGroups()
    {
        var settings = AddressableAssetSettingsDefaultObject.GetSettings(false);

        AssetDatabase.StartAssetEditing();
        {
            var path_Root = $"{Application.dataPath}/01_Resources".Replace("\\", "/");
            var paths_ABFolder = Directory.GetDirectories(path_Root, $"{_c_prefix_AddressableFolder}*",
                SearchOption.AllDirectories);

            for (int i = 0; i < paths_ABFolder.Length; ++i)
            {
                var path_ABFolder = paths_ABFolder[i]
                    .Replace(Application.dataPath, "Assets")
                    .Replace("\\", "/");

                EditorUtility.DisplayProgressBar(
                    "Create Addressable Asset Group"
                    , $"({i + 1}/{paths_ABFolder.Length}){path_ABFolder}"
                    , (i + 1f) / paths_ABFolder.Length);

                _RegistAddress(path_ABFolder, settings);

                var lPaths_ChildFolder = Directory.GetDirectories(path_ABFolder, "*", SearchOption.AllDirectories);
                for (int j = 0; j < lPaths_ChildFolder.Length; ++j)
                {
                    var path_ChildFolder = lPaths_ChildFolder[j];

                    path_ChildFolder = path_ChildFolder.Replace(Application.dataPath, "Assets");
                    path_ChildFolder = path_ChildFolder.Replace("\\", "/");
                    _RegistAddress(path_ChildFolder, settings);
                }
            }
        }
        AssetDatabase.StopAssetEditing();
        AssetDatabase.SaveAssets();

        /* 빈 그룹 삭제 */
        _RemoveEmptyGroups(settings);
        
        AssetDatabase.StartAssetEditing();
        {
            /* Sorting */
            int _GetGroupSortOrder(AddressableAssetGroup lGroup)
            {
                if (lGroup == null)
                    return 3;

                switch (lGroup.Name)
                {
                    case "Built In Data":
                        return 0;
                    case "Default Local Group":
                        return 1;
                    case "Duplicate Asset Isolation":
                        return 2;
                }

                if (lGroup.name.StartsWith(_c_prefix_AddressableFolder))
                    return 99;

                return 4;
            }

            settings.groups.Sort((x, y) =>
            {
                var lXOrder = _GetGroupSortOrder(x);
                var lYOrder = _GetGroupSortOrder(y);

                var lResult = lXOrder.CompareTo(lYOrder);
                if (lResult != 0)
                    return lResult;

                if (x == null || y == null)
                    return lResult;

                lResult = x.name.CompareTo(y.name);
                return lResult;
            });

        }
        AssetDatabase.StopAssetEditing();
        AssetDatabase.SaveAssets();
        
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("AssetManager", "Done :: Create Addressable Asset Group", "Confirm");
    }
}
#endif