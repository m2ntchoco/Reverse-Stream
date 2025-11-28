using UnityEngine;
using System.Collections;

public class SlashEffect : MonoBehaviour
{
    public float duration = 0.3f;      // ����Ʈ ��� �ð�
    public int damage = 20;            // ������ ��
    EnemyAI enemyAI;
    private void Start()
    {
        Destroy(gameObject, duration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var enemyAI = other.GetComponent<EnemyAI>();
        if (enemyAI != null)
        {
            enemyAI.TakeDamage(damage);
        }
    }

}
