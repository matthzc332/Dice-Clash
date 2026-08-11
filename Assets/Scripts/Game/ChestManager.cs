using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChestSlot
{
    public bool occupied;
    public long openStartTimestamp;
    public bool ready;
}

[Serializable]
public struct ChestReward
{
    public int gold;
    public List<string> insignias;
    public List<bool> isDuplicate;
}

public static class ChestManager
{
    private const string SLOT_PREFIX = "ChestSlot_";
    private const int MAX_SLOTS = 2;
    private const int BASE_GOLD = 50;

    private static ChestSlot[] _slots;

    public static ChestSlot[] Slots
    {
        get
        {
            if (_slots == null) LoadSlots();
            return _slots;
        }
    }

    static void LoadSlots()
    {
        _slots = new ChestSlot[MAX_SLOTS];
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            _slots[i] = new ChestSlot();
            string data = PlayerPrefs.GetString(SLOT_PREFIX + i, "");
            if (!string.IsNullOrEmpty(data))
            {
                string[] parts = data.Split('|');
                if (parts.Length >= 2)
                {
                    _slots[i].occupied = parts[0] == "1";
                    _slots[i].openStartTimestamp = long.Parse(parts[1]);
                }
            }
            UpdateSlotReady(i);
        }
    }

    static void SaveSlot(int index)
    {
        var slot = _slots[index];
        string data = slot.occupied ? $"1|{slot.openStartTimestamp}" : "0|0";
        PlayerPrefs.SetString(SLOT_PREFIX + index, data);
        PlayerPrefs.Save();
    }

    static void UpdateSlotReady(int index)
    {
        var slot = _slots[index];
        if (!slot.occupied)
        {
            slot.ready = false;
            return;
        }
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long elapsed = now - slot.openStartTimestamp;
        int openTimeSeconds = (EconomyConfig.Load()?.chestConfig?.openTimeHours ?? 8) * 3600;
        slot.ready = elapsed >= openTimeSeconds;
    }

    public static float GetSlotProgress(int index)
    {
        if (index < 0 || index >= MAX_SLOTS) return 0;
        var slot = Slots[index];
        if (!slot.occupied) return 0;

        int openTimeSeconds = (EconomyConfig.Load()?.chestConfig?.openTimeHours ?? 8) * 3600;
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long elapsed = now - slot.openStartTimestamp;
        return Mathf.Clamp01((float)elapsed / openTimeSeconds);
    }

    public static float GetSlotTimeRemainingHours(int index)
    {
        if (index < 0 || index >= MAX_SLOTS) return 0;
        var slot = Slots[index];
        if (!slot.occupied) return 0;

        int openTimeSeconds = (EconomyConfig.Load()?.chestConfig?.openTimeHours ?? 8) * 3600;
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long elapsed = now - slot.openStartTimestamp;
        long remaining = openTimeSeconds - elapsed;
        return Mathf.Max(0, remaining / 3600f);
    }

    public static bool HasEmptySlot()
    {
        foreach (var slot in Slots)
        {
            if (!slot.occupied) return true;
        }
        return false;
    }

    public static bool AddChest()
    {
        LoadSlots();
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            if (!_slots[i].occupied)
            {
                _slots[i].occupied = true;
                _slots[i].openStartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                _slots[i].ready = false;
                SaveSlot(i);
                Debug.Log($"[Chest] Chest added to slot {i}");
                return true;
            }
        }
        Debug.Log("[Chest] No empty slots available");
        return false;
    }

    public static ChestReward OpenChest(int slotIndex)
    {
        LoadSlots();
        ChestReward reward = new ChestReward
        {
            gold = 0,
            insignias = new List<string>(),
            isDuplicate = new List<bool>()
        };

        if (slotIndex < 0 || slotIndex >= MAX_SLOTS) return reward;
        UpdateSlotReady(slotIndex);

        var slot = _slots[slotIndex];
        if (!slot.occupied || !slot.ready) return reward;

        reward.gold = BASE_GOLD;

        int insigniasPerChest = EconomyConfig.Load()?.chestConfig?.insigniasPerChest ?? 3;
        var chestInsignias = InsigniaData.GetBySource("chest");
        if (chestInsignias == null || chestInsignias.Length == 0)
        {
            ClearSlot(slotIndex);
            return reward;
        }

        for (int i = 0; i < insigniasPerChest; i++)
        {
            string id = RollInsigniaProgressive(chestInsignias);
            if (id != null)
            {
                bool wasAlready = InsigniaManager.HasInsignia(id);
                reward.insignias.Add(id);
                reward.isDuplicate.Add(wasAlready);
                InsigniaManager.GrantInsignia(id);
            }
        }

        ClearSlot(slotIndex);
        return reward;
    }

    static void ClearSlot(int slotIndex)
    {
        _slots[slotIndex].occupied = false;
        _slots[slotIndex].ready = false;
        _slots[slotIndex].openStartTimestamp = 0;
        SaveSlot(slotIndex);
    }

    static string RollInsigniaProgressive(Insignia[] pool)
    {
        int collected = InsigniaManager.GetCollectedBySource("chest");
        int total = InsigniaManager.GetTotalBySource("chest");

        float chanceNew = 0.2f;
        if (total > 0)
        {
            float pct = (float)collected / total;
            if (pct < 0.5f) chanceNew = 0.8f;
            else if (pct < 0.8f) chanceNew = 0.5f;
            else chanceNew = 0.2f;
        }

        bool wantNew = collected < total && UnityEngine.Random.value < chanceNew;

        if (wantNew)
        {
            List<Insignia> uncollected = new List<Insignia>();
            foreach (var ins in pool)
            {
                if (!InsigniaManager.HasInsignia(ins.id))
                    uncollected.Add(ins);
            }
            if (uncollected.Count > 0)
                return uncollected[UnityEngine.Random.Range(0, uncollected.Count)].id;
        }

        return RollInsigniaByRarity(pool);
    }

    static string RollInsigniaByRarity(Insignia[] pool)
    {
        float roll = UnityEngine.Random.value;
        string targetRarity;
        if (roll < 0.05f) targetRarity = "legendary";
        else if (roll < 0.20f) targetRarity = "epic";
        else if (roll < 0.50f) targetRarity = "rare";
        else targetRarity = "common";

        List<Insignia> filtered = new List<Insignia>();
        foreach (var ins in pool)
        {
            if (ins.rarity == targetRarity) filtered.Add(ins);
        }

        if (filtered.Count == 0) filtered.AddRange(pool);
        if (filtered.Count == 0) return null;

        return filtered[UnityEngine.Random.Range(0, filtered.Count)].id;
    }

    public static void TryGrantChestAfterLevel(int levelId)
    {
        if (!HasEmptySlot()) return;

        float chance = 0.4f;
        if (levelId >= 16) chance = 0.7f;
        else if (levelId >= 10) chance = 0.55f;

        if (UnityEngine.Random.value < chance)
        {
            AddChest();
            Debug.Log($"[Chest] Chest awarded after level {levelId} ({chance * 100}%)");
        }
    }

    public static void ResetAll()
    {
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            PlayerPrefs.DeleteKey(SLOT_PREFIX + i);
        }
        PlayerPrefs.Save();
        _slots = null;
    }

    public static bool AddReadyChestForTest()
    {
        LoadSlots();
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            if (!_slots[i].occupied)
            {
                _slots[i].occupied = true;
                int openTimeSeconds = (EconomyConfig.Load()?.chestConfig?.openTimeHours ?? 8) * 3600;
                _slots[i].openStartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - openTimeSeconds;
                _slots[i].ready = true;
                SaveSlot(i);
                Debug.Log($"[Chest] Test ready chest added to slot {i}");
                return true;
            }
        }
        Debug.Log("[Chest] No empty slots available");
        return false;
    }
}
