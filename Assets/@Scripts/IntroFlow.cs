using FpsHorrorKit;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

enum IntroState
{
    PreIntro,           // ���� �� �ؽ�Ʈ��
    InGameIntro,        // ���� ȭ�鿡�� ��ũ �ȳ�
    BlinkPhase,         // 3�� ��ũ �ܰ�
    SleepingPhase,      // ��� ���� �ؽ�Ʈ��
    GameStart           // ���� ����
}

public class IntroFlow : MonoBehaviour
{
    [Header("�÷��̾� & ��Ʈ�ѷ�")]
    [SerializeField] private GameObject player;
    [SerializeField] private ControllerTransitionManager transitionManager;

    [Header("UI")]
    [SerializeField] private GameObject ingameUI;
    [SerializeField] private GameObject sleepUI;
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private FadeOut fadeController;

    [Header("ȯ�� ������Ʈ")]
    [SerializeField] private GameObject npcParent; // NPC���� �θ� ������Ʈ
    [SerializeField] private Material emissiveMaterial; // �߱� ����
    [SerializeField] private Light directionalLight;

    [Header("�����")]
    [SerializeField] private AudioSource bgmSource; //TODO : ���� ����ö�Ҹ�

    // ���� ���� ��Ȳ
    private int step = 0; // 0=�����ؽ�Ʈ, 1=��ũ, 2=������ؽ�Ʈ, 3=���ӽ���
    private int textIndex = 0; // ���� �ؽ�Ʈ ��ȣ
    private int blinkCount = 0; // ��ũ Ƚ��
    private bool canInput = true; // �Է� ���� ����
    void Start()
    {
        // ���� ���� ����
        sleepUI.SetActive(true);  // ���丮 �ؽ�Ʈ ȭ�� �ѱ�
        ingameUI.SetActive(false); // ���� UI ����
        

        // ù ��° �ؽ�Ʈ �����ֱ�
        ShowText("���� ���� ���� ����� ��ٱ��̾���.");
    }

    void Update()
    {
        // Space Ű�� ������ ��
        if (Input.GetKeyDown(KeyCode.Space) && canInput)
        {
            if (step == 0) // ���� �ؽ�Ʈ��
            {
                ShowStartTexts();
            }
            else if (step == 1) // ��ũ �ܰ�
            {
                DoEyeBlink();
            }
            else if (step == 2) // ��� �� �ؽ�Ʈ��
            {
                ShowSleepTexts();
            }
        }
    }

    private void ShowStartTexts()
    {
        textIndex++;

        if (textIndex == 1)
        {
            ShowText("�߱��� ��ġ�� ���� ����ö�� �ѻ��ߴ�.");
        }
        else if (textIndex == 2)
        {
            ShowText("������ ����� ����, �� �� �Ǵ� ������ �°���..");
        }
        else if (textIndex == 3)
        {
            ShowText("���� ������ �ڸ��� ���� �ñ�� ���� �����̸�, �����־���.");
        }
        else if (textIndex == 4)
        {
            // ���� ȭ������ �ٲ�
            ShowText("");
            StartCoroutine(StartBlinkPhase());
        }
    }

    private IEnumerator StartBlinkPhase()
    {
        sleepUI.SetActive(false); // �ؽ�Ʈ ȭ�� ����
        yield return new WaitForSeconds(2f); // 2�� ���

        ShowText("Space�ٸ� ������ ���� ��� ���� �� �ֽ��ϴ�.\n��ſ��� ������ �������ּ���.");
        
        step = 1; // ��ũ �ܰ�� ����
        textIndex = 0; // �ؽ�Ʈ ��ȣ ����
    }

    private void DoEyeBlink()
    {
        fadeController.Blink(); // ȭ�� ������
        blinkCount++;

        if (blinkCount >= 3) // 3�� ����������
        {
            StartCoroutine(GoToSleep());
        }
    }

    private IEnumerator GoToSleep()
    {
        canInput = false; // �Է� ����
        sleepUI.SetActive(true); // �ؽ�Ʈ ȭ�� ����
        ShowText("�Ϸ� ���� ���� �Ƿΰ� ����� ��������\n����ö�� �����ο� ������ ���� ������ �̲�����.");
        
        yield return new WaitForSeconds(5f); // 5�� ���

        canInput = true;
        // ��� �� �ؽ�Ʈ ����
        step = 2; // ��� �� �ؽ�Ʈ �ܰ�
        textIndex = 0; // �ؽ�Ʈ ��ȣ ����
        sleepUI.SetActive(true);
        
    }

    private void MakeEnvironmentDark()
    {
        // NPC�� ���� (�θ� ������Ʈ ����)
        if (npcParent != null)
        {
            npcParent.SetActive(false);
        }

        // �߱� ���� ����
        if (emissiveMaterial != null)
        {
            emissiveMaterial.SetColor("_EmissionColor", Color.black);
        }

        // ���� ��Ӱ�
        if (directionalLight != null)
        {
            directionalLight.intensity = 0.05f;
        }
    }

    private void ChangeToFpsController()
    {
        // ���� ��Ʈ�ѷ� ����
        player.GetComponent<IntroPlayerController>().enabled = false;

        // �� ��Ʈ�ѷ� �ѱ�
        transitionManager.StartTransition();
        player.GetComponent<FpsAssetsInputs>().enabled = true;
        player.GetComponent<FpsController>().enabled = true;
        ingameUI.SetActive(true);
    }

    private void ShowSleepTexts()
    {
        textIndex++;

        if (textIndex == 1)
        {
            ShowText("�׷���... ���� �̻��ߴ�.");
        }
        else if (textIndex == 2)
        {
            ShowText("�������, �Ⱬ�������� �ʹ� ����������.");
        }
        else if (textIndex == 3)
        {
            ShowText("����ö Ư���� �������, �°����� ���� ������ �Ҹ��� �鸮�� �ʾҴ�.");
        }
        else if (textIndex == 4)
        {
            ShowText("����.. õõ�� ���� ����.");
        }
        else if (textIndex >= 5)
        {
            ShowText("");
            // ���� ����!
            StartGame();
        }
    }

    private void StartGame()
    {
        sleepUI.SetActive(false); // �ؽ�Ʈ ȭ�� ����
        step = 3; // ���� ���� �ܰ�

        Debug.Log("���� ����!");
        // ���⿡ �߰� ���� ���� ����...

        // �÷��̾� �Ͼ��
        player.GetComponent<IntroPlayerController>().StandUp();
        // ȯ���� ��Ӱ� �����
        MakeEnvironmentDark();
        // ��Ʈ�ѷ� �ٲٱ�
        ChangeToFpsController();

    }

    // �ؽ�Ʈ �����ֱ�
    private void ShowText(string message)
    {
        storyText.text = message;
    }
}
