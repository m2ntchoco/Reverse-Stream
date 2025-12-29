using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

// [저장용 데이터 구조] - JSON으로 변환될 부분
[System.Serializable]
public class ItemStats
{
    public float bonusHp;
    public float bonusDef;
    public float bonusPhyAtk;
    public float bonusMagAtk;
    public float bonusCritChance;
    public float bonusCritDmg;
    
    // 실제 저장될 때는 "아이템 이름(문자열)"만 저장됨
    public List<string> equipment_inventory = new List<string>();
    public List<string> synergy_inventory = new List<string>();
}

public class ItemDataManager : MonoBehaviour
{
    public static ItemDataManager Instance;

    public event Action OnInventoryChanged;

    [Header("💾 저장 데이터 (JSON)")]
    public ItemStats itemData; 

    [Header("👀 눈으로 확인하는 인벤토리 (저장 안됨, 보기용)")]
    // 여기에 실제 ItemData 파일들이 표시됩니다. 더블클릭하면 이동 가능!
    public List<ItemData> debugInventoryView = new List<ItemData>();

    private string filePath;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        // 경로 설정: AppData/LocalLow/회사명/게임명/Item_data.json
        filePath = Path.Combine(Application.persistentDataPath, "Item_data.json");
        LoadData();
    }

    // 데이터 저장
    public void SaveData()
    {
        string json = JsonUtility.ToJson(itemData, true);
        File.WriteAllText(filePath, json);
        
#if UNITY_EDITOR
        // 에디터에서 파일 갱신 확인용
        // AssetDatabase.Refresh(); // (빌드 시 에러 방지를 위해 주석 처리하거나 #if로 감싸도 됨)
#endif
    }

    // 데이터 불러오기
    public void LoadData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            itemData = JsonUtility.FromJson<ItemStats>(json);

            // [중요] 저장된 이름(String)을 보고 -> 실제 아이템(ItemData)을 찾아서 디버그 리스트 복구
            RebuildDebugView(); 
        }
        else
        {
            ResetData();
        }
    }

    // 초기화
    public void ResetData()
    {
        Debug.Log("💀 아이템 데이터 리셋 (Item_data.json 초기화)");
        itemData = new ItemStats();
        debugInventoryView.Clear(); // 보는 리스트도 비우기
        SaveData();
    }

    // 아이템 획득 로직
    public void AddItem(int itemId)
    {
        // 1. Resources/Items 폴더에서 모든 아이템 정보 가져오기
        ItemData[] allItems = Resources.LoadAll<ItemData>("");

        // 2. ID로 해당 아이템 찾기
        ItemData foundItem = null;
        foreach (var item in allItems)
        {
            if (item.id == itemId)
            {
                foundItem = item;
                break;
            }
        }

        // 3. 예외 처리
        if (foundItem == null)
        {
            Debug.LogError($"[오류] ID {itemId}번 아이템을 Resources/Items 폴더에서 찾을 수 없습니다.");
            return;
        }

        // 4. 데이터 반영 (저장용)
        itemData.equipment_inventory.Add(foundItem.itemName);
        itemData.bonusPhyAtk += foundItem.bonusPhyAtk;
        itemData.bonusHp += foundItem.bonusHp;
        itemData.bonusDef += foundItem.bonusDef;
        itemData.bonusCritChance += foundItem.bonusCritChance;
        itemData.bonusCritDmg += foundItem.bonusCritDmg;

        // 5. [핵심] 눈으로 보기 편하게 디버그 리스트에도 추가
        debugInventoryView.Add(foundItem);

        Debug.Log($"✨ 아이템 획득: {foundItem.itemName} (공격력 +{foundItem.bonusPhyAtk})");

        // InventoryUI 인스턴스 가져오기
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            // initialItems 배열에 추가
            inventoryUI.initialItems.Add(foundItem);

            // UI 갱신
            inventoryUI.DistributeItems();
        }
        else
        {
            Debug.LogWarning("InventoryUI를 찾을 수 없어 UI를 갱신할 수 없습니다.");
        }
        
        // 6. 저장 및 스탯 갱신
        SaveData();
 
        if (StatDataManager.Instance != null)
            StatDataManager.Instance.CalculateFinalStats();
    }

    // 저장된 이름 목록을 기반으로 시각적 리스트(List<ItemData>)를 다시 채우는 함수
    private void RebuildDebugView()
    {
        debugInventoryView.Clear();
        
        // 리소스 폴더의 모든 아이템 로드
        ItemData[] allItems = Resources.LoadAll<ItemData>("Items");

        // 저장된 이름 리스트를 순회하며 매칭되는 ItemData 찾기
        foreach (string savedName in itemData.equipment_inventory)
        {
            foreach (var dbItem in allItems)
            {
                if (dbItem.itemName == savedName)
                {
                    debugInventoryView.Add(dbItem);
                    break;
                }
            }
        }
    }
}