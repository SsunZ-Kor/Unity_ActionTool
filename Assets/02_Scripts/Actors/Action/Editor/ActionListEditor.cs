using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

namespace Actor
{

    public class ActionListEditor : EditorWindow
    {

        private List<ActionEditor.ActionDataInfo> _listActionDataInfo = new(1000);
        private List<ActionEditor.ActionDataInfo> _listActionDataInfoFiltered = null;
        private HashSet<string> _setActionDataGroupFolded = new();

        private string _txtFilter = null;
        private Vector2 vScroll = Vector2.zero;
        private int _idx_SelectedActionDataInfo;

        private GUIStyle guiStyle_CenterLabel;
        private GUIStyle guiStyle_ActionNormal;
        private GUIStyle guiStyle_ActionSelected;
        private GUIStyle guiStyle_ActionGroupNormal;
        private GUIStyle guiStyle_ActionGroupSelected;

        public static ActionListEditor ShowWindow()
        {
            var wnd = GetWindow<ActionListEditor>("Action List");
            return wnd;
        }

        private void OnEnable()
        {
            ActionEditor.editorForList = this;
            LoadAllActionList();
        }

        private void LoadAllActionList()
        {
            var path_Root = ActionEditor.PATH_ACTOR_ROOT.Replace("Assets", Application.dataPath);
            var paths_ActionDataFolder = Directory.GetDirectories(path_Root, ActionEditor.FOLDERNAME_ACTIONDATA,
                SearchOption.AllDirectories);

            for (int i = 0; i < paths_ActionDataFolder.Length; ++i)
            {
                var path_ActionDataFolder = paths_ActionDataFolder[i]
                    .Replace("\\", "/");

                EditorUtility.DisplayProgressBar(
                    "Create Addressable Asset Group"
                    , $"({i + 1}/{paths_ActionDataFolder.Length}){path_ActionDataFolder}"
                    , (i + 1f) / paths_ActionDataFolder.Length);

                var name_Group = AssetManager.FindAddressableGroupNameInPath(path_ActionDataFolder)
                    .Replace("AB_Actor_", "");

                var paths_ActionData = Directory.GetFiles(path_ActionDataFolder);
                if (paths_ActionData != null)
                {
                    for (int j = 0; j < paths_ActionData.Length; ++j)
                    {
                        var path_ActionData = paths_ActionData[j]
                            .Replace(Application.dataPath, "Assets")
                            .Replace("\\", "/");

                        if (Path.GetExtension(path_ActionData) == ".meta")
                            continue;

                        _listActionDataInfo.Add(new()
                        {
                            name_Data = Path.GetFileNameWithoutExtension(path_ActionData),
                            name_Group = name_Group,
                            path = path_ActionData
                        });
                    }
                }
            }

            _listActionDataInfo.Sort((x, y) =>
            {
                var result = String.Compare(x.name_Group, y.name_Group, StringComparison.Ordinal);
                if (result != 0)
                    return result;

                result = String.Compare(x.name_Data, y.name_Data, StringComparison.Ordinal);
                return result;
            });

            UpdateActionDataList();

            EditorUtility.ClearProgressBar();
        }

        private void UpdateActionDataList()
        {
            if (string.IsNullOrWhiteSpace(_txtFilter))
                _listActionDataInfoFiltered = _listActionDataInfo;
            else
                _listActionDataInfoFiltered = _listActionDataInfo.Where((x) =>
                    x.name_Group.Contains(_txtFilter) || x.name_Data.Contains(_txtFilter)).ToList();

            _idx_SelectedActionDataInfo =
                _listActionDataInfoFiltered.FindIndex((x) => x == ActionEditor.SelectedActionDataInfo);
        }


        private void OnGUI()
        {
            OnGUI_CheckAndInit();

            OnGUI_InputUpDown();
            OnGUI_InputLeftRight();

            EditorGUILayout.BeginVertical();
            {
                OnGUI_TopMenu();
                OnGUI_ActionList();
            }
            EditorGUILayout.EndVertical();
        }

        private void OnGUI_InputUpDown()
        {
            if (!Event.current.isKey || Event.current.type == EventType.KeyUp)
                return;

            var newIdx = _idx_SelectedActionDataInfo;
            switch (Event.current.keyCode)
            {
                case KeyCode.UpArrow:
                {
                    if (_setActionDataGroupFolded.Contains(ActionEditor.SelectedActionDataInfo.name_Group))
                    {
                        while (newIdx > 0)
                        {
                            --newIdx;
                            var newSelectedActionDataInfo = _listActionDataInfoFiltered[newIdx];
                            if (newSelectedActionDataInfo.name_Group !=
                                ActionEditor.SelectedActionDataInfo.name_Group)
                                break;
                        }
                    }
                    else
                    {
                        if (--newIdx < 0)
                            newIdx = 0;
                    }

                    Event.current.Use();
                }
                    break;
                case KeyCode.DownArrow:
                {
                    if (_setActionDataGroupFolded.Contains(ActionEditor.SelectedActionDataInfo.name_Group))
                    {
                        while (newIdx < _listActionDataInfoFiltered.Count - 1)
                        {
                            ++newIdx;
                            var newSelectedActionDataInfo = _listActionDataInfoFiltered[newIdx];
                            if (newSelectedActionDataInfo.name_Group !=
                                ActionEditor.SelectedActionDataInfo.name_Group)
                                break;
                        }
                    }
                    else
                    {
                        if (++newIdx >= _listActionDataInfoFiltered.Count)
                            newIdx = _listActionDataInfoFiltered.Count - 1;
                    }

                    Event.current.Use();
                }
                    break;
            }

            if (newIdx == _idx_SelectedActionDataInfo)
                return;

            var actionDataInfo = _listActionDataInfoFiltered[newIdx];
            ActionEditor.SetSelectedActionDataInfo(actionDataInfo);
            _idx_SelectedActionDataInfo = newIdx;

            _setActionDataGroupFolded.Remove(actionDataInfo.name_Group);

            this.Repaint();
        }

