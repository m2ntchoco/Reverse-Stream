// using UnityEngine;
// using UnityEngine.UIElements;
// using System.Collections.Generic;
// using Unity.VisualScripting;
// // Inventory UI 리팩토링을 위한 세부 스크립트. 
// public class InventorySynergyUI : MonoBehaviour
// {

//     public UIDocument uiDocument;
//     private SynergyManager _synergyManager;
//     public InventoryUI _inventoryUI;
//     private VisualElement _synergyContainer;
//     private VisualElement _root;
//     private VisualElement _noSynergyMessage;
//     private VisualElement _synergyUIElements;

//     private VisualElement _synergyContextMenu;
//     private OnEnable()
//   {
//     var doc = uiDocument != null ? uiDocument : GetComponent<UIDocument>();
//         if (doc == null) return;

//         uiDocument = GetComponent<UIDocument>();
    
//     _root = uiDocument.rootVisualElement;
//     _synergyContainer = _root.Q<VisualElement>("SynergyContainer");
//     _synergyUIElements = _root.Q<VisualElement>("SynergyUIElements");
//   }
//     // private ItemData[] _gridItems;
//     // public List<ItemData> initialItems = new List<ItemData>();
//     // Start is called once before the first execution of Update after the MonoBehaviour is created

//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }
//     public void InitializeSynergySystem()
//     {
//         _synergyManager = FindObjectOfType<SynergyManager>();
//         if (_synergyManager == null)
//         {
//             Debug.LogWarning("[InventoryUI] SynergyManager를 찾을 수 없습니다.");
//             return;
//         }
//     }


//     /////////////////////////////////////////
//     /*       시너지 핵심 로직 및 데이터 계산     */
//     ///////////////////////////////////////


//     public Dictionary<string, int> CalculateTagCounts(List<ItemData> items)
//     {
//         Dictionary<string, int> tagCounts = new Dictionary<string, int>();
        
//         foreach (var item in items)
//         {
//             if (item != null && item.synergyTags != null)
//             {
//                 foreach (var tag in item.synergyTags)
//                 {
//                     if (!string.IsNullOrEmpty(tag))
//                     {
//                         if (!tagCounts.ContainsKey(tag))
//                             tagCounts[tag] = 0;
//                         tagCounts[tag] += item.GetSynergyContribution();
//                     }
//                 }
//             }
//         }
        
//         return tagCounts;
//     }

//     public Dictionary<string, int> GetActiveSynergies(Dictionary<string, int> tagCounts)
//     {
//         Dictionary<string, int> activeSynergies = new Dictionary<string, int>();
        
//         if (_synergyManager == null) return activeSynergies;
        
//         // SynergyManager를 통해 정확한 계산
//         foreach (var kvp in tagCounts)
//         {
//             string tag = kvp.Key;
//             int itemCount = kvp.Value;
            
//             // SynergyData에서 해당 태그의 시너지 찾기
//             var synergyData = _synergyManager.GetSynergyDataByTag(tag);
//             if (synergyData != null)
//             {
//                 // SynergyData의 실제 thresholds를 사용하여 레벨 계산
//                 int level = synergyData.GetMaxLevel(itemCount);
                
//                 if (level > 0)
//                 {
//                     activeSynergies[synergyData.synergyId] = level; // 시너지 ID를 키로 사용
                    
//                     Debug.Log($"[GetActiveSynergies] {synergyData.displayName}: {itemCount}개 아이템으로 레벨 {level} 활성화 " +
//                              $"(thresholds: [{string.Join(", ", synergyData.thresholds)}])");
//                 }
//             }
//             else
//             {
//                 Debug.LogWarning($"[GetActiveSynergies] 태그 '{tag}'에 대한 SynergyData를 찾을 수 없습니다.");
//             }
//         }
        
//         return activeSynergies;
//     }

