using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ReceiptData", menuName = "Scriptable Objects/ReceiptData")]
public class ReceiptData : ScriptableObject,IData
{
    [field: SerializeField]public string DataName { get; set; }
    [field:SerializeField]public List<ReceiptItem> ReceiptItemList = new List<ReceiptItem>();
    [field: SerializeField] public string resultItem { get; set; }
}

[Serializable]
public struct ReceiptItem
{
    public string itemName;
    public int requireItemCount;
}

