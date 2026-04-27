using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerCombat))]
public class PlayerEquipment : MonoBehaviour
{
    private Health health;
    private PlayerCombat combat;
    private PlayerClassController classController;

    private ItemData head;
    private ItemData chest;
    private ItemData legs;
    private ItemData feet;
    private ItemData weapon;
    private ItemData offhand;

    public event Action OnEquipmentChanged;

    private void Awake()
    {
        health = GetComponent<Health>();
        combat = GetComponent<PlayerCombat>();
        classController = GetComponent<PlayerClassController>();

        ApplyStatBonuses();
    }

    public ItemData GetEquippedItem(EquipmentSlotType slotType)
    {
        return slotType switch
        {
            EquipmentSlotType.Head => head,
            EquipmentSlotType.Chest => chest,
            EquipmentSlotType.Legs => legs,
            EquipmentSlotType.Feet => feet,
            EquipmentSlotType.Weapon => weapon,
            EquipmentSlotType.Offhand => offhand,
            _ => null
        };
    }

    public ItemData GetEquippedWeapon()
    {
        return weapon;
    }

    public bool IsOffhandBlockedByTwoHandedWeapon()
    {
        return weapon != null && weapon.IsTwoHanded;
    }

    public bool CanEquipItemInSlot(ItemData item, EquipmentSlotType targetSlotType)
    {
        return string.IsNullOrWhiteSpace(GetCannotEquipReason(item, targetSlotType));
    }

    public string GetCannotEquipReason(ItemData item, EquipmentSlotType targetSlotType)
    {
        if (item == null)
        {
            return "No item selected.";
        }

        if (!item.IsEquippable)
        {
            return $"{item.DisplayName} cannot be equipped.";
        }

        if (!item.CanEquipInSlot(targetSlotType))
        {
            return $"{item.DisplayName} cannot be equipped there.";
        }

        if (targetSlotType == EquipmentSlotType.Offhand && IsOffhandBlockedByTwoHandedWeapon())
        {
            return $"You cannot equip an offhand item while using {weapon.DisplayName}.";
        }

        if (targetSlotType == EquipmentSlotType.Weapon && item.IsTwoHanded && offhand != null)
        {
            return $"Unequip your offhand item before equipping {item.DisplayName}.";
        }

        CharacterClassData selectedClass = classController != null ? classController.SelectedClass : null;

        if (selectedClass == null || item.WeaponType == WeaponType.None)
        {
            return string.Empty;
        }

        if (targetSlotType == EquipmentSlotType.Weapon)
        {
            if (!selectedClass.CanUseMainHandWeaponType(item.WeaponType))
            {
                return $"{selectedClass.ClassName} cannot equip {FormatWeaponType(item.WeaponType)}.";
            }
        }

        if (targetSlotType == EquipmentSlotType.Offhand)
        {
            if (item.IsWeapon && !item.IsOffhandOnly && !selectedClass.CanDualWieldWeapons)
            {
                return $"{selectedClass.ClassName} cannot dual wield weapons.";
            }

            if (!selectedClass.CanUseOffhandWeaponType(item.WeaponType))
            {
                return $"{selectedClass.ClassName} cannot equip {FormatWeaponType(item.WeaponType)} in the offhand.";
            }
        }

        return string.Empty;
    }

    public bool Equip(ItemData item, out ItemData previousItem)
    {
        EquipmentSlotType targetSlot = item != null ? item.EquipmentSlot : EquipmentSlotType.None;
        return Equip(item, targetSlot, out previousItem);
    }

    public bool Equip(ItemData item, EquipmentSlotType targetSlotType, out ItemData previousItem)
    {
        previousItem = null;

        string reason = GetCannotEquipReason(item, targetSlotType);
        if (!string.IsNullOrWhiteSpace(reason))
        {
            PostSystem(reason);
            return false;
        }

        previousItem = GetEquippedItem(targetSlotType);
        SetEquippedItem(targetSlotType, item);

        ApplyStatBonuses();
        OnEquipmentChanged?.Invoke();

        return true;
    }

