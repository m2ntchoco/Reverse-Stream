using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System;

public class PlayerExpManager
{
    // �ܺ� �ý��� ����(�ٸ� ��ũ��Ʈ���� ���)
    public static PlayerHealth PlayerHealth;
    public static PlayerData PlayerData;

    // �ʱ� ���� �����ÿ��� �����ϴ� �⺻��(��Ÿ�� ������ ���� X)
    public static int currentLevel = 1;
    public static int currentExp = 0;
    public static int totalExp = 0;
    public static int expToNextLevel = 100;

    public static int MaxLevel = 50;

    // UI ���� �̺�Ʈ
    public static event Action OnExpChanged;                             // �ܼ� ���� Ʈ����
    public static event Action<int, int, int> OnExpChangedWithValue;     // (current, max, level)

    // �ʿ� ���ٸ� ���� ����
    //public UIController uiController;

    /// <summary>
    /// Player_Data.json ���� ������ ���� �ű� ���� �Ǵ� �ε�.
    /// ���� ���� 0/������ �߸� ���� ��쵵 �����ϰ� ����.
    /// ���� ����(ù �� �ε� ��)�� �ݵ�� �� �� ȣ��ǵ��� ���� ����.
    /// </summary>
    public static void InitPlayerData()
    {
        string path = Path.Combine(Application.persistentDataPath, "Player_Data.json");

        if (!File.Exists(path))
        {
            var data = new PlayerData
            {
                playerLevel = Mathf.Max(1, currentLevel),
                playerExp = Mathf.Max(0, currentExp),
                soulExp = 0,
                playerTotalExp = Mathf.Max(0, totalExp),
                expToNextLevel = Mathf.Max(1, expToNextLevel), // 0 ����
                unlockedButtons = new List<string>()
            };

            PlayerData = data;
            File.WriteAllText(path, JsonUtility.ToJson(data, true), Encoding.UTF8);
            Debug.Log("�ʱ� Player_Data.json ���� ���� �Ϸ�");
        }
        else
        {
            LoadPlayerData(); // �̹� ���� ������ �ε�
        }

        // UI �ʱⰪ �ݿ�
        OnExpChanged?.Invoke();
        OnExpChangedWithValue?.Invoke(PlayerData.playerExp, PlayerData.expToNextLevel, PlayerData.playerLevel);
    }

    /// <summary>
    /// ����ġ ȹ��
    /// </summary>
    public static void AddExp(int amount)
    {
        if (PlayerData == null) InitPlayerData();

        PlayerData.playerExp += amount;
        PlayerData.playerTotalExp += amount;

        Debug.Log($"����ġ +{amount}, ���� ����ġ: {PlayerData.playerExp}/{PlayerData.expToNextLevel}, �� ����ġ:{PlayerData.playerTotalExp}");

        Level_Up();

        OnExpChanged?.Invoke();
    }

    /// <summary>
    /// ������ ó��(���� ���� ���� ��� ����)
    /// expToNextLevel�� 0/������ �Ǹ� ���ѷ����� ���Ƿ� �׻� 1 �̻����� ����.
    /// </summary>
    public static void Level_Up()
    {
        if (PlayerData == null) InitPlayerData();

        if (PlayerData.playerLevel >= MaxLevel) return;

        // �߸� ����� �� ���
        if (PlayerData.expToNextLevel <= 0)
            PlayerData.expToNextLevel = 100;

        int safety = 0; // ���ѷ��� ����
        while (PlayerData.playerExp >= PlayerData.expToNextLevel)
        {
            PlayerData.playerExp -= PlayerData.expToNextLevel;
            PlayerData.playerLevel++;
            PlayerData.expToNextLevel = Mathf.Max(1, Mathf.RoundToInt(PlayerData.expToNextLevel * 1.2f));

            Ark_stat.remainingStatPoints += 4;

            Debug.Log($"������! ���� ���� : {PlayerData.playerLevel}");

            // ���� ���� �� �߰� ���� ����
            if (PlayerData.playerLevel >= MaxLevel)
            {
                PlayerData.playerExp = 0;
                PlayerData.expToNextLevel = int.MaxValue;
                break;
            }

            if (++safety > 1000)
            {
                Debug.LogError("Level_Up safety break: exp/requirement �� ���� �ʿ�");
                break;
            }
        }

        OnExpChanged?.Invoke();
        OnExpChangedWithValue?.Invoke(PlayerData.playerExp, PlayerData.expToNextLevel, PlayerData.playerLevel);
    }

    public static void MaxUp()
    {
        if (PlayerData == null) InitPlayerData();

        if (PlayerData.playerLevel == MaxLevel)
        {
            Debug.Log($"����� ����:{PlayerData.playerLevel} �ְ� ����:{MaxLevel} �� �̻� �ø� �� �����ϴ�.");
        }
    }

    /// <summary>
    /// ��� ó��(�ҿ� ����ġ ȯ�� ��)
    /// </summary>
    public static void PlayerDead()
    {
        if (PlayerData == null) InitPlayerData();

        Debug.Log("PlayerDead �Լ�ȣ��");

        // �� ����ġ�� 10% + ������ �ݿø�
        PlayerData.soulExp += PlayerData.playerTotalExp / 10;
        float semiresult = PlayerData.playerTotalExp % 10;
        int rounded = Mathf.RoundToInt(semiresult);
        PlayerData.soulExp += rounded;

        // ����
        PlayerData.playerLevel = 1;
        PlayerData.playerExp = 0;
        PlayerData.playerTotalExp = 0;
        PlayerData.expToNextLevel = 100;

        PlayerHealth.discountDamage = 0;
        PlayerHealth.maxHP = 100; // �ǵ��� ������ Ȯ�� �ʿ�(�⺻ 500�� ����ġ ����)

        Ark_stat.ResetStats();

        Debug.Log($"�ҿ� ����ġ:{PlayerData.soulExp}");

        //SoulBuffManager.P_dead = true;
        //SoulBuffManager.ApplyBuffByButtonId("deathTrigger");

        //SaveSystemManager.SaveOnDeath();

        OnExpChanged?.Invoke();
        OnExpChangedWithValue?.Invoke(PlayerData.playerExp, PlayerData.expToNextLevel, PlayerData.playerLevel);
    }

    public static void SavePlayerData()
    {
        if (PlayerData == null) return;
        PlayerExpSave.SavedExp();
    }

    public static void LoadPlayerData()
    {
        PlayerExpSave.LoadExp();
        //PlayerDataSaveSystem.Load();

        if (PlayerData == null) PlayerData = new PlayerData();

        // ���� ���̺� ���
        if (PlayerData.playerLevel <= 0) PlayerData.playerLevel = 1;
        if (PlayerData.expToNextLevel <= 0) PlayerData.expToNextLevel = 100;
        if (PlayerData.playerExp < 0) PlayerData.playerExp = 0;
        if (PlayerData.playerTotalExp < 0) PlayerData.playerTotalExp = 0;
        if (PlayerData.unlockedButtons == null) PlayerData.unlockedButtons = new List<string>();
    }
}

/// <summary>
/// ���� ���� �� ���� ���� PlayerExp ������ �ʱ�ȭ ����
/// (��ġ ������ �� Ŭ������ �����ص� �˴ϴ�)
/// </summary>
public static class GameBoot_PlayerExp
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Boot()
    {
        PlayerExpManager.InitPlayerData();
    }
}
