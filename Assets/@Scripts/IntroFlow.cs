using FpsHorrorKit;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

enum IntroState
{
    PreIntro,           // 시작 전 텍스트들
    InGameIntro,        // 게임 화면에서 블링크 안내
    BlinkPhase,         // 3번 블링크 단계
    SleepingPhase,      // 잠든 상태 텍스트들
    GameStart           // 게임 시작
}

public class IntroFlow : MonoBehaviour
{
    [Header("플레이어 & 컨트롤러")]
    [SerializeField] private GameObject player;
    [SerializeField] private ControllerTransitionManager transitionManager;

    [Header("UI")]
    [SerializeField] private GameObject ingameUI;
    [SerializeField] private GameObject sleepUI;
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private FadeOut fadeController;

    [Header("환경 오브젝트")]
    [SerializeField] private GameObject npcParent; // NPC들의 부모 오브젝트
    [SerializeField] private Material emissiveMaterial; // 발광 재질
    [SerializeField] private Light directionalLight;

    [Header("오디오")]
    [SerializeField] private AudioSource bgmSource; //TODO : 잠들고 지하철소리

    // 현재 진행 상황
    private int step = 0; // 0=시작텍스트, 1=블링크, 2=잠든후텍스트, 3=게임시작
    private int textIndex = 0; // 현재 텍스트 번호
    private int blinkCount = 0; // 블링크 횟수
    private bool canInput = true; // 입력 가능 여부
    void Start()
    {
        // 게임 시작 설정
        sleepUI.SetActive(true);  // 스토리 텍스트 화면 켜기
        ingameUI.SetActive(false); // 게임 UI 끄기
        

        // 첫 번째 텍스트 보여주기
        ShowText("여느 때와 같은 평범한 퇴근길이었다.");
    }

    void Update()
    {
        // Space 키를 눌렀을 때
        if (Input.GetKeyDown(KeyCode.Space) && canInput)
        {
            if (step == 0) // 시작 텍스트들
            {
                ShowStartTexts();
            }
            else if (step == 1) // 블링크 단계
            {
                DoEyeBlink();
            }
            else if (step == 2) // 잠든 후 텍스트들
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
            ShowText("야근을 마치고 나온 지하철은 한산했다.");
        }
        else if (textIndex == 2)
        {
            ShowText("막차에 가까운 열차, 몇 안 되는 조용한 승객들..");
        }
        else if (textIndex == 3)
        {
            ShowText("나는 구석진 자리에 몸을 맡기고 눈을 깜빡이며, 졸고있었다.");
        }
        else if (textIndex == 4)
        {
            // 게임 화면으로 바뀜
            ShowText("");
            StartCoroutine(StartBlinkPhase());
        }
    }

    private IEnumerator StartBlinkPhase()
    {
        sleepUI.SetActive(false); // 텍스트 화면 끄기
        yield return new WaitForSeconds(2f); // 2초 대기

        ShowText("Space바를 누르면 눈을 잠시 감을 수 있습니다.\n당신에게 졸음을 선물해주세요.");
        
        step = 1; // 블링크 단계로 변경
        textIndex = 0; // 텍스트 번호 리셋
    }

    private void DoEyeBlink()
    {
        fadeController.Blink(); // 화면 깜빡임
        blinkCount++;

        if (blinkCount >= 3) // 3번 깜빡였으면
        {
            StartCoroutine(GoToSleep());
        }
    }

    private IEnumerator GoToSleep()
    {
        canInput = false; // 입력 차단
        sleepUI.SetActive(true); // 텍스트 화면 끄기
        ShowText("하루 종일 쌓인 피로가 어깨를 짓눌렀고\n지하철의 단조로운 진동이 나를 잠으로 이끌었다.");
        
        yield return new WaitForSeconds(5f); // 5초 대기

        canInput = true;
        // 잠든 후 텍스트 시작
        step = 2; // 잠든 후 텍스트 단계
        textIndex = 0; // 텍스트 번호 리셋
        sleepUI.SetActive(true);
        
    }

    private void MakeEnvironmentDark()
    {
        // NPC들 끄기 (부모 오브젝트 끄기)
        if (npcParent != null)
        {
            npcParent.SetActive(false);
        }

        // 발광 재질 끄기
        if (emissiveMaterial != null)
        {
            emissiveMaterial.SetColor("_EmissionColor", Color.black);
        }

        // 조명 어둡게
        if (directionalLight != null)
        {
            directionalLight.intensity = 0.05f;
        }
    }

    private void ChangeToFpsController()
    {
        // 기존 컨트롤러 끄기
        player.GetComponent<IntroPlayerController>().enabled = false;

        // 새 컨트롤러 켜기
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
            ShowText("그런데... 뭔가 이상했다.");
        }
        else if (textIndex == 2)
        {
            ShowText("어느순간, 기괴할정도로 너무 조용해졌다.");
        }
        else if (textIndex == 3)
        {
            ShowText("지하철 특유의 기계음도, 승객들의 작은 움직임 소리도 들리지 않았다.");
        }
        else if (textIndex == 4)
        {
            ShowText("나는.. 천천히 눈을 떴다.");
        }
        else if (textIndex >= 5)
        {
            ShowText("");
            // 게임 시작!
            StartGame();
        }
    }

    private void StartGame()
    {
        sleepUI.SetActive(false); // 텍스트 화면 끄기
        step = 3; // 게임 시작 단계

        Debug.Log("게임 시작!");
        // 여기에 추가 게임 시작 로직...

        // 플레이어 일어서기
        player.GetComponent<IntroPlayerController>().StandUp();
        // 환경을 어둡게 만들기
        MakeEnvironmentDark();
        // 컨트롤러 바꾸기
        ChangeToFpsController();

    }

    // 텍스트 보여주기
    private void ShowText(string message)
    {
        storyText.text = message;
    }
}
