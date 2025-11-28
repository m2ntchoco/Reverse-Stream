using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEditor;
using Unity.VisualScripting;

public class SteamPunk_Attack : MonoBehaviour, IAttackAnimEvents
{
    [Header("��Ÿ ����")]
    public float step1Distance = 0.35f;
    public float step1Duration = 0.08f;
    public float step2Distance = 0.28f;
    public float step2Duration = 0.07f;
    public AnimationCurve stepCurve; // �ν����Ϳ��� ������ EaseOut Ŀ�� ���� ����

    public float requiredHoldTime = 0.35f; // ��¡ ������ ��¡ �ð�
    private const float maxCommandWindow = 1f;  // Ŀ�ǵ� �Է� ���� �ִ� �ð� (��: 0.5��)

    //Attack
    public float m_timeSinceAttack = 0.0f;
    public int m_currentAttack = 0;
    public bool isCommandWindow = false;         // 2��° ���� �� Ŀ�ǵ� �Է� ��� ������ ����
    public float commandWindowTimer = 0f;        // Ŀ�ǵ� �Է� ���� �ð� ������ Ÿ�̸�
    public float BonusDamage = 1f;

    //speed
    public float SpeedMulti = 1.2f;
    public static bool AttackCountReady = false; //������ �ߵ��ߴ��� Ȯ���ϴ� bool��

    //����
    private SteamPressureSystem steamSystem;
    private PlayerAnimationSync sync;
    private Player_move move;

    void Awake()
    {
        sync = GetComponentInParent<PlayerAnimationSync>();
        steamSystem = GetComponentInParent<SteamPressureSystem>();
        move = GetComponentInParent<Player_move>();
        //AttackSpeed.RegisterRunner(this);  // ���� ��ü ���
    }

    private void Update()
    {
        // ���� [�ִϸ��̼� ���] ���� ���� ���(Attack1/Attack2)�� ������ ������ ��� �Է� ����
        var state = sync.CurrentStateInfo;
        bool inAttackAnim = (state.IsName("Attack 1") || state.IsName("Attack 2") || state.IsName("OverHit_Attack 1")
            || state.IsName("OverHit_Attack 2") || state.IsName("OverHit_Attack1") || state.IsName("Side_Command") || state.IsName("Down_Command"));
        if (inAttackAnim && state.normalizedTime < 1f)
            return;

        // 1) �⺻ Ÿ�̸� ������Ʈ
        m_timeSinceAttack += Time.deltaTime;

        // 2) Ŀ�ǵ� ������ ó�� (3��° ���� �� �Է� ���)
        if (isCommandWindow)
        {
            // ������Ʈ ���°� �ƴϾ�� Ŀ�ǵ� �Է°� �ð� ����
            if (steamSystem != null && !steamSystem.isOverheated)
            {
                commandWindowTimer += Time.deltaTime;

                // 2-1) �ð� �ʰ� �� ������ ����
                if (commandWindowTimer >= maxCommandWindow)
                {
                    isCommandWindow = false;
                    m_currentAttack = 0;
                    m_timeSinceAttack = 0f;
                    return;
                }

                // 2-2) A Ű ���� �� �Ʒ� ����Ű ������ �Ʒ� ����
                if (Input.GetKeyDown(KeyCode.A))
                {
                    AttackCountReady = true;
                    //AttackSpeed.AttackSpeedUP();
                    //AttackSpeed.SpeedUPSize();
                    //f (TryGetComponent<SoulBuffAttack>(out var buff))
                    {
                        //buff.TryBuffAttack(); // ���� ���¸� ���� (return �� �ᵵ ��)
                    }

                    if (Input.GetKey(KeyCode.DownArrow))
                    {
                        Debug.Log("�Ʒ� ����");
                        move.AttackSpeedDownDuringAnimation(0.3f);
                        move.SetAttackLock(true);
                        sync.DownCommand();


                        isCommandWindow = false;
                        m_currentAttack = 0;
                        m_timeSinceAttack = 0f;
                        steamSystem.ApplyCommandSkill(steamSystem.pressureIncreasePerSkill_2);
                        return;
                    }

                    if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
                    {
                        Debug.Log("�� ����");
                        move.AttackSpeedDownDuringAnimation(0.3f);
                        move.SetAttackLock(true);
                        sync.SideCommand();

                        isCommandWindow = false;
                        m_currentAttack = 0;
                        m_timeSinceAttack = 0f;
                        steamSystem.ApplyCommandSkill(steamSystem.pressureIncreasePerSkill_1);
                        return;
                    }
                }
            }


            // Ŀ�ǵ� ������ ���̹Ƿ� �Ϲ� ���� ����
            return;
        }


        // 3) �Ϲ� �޺� �Է� ó�� (Ŀ�ǵ� ������ �ƴ� ���� A Ű�� ����)
        if (Input.GetKeyDown(KeyCode.A) && m_timeSinceAttack > 0.1f)
        {
            AttackCountReady = true;
            //AttackSpeed.AttackSpeedUP();
            //AttackSpeed.SpeedUPSize();
            //if (TryGetComponent<SoulBuffAttack>(out var buff))
            {
                //buff.TryBuffAttack(); // ���� ���¸� ���� (return �� �ᵵ ��)
            }

            m_currentAttack++;

            BonusDamage = 2f; //������Ʈ�� �Ǹ� ������ 2��

            // 3-1) �޺� ���� �ð� �ʰ� �� �ʱ�ȭ
            if (m_timeSinceAttack > 2.0f)
                m_currentAttack = 1;

            // 3-2) �޺��� 3�ܰ踦 �Ѿ�� 1�ܰ�� ��ȯ
            if (m_currentAttack > 2)
                m_currentAttack = 1;

            // 3-3) �ִϸ����� Ʈ���� �ߵ�
            if (steamSystem.isOverheated)
            {
                move.AttackSpeedDownDuringAnimation(0.3f);
                sync.ApplyAttackSpeed();
                sync.OverHitAttack(m_currentAttack);
                m_timeSinceAttack = 0f;
            }
            else
            {
                move.AttackSpeedDownDuringAnimation(0.3f);
                sync.ApplyAttackSpeed();
                sync.NomalAttack(m_currentAttack);
                m_timeSinceAttack = 0f;
            }

            // 3-4) 3��° ������ �ߵ����� ��� Ŀ�ǵ� ������ ����
            if (m_currentAttack == 2)
            {
                if (steamSystem.isOverheated)
                {
                    isCommandWindow = false;
                }
                else
                {
                    isCommandWindow = true;
                    commandWindowTimer = 0f;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            Debug.Log("��������");
            //Ark_stat.SetAttackSpeed(SpeedMulti); // ���� 20% ����
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            Debug.Log("���� �ʱ�ȭ");
            Ark_stat.ResetAttackSpeed();
        }
    }

    // === �ִϸ��̼� �̺�Ʈ�� ������ ���� ȣ�� ===
    public void OnStep(int index)
    {
        if (index == 1)
            move.StepForward(step1Distance, step1Duration, stepCurve);
        else if (index == 2)
            move.StepForward(step2Distance, step2Duration, stepCurve);
    }

    public void OnAttackEnd()
    {
        move.SetAttackLock(false);
    }

    public void OnSkillStart()
    {
        move.SetAttackLock(true);
    }

    public void OnSkillEnd()
    {
        move.SetAttackLock(false);
    }
}