//     public SynergyData GetSynergyDataByTag(string tag)
//     {
//         // 1. 캐시된 참조 확인
//         if (_synergyManager == null) 
//         {
//             _synergyManager = FindObjectOfType<SynergyManager>();
            
//             // 2. SynergyManager를 찾을 수 없으면 경고 로그
//             if (_synergyManager == null)
//             {
//                 Debug.LogWarning("[InventoryUI] SynergyManager를 씬에서 찾을 수 없습니다. 시너지 기능이 작동하지 않습니다.");
//                 return null;
//             }
//         }
        
//         // 3. SynergyManager가 비활성화되었을 가능성 체크
//         if (!_synergyManager.enabled || !_synergyManager.gameObject.activeInHierarchy)
//         {
//             Debug.LogWarning("[InventoryUI] SynergyManager가 비활성화되어 있습니다.");
//             return null;
//         }
        
//         return _synergyManager.GetSynergyDataByTag(tag);
//     }

//     public List<ItemData> GetAllItemsWithSynergyTag(string synergyTag)
//     {
//         var items = new List<ItemData>();
        
//         // Resources/Items 폴더에서 모든 ItemData 로드
//         var allItems = Resources.LoadAll<ItemData>("SynergyItem");
        
//         foreach (var item in allItems)
//         {
//             if (item != null && item.synergyTags != null && item.synergyTags.Contains(synergyTag))
//             {
//                 items.Add(item);
//             }
//         }
        
//         // 티어 높은 순으로 정렬
//         items.Sort((a, b) => b.synergyTier.CompareTo(a.synergyTier));
        
//         return items;
//     }
//     public List<ItemData> GetAllItems()
//     {
//         List<ItemData> allItems = new List<ItemData>();
        
//         // 그리드 아이템들 추가
//         foreach (var item in _inventoryUI._gridItems)
//         {
//             if (item != null) allItems.Add(item);
//         }
        
//         // 장비 아이템들 추가
//         foreach (var item in _inventoryUI.initialItems)
//         {
//             if (item != null && item.itemType != 0 && !allItems.Contains(item))
//             {
//                 allItems.Add(item);
//             }
//         }
        
//         return allItems;
//     }

//     public int GetCurrentSynergyLevel(string synergyTag)
//     {
//         var synergyData = GetSynergyDataByTag(synergyTag);
//         if (synergyData == null) return 0;

//         var allItems = GetAllItems();
//         var tagCounts = CalculateTagCounts(allItems);
//         int itemCount = tagCounts.GetValueOrDefault(synergyTag, 0);

//         return synergyData.GetMaxLevel(itemCount);
//     }

//     public string GetCurrentSynergyLevelInfo(string synergyTag)
//     {
//         var synergyData = GetSynergyDataByTag(synergyTag);
//         if (synergyData == null) return "정보 없음";

//         var allItems = GetAllItems();
//         var tagCounts = CalculateTagCounts(allItems);
//         int itemCount = tagCounts.GetValueOrDefault(synergyTag, 0);
//         int level = synergyData.GetMaxLevel(itemCount);

//         if (level > 0)
//         {
//             return $"Lv.{level} ({itemCount}개)";
//         }
//         else
//         {
//             int needForLv1 = synergyData.thresholds.Length > 0 ? synergyData.thresholds[0] : 2;
//             return $"비활성 ({itemCount}/{needForLv1}개)";
//         }
//     }

//     public bool IsItemOwned(ItemData item)
//     {
//         var ownedItems = GetAllItems();
//         return ownedItems.Contains(item);
//     }
//     //////////////////////////////////////////
//     /*          시너지 메인 패널 UI         */ //------------------------------------> 여기부터 시작
//     //////////////////////////////////////////
    
//     public void UpdateSynergyUI()
//     {
//         if (_synergyContainer == null) 
//         {
//             ShowNoSynergyMessage(true);
//             return;
//         }

