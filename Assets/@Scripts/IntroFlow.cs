using FpsHorrorKit;
using TMPro;
using UnityEngine;

public class IntroFlow : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] GameObject ingameUI;
    [SerializeField] GameObject npc;
    [SerializeField] Material EM;

    [SerializeField] GameObject sleepUI;
    [SerializeField] TextMeshProUGUI text;

    [SerializeField] FadeOut UI;
    private int blinkCount = 0;
    private bool isblink;


    void Start()
    {

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isblink == false)
        {
            UI.GetComponent<FadeOut>().Blink();
            blinkCount++;

            if (blinkCount == 3 && isblink == false) // 세 번째 블링크 후 일어서기
            {
                isblink = true;
                player.GetComponent<IntroPlayerController>().StandUp();
                player.GetComponent<IntroPlayerController>().enabled = false;



                player.transform.rotation = Quaternion.identity;
                player.GetComponent<FpsAssetsInputs>().enabled = true;
                player.GetComponent<FpsController>().enabled = true;
                ingameUI.SetActive(true);


                // FIXME : FOREACH로 추후에 바꿔야함.
                npc.SetActive(false);
                EM.SetColor("_EmissionColor", Color.black);
            }
        }






    }

    void showText(string msg)
    {
        text.text = msg;
        sleepUI.SetActive(true);

    }
}
