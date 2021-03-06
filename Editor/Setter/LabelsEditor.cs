﻿using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;
using static AddressableManager.AddressableSetter.Editor.Utilities;

namespace AddressableManager.AddressableSetter.Editor
{
    internal class LabelsEditor
    {
       
        public Setter Setter => (Setter)MainEditor.target;
        public static LabelsEditor Instance { get; set; }
        public UnityEditor.Editor MainEditor { get; set; }
        private bool Foldout { get; set; } = true;
        private bool FolderNameIsGroupName { get; set; }
        private bool RemoveFolderLabelButton { get; set; }
        private bool AddFolderLabelButton { get; set; } = true;
        private bool RemoveGroupLabelButton { get; set; }
        private bool AddGroupLabelButton { get; set; } = true;
        private bool IsValidCustomLabel { get; set; }
        private bool ApplyButton { get; set; }
        private bool RemoveLabelButton { get; set; }
        private const string Status = "Label";
        private List<string> CustomLabelList { get => Setter.customLabelList; set => Setter.customLabelList = value; }
        private string[] CustomLabelArray => CustomLabelList?.ToArray();
        public int GroupsNameIndex { get; private set; }

        public LabelsEditor(UnityEditor.Editor editor)
        {
            MainEditor = editor;
        }
        internal void Init()
        {
            FolderNameIsGroupName = CompareOrdinal(Setter.name, Setter.newGroupName);

            Foldout = BeginFoldoutHeaderGroup(Foldout, Status + " Settings");
            if (Foldout)
            {
                var headerCount = FolderNameIsGroupName ? 3 : 4;
                BeginVertical("Box");
                BeginHorizontal();

                var header = FolderNameIsGroupName ? 
                    new[] { "Set " + nameof(Setter.autoLoad), "[+/-] FolderLabel", "[+/-] " + nameof(Setter.customLabel) } :
                    new[] { "Set " + nameof(Setter.autoLoad), "[+/-] FolderLabel", "[+/-] GroupLabel", "[+/-] " + nameof(Setter.customLabel) };

                Labels(header, headerCount);

                EndHorizontal();
                BeginHorizontal();
                AutoLoadLabel(headerCount);
                FolderNameLabel(headerCount);
                GroupNameLabel(headerCount);
                CustomLabel(headerCount);
                EndHorizontal();
                SetCustomLabel();
                EndVertical();
                LabelToApply();
            }
            EndFoldoutHeaderGroup();
            Space(5);


           
        }
        private void AutoLoadLabel(int headerCount)
        {
            EditorGUI.BeginChangeCheck();

            PropertyField(MainEditor, nameof(Setter.autoLoad), GUIContent.none, headerCount);
            MainEditor.serializedObject.ApplyModifiedProperties();
            MainEditor.serializedObject.Update();

            if (!EditorGUI.EndChangeCheck()) return;
            Setter.ManageLabel.ManageAutoLoad();

            if (Setter.AssetCount <= 0) return;

            MainEditor.serializedObject.ApplyModifiedProperties();
            MainEditor.serializedObject.Update();
            Setter.Add();


        }
        private void FolderNameLabel(int headerCount)
        {
            EditorGUI.BeginChangeCheck();

            var labelExist = Setter.labelReferences?.Count > 0 && Setter.labelReferences.Any(o => o.labelString == Setter.name);

            if (labelExist)
            {
                RemoveFolderLabelButton = GUILayout.Button("Remove", GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth / headerCount));
                if (RemoveFolderLabelButton) Setter.ManageLabel.RemoveLabel(Setter.name);
            }
            else
            {
                AddFolderLabelButton = GUILayout.Button("Add", GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth / headerCount));
                if (AddFolderLabelButton) Setter.ManageLabel.AddLabel(Setter.name);
            }

            if (!EditorGUI.EndChangeCheck()) return;
            if (Setter.ManageEntry.EntriesAdded) Setter.ManageEntry.RefreshEntryLabels();

        }
        private void GroupNameLabel(int headerCount)
        {

            if (FolderNameIsGroupName) return;

            EditorGUI.BeginChangeCheck();

            var labelExist = Setter.assetSettings != null && Setter.assetSettings.GetLabels().Any(o => o == Setter.newGroupName);

            if (labelExist)
            {
                RemoveGroupLabelButton = GUILayout.Button("Remove", GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth / headerCount));
                if (RemoveGroupLabelButton) Setter.ManageLabel.RemoveLabel(Setter.newGroupName);
            }
            else
            {
                AddGroupLabelButton = GUILayout.Button("Add", GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth / headerCount));
                if (AddGroupLabelButton) Setter.ManageLabel.AddLabel(Setter.newGroupName);
            }

            if (!EditorGUI.EndChangeCheck()) return;
            if (Setter.ManageEntry.EntriesAdded) Setter.ManageEntry.RefreshEntryLabels();
        }
        private void CustomLabel(int headerCount)
        {
            PropertyField(MainEditor,nameof(Setter.customLabel), GUIContent.none, headerCount);
            IsValidCustomLabel = !string.IsNullOrEmpty(Setter.customLabel);
            if (IsValidCustomLabel) ApplyButton = GUILayout.Button("Apply", GUILayout.MaxWidth(100));
            MainEditor.serializedObject.ApplyModifiedProperties();

        }
        private void SetCustomLabel()
        {
            BeginHorizontal();
           
            if (ApplyButton)
            {
                if (!CustomLabelList.Contains(Setter.customLabel)) CustomLabelList?.Add(Setter.customLabel);
                MainEditor.serializedObject?.Update();
                CustomLabelList.ForEach(o=> Setter.ManageLabel.AddLabel(o));
            }

            if (CustomLabelList?.Count > 0)
            {
                RemoveLabelButton = GUILayout.Button("Remove Label", GUILayout.MinWidth(50), GUILayout.MaxWidth(100));
                GroupsNameIndex = Popup(GroupsNameIndex, CustomLabelArray, GUILayout.MinWidth(10), GUILayout.MaxWidth(150));
                if (RemoveLabelButton) Setter.ManageLabel.RemoveLabel(CustomLabelArray[GroupsNameIndex]);
            }

            EndHorizontal();


        }
        private void LabelToApply()
        {
            BeginVertical("Box");
            var list = Setter.labelReferences;
            var style = new GUIStyle { richText = true };
            var content = list.Count > 0 ? 
                $" <color=grey> Labels To Apply :</color> <color=green> {list.Count} </color>  " :
                $" <color=yellow> Labels To Apply : {list.Count} </color> <color=grey>! Add Labels From Label Settings and Click </color> <color=white> Add  </color>";
            GUILayout.Label(content, style, MaxWidth(1));
            var property = MainEditor.serializedObject.FindProperty(nameof(Setter.labelReferences));
            property.serializedObject.Update();
            if (list.Count > 0) for (var i = 0; i < list.Count; i++) PropertyField(property.GetArrayElementAtIndex(i));
            EndVertical();
        }

    }


}