//         List<ItemData> allItems = GetAllItems();
//         Dictionary<string, int> tagCounts = CalculateTagCounts(allItems);
//         Dictionary<string, int> activeSynergies = GetActiveSynergies(tagCounts);

//         HashSet<string> allSynergyTags = new HashSet<string>(tagCounts.Keys);
//         UpdateSynergyCount(activeSynergies.Count, allSynergyTags.Count);

// // 시너지가 하나도 없으면 메시지 표시
//         if (allSynergyTags.Count == 0)
//         {
//             ShowNoSynergyMessage(true);
//             ClearSynergyElements();
//             return;
//         }

// // 시너지가 있으면 메시지 숨기고 모든 시너지 표시
//         ShowNoSynergyMessage(false);
//         UpdateAllSynergyElements(allSynergyTags, activeSynergies, tagCounts, allItems);
//     }

//     // 시너지 메소드 (3) - 시너지 개수 텍스트 업데이트
//     private void UpdateSynergyCount(int activeCount, int totalCount)
//     {
//         if (_synergyCountText != null)
//         {
//             _synergyCountText.text = $"시너지: {activeCount}/{totalCount} 활성화";
//         }
//     }

//     private void UpdateSynergyElement(string synergyTag, int level, int currentCount, List<ItemData> allItems, bool isActive)
//     {
//         if (!_synergyUIElements.ContainsKey(synergyTag)) return;

//         var synergyItem = _synergyUIElements[synergyTag];
        
//         // [중요] 활성화 상태에 따른 클래스 추가/제거
//         if (isActive)
//         {
//             synergyItem.AddToClassList("active");
//         }
//         else
//         {
//             synergyItem.RemoveFromClassList("active");
//         }

//         // [새로 추가] SynergyData에서 색상 정보 가져와서 적용
//         ApplySynergyBorderColor(synergyItem, synergyTag, level, isActive);

//         // 레벨 업데이트
//         var levelLabel = synergyItem.Q<Label>(className: "synergy-level");
//         if (levelLabel != null)
//         {
//             if (isActive)
//             {
//                 levelLabel.text = $"LV.{level}";
//                 levelLabel.AddToClassList("active");
//             }
//             else
//             {
//                 levelLabel.text = "대기";
//                 levelLabel.RemoveFromClassList("active");
//             }
//         }

//         // [수정] SynergyData의 실제 thresholds를 사용한 진행도 업데이트
//         var progressText = synergyItem.Q<Label>(className: "synergy-progress-text");
//         var progressFill = synergyItem.Q<VisualElement>(className: "synergy-progress-fill");
        
//         if (progressText != null && progressFill != null)
//         {
//             var synergyData = GetSynergyDataByTag(synergyTag);
//             if (synergyData != null)
//             {
//                 int nextThreshold;
                
//                 if (isActive && level < synergyData.thresholds.Length)
//                 {
//                     // 활성화된 상태: 다음 레벨까지
//                     nextThreshold = synergyData.thresholds[level]; // level은 0부터 시작하므로 다음 레벨 임계값
//                 }
//                 else if (!isActive && synergyData.thresholds.Length > 0)
//                 {
//                     // 비활성화 상태: 첫 번째 레벨까지
//                     nextThreshold = synergyData.thresholds[0];
//                 }
//                 else
//                 {
//                     nextThreshold = 0; // 최고 레벨이거나 thresholds가 없음
//                 }
                
//                 if (nextThreshold > 0)
//                 {
//                     float progress = Mathf.Clamp01((float)currentCount / nextThreshold);
//                     progressText.text = $"{currentCount} / {nextThreshold}";
//                     progressFill.style.width = Length.Percent(progress * 100);
//                 }
//                 else
//                 {
//                     progressText.text = $"{currentCount}";
//                     progressFill.style.width = Length.Percent(100);
//                 }
//             }
//         }

//     }

