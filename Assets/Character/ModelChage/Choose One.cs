using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// -100���� ���� ���ϼ��� �� ���� ����˴ϴ�.
[DefaultExecutionOrder(15)]
public class ChooseOne : MonoBehaviour
{
    [Header("UI �г� & ��ư ����")]
    public GameObject selectionPanel;    // ����â ��ü �г� (Canvas �Ʒ��� �ΰ� ��Ȱ��ȭ)
    public Button steampunkButton;       // ������ũ ���� ��ư
    public Button runeButton;            // �鸶�� ���� ��ư

    [Header("������ũ/�鸶�� �÷��̾�")]
    [SerializeField] private GameObject SteamPunkPlayer;
    [SerializeField] private GameObject MagicPlayer;

    // UIcontoller ����
    public GameObject UI;
    //[SerializeField] private InventoryUIController inventory;

    public bool select = false;
    public bool SystemSteamPunk = false;
    public bool SystemMagic = false;

    // �Ͻ����� �� ���� ������Ʈ �ֱ⸦ ������ ����
    private float _savedFixedDeltaTime;


    private SystemSwitch systemswitch;
    [SerializeField] private DashStackUI dashstackUI;

    // ����
    [SerializeField] private CameraController cameracontroller;
    [SerializeField] private PlayerGoldManager playergoldmanager;
    //[SerializeField] private EnemyAI enemy;

    private void Awake()
    {
        systemswitch = GetComponent<SystemSwitch>();
        // ���� ���� ����
        PauseGame();

        // ���� UI ����
        if (selectionPanel != null)
            selectionPanel.SetActive(true);

        // ��ư �ݹ� ����
        steampunkButton.onClick.AddListener(OnSteampunkSelected);
        runeButton.onClick.AddListener(OnRuneSelected);
    }

    private void OnSteampunkSelected()
    {
        GameSettings.Instance.CurrentMode = GameMode.Steampunk;
        StartGame();
        SteamPunkPlayer.SetActive(true);
        SystemSteamPunk = true;
        select = true;
        systemswitch.SteamPunk();
        dashstackUI.SteamPunkDash();
        cameracontroller.SetSteamPunktype();
        UI.SetActive(true);
        //inventory.OpenInventory();
        //playergoldmanager.Init();
    }

    private void OnRuneSelected()
    {
        GameSettings.Instance.CurrentMode = GameMode.Rune;
        StartGame();
        MagicPlayer.SetActive(true);
        SystemMagic = true;
        select = true;
        systemswitch.Magic();
        dashstackUI.MagicDash();
        cameracontroller.SetMagictype();
        UI.SetActive(true);
        //inventory.OpenInventory();
        //playergoldmanager.Init();
    }

    private void StartGame()
    {
        // UI �����
        selectionPanel.SetActive(false);

        // ���� �簳
        ResumeGame();

        // �� �ε峪 ���� �Ŵ��� ���� ȣ��
        // SceneManager.LoadScene("MainGame");
        // GameManager.Instance.Begin();
    }

    private void PauseGame()
    {
        // 1) ���� ������ ���� �� 0����
        _savedFixedDeltaTime = Time.fixedDeltaTime;
        Time.fixedDeltaTime = 0f;

        // 2) ���� �ð� ����
        Time.timeScale = 0f;

        // 3) ������� ����
        AudioListener.pause = true;
    }

    private void ResumeGame()
    {
        // 1) ���� �ð� ����
        Time.timeScale = 1f;

        // 2) ���� ������ ����
        Time.fixedDeltaTime = _savedFixedDeltaTime;

        // 3) ����� ��� �簳
        AudioListener.pause = false;
    }
}