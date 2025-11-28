using UnityEngine;

public class AttackState : IEnemyState
{
    private float StateCoolDown = 1f;
    private int decision = -1;
    private EnemyAI enemy;
    private MonsterAnimatorController ani;


    public AttackState(EnemyAI enemy, MonsterAnimatorController ani)
    {
        this.enemy = enemy;
        this.ani = ani;
        ani.SetAttackState(this);

    }

    public void Enter()
    {
        enemy.PrepareForAttack();
        enemy.GetRigidbody().linearVelocity = new Vector2(0, enemy.GetRigidbody().linearVelocity.y);
        Debug.Log("�����غ���");

    }

    public void Update()
    {
        if (enemy.canAttack)
        {
            /*if (enemy.CompareTag("DwarfBuster")) BusterAttack();
            else if(enemy.CompareTag("Wolf")) WolfAttack();
            else ExecuteAttack(); 조건 분기 삭제*/
            if(!enemy.justAlerted)
            {
                enemy.AlertNearbyAllies(); // 경보 발령
            }
            // '데이터'에 저장된 공격 타입으로 분기
            // 만약 공격타입 추가할때는 SpeciesData.cs의 AttackPatternType의 enum도 수정하고 여기에도 추가할것!
            switch (enemy.speciesData.attackType)
            {
                case AttackPatternType.Buster:
                    BusterAttack();
                    break;
                case AttackPatternType.Wolf:
                    WolfAttack();
                    break;
                case AttackPatternType.Standard:
                default:
                    ExecuteAttack();
                    break;
            }
        }

        if (!enemy.IsPlayerAttackable()) enemy.ChangeState(enemy.chaseState);
    }

    private void ExecuteAttack()
    {
        enemy.canAttack = false;
        enemy.GetRigidbody().linearVelocity = Vector2.zero;
        ani.SetMoving(false);
        enemy.FacetoPlayer();

        // '공격성' (1~10)이 SAttack 또는 Backstep 확률 결정
        float roll = Random.Range(0f, 10f); // 0.0 ~ 10.0

        // 1. 공격성 체크 통과 (SAttack 또는 Backstep)
        if (roll < enemy.speciesData.aggression)
        {
            if (Random.value > 0.5f) // 50% 확률로
            {
                // SAttack (특수 공격)
                ani.SAttack();
                // 쿨타임도 데이터에서 읽어옴 (SAttack은 1.5배)
                enemy.cooldown = enemy.speciesData.attackCooldown * 1.5f; 
            }
            else
            {
                // Backstep
                enemy.Backstep(); // (BackstepForce도 SpeciesData에 정의해야 함)
                enemy.cooldown = enemy.speciesData.attackCooldown * 1.2f;
            }
        }
        // 2. 공격성 체크 실패 (NAttack)
        else
        {
            ani.NAttack();
            enemy.cooldown = enemy.speciesData.attackCooldown; // 기본 쿨타임
        }
    }
    private void BusterAttack()
    {
        enemy.canAttack = false;
        
        // '공격성'이 높을수록 돌진(SAttack) 확률 증가
        float roll = Random.Range(0f, 10f);

        if (roll < enemy.speciesData.aggression)
        {
            // SAttack (돌진)
            ani.Chaging();
            enemy.StartBusterCharge(); // (cooldown은 BusterCharge 코루틴에서 설정)
        }
        else
        {
            // NAttack
            enemy.GetRigidbody().linearVelocity = Vector2.zero;
            ani.SetMoving(false);
            ani.NAttack();
            enemy.cooldown = enemy.speciesData.attackCooldown;
        }

    }
    private void WolfAttack()
    {
        // 늑대는 (설정상) 항상 SAttack(점프 공격)만 함
        enemy.canAttack = false;
        enemy.GetRigidbody().linearVelocity = Vector2.zero;
        enemy.FacetoPlayer();
        enemy.Jumpoo();
        ani.NAttack(); // (애니메이션 이름이 NAttack이지만 WolfAttack 전용일 수 있음)
        enemy.cooldown = enemy.speciesData.attackCooldown * 1.2f;
    }

    public void Exit()
    {

    }

}