//     private void RemoveSynergyElement(string synergyTag)
//     {
//         if (_synergyUIElements.ContainsKey(synergyTag))
//         {
//             var element = _synergyUIElements[synergyTag];
//             if (element.parent != null)
//             {
//                 element.parent.Remove(element);
//             }
//             _synergyUIElements.Remove(synergyTag);
//         }
//     }
//     private void ClearSynergyElements()
//     {
//         foreach (var kvp in _synergyUIElements)
//         {
//             if (kvp.Value.parent != null)
//             {
//                 kvp.Value.parent.Remove(kvp.Value);
//             }
//         }
//         _synergyUIElements.Clear();
//     }

//     private void RearrangeSynergyContainer(List<SynergyDisplayInfo> sortedList)
//     {
//         if (_synergyContainer == null) return;
        
//         if (enableSynergyOrderDebug)
//             Debug.Log("=== UI 컨테이너 순서 재정렬 시작 ===");
        
//         // 1. 기존 UI 요소들을 모두 컨테이너에서 제거 (딕셔너리는 유지)
//         foreach (var kvp in _synergyUIElements)
//         {
//             if (kvp.Value.parent != null)
//             {
//                 kvp.Value.RemoveFromHierarchy();
//             }
//         }
        
//         // 2. 정렬된 순서대로 다시 컨테이너에 추가
//         foreach (var info in sortedList)
//         {
//             if (_synergyUIElements.ContainsKey(info.synergyTag))
//             {
//                 var synergyElement = _synergyUIElements[info.synergyTag];
//                 _synergyContainer.Add(synergyElement);
                
//                 if (enableSynergyOrderDebug)
//                     Debug.Log($"[재배치] {info.synergyTag} -> {(info.isActive ? "활성" : "비활성")} Lv.{info.level}");
//             }
//         }
        
//         if (enableSynergyOrderDebug)
//             Debug.Log("=== UI 컨테이너 순서 재정렬 완료 ===");
//     }

//     private void ShowNoSynergyMessage(bool show)
//     {
//         if (_noSynergyMessage != null)
//         {
//             _noSynergyMessage.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
//         }
//     }

//     private void ApplySynergyBorderColor(VisualElement synergyItem, string synergyTag, int level, bool isActive)
//     {
//         if (_synergyManager == null) return;

//         // SynergyManager에서 해당 시너지 데이터 찾기
//         var synergyData = GetSynergyDataByTag(synergyTag);
//         if (synergyData == null) return;

//         if (isActive && level > 0)
//         {
//             // 활성화된 경우: SynergyEffect에서 설정한 색상 사용
//             var effect = synergyData.GetEffect(level);
//             if (effect != null)
//             {
//                 // Unity Color를 UI Toolkit의 스타일로 적용
//                 synergyItem.style.borderLeftColor = effect.borderColor;
//                 synergyItem.style.borderRightColor = effect.borderColor;
//                 synergyItem.style.borderTopColor = effect.borderColor;
//                 synergyItem.style.borderBottomColor = effect.borderColor;
//             }
//         }
//         else
//         {
//             // 비활성화된 경우: 기본 회색 색상
//             Color inactiveColor = new Color(0.67f, 0.67f, 0.67f, 1f); // #AAA
//             synergyItem.style.borderLeftColor = inactiveColor;
//             synergyItem.style.borderRightColor = inactiveColor;
//             synergyItem.style.borderTopColor = inactiveColor;
//             synergyItem.style.borderBottomColor = inactiveColor;
//         }
//     }

//     //////////////////////////////////////////
//     /*          시너지 컨텍스트 메뉴 UI         */
//     //////////////////////////////////////////

//     public void OnSynergyItemClicked(PointerDownEvent evt)
//     {
//         evt.StopPropagation();
        
//         var synergyElement = evt.currentTarget as VisualElement;
//         if (synergyElement == null || synergyElement.userData == null) return;

//         string synergyTag = synergyElement.userData as string;
//         if (string.IsNullOrEmpty(synergyTag)) return;

