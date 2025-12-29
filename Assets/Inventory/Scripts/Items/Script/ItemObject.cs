using UnityEngine;

// 이 스크립트를 프리팹에 붙이세요.
// 기존 내용을 모두 지우고 이 코드로 덮어씌워야 합니다.

[RequireComponent(typeof(SpriteRenderer))]
public class ItemObject : MonoBehaviour
{
    [Header("아이템 데이터 연결")]
    // 'ItemData' 클래스가 존재해야 에러가 안 납니다. (스크립터블 오브젝트)
    public ItemData data; 

    [Header("감지 및 상호작용")]
    public float detectRange = 1.5f; 
    public GameObject interactionHint; // F키 UI (선택)

    private Transform _playerTransform;
    private bool _isPlayerInRange = false;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) 
            _playerTransform = player.transform;
        
        if (interactionHint != null) 
            interactionHint.SetActive(false);

        UpdateVisual();
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        // 1. 거리 체크
        float distance = Vector2.Distance(transform.position, _playerTransform.position);
        _isPlayerInRange = distance <= detectRange;

        // 2. 힌트 UI 켜기/끄기
        if (interactionHint != null)
            interactionHint.SetActive(_isPlayerInRange);

        // 3. 상호작용 키 (KeyManager가 없으면 F키)
        KeyCode interactKey = KeyCode.F;
        if (KeyManager.Instance != null) 
            interactKey = KeyManager.Instance.KeyInteract;

        // 범위 안이고 키를 눌렀으면 획득
        if (_isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            CollectItem();
        }
    }

    private void CollectItem()
    {
        if (data == null)
        {
            Debug.LogError($"[ItemObject] {gameObject.name}에 ItemData가 연결되지 않았습니다!");
            return;
        }

        // 아이템 매니저에 추가 요청
        if (ItemDataManager.Instance != null)
        {
            // ItemDataManager의 AddItem 함수 호출
            ItemDataManager.Instance.AddItem(data.id);
            Debug.Log($"아이템 획득: {data.itemName}");
            
            // 필드에서 삭제
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("씬에 ItemDataManager가 없습니다!");
        }
    }

    // 에디터에서 데이터 변경 시 이미지 자동 업데이트
    private void OnValidate()
    {
        if (_spriteRenderer == null) 
            _spriteRenderer = GetComponent<SpriteRenderer>();
            
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (data != null && data.icon != null && _spriteRenderer != null)
        {
            _spriteRenderer.sprite = data.icon;
            gameObject.name = "Item_" + data.itemName;
        }
    }

    // 범위 눈으로 확인하기 (기즈모)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}