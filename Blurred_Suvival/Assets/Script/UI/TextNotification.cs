using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextNotification : MonoBehaviour
{
    public static TextNotification Instance;

    [Header("Notification Message")]
    private Queue<(string message, FloatingTextType type)> floatingTextQueue
    = new Queue<(string, FloatingTextType)>();

    private string NoSpaceInInventory = "";
    private bool isShowingText = false;

    public float MessageInterval = 1f;

    public Transform TextGenerationPoint;

    public void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void EnqueueCollectedText(string msg, FloatingTextType type)
    {
        floatingTextQueue.Enqueue((msg, type));

        if (!isShowingText)
            StartCoroutine(ProcessFloatingTextQueue());
    }

    public enum FloatingTextType
    {
        Damage,
        Heal, // for things like "Inventory Full"
    }

    private IEnumerator ProcessFloatingTextQueue()
    {
        isShowingText = true;
        Vector3 basePosition = TextGenerationPoint.position;

        while (floatingTextQueue.Count > 0)
        {
            var entry = floatingTextQueue.Dequeue();
            Vector3 spawnPos = basePosition + Vector3.up;

            switch (entry.type)
            {
                case FloatingTextType.Damage:
                    DamageTextManager.Instance.ShowDamage(spawnPos, entry.message, false);
                    break;

                case FloatingTextType.Heal:
                    DamageTextManager.Instance.ShowHeal(spawnPos, entry.message);
                    break;
            }

            yield return new WaitForSeconds(MessageInterval);
        }

        isShowingText = false;
    }
}
