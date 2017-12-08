using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HoloToolkit.Unity
{
    [System.Serializable]
    public class BankInfo
    {
#if UNITY_EDITOR
        public string AssetPath { get { return AssetDatabase.GetAssetPath(Bank.GetInstanceID()); } }
        public string AssetName { get { return Bank.name; } }
#endif

        [SerializeField]
        public AudioEventBank Bank;

        [System.NonSerialized]
        [HideInInspector]
        public Rect ContextMenuRect;

        [System.NonSerialized]
        [HideInInspector]
        public bool ShowMoveToNewGroup;

        public BankInfo()
        {
        }

#if UNITY_EDITOR
        public BankInfo(string assetGuid)
        {
            Bank = AssetDatabase.LoadAssetAtPath<AudioEventBank>(AssetDatabase.GUIDToAssetPath(assetGuid));
        }
#endif
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
        public List<BankInfo> Banks;
        public AudioEventBank PrimaryBank;

        [System.NonSerialized]
        [HideInInspector]
        public bool Foldout;
    }

    public class BankGroupInfo : ScriptableObject
    {
        public List<GroupInfo> Groups = new List<GroupInfo>();
    }
}