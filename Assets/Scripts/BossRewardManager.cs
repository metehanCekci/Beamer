using UnityEngine;
using System.Collections.Generic;

public enum BossRewardType
{
    None,
    LetsGoGambling,
    BeamerBoy,
    Vampire,
    Regreter,
    PhantomDash,
    ShatteredCogs,
    Executioner,
    Overload,
    IAmTheBoss,
    Bandit
}

public class BossRewardManager : MonoBehaviour
{
    public static BossRewardManager Instance { get; private set; }

    public HashSet<BossRewardType> activeRewards = new HashSet<BossRewardType>();

    // Beamer Boy Counter
    public int beamerBoyKillCount = 0;
    public int beamerBoyThreshold = 5;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddReward(BossRewardType reward)
    {
        if (!activeRewards.Contains(reward))
        {
            activeRewards.Add(reward);
            // ApplyRewardEffect(reward); // Removed: Effects are now handled by other managers checking HasReward
        }
    }

    public bool HasReward(BossRewardType reward)
    {
        return activeRewards.Contains(reward);
    }

    public void OnEnemyKilled()
    {
        if (HasReward(BossRewardType.BeamerBoy))
        {
            beamerBoyKillCount++;
        }
    }

    public bool CheckBeamerBoyReady()
    {
        if (HasReward(BossRewardType.BeamerBoy) && beamerBoyKillCount >= beamerBoyThreshold)
        {
            beamerBoyKillCount = 0; // Reset
            return true;
        }
        return false;
    }

    public List<BossRewardType> GetRandomRewards(int count)
    {
        List<BossRewardType> allRewards = new List<BossRewardType>();
        foreach (BossRewardType type in System.Enum.GetValues(typeof(BossRewardType)))
        {
            if (type != BossRewardType.None && !activeRewards.Contains(type))
            {
                allRewards.Add(type);
            }
        }

        List<BossRewardType> selected = new List<BossRewardType>();
        for (int i = 0; i < count; i++)
        {
            if (allRewards.Count == 0) break;
            int randomIndex = Random.Range(0, allRewards.Count);
            selected.Add(allRewards[randomIndex]);
            allRewards.RemoveAt(randomIndex);
        }
        return selected;
    }

    public string GetRewardDescription(BossRewardType type)
    {
        switch (type)
        {
            case BossRewardType.LetsGoGambling: return "Start each level with 5 Free Rerolls.";
            case BossRewardType.BeamerBoy: return "Every 5 kills, your next attack fires a powerful Beam.";
            case BossRewardType.Vampire: return "Heal for 10% of damage dealt.";
            case BossRewardType.Regreter: return "Unlock 'Undo' button in upgrade menu (3 uses).";
            case BossRewardType.PhantomDash: return "Dash creates an explosion damaging enemies.";
            case BossRewardType.ShatteredCogs: return "Orbitals periodically emit damaging waves.";
            case BossRewardType.Executioner: return "Insta-kill enemies < 20% HP. Double damage to Bosses < 20% HP.";
            case BossRewardType.Overload: return "Deal 2x Damage, but take 50% more damage.";
            case BossRewardType.IAmTheBoss: return "Gain a passive ability from the defeated boss.";
            case BossRewardType.Bandit: return "Enemies drop 2x XP/Coins.";
            default: return "Unknown Reward";
        }
    }
}
