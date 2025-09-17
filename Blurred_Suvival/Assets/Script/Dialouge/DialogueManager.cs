using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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

    // ✅ List of survivors
    private List<GameObject> Survivors;

    public float soundCooldown = 0.1f;

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

    /// <summary>
    /// Initiate dialogue with a list of survivors
    /// </summary>
    public void InitiateDialogue(Event EventData, List<GameObject> survivors)
    {
        this.changeFrequency = EventData.DialougeChangeFrequency;
        TemporaryEvent = EventData;

        showChoicesAtEnd = EventData.ShowChoiceButtonAtEndOfDialouge;
        Survivors = survivors;
        StartDialogue(EventData.dialouge, showChoicesAtEnd);
    }

    public void SetSpeakerDetails(Sprite characterSprite, string characterName)
    {
        CharacterImage.sprite = characterSprite;
        CharacterName.text = characterName;

        CharacterImage.SetNativeSize();

        RectTransform rt = CharacterImage.GetComponent<RectTransform>();
        rt.sizeDelta = rt.sizeDelta * 1.4f;
    }

    // ✅ StartDialogue with optional callback
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

    [Header("Dialogue Settings")]
    [SerializeField] private int changeFrequency = 1; // default 1 = change every line

    private void ShowLine()
    {
        string line = currentDialogue[currentLine];

        // Determine which survivor speaks
        if (Survivors != null && Survivors.Count > 0)
        {
            int speakerIndex = (currentLine / changeFrequency) % Survivors.Count;
            GameObject speaker = Survivors[speakerIndex];

            SetSpeakerDetails(
                speaker.GetComponentInChildren<SpriteRenderer>().sprite,
                speaker.GetComponent<CharacterStats>().CharacterName
            );
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(line));
    }


    private IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        float lastSoundTime = -soundCooldown;

        foreach (char c in text)
        {
            dialogueText.text += c;

            // Play keystroke sound if enough time has passed
            if (Time.time - lastSoundTime >= soundCooldown)
            {
                SFXManager.Instance.PlayKeyStroke();
                lastSoundTime = Time.time;
            }

            yield return new WaitForSeconds(typeSpeed);
        }

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
            if (!TemporaryEvent.ShowChoiceButtonAtEndOfDialouge)
            {
                SetPrimaryOutcome();
            }
            ChoicePanel.SetActive(false);
            MainPanel.SetActive(false);

            onDialogueFinished?.Invoke();
        }
    }

    public void OnClickDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentDialogue[currentLine];
            typingCoroutine = null;
        }
        else
        {
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

    #region Primary Outcome
    Event TemporaryEvent;

    public void SetPrimaryOutcome()
    {
        switch (TemporaryEvent.PrimaryChoiceType)
        {
            case ChoiceOutcome.ChoiceType.Runaway:
                TurnManager.Instance.NPCRunAway();
                Debug.Log("Tryue");
                break;

            case ChoiceOutcome.ChoiceType.DropLoot:
                TurnManager.Instance.NPCDropLootAndRunAway();
                break;

            case ChoiceOutcome.ChoiceType.Join:
                TurnManager.Instance.NPCJoin();
                break;

            case ChoiceOutcome.ChoiceType.Fight:
                TurnManager.Instance.SetNPCAsEnemy();
                break;
            case ChoiceOutcome.ChoiceType.Threaten:
                TurnManager.Instance.SetNPCAsEnemy();
                break;
            case ChoiceOutcome.ChoiceType.RejectJoin:
                TurnManager.Instance.NPCRunAway();
                break;
            case ChoiceOutcome.ChoiceType.OfferTruce:
                TurnManager.Instance.NPCRunAway();
                break;
        }
    }
    #endregion
}
