using UnityEngine;

/// <summary>
/// Scriptable Object that holds all data for a playable character.
/// Create new instances via: Assets/Create/Game/Character Data
/// </summary>
[CreateAssetMenu(fileName = "New Character", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Character Info")]
    public string characterName = "New Character";
    [TextArea(2, 4)]
    public string description = "Character description...";
    public Sprite characterPortrait; // For character select screen
    public Sprite characterIcon; // For UI/HUD

    [Header("Character Prefab & Animation")]
    public GameObject characterPrefab; // The actual character GameObject
    public RuntimeAnimatorController animatorController; // Character-specific animations

    [Header("Base Stats")]
    public float baseMaxHealth = 100f;
    public float baseMoveSpeed = 5f;
    public float baseDashSpeed = 15f;
    public float baseDashCooldown = 1f;

    [Header("Weapon Settings")]
    public WeaponType characterWeaponType = WeaponType.Ranged;
    
    [Header("Weapon Stats")]
    public float baseDamage = 10f;
    public float baseFireRate = 1f;
    public float baseRange = 10f;
    public float baseProjectileSpeed = 15f;

    [Header("Special Abilities")]
    [Tooltip("Does this character have unique passive abilities?")]
    public bool hasUniquePassive = false;
    [TextArea(2, 3)]
    public string passiveDescription = "";

    [Header("Unlock Requirements")]
    public bool isUnlocked = true; // Beamer starts unlocked
    public int unlockCost = 0; // Coins needed to unlock
}