//         // 기존 컨텍스트 메뉴 닫기
//         CloseContextMenu();
//         CloseSynergyContextMenu();

//         // 시너지 컨텍스트 메뉴 표시
//         ShowSynergyContextMenu(synergyTag, evt.position);
//     }

//     private void CreateSynergyContextMenu()
//     {
//         if (_synergyContextMenu != null) return; // 이미 생성됨

//         _synergyContextMenu = new VisualElement();
//         _synergyContextMenu.AddToClassList("synergy-context-menu");
//         _synergyContextMenu.style.position = Position.Absolute;
//         _synergyContextMenu.style.display = DisplayStyle.None;
//         // zIndex는 UI Toolkit에서 직접 지원하지 않으므로 제거

//         // 클릭 이벤트가 부모로 전파되지 않도록 차단
//         _synergyContextMenu.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());

//         _root.Add(_synergyContextMenu);
//     }

//     private void ShowSynergyContextMenu(string synergyTag, Vector2 position)
//     {
//         _selectedSynergyTag = synergyTag;

//         // 컨텍스트 메뉴가 없으면 생성
//         CreateSynergyContextMenu();

//         // 내용 업데이트
//         UpdateSynergyContextMenuContent();

//         // 위치 설정
//         _synergyContextMenu.style.left = position.x;
//         _synergyContextMenu.style.top = position.y;

//         // 화면 경계 체크 및 조정
//         AdjustSynergyContextMenuPosition();

//         // 표시
//         _synergyContextMenu.style.display = DisplayStyle.Flex;

//         Debug.Log($"[시너지 컨텍스트 메뉴] {synergyTag} 메뉴 표시");
//     }

//     public void CreateSynergyElement(string synergyTag)
//     {
//         if (_synergyContainer == null) return;

//         var synergyItem = new VisualElement();
//         synergyItem.AddToClassList("synergy-item");
//         synergyItem.AddToClassList(synergyTag.ToLower()); // 시너지별 색상을 위한 클래스

//         // [추가] 시너지 태그를 userData에 저장 (클릭 시 식별용)
//         synergyItem.userData = synergyTag;

//         // [추가] 시너지 컨테이너 클릭 이벤트 등록
//         synergyItem.RegisterCallback<PointerDownEvent>(OnSynergyItemClicked);

//         // 헤더 (아이콘 + 이름 + 레벨)
//         var header = new VisualElement();
//         header.AddToClassList("synergy-header");

//         var icon = new VisualElement();
//         icon.AddToClassList("synergy-icon");
//         icon.AddToClassList(synergyTag.ToLower()); // 시너지별 아이콘 색상
        
//         var nameLabel = new Label(GetSynergyDisplayName(synergyTag));
//         nameLabel.AddToClassList("synergy-name");
        
//         var levelLabel = new Label();
//         levelLabel.AddToClassList("synergy-level");

//         header.Add(icon);
//         header.Add(nameLabel);
//         header.Add(levelLabel);

//         // 진행도 컨테이너
//         var progressContainer = new VisualElement();
//         progressContainer.AddToClassList("synergy-progress-container");

//         var progressText = new Label();
//         progressText.AddToClassList("synergy-progress-text");

//         var progressBar = new VisualElement();
//         progressBar.AddToClassList("synergy-progress-bar");

//         var progressFill = new VisualElement();
//         progressFill.AddToClassList("synergy-progress-fill");
//         progressBar.Add(progressFill);

//         progressContainer.Add(progressText);
//         progressContainer.Add(progressBar);

//         // 효과 텍스트
//         var effectText = new Label();
//         effectText.AddToClassList("synergy-effect-text");

//         // 다음 레벨 정보
//         var nextLevelText = new Label();
//         nextLevelText.AddToClassList("synergy-next-level");

//         // [추가] 비활성화 상태 라벨


