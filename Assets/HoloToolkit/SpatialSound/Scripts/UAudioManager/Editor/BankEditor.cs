namespace HoloToolkit.Unity
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    [System.Serializable]
    public class BankInfo
    {
        public string AssetPath { get { return AssetDatabase.GetAssetPath(Bank.GetInstanceID()); } }
        public string AssetName { get { return Bank.name; } }

        public AudioEventBank Bank;

        public BankInfo()
        {
        }

        public BankInfo(string assetGuid)
        {
            Bank = AssetDatabase.LoadAssetAtPath<AudioEventBank>(AssetDatabase.GUIDToAssetPath(assetGuid));
        }
    }

    public enum ComparisonType
    {
        Unique,
        Ignored,
        MatchThePrimary
    }

    [System.Serializable]
    public class GroupInfo
    {
        public string Name;
        public ComparisonType Comparison;
        public BankInfo[] Banks;
        public int PrimaryBank;

        [System.NonSerialized]
        [HideInInspector]
        public bool Foldout;
    }

    public class BankGroupInfo : ScriptableObject
    {
        public List<GroupInfo> Groups = new List<GroupInfo>();
    }

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

    public class BankEditor : EditorWindow
    {
        private GUISkin Skin;

        private BankGroupInfo Info;

        private BankInfo[] BankInfo;

        private float BankNameColumnWidth = 50.0f;

        private float EventCountColumnWidth = 50.0f;

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

            if (BankInfo == null)
            {
                GetBankList();
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
                        foreach (var bank in groupInfo.Banks)
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                GUILayout.Space(15);

                                var bankName = new GUIContent(bank.AssetName);
                                var eventCount = new GUIContent("<b><color=cyan>" + (bank.Bank.Events.Length * 34).ToString() + "</color></b> Events");

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
                                GUILayout.Button(bankName, BankEditorSkin.Button, minWidth);
                                GUILayout.Label(eventCount, BankEditorSkin.Label, eventWidth);
                                GUILayout.Label(bank.AssetPath, BankEditorSkin.Label);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void LoadInfo()
        {
            var assets = AssetDatabase.FindAssets("t:BankGroupInfo");

            if (assets.Length >= 1)
            {
                Info = AssetDatabase.LoadAssetAtPath<BankGroupInfo>(AssetDatabase.GUIDToAssetPath(assets[0]));
            }
            else
            {
                Info = CreateInstance<BankGroupInfo>();
                Info.Groups.Add(new GroupInfo()
                {
                    Name = "Unique",
                    Comparison = ComparisonType.Unique,
                    PrimaryBank = -1,
                    Banks = GetBankList()
                });

                AssetDatabase.CreateAsset(Info, "Assets/BankInfo.asset");
            }
        }

        private BankInfo[] GetBankList()
        {
            var assets = AssetDatabase.FindAssets("t:AudioEventBank");

            BankInfo = new BankInfo[assets.Length];
            for (int i = 0; i < assets.Length; i++)
            {
                BankInfo[i] = new BankInfo(assets[i]);
            }

            return BankInfo;
        }
    }
}