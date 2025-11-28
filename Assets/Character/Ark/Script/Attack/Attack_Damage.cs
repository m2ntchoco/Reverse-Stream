using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using System.Security.Cryptography.X509Certificates;

public class Attack_Damage : MonoBehaviour
{
    [Header("���� ����")]
    [SerializeField] private Transform attackPoint;          // �⺻ ���� ���� ��ġ
    [SerializeField] private float attackRange = 0.5f;       // �⺻ ���� �ݰ�
    [SerializeField] private LayerMask enemyLayer;           // �� ���̾� ����
    [SerializeField] public int BasicattackDamage = 30;          // �⺻ ���� ������
    [SerializeField] public int BasicstrongAttackDamage = 100;    // ������(��¡) ������
    [SerializeField] public int BasicstrongUPAttackDamage = 60;    // ������(��¡) ������
    [SerializeField] public int OverHeatBounsDamage = 2;    // ������(��¡) ������
    public static float CriticalDamage = 0;
    public static float CritChance = 0;
    public static float MaxHPDamage = 0;
    public static float SoulBuffCriticalDamage = 0;
    public static float SoulBuffCritChance = 0;
    public static int Critrand;
    private float weaponcirdmg = 0f;
    private float luckstat;

    // ===== ���ο��� ����� ���� =====
    //public WeaponStatPackage currentWeaponData;
    private SteamPressureSystem steampunk;

    private void Awake()
    {
        steampunk = GetComponent<SteamPressureSystem>();
        if (gameObject.name == "Ark/mana")
        {
            OverHeatBounsDamage = 1;
        }
    }

    private void Update()
    {
        /*BasicattackDamage += + StrStat;
        BasicstrongAttackDamage += StrStat;
        BasicstrongUPAttackDamage += StrStat;
        Debug.Log($"�ҿ� ������ ���� ũ��Ƽ�� ������ : {SoulBuffCriticalDamage}, �ҿ� ������ ���� ũ��Ƽ�� Ȯ�� : {SoulBuffCritChance}");*/
        luckstat = Ark_stat.luck;
        //weaponcirdmg = (currentWeaponData.critDamage / 100) - 1; 
        //CritChance = (luckstat * 0.1f) + currentWeaponData.critChance + SoulBuffCritChance; 
        //Debug.Log($"���� �������� ���� ������ {currentWeaponData.attackPower}");
    }

    // ���� �޼���
    private void AttackWithModifiers(float baseAttackDamage)
    {
        // 1) ���̽� + ���� + MaxHP ������
        float damage = baseAttackDamage;
                     //+ MaxHPDamage
                     //+ currentWeaponData.attackPower + (Ark_stat.strength * 2);

        // 2) ���� ���
        damage *= GetBuffMultiplier();

        // 3) ũ��Ƽ�� ���
        Critrand = UnityEngine.Random.Range(0, 100); //ũ��Ƽ�� Ȯ�� ����� ���� ���� �� �ޱ�
        if(CritChance >= Critrand)
        {
            damage *= 1 + (weaponcirdmg + SoulBuffCriticalDamage);
            Debug.Log("ũ���߻�");
        }

        // 4) ������Ʈ ��� (������Ʈ ������ ����)
        if (gameObject.name == "Ark/SteamPunk")
        {
            if (steampunk.isOverheated)
                damage *= OverHeatBounsDamage;
        }

        // 5) ���� ����
        DoAttack(attackPoint.position, attackRange, damage);
    }
    public void NormalAttack()
    {
        AttackWithModifiers(BasicattackDamage);
    }
    public void DownCommand()
    {
        AttackWithModifiers(BasicstrongAttackDamage);
    }
    public void SideCommand()
    {
        AttackWithModifiers(BasicstrongUPAttackDamage);
    }

    private float GetBuffMultiplier()
    {
        //if (TryGetComponent<SoulBuffAttack>(out var buff) && buff.IsBuffActive)
        {
            //return buff.BuffMultiplier;
        }
        //Debug.Log("[���� Ȯ��] ���� ������ ���� ��Ȱ��ȭ �� �⺻ ��� 1.0");
        return 1f;
    }
    private void DoAttack(Vector2 point, float range, float damage)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(point, range, enemyLayer);
        foreach (var col in hitEnemies)
        {
            /*if (col.TryGetComponent<MonsterHP>(out MonsterHP monster))
            {
                //Debug.Log($"�� ������: {damage} ü�� ��� ������ ���� : {MaxHPDamage}");
                Debug.Log("�� ����");
                monster.Getdamage(damage);
                //Debug.Log($"[PerformAttack] ���� ������: {damage}, ���: {GetBuffMultiplier()}");
                Debug.Log(damage);
            }
            else if (col.TryGetComponent<MagicGoblinAI>(out MagicGoblinAI goblin))
            {
                //Debug.Log($"�� ������: {damage} ü�� ��� ������ ���� : {MaxHPDamage}");
                goblin.TakeDamage(damage);
                Debug.Log($"[PerformAttack] ���� ������: {damage}, ���: {GetBuffMultiplier()}");
                Debug.Log(damage);
            }
            else if (col.TryGetComponent<SandWorm_FSM>(out SandWorm_FSM Sandworm))
            {
                Debug.Log("����� ����");
                Sandworm.Getdamage(damage);
                
                Debug.Log(damage);
            }
            else if (col.TryGetComponent<Boss1_FSM>(out Boss1_FSM boss1))
            {
                Debug.Log("����� ����");
                boss1.Getdamage(damage);
                Debug.Log(damage);
            } 삭제*/
        // 1. (NEW) 모든 '표준 몬스터'는 EnemyAI 스크립트를 가짐
        // MonsterHP, MagicGoblinAI 등을 모두 이것 하나로 대체합니다.
        if (col.TryGetComponent<EnemyAI>(out EnemyAI enemy))
        {
            Debug.Log("표준 몬스터(" + enemy.name + ") 공격!");
            enemy.TakeDamage(damage); // 우리가 설계한 표준 함수 호출
        }
        // 2. (유지) 보스는 별도의 FSM을 사용하므로 그대로 둡니다.
        else if (col.TryGetComponent<SandWorm_FSM>(out SandWorm_FSM Sandworm))
        {
            Debug.Log("샌드웜 공격!");
            Sandworm.Getdamage(damage);
        }
        else if (col.TryGetComponent<Boss1_FSM>(out Boss1_FSM boss1))
        {
            Debug.Log("보스1 공격!");
            boss1.Getdamage(damage);
        }
        }
    }

    public void SetWeaponStats(/*WeaponStatPackage stats*/)
    {
        //currentWeaponData = stats;
    }
    private void OnDrawGizmosSelected()
    {
        // attackPoint가 할당되지 않았으면 그리지 않음
        if (attackPoint == null)
        {
            Debug.Log("AttackPoint 할당되지 않았음");
            return;
        }

        // 공격 범위를 빨간색 원으로 그림
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

}
