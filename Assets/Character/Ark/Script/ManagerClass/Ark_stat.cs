using System.Collections;
using System;
using UnityEngine;

[System.Serializable]
public static class Ark_stat
{
    public static int health;
    public static int strength;
    public static int dexterity;
    public static int intelligence;
    public static int luck;
    public static int remainingStatPoints;

    public static event Action StatsChanged;   // �� �߰�

    // --- ���� �ӵ� �� ---
    // ���/���� ������ �����Ǵ� "�⺻ ����"
    public static float BaseAttackSpeed = 1.3f;

    // ����/�нú� ��� �����ϴ� "���� ���" (AttackSpeed.cs�� �� ���� �ǵ帲: 1.10~1.60 ��)
    public static float attackSpeedMultiplier = 1.0f;

    // (����) ����, ���� ���� �ܺ� �Ͻù��� ���
    public static float externalBuffMultiplier = 1.0f;

    // ���� ������ �׻� ���⼭�� ����ؼ� �ѱ�
    public static float GetAttackSpeed()
    {
        return BaseAttackSpeed * attackSpeedMultiplier * externalBuffMultiplier;
    }

    // ���� ����� ����(���� ������ ȣ��)
    public static void SetAttackSpeedMultiplier(float multiplier)
    {
        attackSpeedMultiplier = Mathf.Max(0.0001f, multiplier);
    }

    // �ܺ� ���� ��� ����(�ʿ� ������ �� �ᵵ ��)
    public static void SetExternalBuffMultiplier(float multiplier)
    {
        externalBuffMultiplier = Mathf.Max(0.0001f, multiplier);
    }

    // ���� ���� ����: ������� 1��, Base�� ����(��� �ٲٴ� ��)
    public static void ResetAttackSpeed()
    {
        attackSpeedMultiplier = 1.0f;
        externalBuffMultiplier = 1.0f;
        // BaseAttackSpeed�� ���/���� �ý��ۿ��� ����
    }

    // ====== ����/�ε� ======
    public static void LoadFrom(statSaveData data)
    {
        health = data.health;
        strength = data.strength;
        dexterity = data.dexterity;
        intelligence = data.intelligence;
        luck = data.luck;                    // �� �����Ǿ� �ִ� �κ� �߰�
        remainingStatPoints = data.remainingStatPoints;

        // BaseAttackSpeed/����� ���� ���/������ ���� ������. �ʿ� �� data�� ���Խ��ѵ� ��.
    }

    public static void ApplyTo(statSaveData data)
    {
        data.health = health;
        data.strength = strength;
        data.dexterity = dexterity;
        data.intelligence = intelligence;
        data.luck = luck;                    // �� �����Ǿ� �ִ� �κ� �߰�
        data.remainingStatPoints = remainingStatPoints;

        // �ʿ� �� BaseAttackSpeed � ����
    }

    public static void ResetStats()
    {
        health = 0;
        strength = 0;
        dexterity = 0;
        intelligence = 0;
        luck = 0;
        remainingStatPoints = 20;
        Debug.Log("�÷��̾� ������ �ʱ�ȭ�Ǿ����ϴ�.");
    }

    

    public enum StatType { Health, Strength, Dexterity, Intelligence, Luck }

    public static bool IncreaseStat(StatType type)
    {
        if (remainingStatPoints <= 0) return false;

        switch (type)
        {
            case StatType.Health: health++; break;
            case StatType.Strength: strength++; break;
            case StatType.Dexterity: dexterity++; break;
            case StatType.Intelligence: intelligence++; break;
            case StatType.Luck: luck++; break;
        }

        remainingStatPoints--;
        SaveManager.Instance.SaveNow();
        StatsChanged?.Invoke();  // �� ���� �ٲ�� �˸� �߻�
        return true;
    }
}
