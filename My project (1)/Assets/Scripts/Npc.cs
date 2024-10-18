using UnityEngine;
using TMPro;

public class Npc : MonoBehaviour
{
    [SerializeField] float radius;
    [SerializeField] PlayerController playerController;
    [SerializeField] GameObject textCanvas, player;
    [SerializeField] TMP_Text messageText, continueText;
    [SerializeField] string[] message;
    [SerializeField] bool playerInRange, canContinue = true;
    private int currentMessage;
    private void Start()
    {
        NextText();
    }
    private void Update()
    {
        CheckPlayer();
        RotateToPlayer();
        if (Input.GetKeyDown(KeyCode.Return) && playerController.punched >= 49)
        {
            CompleteQuest();
            print("Quest is completed");
        }
    }

    private void CheckPlayer()
    {
        playerInRange = Vector3.Distance(transform.position, player.transform.position) < radius;
        textCanvas.SetActive(playerInRange);
        if (playerInRange && canContinue)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                NextText();
                // здесь можно выбрать число реплик перед тем как нпс выдаст квест
                if (currentMessage == 2)
                {
                    continueText.text = "[выполни задание, чтобы продолжить]";
                    canContinue = false;
                }
            }
        }
    }
    public void CompleteQuest()
    {
        continueText.text = "[нажми Enter чтобы продолжить]";
        canContinue = true;
    }
    private void NextText()
    {
        if (currentMessage < message.Length)
        {
            messageText.text = message[currentMessage];
            currentMessage++;
        }
    }
    private void RotateToPlayer()
    {
        // повернуть объект к игроку
        textCanvas.transform.LookAt(player.transform);
        // обнулить поворот по x и z
        textCanvas.transform.eulerAngles = new Vector3(0, textCanvas.transform.eulerAngles.y, 0);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