    public bool MoveEquippedItemToSlot(EquipmentSlotType sourceSlotType, EquipmentSlotType targetSlotType)
    {
        if (sourceSlotType == targetSlotType)
        {
            return false;
        }

        ItemData sourceItem = GetEquippedItem(sourceSlotType);
        if (sourceItem == null)
        {
            return false;
        }

        string reason = GetCannotEquipReason(sourceItem, targetSlotType);
        if (!string.IsNullOrWhiteSpace(reason))
        {
            PostSystem(reason);
            return false;
        }

        ItemData targetItem = GetEquippedItem(targetSlotType);

        if (targetItem != null)
        {
            string reverseReason = GetCannotEquipReason(targetItem, sourceSlotType);
            if (!string.IsNullOrWhiteSpace(reverseReason))
            {
                PostSystem(reverseReason);
                return false;
            }
        }

        SetEquippedItem(sourceSlotType, targetItem);
        SetEquippedItem(targetSlotType, sourceItem);

        ApplyStatBonuses();
        OnEquipmentChanged?.Invoke();

        PostSystem($"You move {sourceItem.DisplayName} to {GetSlotDisplayName(targetSlotType)}.");

        return true;
    }

    public bool Unequip(EquipmentSlotType slotType, out ItemData removedItem)
    {
        removedItem = null;

        if (slotType == EquipmentSlotType.Offhand && IsOffhandBlockedByTwoHandedWeapon())
        {
            PostSystem("The offhand slot is occupied by your two-handed weapon.");
            return false;
        }

        removedItem = GetEquippedItem(slotType);

        if (removedItem == null)
        {
            return false;
        }

        SetEquippedItem(slotType, null);

        ApplyStatBonuses();
        OnEquipmentChanged?.Invoke();

        return true;
    }

    private void SetEquippedItem(EquipmentSlotType slotType, ItemData item)
    {
        switch (slotType)
        {
            case EquipmentSlotType.Head:
                head = item;
                break;
            case EquipmentSlotType.Chest:
                chest = item;
                break;
            case EquipmentSlotType.Legs:
                legs = item;
                break;
            case EquipmentSlotType.Feet:
                feet = item;
                break;
            case EquipmentSlotType.Weapon:
                weapon = item;
                break;
            case EquipmentSlotType.Offhand:
                offhand = item;
                break;
        }
    }

    private void ApplyStatBonuses()
    {
        int totalHealthBonus = 0;
        int totalDamageBonus = 0;

        AddBonuses(head, ref totalHealthBonus, ref totalDamageBonus);
        AddBonuses(chest, ref totalHealthBonus, ref totalDamageBonus);
        AddBonuses(legs, ref totalHealthBonus, ref totalDamageBonus);
        AddBonuses(feet, ref totalHealthBonus, ref totalDamageBonus);
        AddBonuses(weapon, ref totalHealthBonus, ref totalDamageBonus);
        AddBonuses(offhand, ref totalHealthBonus, ref totalDamageBonus);

        if (health != null)
        {
            health.SetEquipmentBonusHealth(totalHealthBonus);
        }

        if (combat != null)
        {
            combat.SetEquipmentBonusDamage(totalDamageBonus);
        }
    }

    private void AddBonuses(ItemData item, ref int totalHealthBonus, ref int totalDamageBonus)
    {
        if (item == null)
        {
            return;
        }

        totalHealthBonus += item.HealthBonus;
        totalDamageBonus += item.DamageBonus;
    }

    private string GetSlotDisplayName(EquipmentSlotType slotType)
    {
        return slotType switch
        {
            EquipmentSlotType.Head => "Head",
            EquipmentSlotType.Chest => "Chest",
            EquipmentSlotType.Legs => "Legs",
            EquipmentSlotType.Feet => "Feet",
            EquipmentSlotType.Weapon => "Weapon",
            EquipmentSlotType.Offhand => "Offhand",
            _ => "Unknown"
        };
    }

    private string FormatWeaponType(WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.OneHandedSword => "one-handed sword",
            WeaponType.TwoHandedSword => "two-handed sword",
            WeaponType.OneHandedAxe => "one-handed axe",
            WeaponType.TwoHandedAxe => "two-handed axe",
            WeaponType.OneHandedMace => "one-handed mace",
            WeaponType.TwoHandedMace => "two-handed mace",
            WeaponType.Dagger => "dagger",
            WeaponType.Staff => "staff",
            WeaponType.Wand => "wand",
            WeaponType.Bow => "bow",
            WeaponType.Crossbow => "crossbow",
            WeaponType.Shield => "shield",
            WeaponType.Grimoire => "grimoire",
            _ => "weapon"
        };
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}