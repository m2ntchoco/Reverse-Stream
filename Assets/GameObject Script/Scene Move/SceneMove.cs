using System.Linq;
using UnityEngine;

public class SceneMove : MonoBehaviour
{
    [Header("������Ʈ ����")]
    [SerializeField] GameObject Player;             // �÷��̾� ������Ʈ
    [SerializeField] GameObject Portal;             // ��Ż ��ü
    [SerializeField] GameObject PortalImpact;       // ��Ż Ȱ��ȭ ����Ʈ

    [Header("�� �帧")]
    [SerializeField] SceneFlowLoader flowLoader;    // �ڵ� �� ��ȯ�� ���� SceneFlowLoader ����

    private int layerToFind;
    private bool layerExistsInScene;
    private bool isPlayerInPortalZone = false;

    void Start()
    {
        layerToFind = LayerMask.NameToLayer("enemy");
        UpdateEnemyPresence();
    }

    void Update()
    {
        UpdateEnemyPresence();

        // F Ű ������ �÷��̾ ��Ż ���� �ȿ� ������, ��Ż�� Ȱ��ȭ�Ǿ� ������ �� ��ȯ
        if (isPlayerInPortalZone && PortalImpact.activeInHierarchy && Input.GetKeyDown(KeyCode.F))
        {
            //SaveSystemManager.SaveOnSceneTransition(); //���� �ٲ��� ���� �ý��� ���հ��� �� �κи� ����
            flowLoader.LoadNextScene(); // ���⼭ ���� �� ��ȯ �߻�

        }
    }

    void UpdateEnemyPresence()
    {
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        layerExistsInScene = allObjects.Any(obj => obj.layer == layerToFind);
        int enemyCount = 0;

        foreach (var obj in allObjects)
        {
            if (obj.layer == layerToFind)
            {
                enemyCount++;
                Debug.Log($"Enemy Object: {obj.name}, Active: {obj.activeSelf}");
            }
        }

        layerExistsInScene = enemyCount > 0;
        Debug.Log($"[SceneMove] Enemy count: {enemyCount}, Portal Ȱ��ȭ ����: {!layerExistsInScene}");

        // Enemy�� ������ ��Ż Ȱ��ȭ
        PortalImpact.SetActive(!layerExistsInScene);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInPortalZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInPortalZone = false;
        }
    }
}
