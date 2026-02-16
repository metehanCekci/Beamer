using UnityEngine;

public enum WeaponType
{
    Ranged,  // Long range projectiles (Beamer)
    Melee    // Close range attacks (Vity)
}

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Survivor/Weapon Data")]
public class WeaponData : ScriptableObject // Note: ScriptableObject, not MonoBehaviour!
{
    [Header("Weapon Type")]
    public WeaponType weaponType = WeaponType.Ranged;
    
    [Header("Base Values (Do Not Change)")]
    [SerializeField] private float baseDamage = 10f;
    public float BaseDamage => baseDamage; // Read-only property to access baseDamage
    [SerializeField] private float baseCooldown = 1.0f;
    [SerializeField] private float baseProjectileSpeed = 10f;
    [SerializeField] private float baseRange = 10f;

    [Header("Current Weapon Settings")]
    public float Damage = 10f;
    public float Cooldown = 1.0f;
    public float ProjectileSpeed = 10f;
    public float Range = 10f;
    public bool canBounce = false;
    public int bounceCount = 0; // Bounce count
    public bool canExplode = false;
    public float explosionRadius = 2f; // Explosion radius
    public int projectileCount = 1; // Projectile count
    public float critChance = 0f; // Critical hit chance (0-100)
    public float critDamageMultiplier = 1.5f; // Critical damage multiplier (1.5 = 150%)

    public void ResetValues()
    {
        Damage = baseDamage;
        Cooldown = baseCooldown;
        ProjectileSpeed = baseProjectileSpeed;
        Range = baseRange;
        canBounce = false;
        bounceCount = 0;
        canExplode = false;
        explosionRadius = 2f;
        projectileCount = 1;
        critChance = 0f;
        critDamageMultiplier = 1.5f;
    }

    private void OnEnable()
    {
        // Reset values when entering play mode in editor or game starts
        // Note: OnEnable might not always be called in Build, so we'll also call from UpgradeManager.
        ResetValues();
    }

    [Header("Visual Settings")]
    public GameObject ProjectilePrefab;
}