using UnityEngine;

public class Thorn:MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer; // Inspector ���� Enemy ���̾� ����
    EnemyAI enemyAI;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                Debug.Log("rktlrktl");
                player.TakeDamage(5, transform, 0f,0f);
            }
        }
        else if ((enemyLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            // Ʈ���� �ݶ��̴��� �ڽĿ� �پ����� �� ������ �θ𿡼� ������Ʈ �˻�
            var enemyAI = other.GetComponentInParent<EnemyAI>();
            if (enemyAI != null)
            {
                // ��: 10 �����, �˹� 0, 0
                enemyAI.TakeDamage(100);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                Debug.Log("rktlrktl");
                player.TakeDamage(5, transform, 0f, 0f);
            }
        }
        else if ((enemyLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            // Ʈ���� �ݶ��̴��� �ڽĿ� �پ����� �� ������ �θ𿡼� ������Ʈ �˻�
            var enemyAI = other.GetComponentInParent<EnemyAI>();
            if (enemyAI != null)
            {
                // ��: 10 �����, �˹� 0, 0
                enemyAI.TakeDamage(10);
            }
        }
    }


}