        private void OnGUI_InputLeftRight()
        {
            if (!Event.current.isKey || Event.current.type != EventType.KeyDown)
                return;

            switch (Event.current.keyCode)
            {
                case KeyCode.LeftArrow:
                {
                    _setActionDataGroupFolded.Add(ActionEditor.SelectedActionDataInfo.name_Group);
                    Event.current.Use();
                }
                    break;
                case KeyCode.RightArrow:
                {
                    _setActionDataGroupFolded.Remove(ActionEditor.SelectedActionDataInfo.name_Group);
                    Event.current.Use();
                }
                    break;
            }

            this.Repaint();
        }

        private void OnGUI_CheckAndInit()
        {
            if (guiStyle_CenterLabel == null)
            {
                guiStyle_CenterLabel = new(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
            }

            if (guiStyle_ActionNormal == null)
            {
                guiStyle_ActionNormal = new(EditorStyles.label);
            }

            if (guiStyle_ActionSelected == null)
            {
                guiStyle_ActionSelected = new(EditorStyles.label);

                var tex = new Texture2D(2, 2);
                tex.SetColor(new(1f, 165f / 255f, 0f));
                guiStyle_ActionSelected.normal.background = tex;
            }

            if (guiStyle_ActionGroupNormal == null)
            {
                guiStyle_ActionGroupNormal = new(EditorStyles.foldoutHeader);
            }

            if (guiStyle_ActionGroupSelected == null)
            {
                guiStyle_ActionGroupSelected = new(EditorStyles.foldoutHeader);

                var tex = new Texture2D(2, 2);
                tex.SetColor(new(1f, 165f / 255f, 0f));
                guiStyle_ActionGroupSelected.normal.background = tex;
            }
        }

        private void OnGUI_TopMenu()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    _txtFilter = EditorGUILayout.TextField("", _txtFilter);

                    if (GUILayout.Button("Search"))
                        UpdateActionDataList();
                }
            }
        }

        private void OnGUI_ActionList()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                if (_listActionDataInfoFiltered == null || _listActionDataInfoFiltered.Count == 0)
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("List is Empty", guiStyle_CenterLabel);
                    GUILayout.FlexibleSpace();
                }
                else
                {
                    vScroll = EditorGUILayout.BeginScrollView(vScroll);
                    {
                        var crrGroupName = (string)null;
                        bool isFolded = false;

                        for (int i = 0; i < _listActionDataInfoFiltered.Count; ++i)
                        {
                            var actionDataInfo = _listActionDataInfoFiltered[i];

                            /* Draw Group */
                            if (crrGroupName != actionDataInfo.name_Group)
                            {
                                crrGroupName = actionDataInfo.name_Group;
                                isFolded = _setActionDataGroupFolded.Contains(crrGroupName);
                                var guiStyle = isFolded && ActionEditor.SelectedActionDataInfo?.name_Group ==
                                    crrGroupName
                                        ? guiStyle_ActionGroupSelected
                                        : guiStyle_ActionGroupNormal;

                                var isFoldedNew = !EditorGUILayout.Foldout(!isFolded, crrGroupName, true, guiStyle);
                                if (isFolded != isFoldedNew)
                                {
                                    isFolded = isFoldedNew;
                                    if (isFolded)
                                        _setActionDataGroupFolded.Add(crrGroupName);
                                    else
                                        _setActionDataGroupFolded.Remove(crrGroupName);
                                }
                            }

                            if (isFolded)
                                continue;

                            /* Draw Action */
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                var guiStyle = ActionEditor.SelectedActionDataInfo == actionDataInfo
                                    ? guiStyle_ActionSelected
                                    : guiStyle_ActionNormal;

                                GUILayout.Space(25f);
                                if (GUILayout.Button(actionDataInfo.name_Data, guiStyle))
                                {
                                    ActionEditor.SetSelectedActionDataInfo(actionDataInfo);
                                    _idx_SelectedActionDataInfo = i;
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndScrollView();
                    GUILayout.FlexibleSpace();
                }
            }
        }
    }
}