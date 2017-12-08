namespace HoloToolkit.Unity
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System;


    public static class BankEditorSkin
    {
        public static GUIStyle Label = new GUIStyle(EditorStyles.label);
        public static GUIStyle Button = new GUIStyle(EditorStyles.miniButton);
        public static GUIStyle Foldout = new GUIStyle(EditorStyles.foldout);

        public static void Init()
        {
            // Stop the label filling the remaining space
            Label.stretchWidth = false;
            Label.richText = true;

            // Change the active text colour
            Button.active.textColor = Color.green;

            // Change the foldout text colour
            Foldout.active.textColor = Color.green;
            Foldout.focused.textColor = Color.green;
            Foldout.onActive.textColor = Color.green;
            Foldout.onFocused.textColor = Color.green;
            Foldout.hover.textColor = Color.green;
            Foldout.onHover.textColor = Color.green;
        }
    }

    public class BankMoveInfo
    {
        public BankInfo Bank;
        public GroupInfo FromGroup;
        public ComparisonType Comparison;
        public string ToGroup;
        public bool RemoveFromGroup;
        public bool RemoveFromAll;
    }

    public class BankEditor : EditorWindow
    {
        private GUISkin Skin;

        private BankGroupInfo Info;

        private float BankNameColumnWidth = 50.0f;

        private float EventCountColumnWidth = 50.0f;

        private string NewBankName;

        private BankMoveInfo PostPaintMove;

        [MenuItem("Mixed Reality Toolkit/UAudioTools/Bank Editor")]
        static void ShowEditor()
        {
            BankEditor editor = GetWindow<BankEditor>();
            editor.titleContent = new GUIContent("Bank Editor");
            editor.Skin = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/HoloToolkit/SpatialSound/Scripts/UAudioManager/Editor/BankManagerSkin.guiskin");
            editor.Show();
            BankEditorSkin.Init();
        }

        private void OnFocus()
        {
            Debug.Log("Windw has focus");
        }
        private void OnLostFocus()
        {
            Debug.Log("Window has lost focus");
        }

        private void OnEnable()
        {
            Debug.Log("Window enabled");
        }
        private void OnDisable()
        {
            Debug.Log("Window disabled");
        }

        private void OnProjectChange()
        {
            Debug.Log("Project has changed");
        }

        private void OnGUI()
        {
            if (Info == null)
            {
                LoadInfo();
            }

            BankEditorSkin.Init();

            EditorGUILayout.BeginVertical();
            {
                var minWidth = GUILayout.Width(BankNameColumnWidth);
                var eventWidth = GUILayout.Width(EventCountColumnWidth);

                foreach (var groupInfo in Info.Groups)
                {
                    groupInfo.Foldout = EditorGUILayout.Foldout(groupInfo.Foldout, groupInfo.Name, BankEditorSkin.Foldout);
                    if (groupInfo.Foldout)
                    {
                        foreach (var bankInfo in groupInfo.Banks)
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                if (groupInfo.PrimaryBank == bankInfo.Bank)
                                {
                                    GUIContent primaryBank = new GUIContent("<color=yellow>*</color>");
                                    var primaryWidth = GUILayout.Width(15);
                                    GUILayout.Label(primaryBank, BankEditorSkin.Label, primaryWidth);
                                }
                                else
                                {
                                    GUILayout.Space(24);
                                }

                                var bankName = new GUIContent(bankInfo.AssetName);
                                var eventCount = new GUIContent("<b><color=cyan>" + bankInfo.Bank.Events.Length.ToString() + "</color></b> Events");

                                // Calculate the maximum field widths
                                float min, max;

                                // BankName
                                BankEditorSkin.Button.CalcMinMaxWidth(bankName, out min, out max);
                                if (max > BankNameColumnWidth)
                                {
                                    BankNameColumnWidth = max+20;
                                    minWidth = GUILayout.Width(BankNameColumnWidth);
                                }

                                // Event count
                                BankEditorSkin.Label.CalcMinMaxWidth(eventCount, out min, out max);
                                if (max > EventCountColumnWidth)
                                {
                                    EventCountColumnWidth = max;
                                    eventWidth = GUILayout.Width(EventCountColumnWidth);
                                }

                                // Layout the UI
                                if (GUILayout.Button(bankName, BankEditorSkin.Button, minWidth))
                                {
                                    // Select the bank for editing
                                    Selection.activeObject = bankInfo.Bank;
                                }
                                GUILayout.Label(eventCount, BankEditorSkin.Label, eventWidth);
                                GUILayout.Label(bankInfo.AssetPath, BankEditorSkin.Label);

                                // Calculate the context menu hit rect
                                if (Event.current.type == EventType.Repaint)
                                {
                                    bankInfo.ContextMenuRect = GUILayoutUtility.GetLastRect();
                                    bankInfo.ContextMenuRect.xMin = 0;
                                    bankInfo.ContextMenuRect.xMax = this.position.width;
                                }

                                // Display the context menu for this item
                                if (Event.current.type == EventType.ContextClick && bankInfo.ContextMenuRect.Contains(Event.current.mousePosition))
                                {
                                    GenericMenu menu = new GenericMenu();
                                    menu.AddDisabledItem(new GUIContent(bankName));
                                    menu.AddSeparator(string.Empty);

                                    int currentGroupType;
                                    if (groupInfo.Name == "Ignore") currentGroupType = 0;
                                    else if (groupInfo.Name == "Unique") currentGroupType = 1;
                                    else currentGroupType = 2;

                                    switch(currentGroupType)
                                    {
                                        case 0:
                                            menu.AddItem(new GUIContent("Unique Bank"), false, (x) => MoveTo(x), new BankMoveInfo { Bank = bankInfo, FromGroup = groupInfo, Comparison = ComparisonType.Unique, ToGroup = "Unique", RemoveFromGroup = true, RemoveFromAll = false });
                                            break;
                                        case 1:
                                            menu.AddItem(new GUIContent("Ignore Bank"), false, (x) => MoveTo(x), new BankMoveInfo { Bank = bankInfo, FromGroup = groupInfo, Comparison = ComparisonType.Ignored, ToGroup = "Ignore", RemoveFromGroup = true, RemoveFromAll = true });
                                            menu.AddItem(new GUIContent("Add To New Group"), false, (x) => AddToNewGroup(x), bankInfo);
                                            break;
                                        case 2:
                                            menu.AddItem(new GUIContent("Ignore Bank"), false, (x) => MoveTo(x), new BankMoveInfo { Bank = bankInfo, FromGroup = groupInfo, Comparison = ComparisonType.Ignored, ToGroup = "Ignore", RemoveFromGroup = true, RemoveFromAll = true });
                                            break;
                                    }
                                    //if (groupInfo.Name != "Ignore")
                                    //{
                                    //    menu.AddItem(new GUIContent("Ignore Bank"), false, (x) => MoveTo(x), new BankMoveInfo { Bank = bank, FromGroup = groupInfo, Comparison = ComparisonType.Ignored, ToGroup = "Ignore", RemoveFromGroup = true, RemoveFromAll = true });
                                    //}
                                    //if (groupInfo.Name != "Unique")
                                    //{
                                    //    menu.AddItem(new GUIContent("Unique Bank"), false, (x) => MoveTo(x), new BankMoveInfo { Bank = bank, FromGroup = groupInfo, Comparison = ComparisonType.Unique, ToGroup = "Unique", RemoveFromGroup = true, RemoveFromAll = false });
                                    //}
                                    //menu.AddItem(new GUIContent("Add To New Group"), false, (x) => AddToNewGroup(x), bank);
                                    menu.ShowAsContext();
                                    Event.current.Use();
                                }
                            }
                            EditorGUILayout.EndHorizontal();

                            if (bankInfo.ShowMoveToNewGroup)
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    GUILayout.Space(30);

                                    float min, max;
                                    BankEditorSkin.Button.CalcMinMaxWidth(new GUIContent("Create"), out min, out max);
                                    var createWidth = GUILayout.Width(max + 20);
                                    BankEditorSkin.Button.CalcMinMaxWidth(new GUIContent("Cancel"), out min, out max);
                                    var cancelWidth = GUILayout.Width(max + 20);

                                    GUILayout.Label("New Bank Name", BankEditorSkin.Label);
                                    NewBankName = GUILayout.TextField(NewBankName);
                                    if (GUILayout.Button("Create", BankEditorSkin.Button, createWidth))
                                    {
                                        PostPaintMove = new BankMoveInfo
                                        {
                                            Bank = bankInfo,
                                            FromGroup = groupInfo,
                                            ToGroup = NewBankName,
                                            RemoveFromAll = false,
                                            RemoveFromGroup = false
                                        };
                                        bankInfo.ShowMoveToNewGroup = false;
                                    }
                                    if (GUILayout.Button("Cancel", BankEditorSkin.Button, cancelWidth))
                                    {
                                        bankInfo.ShowMoveToNewGroup = false;
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                    }
                }
            }
            EditorGUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint)
            {
                if (PostPaintMove != null)
                {
                    MoveTo(PostPaintMove);
                    PostPaintMove = null;
                }
            }
        }

        private void AddToNewGroup(object userData)
        {
            BankInfo bankInfo = userData as BankInfo;

            Debug.AssertFormat(bankInfo != null, "Expecting a BankInfo, but got a {0}", userData.GetType().FullName);

            bankInfo.ShowMoveToNewGroup = true;
            NewBankName = string.Empty;
        }

        private void MoveTo(object userData)
        {
            BankMoveInfo moveInfo = userData as BankMoveInfo;

            Debug.AssertFormat(moveInfo != null, "Expecting a BankMoveInfo, but got a {0}", userData.GetType().FullName);

            GroupInfo group = Info.Groups.Find((grp) => grp.Name == moveInfo.ToGroup);
            if (group == null)
            {
                group = new GroupInfo
                {
                    Name = moveInfo.ToGroup,
                    Comparison = moveInfo.Comparison,
                    PrimaryBank = null,
                    Banks = new List<BankInfo>(2)
                };
                Info.Groups.Add(group);
            }

            // Don't add the same bank twice to one group
            if (group.Banks.Find((x) => x.Bank == moveInfo.Bank.Bank) == null)
            {
                group.Banks.Add(moveInfo.Bank);
            }

            if (group.PrimaryBank == null)
            {
                group.PrimaryBank = moveInfo.Bank.Bank;
            }

            if (moveInfo.RemoveFromGroup)
            {
                moveInfo.FromGroup.Banks.Remove(moveInfo.Bank);
                if (moveInfo.FromGroup.PrimaryBank == moveInfo.Bank.Bank)
                {
                    if (moveInfo.FromGroup.Banks.Count > 0)
                    {
                        moveInfo.FromGroup.PrimaryBank = moveInfo.FromGroup.Banks[0].Bank;
                    }
                }
            }

            // Remove from all other groups
            if (moveInfo.RemoveFromAll)
            {
                foreach (var grp in Info.Groups)
                {
                    if (grp != group)
                    {
                        grp.Banks.Remove(moveInfo.Bank);
                    }
                }
            }

            // Remove any unused groups
            Info.Groups.RemoveAll((grp) => grp.Banks.Count == 0);

            EditorUtility.SetDirty(Info);
        }

        private void LoadInfo()
        {
            var assets = AssetDatabase.FindAssets("t:BankGroupInfo");

            if (assets.Length >= 1)
            {
                Info = AssetDatabase.LoadAssetAtPath<BankGroupInfo>(AssetDatabase.GUIDToAssetPath(assets[0]));
                if (Info == null)
                {
                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(assets[0]));
                    Debug.LogFormat("Failed to load asset {0}", AssetDatabase.GUIDToAssetPath(assets[0]));
                    if (obj != null)
                    {
                        Debug.LogFormat("But it did load an object");

                    }
                    Info = FindObjectOfType<BankGroupInfo>();
                }
                Debug.LogFormat("Found {0} Groups", Info.Groups.Count);
            }
            else
            {
                Info = CreateInstance<BankGroupInfo>();
                Info.Groups.Add(new GroupInfo()
                {
                    Name = "Unique",
                    Comparison = ComparisonType.Unique,
                    PrimaryBank = null,
                    Banks = GetBankList()
                });

                AssetDatabase.CreateAsset(Info, "Assets/BankInfo.asset");
            }
        }

        private List<BankInfo> GetBankList()
        {
            var assets = AssetDatabase.FindAssets("t:AudioEventBank");

            var bankInfo = new List<BankInfo>(assets.Length);
            for (int i = 0; i < assets.Length; i++)
            {
                bankInfo.Add(new BankInfo(assets[i]));
            }

            return bankInfo;
        }
    }
}