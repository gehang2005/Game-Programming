using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays player health and lives in UI.
/// Supports TextMeshPro text, Slider, and optional fill image color.
/// </summary>
public class PlayerHealthDisplay : UIelement
{
    [Header("References")]
    [Tooltip("Health component to read from. If empty and auto-find is enabled, the script will search for the Player tag.")]
    public Health healthSource;

    [Tooltip("Text used for health value display, for example: HP: 3 / 5")]
    public TextMeshProUGUI healthText;

    [Tooltip("Optional text for lives display when useLives is enabled")]
    public TextMeshProUGUI livesText;

    [Tooltip("Optional slider for health bar value")]
    public Slider healthSlider;

    [Tooltip("Optional fill image whose color follows health percentage")]
    public Image fillImage;

    [Header("Behavior")]
    [Tooltip("Auto-find player Health by Player tag when healthSource is empty")]
    public bool autoFindPlayerHealth = true;

    [Tooltip("Refresh every frame so UI still updates even if no manager event is fired")]
    public bool autoRefresh = true;

    [Header("Style")]
    [Tooltip("Color when health is high")]
    public Color highHealthColor = new Color(0.2f, 1f, 0.2f);

    [Tooltip("Color when health is low")]
    public Color lowHealthColor = new Color(1f, 0.2f, 0.2f);

    private int cachedHealth = int.MinValue;
    private int cachedMaxHealth = int.MinValue;
    private int cachedLives = int.MinValue;
    private bool cachedUseLives;

    private void Start()
    {
        TryResolveHealthSource();
        Refresh();
    }

    private void Update()
    {
        if (!autoRefresh)
        {
            return;
        }

        TryResolveHealthSource();
        if (HasStateChanged())
        {
            Refresh();
        }
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        TryResolveHealthSource();
        Refresh();
    }

    private void TryResolveHealthSource()
    {
        if (healthSource != null || !autoFindPlayerHealth)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            healthSource = player.GetComponent<Health>();
        }
    }

    private bool HasStateChanged()
    {
        if (healthSource == null)
        {
            return cachedHealth != int.MinValue;
        }

        return cachedHealth != healthSource.currentHealth ||
               cachedMaxHealth != healthSource.maximumHealth ||
               cachedUseLives != healthSource.useLives ||
               cachedLives != healthSource.currentLives;
    }

    private void Refresh()
    {
        if (healthSource == null)
        {
            if (healthText != null)
            {
                healthText.text = "HP: --";
            }
            if (livesText != null)
            {
                livesText.text = string.Empty;
            }
            if (healthSlider != null)
            {
                healthSlider.value = 0f;
            }

            cachedHealth = int.MinValue;
            cachedMaxHealth = int.MinValue;
            cachedLives = int.MinValue;
            cachedUseLives = false;
            return;
        }

        int maxHealth = Mathf.Max(1, healthSource.maximumHealth);
        int currentHealth = Mathf.Clamp(healthSource.currentHealth, 0, maxHealth);
        float normalizedHealth = (float)currentHealth / maxHealth;

        if (healthText != null)
        {
            healthText.text = "HP: " + currentHealth + " / " + maxHealth;
        }

        if (livesText != null)
        {
            livesText.text = healthSource.useLives ? "Lives: " + Mathf.Max(0, healthSource.currentLives) : string.Empty;
        }

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = normalizedHealth;
        }

        if (fillImage != null)
        {
            fillImage.color = Color.Lerp(lowHealthColor, highHealthColor, normalizedHealth);
        }

        cachedHealth = healthSource.currentHealth;
        cachedMaxHealth = healthSource.maximumHealth;
        cachedLives = healthSource.currentLives;
        cachedUseLives = healthSource.useLives;
    }
}