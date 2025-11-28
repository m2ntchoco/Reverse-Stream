// PlayerAnimationEvents.cs (�ִϸ����� ������Ʈ�� ���̱�)
using UnityEngine;

public class AttackEvent : MonoBehaviour
{
    private Attack_Damage attack;
    //private Magic_Attack magicattack;

    void Start()
    {
        attack = GetComponentInParent<Attack_Damage>();
        //magicattack = GetComponentInParent<Magic_Attack>();
    }

    public void NomalAttackEvent()
    {
        attack?.NormalAttack(); // ���� ���� ����
    }
    public void DownCommandEvent()
    {
        attack?.DownCommand();
    }
    public void SideCommnadEvent()
    {
        attack?.SideCommand();
    }
    public void SideCommand_SwordAura()
    {
        //magicattack.UseSlashSkill();
    }

}
