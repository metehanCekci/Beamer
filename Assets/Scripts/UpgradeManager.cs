using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum UpgradeRarity
{
    Common,
    Rare,
    Epic,
    Legendary,
    Mythic,
    Godlike
}

// Y�kseltme veri yap�s� (De�i�medi)
[System.Serializable]
public class UpgradeData
{
    public int id;
    public string title;
    [TextArea(1, 3)]
    public string description;
    public int prerequisiteId = -1; // Ön koşul ID'si (-1 ise yok)
    public UpgradeRarity rarity = UpgradeRarity.Common;
}

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Manager References")]
    public GameObject levelUpPanel;
    public PlayerWeaponController weaponController;
    public PlayerStats playerStats;
    public PlayerMovement playerMovement;
    public OrbitalWeapon orbitalWeapon;

    [Header("Settings")]
    public bool resetOnStart = true; // Should weapon data be reset on start?

    [Header("Card Visuals (Rarity)")]
    public Sprite commonCardSprite;
    public Sprite rareCardSprite;
    public Sprite epicCardSprite;
    public Sprite legendaryCardSprite;
    public Sprite mythicCardSprite;
    public Sprite godlikeCardSprite;

    [Header("Upgrade Values")]
    public float damageMultiplier = 1.15f;
    public float rangeIncrease = 0.5f;
    public float speedIncrease = 0.5f;
    public float projectileSpeedIncrease = 1f;
    public float healthIncrease = 20f;
    public float regenRateIncrease = 1f;

    [Header("UI References")]
    public Button[] upgradeButtons = new Button[3];
    public TMP_Text[] upgradeTexts = new TMP_Text[3];
    public Button skipButton;
    public Button rerollButton;
    public TMP_Text rerollButtonText;

    public Button undoButton; // Regreter Reward Button

    private List<UpgradeData> allUpgrades = new List<UpgradeData>();
    private List<int> unlockedUpgradeIds = new List<int>(); // Alınan yükseltmelerin ID'leri
    private Dictionary<int, int> upgradeLevels = new Dictionary<int, int>(); // Upgrade levels
    private UpgradeData[] currentSelections = new UpgradeData[3];
    private UpgradeData[] previousSelections = new UpgradeData[3]; // For Undo
    private bool isUpgradeProcessing = false; // Lock to prevent double clicking
    private int skipCount = 0; // Accumulated skip count
    private int pendingLevelUps = 0; // Queue for stacked level ups
    private int undoCount = 0; // Regreter uses

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Silah verilerini sıfırla (Sadece resetOnStart true ise)
            if (resetOnStart && weaponController != null && weaponController.weaponData != null)
            {
                weaponController.weaponData.ResetValues();

                // KALICI UPGRADELERİ UYGULA
                // Şu an sadece Health var ve o PlayerStats içinde uygulanıyor.
                // İleride buraya Damage, Speed vb. eklenebilir.
            }
            InitializeUpgradePool();

            // Buton listener'larını ekle
            if (skipButton != null) skipButton.onClick.AddListener(OnSkipButtonClicked);
            if (rerollButton != null) rerollButton.onClick.AddListener(OnRerollButtonClicked);
            if (undoButton != null) undoButton.onClick.AddListener(OnUndoButtonClicked);

            // Boss Reward: Lets Go Gambling (Start with 5 skips)
            if (BossRewardManager.Instance != null && BossRewardManager.Instance.HasReward(BossRewardType.LetsGoGambling))
            {
                skipCount += 5;
            }

            // Initialize Undo Count (Per Scene)
            undoCount = 3;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void InitializeUpgradePool()
    {
        allUpgrades.Clear();
        upgradeLevels.Clear();

        // Common (Yaygın) - %70
        allUpgrades.Add(new UpgradeData { id = 1, title = "Brute Force", description = $"Permanently increases your damage by %{(damageMultiplier - 1) * 100:F0}.", rarity = UpgradeRarity.Common });
        allUpgrades.Add(new UpgradeData { id = 2, title = "Sharp Eye", description = "Increases attack range.", rarity = UpgradeRarity.Common });
        allUpgrades.Add(new UpgradeData { id = 3, title = "Agile Legs", description = "Increases movement speed.", rarity = UpgradeRarity.Common });
        allUpgrades.Add(new UpgradeData { id = 4, title = "Fast Bullets", description = "Reduces firing cooldown.", rarity = UpgradeRarity.Common });
        allUpgrades.Add(new UpgradeData { id = 23, title = "Magnet", description = "Increases XP collection range.", rarity = UpgradeRarity.Common });
        
        // Rare (Nadir) - %15
        allUpgrades.Add(new UpgradeData { id = 5, title = "Sturdy Armor", description = "Increases maximum health.", rarity = UpgradeRarity.Rare });
        allUpgrades.Add(new UpgradeData { id = 6, title = "Health Regeneration", description = "Unlocks health regeneration.", rarity = UpgradeRarity.Rare });
        allUpgrades.Add(new UpgradeData { id = 12, title = "Fast Orbs", description = "Increases orb rotation speed.", prerequisiteId = 11, rarity = UpgradeRarity.Rare });
        allUpgrades.Add(new UpgradeData { id = 11, title = "Protective Orb", description = "Creates a protective orb that rotates around you.", rarity = UpgradeRarity.Rare });
        allUpgrades.Add(new UpgradeData { id = 18, title = "Critical Strike Chance", description = "Increases critical strike chance by 5%.", rarity = UpgradeRarity.Rare });

        // Epic (Destansı) - %10
        allUpgrades.Add(new UpgradeData { id = 7, title = "Rapid Regeneration", description = "Increases regeneration rate.", prerequisiteId = 6, rarity = UpgradeRarity.Epic });
        allUpgrades.Add(new UpgradeData { id = 8, title = "Chain Reaction", description = "Projectiles bounce.", rarity = UpgradeRarity.Epic });
        allUpgrades.Add(new UpgradeData { id = 15, title = "Chain Count", description = "Increases bounce count.", prerequisiteId = 8, rarity = UpgradeRarity.Epic });
        allUpgrades.Add(new UpgradeData { id = 10, title = "Scholar", description = "Increases XP gained.", rarity = UpgradeRarity.Epic });
        allUpgrades.Add(new UpgradeData { id = 13, title = "Multi Orb", description = "Increases orb count.", prerequisiteId = 11, rarity = UpgradeRarity.Epic });
        allUpgrades.Add(new UpgradeData { id = 14, title = "Close Protection", description = "Moves orbs closer to center.", prerequisiteId = 11, rarity = UpgradeRarity.Epic });
        allUpgrades.Add(new UpgradeData { id = 19, title = "Critical Strike Damage", description = "Increases critical strike damage by 10%.", rarity = UpgradeRarity.Epic });
        allUpgrades.Add(new UpgradeData { id = 21, title = "Blast Area", description = "Expands blast area by 10%.", prerequisiteId = 9, rarity = UpgradeRarity.Epic });

        // Legendary (Efsanevi) - %4
        allUpgrades.Add(new UpgradeData { id = 9, title = "Explosive Rounds", description = "Projectiles explode.", rarity = UpgradeRarity.Legendary });

        // Melee-Specific Upgrades (For Vity)
        allUpgrades.Add(new UpgradeData { id = 24, title = "Wider Slash", description = "Increases melee attack arc angle by 20 degrees.", rarity = UpgradeRarity.Common });
        allUpgrades.Add(new UpgradeData { id = 25, title = "Extended Reach", description = "Increases melee attack radius by 15%.", rarity = UpgradeRarity.Rare });
        allUpgrades.Add(new UpgradeData { id = 26, title = "Whirlwind", description = "Adds an extra attack segment (more directions).", rarity = UpgradeRarity.Epic });
        allUpgrades.Add(new UpgradeData { id = 27, title = "Berserker Rage", description = "Melee attacks get faster with each consecutive hit (up to 3 stacks).", rarity = UpgradeRarity.Legendary });

        // Mythic (Mistik) - %0.99
        allUpgrades.Add(new UpgradeData { id = 16, title = "Electroshock", description = "Projectiles bounce to 20 enemies.", prerequisiteId = 8, rarity = UpgradeRarity.Mythic });
        allUpgrades.Add(new UpgradeData { id = 17, title = "Multi Shot", description = "Increases projectile count by 1.", rarity = UpgradeRarity.Mythic });
        allUpgrades.Add(new UpgradeData { id = 20, title = "Luck Up", description = "Increases luck by 3%.", rarity = UpgradeRarity.Mythic });

        // Godlike (Tanrısal) - %0.01
        allUpgrades.Add(new UpgradeData { id = 22, title = "SUPERGUY!", description = "Health Regen +50/s, Damage x2, Speed x2, Projectiles x2...", rarity = UpgradeRarity.Godlike });
    }

    public void OpenPanel()
    {
        if (levelUpPanel == null || allUpgrades.Count == 0) return;

        pendingLevelUps++;

        // If panel is already active, just increment pending and return
        if (levelUpPanel.activeSelf) return;

        ShowPanel();
    }

    private void ShowPanel()
    {
        isUpgradeProcessing = false; // Unlock when opening panel
        
        // undoCount is now initialized in Awake/Start for per-scene persistence
        
        SelectRandomUpgrades();

        DisplayUpgrades();
        UpdateExtraButtons(); // Update buttons

        levelUpPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void UpdateExtraButtons()
    {
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(true);
            // Skip butonu metni varsa güncellenebilir ama genelde sabittir
        }

        if (rerollButton != null)
        if (rerollButton != null)
        {
            // Reroll butonu sadece skip puanı varsa görünür (veya her zaman görünür ama pasif olur, burada görünür yapıyoruz)
            rerollButton.gameObject.SetActive(true);
            
            if (rerollButtonText != null)
            {
                if (skipCount < 3)
                {
                    rerollButtonText.text = $"Reroll ({skipCount})";
                    rerollButton.interactable = skipCount > 0; // Puan yoksa basılamaz
                }
                else
                {
                    string guarantee = "";
                    if (skipCount >= 15) guarantee = "GODLIKE";
                    else if (skipCount >= 10) guarantee = "MYTHIC";
                    else if (skipCount >= 5) guarantee = "LEGENDARY";
                    else if (skipCount >= 3) guarantee = "EPIC";

                    rerollButtonText.text = $"EMPOWER! ({skipCount})\n<size=60%>Guaranteed {guarantee}</size>";
                    rerollButton.interactable = true;
                }
            }
        }
        if (undoButton != null)
        {
            bool hasRegreter = BossRewardManager.Instance != null && BossRewardManager.Instance.HasReward(BossRewardType.Regreter);
            undoButton.gameObject.SetActive(hasRegreter);
            undoButton.interactable = undoCount > 0 && previousSelections[0] != null; // Can undo if count > 0 and has history
            
            TMP_Text undoText = undoButton.GetComponentInChildren<TMP_Text>();
            if (undoText != null) undoText.text = $"Undo ({undoCount})";
        }
    }

    public void OnSkipButtonClicked()
    {
        skipCount++;
        ClosePanel();
    }

    public void OnRerollButtonClicked()
    {
        if (skipCount <= 0) return;

        UpgradeRarity minRarity = UpgradeRarity.Common;

        if (skipCount >= 15) minRarity = UpgradeRarity.Godlike;
        else if (skipCount >= 10) minRarity = UpgradeRarity.Mythic;
        else if (skipCount >= 5) minRarity = UpgradeRarity.Legendary;
        else if (skipCount >= 3) minRarity = UpgradeRarity.Epic;

        SaveCurrentSelectionToHistory(); // Save before reroll

        // Kartları yeniden seç (Minimum rarity ile)
        SelectRandomUpgrades(minRarity);
        DisplayUpgrades();

        // Puanları sıfırla (veya harca) - İstek: "birikmiş şanslarını bi kere... rollayabilecek"
        skipCount = 0; 
        
        UpdateExtraButtons();
    }

    public void OnUndoButtonClicked()
    {
        if (undoCount > 0 && previousSelections[0] != null)
        {
            undoCount--;
            // Restore previous selections
            System.Array.Copy(previousSelections, currentSelections, 3);
            DisplayUpgrades();
            UpdateExtraButtons();
        }
    }

    private void SaveCurrentSelectionToHistory()
    {
        System.Array.Copy(currentSelections, previousSelections, 3);
    }



    private void SelectRandomUpgrades(UpgradeRarity minRarity = UpgradeRarity.Common)
    {
        // Ön koşulları sağlananları filtrele
        var availableUpgrades = allUpgrades.Where(u => u.prerequisiteId == -1 || unlockedUpgradeIds.Contains(u.prerequisiteId)).ToList();

        // YENİ: Zincirleme (ID 8) ve Patlayıcı Mermiler (ID 9) birbirini dışlar
        if (unlockedUpgradeIds.Contains(8))
        {
            availableUpgrades.RemoveAll(u => u.id == 9);
        }
        else if (unlockedUpgradeIds.Contains(9))
        {
            availableUpgrades.RemoveAll(u => u.id == 8);
        }

        // NEW: Don't show Critical Damage (ID 19) before Critical Chance (ID 18) is unlocked
        // If crit chance is 0 (never increased), remove ID 19 from list
        if (weaponController.weaponData.critChance <= 0)
        {
            availableUpgrades.RemoveAll(u => u.id == 19);
        }

        // NEW: If Critical Chance (ID 18) reached 100%, don't show anymore
        if (weaponController.weaponData.critChance >= 100f)
        {
            availableUpgrades.RemoveAll(u => u.id == 18);
        }

        currentSelections = new UpgradeData[3];
        List<int> selectedIdsInThisRound = new List<int>(); // Bu turda seçilenleri takip et
        
        for (int i = 0; i < 3; i++)
        {
            UpgradeRarity selectedRarity;

            // EĞER REROLL İSE (minRarity > Common), DİREKT O ENDERLİĞİ ZORLA
            if (minRarity > UpgradeRarity.Common)
            {
                selectedRarity = minRarity;
            }
            else
            {
                // NORMAL LEVEL UP
                selectedRarity = DetermineRarity();
            }
            
            // 2. O enderlikteki uygun upgradeleri bul
            var pool = availableUpgrades.Where(u => u.rarity == selectedRarity).ToList();
            
            // Unique seçim yapmaya çalış
            var uniquePool = pool.Where(u => !selectedIdsInThisRound.Contains(u.id)).ToList();

            if (uniquePool.Count > 0)
            {
                pool = uniquePool;
            }
            // Eğer unique yoksa, pool olduğu gibi kalır (Duplicates allowed within the same rarity)

            // Eğer o enderlikte hiç kart yoksa (ne unique ne duplicate), mecburen fallback yapmalıyız.
            // Kullanıcı "fix" istediği için, o enderlikte kart olduğundan emin olmalıyız.
            // Eğer yoksa, mecburen bir alt enderliğe bakmak yerine, tüm havuza bakıp en yüksek enderliği seçmeye çalışabiliriz.
            // Ama şimdilik "fix" dediği için o enderlikte kart olduğunu varsayıyoruz (veya duplicate veriyoruz).
            
            if (pool.Count == 0)
            {
                 // Fallback: Eğer istenen enderlikte hiç kart yoksa, tüm havuzdan rastgele seç (Oyun kilitlenmesin)
                 var fallbackPool = availableUpgrades.Where(u => !selectedIdsInThisRound.Contains(u.id)).ToList();
                 if (fallbackPool.Count == 0) fallbackPool = availableUpgrades;
                 pool = fallbackPool;
            }

            if (pool.Count > 0)
            {
                // Rastgele seç
                UpgradeData selected = pool[Random.Range(0, pool.Count)];
                currentSelections[i] = selected;
                selectedIdsInThisRound.Add(selected.id);
            }
        }
            
        string selectionLog = "SELECTED UPGRADES: ";
        for(int i=0; i<currentSelections.Length; i++)
        {
            if(currentSelections[i] != null)
                selectionLog += $"[{i}] {currentSelections[i].title} ({currentSelections[i].rarity}) | ";
        }
        // Debug.Log(selectionLog);
    }

    private UpgradeRarity DetermineRarity()
    {
        float luckMultiplier = playerStats != null ? playerStats.luck : 1f;

        // Ağırlıklar (Toplam 10000 üzerinden)
        float commonWeight = 7000f;
        float rareWeight = 1500f * luckMultiplier;
        float epicWeight = 1000f * luckMultiplier;
        float legendaryWeight = 400f * luckMultiplier;
        float mythicWeight = 99f * luckMultiplier;
        float godlikeWeight = 1f * luckMultiplier;

        float totalWeight = commonWeight + rareWeight + epicWeight + legendaryWeight + mythicWeight + godlikeWeight;
        float randomValue = Random.Range(0, totalWeight);

        if (randomValue < commonWeight) return UpgradeRarity.Common;
        randomValue -= commonWeight;

        if (randomValue < rareWeight) return UpgradeRarity.Rare;
        randomValue -= rareWeight;

        if (randomValue < epicWeight) return UpgradeRarity.Epic;
        randomValue -= epicWeight;

        if (randomValue < legendaryWeight) return UpgradeRarity.Legendary;
        randomValue -= legendaryWeight;

        if (randomValue < mythicWeight) return UpgradeRarity.Mythic;
        
        return UpgradeRarity.Godlike;
    }

    private void DisplayUpgrades()
    {
        for (int i = 0; i < 3; i++)
        {
            if (i < currentSelections.Length)
            {
                Button btn = upgradeButtons[i];
                if (btn == null) continue;

                TMP_Text tmpText = btn.GetComponentInChildren<TMP_Text>();
                Text legacyText = btn.GetComponentInChildren<Text>();

                if (tmpText == null && legacyText == null && i < upgradeTexts.Length)
                {
                    tmpText = upgradeTexts[i];
                }

                UpgradeData data = currentSelections[i];
                
                // YENİ: Kart görselini enderliğe göre değiştir
                Image btnImage = btn.GetComponent<Image>();
                if (btnImage != null)
                {
                    switch (data.rarity)
                    {
                        case UpgradeRarity.Common:
                            if (commonCardSprite != null) btnImage.sprite = commonCardSprite;
                            break;
                        case UpgradeRarity.Rare:
                            if (rareCardSprite != null) btnImage.sprite = rareCardSprite;
                            break;
                        case UpgradeRarity.Epic:
                            if (epicCardSprite != null) btnImage.sprite = epicCardSprite;
                            break;
                        case UpgradeRarity.Legendary:
                            if (legendaryCardSprite != null) btnImage.sprite = legendaryCardSprite;
                            break;
                        case UpgradeRarity.Mythic:
                            if (mythicCardSprite != null) btnImage.sprite = mythicCardSprite;
                            break;
                        case UpgradeRarity.Godlike:
                            if (godlikeCardSprite != null) btnImage.sprite = godlikeCardSprite;
                            break;
                    }
                }

                string title = data.title;
                string statChange = GetUpgradeDescription(data);
                string description = data.description;
                
                int currentLvl = upgradeLevels.ContainsKey(data.id) ? upgradeLevels[data.id] : 0;
                bool isNew = currentLvl == 0;

                // Formatlama
                // YENİ yazısı kaldırıldı.
                
                // 1. Satır: Başlık (Ortalı)
                string titleText = $"<align=center><size=90%><b>{title}</b></size></align>";

                // 2. Satır: Açıklama (Ortalı)
                string descText = $"<align=center><size=70%>{description}</size></align>";

                // 3. Satır: İstatistik (Ortalı, en altta)
                string statText = string.IsNullOrEmpty(statChange) ? "" : $"\n\n<align=center><size=75%><b>{statChange}</b></size></align>";

                // Hepsini birleştir
                string fullTextTMP = $"{titleText}\n\n{descText}{statText}";
                string fullTextLegacy = $"{title}\n{description}\n{statChange}";

                bool textFound = false;

                if (tmpText != null)
                {
                    tmpText.textWrappingMode = TextWrappingModes.Normal;
                    tmpText.margin = new Vector4(15, 0, 15, 0); // Sol ve Sağdan 15 birim boşluk
                    tmpText.text = fullTextTMP;
                    tmpText.raycastTarget = false; 
                    textFound = true;
                }
                else if (legacyText != null)
                {
                    legacyText.text = fullTextLegacy;
                    legacyText.raycastTarget = false;
                    textFound = true;
                }
                
                if (!textFound)
                {
                    Debug.LogError($"HATA: Upgrade Butonu {i} için metin bileşeni bulunamadı!");
                }

                btn.gameObject.SetActive(true);
                
                int index = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ApplyUpgrade(index));
            }
            else
            {
                if (upgradeButtons[i] != null) upgradeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private string GetUpgradeDescription(UpgradeData data)
    {
        switch (data.id)
        {
            case 2: // Keskin Göz (Menzil)
                return $"Range: {weaponController.weaponData.Range:F1} -> {weaponController.weaponData.Range + rangeIncrease:F1}";
            case 3: // Çevik Bacaklar (Hız)
                return $"Speed: {playerMovement.moveSpeed:F1} -> {playerMovement.moveSpeed + speedIncrease:F1}";
            case 4: // Fast Bullets
                return $"Cooldown: {weaponController.weaponData.Cooldown:F2}s -> {weaponController.weaponData.Cooldown * 0.9f:F2}s";
            case 5: // Sağlam Zırh (Can)
                return $"Max Health: {playerStats.maxHealth} -> {playerStats.maxHealth + healthIncrease}";
            case 7: // Hızlı Yenileme
                return $"Regen: {playerStats.healthRegenRate:F1}/s -> {playerStats.healthRegenRate + regenRateIncrease:F1}/s";
            case 10: // Bilge (XP)
                return $"XP Multiplier: {playerStats.xpMultiplier:F1}x -> {playerStats.xpMultiplier + 0.2f:F1}x";
            case 12: // Hızlı Küreler
                float currentSpeed = orbitalWeapon != null ? orbitalWeapon.rotationSpeed : 0;
                return $"Rotation Speed: {currentSpeed:F0} -> {currentSpeed + 30f:F0}";
            case 13: // Çoklu Küre
                int currentCount = orbitalWeapon != null ? orbitalWeapon.orbitalCount : 0;
                return $"Orb Count: {currentCount} -> {currentCount + 1}";
            case 14: // Yakın Koruma
                float currentDist = orbitalWeapon != null ? orbitalWeapon.distance : 0;
                float nextDist = Mathf.Max(1f, currentDist - 0.5f);
                return $"Distance: {currentDist:F1} -> {nextDist:F1}";
            case 15: // Zincirleme Sayısı
                return $"Bounce: {weaponController.weaponData.bounceCount} -> {weaponController.weaponData.bounceCount + 1}";
            case 17: // Multi Shot
                return $"Projectile: {weaponController.weaponData.projectileCount} -> {weaponController.weaponData.projectileCount + 1}";
            case 18: // Critical Chance
                return $"Crit Chance: %{weaponController.weaponData.critChance:F0} -> %{weaponController.weaponData.critChance + 5f:F0}";
            case 19: // Critical Damage
                return $"Crit Damage: %{(weaponController.weaponData.critDamageMultiplier - 1) * 100:F0} -> %{(weaponController.weaponData.critDamageMultiplier + 0.1f - 1) * 100:F0}";
            case 20: // Luck Increase
                return $"Luck: %{(playerStats.luck - 1) * 100:F0} -> %{(playerStats.luck + 0.03f - 1) * 100:F0}";
            case 21: // Explosion Area
                return $"Area: {weaponController.weaponData.explosionRadius:F1} -> {weaponController.weaponData.explosionRadius * 1.1f:F1}";
            case 23: // Magnet
                return $"Range: {playerStats.magnetRange:F1} -> {playerStats.magnetRange + 2f:F1}";
            // Diğerleri için boş döndür ki açıklama tekrar yazılmasın
            default:
                return "";
        }
    }

    public void ApplyUpgrade(int selectionIndex)
    {
        if (isUpgradeProcessing) return; // Eğer zaten işlem yapılıyorsa çık
        if (selectionIndex < 0 || selectionIndex >= currentSelections.Length) return;

        // Tıklanan butonun ID'sini bulmaya çalış (Debug için)
        int clickedButtonID = -1;
        if(selectionIndex < upgradeButtons.Length && upgradeButtons[selectionIndex] != null)
        {
            clickedButtonID = upgradeButtons[selectionIndex].GetInstanceID();
        }

        isUpgradeProcessing = true; // İşlem başladı, kilitle
        UpgradeData chosenUpgrade = currentSelections[selectionIndex];
        
        // Debug.Log($"SEÇİLEN BUTON INDEX: {selectionIndex} | Button InstanceID: {clickedButtonID} | UYGULANAN YÜKSELTME: {chosenUpgrade.title} (ID: {chosenUpgrade.id})");

        // Yükseltmeyi alınanlar listesine ekle
        if (!unlockedUpgradeIds.Contains(chosenUpgrade.id))
        {
            unlockedUpgradeIds.Add(chosenUpgrade.id);
        }

        // Increase level
        if (upgradeLevels.ContainsKey(chosenUpgrade.id))
            upgradeLevels[chosenUpgrade.id]++;
        else
            upgradeLevels[chosenUpgrade.id] = 1;

        switch (chosenUpgrade.id)
        {
            case 1:
                weaponController.weaponData.Damage *= damageMultiplier;
                Debug.Log($"[{chosenUpgrade.title}] applied. New Damage: {weaponController.weaponData.Damage:F2}");
                break;
            case 2:
                weaponController.weaponData.Range += rangeIncrease;
                Debug.Log($"[{chosenUpgrade.title}] applied. New Range: {weaponController.weaponData.Range:F2}");
                break;
            case 3:
                playerMovement.moveSpeed += speedIncrease;
                Debug.Log($"[{chosenUpgrade.title}] applied. New Speed: {playerMovement.moveSpeed:F2}");
                break;
            case 4: // Fast Bullets - Reduces cooldown
                weaponController.weaponData.Cooldown *= 0.9f; // 10% faster firing
                Debug.Log($"[{chosenUpgrade.title}] applied. New Cooldown: {weaponController.weaponData.Cooldown:F2}");
                break;
            case 5:
                playerStats.maxHealth += healthIncrease;
                playerStats.IncreaseMaxHealth(healthIncrease); // Slider güncellemesi için
                playerStats.currentHealth += healthIncrease;
                Debug.Log($"[{chosenUpgrade.title}] applied. New Max Health: {playerStats.maxHealth}");
                break;
            case 6:
                playerStats.canRegenerateHealth = true;
                if (playerStats.healthRegenRate <= 0) playerStats.healthRegenRate = 1f;
                allUpgrades.RemoveAll(u => u.id == 6);
                Debug.Log($"[{chosenUpgrade.title}] applied. Health regeneration unlocked.");
                break;
            case 7:
                playerStats.healthRegenRate += regenRateIncrease;
                Debug.Log($"[{chosenUpgrade.title}] applied. New Regen Rate: {playerStats.healthRegenRate:F2}");
                break;
            case 8:
                weaponController.weaponData.canBounce = true;
                weaponController.weaponData.bounceCount += 2; // İlk açılışta 2 sekme ver
                allUpgrades.RemoveAll(u => u.id == 8);
                // Patlayıcı Mermiler (ID 9) artık alınamaz
                allUpgrades.RemoveAll(u => u.id == 9);
                Debug.Log($"[{chosenUpgrade.title}] applied. Projectiles bounce off enemies.");
                break;
            case 9:
                weaponController.weaponData.canExplode = true;
                allUpgrades.RemoveAll(u => u.id == 9);
                // Zincirleme (ID 8) artık alınamaz
                allUpgrades.RemoveAll(u => u.id == 8);
                Debug.Log($"[{chosenUpgrade.title}] applied. Projectiles became explosive.");
                break;
            case 10:
                playerStats.xpMultiplier += 0.2f; // %20 artış
                Debug.Log($"[{chosenUpgrade.title}] applied. New XP Multiplier: {playerStats.xpMultiplier:F2}");
                break;
            case 11: // Koruyucu Küre (İlk Alım)
                if (orbitalWeapon != null) orbitalWeapon.AddOrbital();
                allUpgrades.RemoveAll(u => u.id == 11); // Tek seferlik açılış
                Debug.Log($"[{chosenUpgrade.title}] applied. Orbital weapon unlocked.");
                break;
            case 12: // Hızlı Küreler
                if (orbitalWeapon != null) orbitalWeapon.IncreaseSpeed(30f);
                Debug.Log($"[{chosenUpgrade.title}] applied. Orb speed increased.");
                break;
            case 13: // Çoklu Küre
                if (orbitalWeapon != null) orbitalWeapon.AddOrbital();
                Debug.Log($"[{chosenUpgrade.title}] applied. New orb added.");
                break;
            case 14: // Yakın Koruma
                if (orbitalWeapon != null) 
                {
                    orbitalWeapon.DecreaseDistance(0.5f);
                    // Mesafe çok kısalırsa (örn 1.0 birim) artık bu upgrade'i listeden kaldır
                    if (orbitalWeapon.distance <= 1.5f) 
                    {
                        allUpgrades.RemoveAll(u => u.id == 14);
                    }
                }
                Debug.Log($"[{chosenUpgrade.title}] applied. Orbs moved closer.");
                break;
            case 15: // Zincirleme Sayısı
                weaponController.weaponData.bounceCount++;
                break;
            case 16: // Elektroşok
                weaponController.weaponData.canBounce = true;
                weaponController.weaponData.bounceCount = 20;
                allUpgrades.RemoveAll(u => u.id == 8); // Zincirleme'yi kaldır (zaten aldık sayılır)
                allUpgrades.RemoveAll(u => u.id == 9); // Patlayıcı'yı kaldır (çakışma)
                allUpgrades.RemoveAll(u => u.id == 16); // Tek seferlik
                break;
            case 17: // Çoklu Atış
                weaponController.weaponData.projectileCount++;
                break;
            case 18: // Kritik Şans
                weaponController.weaponData.critChance += 5f;
                break;
            case 19: // Critical Damage
                weaponController.weaponData.critDamageMultiplier += 0.1f;
                break;
            case 20: // Luck Increase
                playerStats.luck += 0.03f;
                break;
            case 21: // Explosion Area
                weaponController.weaponData.explosionRadius *= 1.1f;
                break;
            case 22: // SUPERGUY!
                // Player Stats
                float addedHealth = playerStats.maxHealth;
                playerStats.IncreaseMaxHealth(addedHealth); // Max canı 2 katına çıkar
                
                if (playerStats.healthRegenRate > 0) playerStats.healthRegenRate *= 2f;
                else playerStats.healthRegenRate = 5f;

                playerStats.xpMultiplier *= 2f;
                playerStats.magnetRange *= 2f;

                // Player Movement
                playerMovement.moveSpeed *= 2f;
                playerMovement.dashSpeed *= 2f;
                playerMovement.dashCooldown /= 2f;

                // Weapon Data
                weaponController.weaponData.Damage *= 2f;
                weaponController.weaponData.Cooldown /= 2f; // Ateş etme hızı 2 katına (süre yarıya)
                weaponController.weaponData.ProjectileSpeed *= 2f;
                weaponController.weaponData.Range *= 2f;
                weaponController.weaponData.projectileCount *= 2;
                
                if (weaponController.weaponData.canBounce) 
                    weaponController.weaponData.bounceCount *= 2;
                
                if (weaponController.weaponData.canExplode) 
                    weaponController.weaponData.explosionRadius *= 2f;
                
                weaponController.weaponData.critChance *= 2f;
                weaponController.weaponData.critDamageMultiplier *= 2f;

                // Orbital Weapon
                if (orbitalWeapon != null && orbitalWeapon.orbitalCount > 0)
                {
                    orbitalWeapon.rotationSpeed *= 2f;
                    orbitalWeapon.damage *= 2f;
                    orbitalWeapon.distance /= 2f; // Daha yakın koruma (mesafe yarıya)
                    
                    // Mevcut sayı kadar ekle (2 katına çıkar)
                    // AddOrbital hem sayıyı artırır hem de UpdateOrbitals çağırarak pozisyonları günceller
                    int countToAdd = orbitalWeapon.orbitalCount;
                    for(int i=0; i<countToAdd; i++)
                    {
                        orbitalWeapon.AddOrbital();
                    }
                }
                // allUpgrades.RemoveAll(u => u.id == 22); // ARTIK TEKRAR ALINABİLİR
                break;
            case 23: // Magnet
                playerStats.magnetRange += 2f;
                Debug.Log($"[{chosenUpgrade.title}] applied. New Magnet Range: {playerStats.magnetRange:F1}");
                break;
            
            // Melee-Specific Upgrades (For Vity)
            case 24: // Wider Slash
                {
                    MeleeWeaponController meleeWeapon = weaponController.GetComponent<MeleeWeaponController>();
                    if (meleeWeapon != null)
                    {
                        meleeWeapon.attackArcAngle += 20f;
                        Debug.Log($"[{chosenUpgrade.title}] applied. New Arc Angle: {meleeWeapon.attackArcAngle:F0}°");
                    }
                }
                break;
            case 25: // Extended Reach
                {
                    MeleeWeaponController meleeWeapon = weaponController.GetComponent<MeleeWeaponController>();
                    if (meleeWeapon != null)
                    {
                        meleeWeapon.attackRadius *= 1.15f;
                        Debug.Log($"[{chosenUpgrade.title}] applied. New Radius: {meleeWeapon.attackRadius:F2}");
                    }
                }
                break;
            case 26: // Whirlwind
                {
                    MeleeWeaponController meleeWeapon = weaponController.GetComponent<MeleeWeaponController>();
                    if (meleeWeapon != null)
                    {
                        meleeWeapon.attackSegments += 1;
                        Debug.Log($"[{chosenUpgrade.title}] applied. New Segments: {meleeWeapon.attackSegments}");
                    }
                }
                break;
            case 27: // Berserker Rage - TODO: Implement stacking attack speed
                Debug.Log($"[{chosenUpgrade.title}] applied. Berserker mode activated!");
                break;
        }

        // Tüm butonların dinleyicilerini temizle ki bir sonraki açılışta çakışma olmasın
        foreach (var btn in upgradeButtons)
        {
            if(btn != null) btn.onClick.RemoveAllListeners();
        }

        ClosePanel();
    }

    public void ClosePanel()
    {
        pendingLevelUps--;
        if (pendingLevelUps > 0)
        {
            ShowPanel(); // Show next level up
        }
        else
        {
            levelUpPanel.SetActive(false);
            Time.timeScale = 1f;
            pendingLevelUps = 0; // Safety
        }
    }
}