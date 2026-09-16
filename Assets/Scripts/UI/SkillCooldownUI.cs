using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUI : MonoBehaviour
{
    [SerializeField] private Image cooldownFill;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private Image readyFlash;
    [SerializeField] private float readyFlashMaxAlpha = 0.75f;
    [SerializeField] private float readyFlashDuration = 0.25f;


    private Coroutine readyFlashCoroutine;

    private void Awake()
    {
        ResetVisuals();
    }

    public void StartCooldown(float duration)
    {
        CancelReadyFlash();
        readyFlash.gameObject.SetActive(false);

        cooldownFill.gameObject.SetActive(true);
        cooldownFill.fillAmount = 0f;
        cooldownText.gameObject.SetActive(true);
        SetCooldownText(duration);
    }

    public void SetCooldownProgress(
        float elapsed,
        float duration)
    {
        if (duration <= 0f)
            return;

        cooldownFill.fillAmount = Mathf.Clamp01(
            elapsed / duration
        );

        float remaining = Mathf.Max(duration - elapsed, 0f);

        if (remaining > 0f)
            SetCooldownText(remaining);
    }

    public void CompleteCooldown()
    {
        cooldownFill.fillAmount = 1f;
        cooldownText.gameObject.SetActive(false);
        readyFlashCoroutine = StartCoroutine(PlayReadyFlash());
    }

    private IEnumerator PlayReadyFlash()
    {
        Color color = readyFlash.color;

        // 매번 시작 밝기로 복구
        color.a = readyFlashMaxAlpha;
        readyFlash.color = color;

        readyFlash.gameObject.SetActive(true);

        float elapsed = 0f;

        while (elapsed < readyFlashDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / readyFlashDuration
            );

            color.a = Mathf.Lerp(
                readyFlashMaxAlpha,
                0f,
                t
            );

            readyFlash.color = color;

            yield return null;
        }

        color.a = 0f;
        readyFlash.color = color;

        readyFlash.gameObject.SetActive(false);

        cooldownFill.fillAmount = 0f;
        cooldownFill.gameObject.SetActive(false);

        readyFlashCoroutine = null;
    }

    private void SetCooldownText(float remaining)
    {
        float displayedRemaining =
            Mathf.Ceil(remaining * 10f) / 10f;

        cooldownText.SetText(
            "{0:0.0}",
            displayedRemaining
        );
    }

    private void OnDisable()
    {
        CancelReadyFlash();
        ResetVisuals();
    }

    private void CancelReadyFlash()
    {
        if (readyFlashCoroutine == null)
            return;

        StopCoroutine(readyFlashCoroutine);
        readyFlashCoroutine = null;
    }

    private void ResetVisuals()
    {
        cooldownFill.fillAmount = 0f;
        cooldownFill.gameObject.SetActive(false);
        cooldownText.gameObject.SetActive(false);
        readyFlash.gameObject.SetActive(false);
    }
}
