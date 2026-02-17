using UnityEngine;

/// <summary>
/// Initializes the Player character based on selected CharacterData.
/// Attach this to the Player GameObject.
/// </summary>
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerWeaponController))]
public class CharacterInitializer : MonoBehaviour
{
    [Header("References")]
    private PlayerStats playerStats;
    private PlayerMovement playerMovement;
    private PlayerWeaponController rangedWeaponController;
    private MeleeWeaponController meleeWeaponController;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Header("Fallback")]
    [SerializeField] private CharacterData fallbackCharacter;

    [Header("Current Character")]
    public CharacterData currentCharacter;

    void Start()
    {
        // Get components
        playerStats = GetComponent<PlayerStats>();
        playerMovement = GetComponent<PlayerMovement>();
        rangedWeaponController = GetComponent<PlayerWeaponController>();
        meleeWeaponController = GetComponent<MeleeWeaponController>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // 1) Load from CharacterDatabase
        if (CharacterDatabase.Instance != null)
        {
            currentCharacter = CharacterDatabase.Instance.GetCurrentCharacter();
            Debug.Log($"[CharacterInitializer] Loaded character: {(currentCharacter != null ? currentCharacter.characterName : "NULL")}");

            if (PersistentGameManager.Instance != null)
            {
                Debug.Log($"[CharacterInitializer] Selected index from PersistentGameManager: {PersistentGameManager.Instance.selectedCharacterIndex}");
            }

            // If still null, try first character in database
            if (currentCharacter == null && CharacterDatabase.Instance.GetCharacterCount() > 0)
            {
                currentCharacter = CharacterDatabase.Instance.GetCharacter(0);
                Debug.LogWarning("[CharacterInitializer] No character selected, using first character in database.");
            }
        }
        else
        {
            Debug.LogWarning("[CharacterInitializer] CharacterDatabase.Instance is NULL!");
        }

        // 2) Use fallbackCharacter
        if (currentCharacter == null && fallbackCharacter != null)
        {
            currentCharacter = fallbackCharacter;
            Debug.LogWarning("[CharacterInitializer] Using fallbackCharacter.");
        }

        // 3) Try Resources.Load
        if (currentCharacter == null)
        {
            currentCharacter = Resources.Load<CharacterData>("Beamer");
            if (currentCharacter != null)
            {
                Debug.LogWarning("[CharacterInitializer] Loaded character from Resources/Beamer.");
            }
        }

        // 4) Nothing found – error
        if (currentCharacter != null)
        {
            InitializeCharacter();
        }
        else
        {
            Debug.LogError("[CharacterInitializer] No CharacterData found! Player cannot be initialized.");
        }
    }

    void InitializeCharacter()
    {
        Debug.Log($"Initializing character: {currentCharacter.characterName}");

        // Apply Stats
        if (playerStats != null)
        {
            playerStats.maxHealth = currentCharacter.baseMaxHealth;
            playerStats.currentHealth = currentCharacter.baseMaxHealth;
        }

        // Apply Movement
        if (playerMovement != null)
        {
            playerMovement.moveSpeed = currentCharacter.baseMoveSpeed;
            playerMovement.dashSpeed = currentCharacter.baseDashSpeed;
            playerMovement.dashCooldown = currentCharacter.baseDashCooldown;
        }

        // Setup weapon based on character type
        Debug.Log($"[CharacterInitializer] ===== WEAPON SETUP START =====");
        Debug.Log($"[CharacterInitializer] Character Name: {currentCharacter.characterName}");
        Debug.Log($"[CharacterInitializer] Weapon Type: {currentCharacter.characterWeaponType}");
        Debug.Log($"[CharacterInitializer] RangedWeaponController exists: {rangedWeaponController != null}");
        Debug.Log($"[CharacterInitializer] MeleeWeaponController exists: {meleeWeaponController != null}");
        
        if (currentCharacter.characterWeaponType == WeaponType.Ranged)
        {
            Debug.Log("[CharacterInitializer] >> RANGED CHARACTER DETECTED - Enabling ranged weapon");
            // Enable ranged weapon, disable melee
            if (rangedWeaponController != null)
            {
                rangedWeaponController.enabled = true;
                Debug.Log($"[CharacterInitializer] >> PlayerWeaponController.enabled = TRUE");
                if (rangedWeaponController.weaponData != null)
                {
                    rangedWeaponController.weaponData.Damage = currentCharacter.baseDamage;
                    rangedWeaponController.weaponData.Cooldown = currentCharacter.baseFireRate;
                    rangedWeaponController.weaponData.Range = currentCharacter.baseRange;
                    rangedWeaponController.weaponData.ProjectileSpeed = currentCharacter.baseProjectileSpeed;
                }
            }
            if (meleeWeaponController != null)
            {
                meleeWeaponController.enabled = false;
                Debug.Log($"[CharacterInitializer] >> MeleeWeaponController.enabled = FALSE");
            }
        }
        else if (currentCharacter.characterWeaponType == WeaponType.Melee)
        {
            Debug.Log("[CharacterInitializer] >> MELEE CHARACTER DETECTED - Enabling melee weapon");
            // Enable melee weapon, disable ranged
            if (meleeWeaponController != null)
            {
                meleeWeaponController.enabled = true;
                Debug.Log($"[CharacterInitializer] >> MeleeWeaponController.enabled = TRUE");
                if (meleeWeaponController.weaponData != null)
                {
                    meleeWeaponController.weaponData.Damage = currentCharacter.baseDamage;
                    meleeWeaponController.weaponData.Cooldown = currentCharacter.baseFireRate;
                    meleeWeaponController.weaponData.Range = currentCharacter.baseRange;
                }
            }
            else
            {
                Debug.LogError("[CharacterInitializer] >> MeleeWeaponController component MISSING!");
            }
            if (rangedWeaponController != null)
            {
                rangedWeaponController.enabled = false;
                Debug.Log($"[CharacterInitializer] >> PlayerWeaponController.enabled = FALSE");
            }
        }
        else
        {
            Debug.LogError($"[CharacterInitializer] >> UNKNOWN WEAPON TYPE: {currentCharacter.characterWeaponType}");
        }
        Debug.Log($"[CharacterInitializer] ===== WEAPON SETUP END =====");

        // Apply Animator Controller
        if (animator != null && currentCharacter.animatorController != null)
        {
            animator.runtimeAnimatorController = currentCharacter.animatorController;
        }

        // Apply character color tint
        if (spriteRenderer != null)
        {
            // Vity = Red tint, Beamer = White (default)
            if (currentCharacter.characterName == "Vity")
            {
                spriteRenderer.color = new Color(1f, 0.3f, 0.3f); // Red tint
                Debug.Log("[CharacterInitializer] Applied RED color to Vity");
            }
            else
            {
                spriteRenderer.color = Color.white; // Default white
            }
        }

        Debug.Log($"Character {currentCharacter.characterName} initialized successfully!");
    }

    /// <summary>
    /// Get the current character name (for UI display)
    /// </summary>
    public string GetCharacterName()
    {
        return currentCharacter != null ? currentCharacter.characterName : "Unknown";
    }
}