//         synergyItem.Add(header);
//         synergyItem.Add(progressContainer);
//         synergyItem.Add(effectText);
//         synergyItem.Add(nextLevelText);

//         _synergyContainer.Add(synergyItem);
//         _synergyUIElements[synergyTag] = synergyItem;
//     }
//     //////////////////////////////////////////
//     /*              헬퍼 & 유틸리티            */
//     //////////////////////////////////////////

//     private string GetSynergyDisplayName(string tag)
//     {
//         var synergyData = GetSynergyDataByTag(tag);
        
//         if (synergyData != null && !string.IsNullOrEmpty(synergyData.displayName))
//         {
//             return synergyData.displayName;
//         }
        
//         return tag; // 못 찾으면 태그 그대로 반환
//     }

//     private string GetLevelEffectDescription(SynergyEffect effect)
//     {
//         List<string> effects = new List<string>();

//         if (effect.hpBonus != 0)
//             effects.Add($"체력 {(effect.hpBonus > 0 ? "+" : "")}{effect.hpBonus}");

//         if (effect.physicalAttackBonus != 0)
//             effects.Add($"물리 공격 {(effect.physicalAttackBonus > 0 ? "+" : "")}{effect.physicalAttackBonus}");

//         if (effect.magicalAttackBonus != 0)
//             effects.Add($"마법 공격 {(effect.magicalAttackBonus > 0 ? "+" : "")}{effect.magicalAttackBonus}");

//         if (effect.defenseBonus != 0)
//             effects.Add($"방어력 {(effect.defenseBonus > 0 ? "+" : "")}{effect.defenseBonus}");

//         if (effect.criticalChanceBonus != 0)
//             effects.Add($"치명타 확률 {(effect.criticalChanceBonus > 0 ? "+" : "")}{effect.criticalChanceBonus}%");

//         if (effect.attackSpeedBonus != 0)
//             effects.Add($"공격 속도 {(effect.attackSpeedBonus > 0 ? "+" : "")}{effect.attackSpeedBonus}%");

//         if (effect.moveSpeedBonus != 0)
//             effects.Add($"이동 속도 {(effect.moveSpeedBonus > 0 ? "+" : "")}{effect.moveSpeedBonus}%");

//         // 커스텀 설명이 있으면 추가
//         if (!string.IsNullOrEmpty(effect.effectDescription))
//             effects.Add(effect.effectDescription);

//         return effects.Count > 0 ? string.Join(", ", effects) : "효과 없음";
//     }

//     private void UpdateSynergyElement(string synergyTag, int level, int currentCount, List<ItemData> allItems, bool isActive)
//     {
//         if (!_synergyUIElements.ContainsKey(synergyTag)) return;

//         var synergyItem = _synergyUIElements[synergyTag];
        
//         // [중요] 활성화 상태에 따른 클래스 추가/제거
//         if (isActive)
//         {
//             synergyItem.AddToClassList("active");
//         }
//         else
//         {
//             synergyItem.RemoveFromClassList("active");
//         }

//         // [새로 추가] SynergyData에서 색상 정보 가져와서 적용
//         ApplySynergyBorderColor(synergyItem, synergyTag, level, isActive);

//         // 레벨 업데이트
//         var levelLabel = synergyItem.Q<Label>(className: "synergy-level");
//         if (levelLabel != null)
//         {
//             if (isActive)
//             {
//                 levelLabel.text = $"LV.{level}";
//                 levelLabel.AddToClassList("active");
//             }
//             else
//             {
//                 levelLabel.text = "대기";
//                 levelLabel.RemoveFromClassList("active");
//             }
//         }

//         // [수정] SynergyData의 실제 thresholds를 사용한 진행도 업데이트
//         var progressText = synergyItem.Q<Label>(className: "synergy-progress-text");
//         var progressFill = synergyItem.Q<VisualElement>(className: "synergy-progress-fill");
        
