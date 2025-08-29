using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Elements")]
    public GameObject MainPanel;
    public GameObject dialoguePanel, ChoicePanel;
    public TMP_Text dialogueText;

    [Header("Details Setting")]
    public Image CharacterImage;
    public TextMeshProUGUI CharacterName;

    [Header("Typewriter Settings")]
    public float typeSpeed = 0.03f;

    private string[] currentDialogue;
    private int currentLine;
    private Coroutine typingCoroutine;

    private bool showChoicesAtEnd = true;

    // ✅ Callback for when dialogue finishes
    private Action onDialogueFinished;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && dialoguePanel.activeSelf)
        {
            OnClickDialogue();
        }
    }
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        dialoguePanel.SetActive(false);
        MainPanel.SetActive(false);
        ChoicePanel.SetActive(false);
    }

    public void InitiateDialouge(Event EventData, GameObject obj)
    {
        SetSpeakerDetails(
            obj.GetComponentInChildren<SpriteRenderer>().sprite,
            obj.GetComponent<CharacterStats>().CharacterName
        );

        StartDialogue(EventData.dialouge);
    }

    public void SetSpeakerDetails(Sprite characterSprite, string characterName)
    {
        this.CharacterImage.sprite = characterSprite;
        this.CharacterName.text = characterName;

        this.CharacterImage.SetNativeSize();

        RectTransform rt = this.CharacterImage.GetComponent<RectTransform>();
        rt.sizeDelta = rt.sizeDelta * 1.4f;
    }

    // ✅ Updated StartDialogue with optional callback
    public void StartDialogue(string[] dialogue, bool showChoices = true, Action onFinish = null)
    {
        currentDialogue = dialogue;
        currentLine = 0;
        showChoicesAtEnd = showChoices;
        onDialogueFinished = onFinish;

        MainPanel.SetActive(true);
        dialoguePanel.SetActive(true);
        ChoicePanel.SetActive(false);

        ShowLine();
    }

    private void ShowLine()
    {
        string line = currentDialogue[currentLine];

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(line));
    }

    private IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        foreach (char c in text.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        // Coroutine ends naturally when line is fully typed
        typingCoroutine = null;
    }


    private void NextLine()
    {
        currentLine++;
        if (currentLine < currentDialogue.Length)
            ShowLine();
        else
            EndDialogue();
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        if (showChoicesAtEnd)
        {
            ChoicePanel.SetActive(true);
        }
        else
        {
            ChoicePanel.SetActive(false);
            MainPanel.SetActive(false);

            // ✅ Invoke callback when dialogue fully ends
            onDialogueFinished?.Invoke();
        }
    }

    public void OnClickDialogue()
    {
        if (typingCoroutine != null)
        {
            // Stop the typewriter and instantly show full line
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentDialogue[currentLine];
            typingCoroutine = null;
        }
        else
        {
            // Move to next line if fully displayed
            NextLine();
        }
    }


    #region ChoiceButton
    public void Onclick()
    {
        ChoicePanel.SetActive(false);
        MainPanel.SetActive(false);
    }
    #endregion
}