//         if (progressText != null && progressFill != null)
//         {
//             var synergyData = GetSynergyDataByTag(synergyTag);
//             if (synergyData != null)
//             {
//                 int nextThreshold;
                
//                 if (isActive && level < synergyData.thresholds.Length)
//                 {
//                     // 활성화된 상태: 다음 레벨까지
//                     nextThreshold = synergyData.thresholds[level]; // level은 0부터 시작하므로 다음 레벨 임계값
//                 }
//                 else if (!isActive && synergyData.thresholds.Length > 0)
//                 {
//                     // 비활성화 상태: 첫 번째 레벨까지
//                     nextThreshold = synergyData.thresholds[0];
//                 }
//                 else
//                 {
//                     nextThreshold = 0; // 최고 레벨이거나 thresholds가 없음
//                 }
                
//                 if (nextThreshold > 0)
//                 {
//                     float progress = Mathf.Clamp01((float)currentCount / nextThreshold);
//                     progressText.text = $"{currentCount} / {nextThreshold}";
//                     progressFill.style.width = Length.Percent(progress * 100);
//                 }
//                 else
//                 {
//                     progressText.text = $"{currentCount}";
//                     progressFill.style.width = Length.Percent(100);
//                 }
//             }
//         }

//         // [수정] SynergyData의 effectDescription 사용
//         var effectText = synergyItem.Q<Label>(className: "synergy-effect-text");
//         if (effectText != null)
//         {
//             if (isActive)
//             {
//                 var synergyData = GetSynergyDataByTag(synergyTag);
//                 if (synergyData != null)
//                 {
//                     var effect = synergyData.GetEffect(level);
//                     if (effect != null && !string.IsNullOrEmpty(effect.effectDescription))
//                     {
//                         effectText.text = effect.effectDescription;
//                     }
//                     else
//                     {
//                         effectText.text = GetSynergyEffectDescription(synergyTag, level); // 기본값
//                     }
//                 }
//                 else
//                 {
//                     effectText.text = GetSynergyEffectDescription(synergyTag, level); // 기본값
//                 }
//                 effectText.AddToClassList("active");
//             }
//             else
//             {
//                 effectText.text = "아직 발동되지 않음";
//                 effectText.RemoveFromClassList("active");
//             }
//         }

//         // [수정] SynergyData 기반 다음 레벨 정보
//         var nextLevelText = synergyItem.Q<Label>(className: "synergy-next-level");
//         if (nextLevelText != null)
//         {
//             var synergyData = GetSynergyDataByTag(synergyTag);
//             if (synergyData != null)
//             {
//                 int itemsNeeded = synergyData.GetItemsNeededForNextLevel(currentCount);
                
//                 if (itemsNeeded > 0)
//                 {
//                     if (isActive)
//                     {
//                         nextLevelText.text = $"다음 레벨까지 {itemsNeeded}개 더 필요";
//                     }
//                     else
//                     {
//                         nextLevelText.text = $"발동까지 {itemsNeeded}개 더 필요";
//                     }
//                     nextLevelText.style.display = DisplayStyle.Flex;
//                 }
//                 else if (isActive)
//                 {
//                     nextLevelText.text = "최고 레벨 달성";
//                     nextLevelText.style.display = DisplayStyle.Flex;
//                 }
//                 else
//                 {
//                     nextLevelText.style.display = DisplayStyle.None;
//                 }
//             }
//             else
//             {
//                 nextLevelText.style.display = DisplayStyle.None;
//             }
//         }

//         // [추가] 비활성화 라벨 표시/숨김
//         var inactiveLabel = synergyItem.Q<Label>(className: "synergy-inactive-label");
//         if (inactiveLabel != null)
//         {
//             inactiveLabel.style.display = isActive ? DisplayStyle.None : DisplayStyle.Flex;
//         }
//     }

//     //////////////////////////////////////////
//     /*                  디버깅               */
//     //////////////////////////////////////////
    

// }
