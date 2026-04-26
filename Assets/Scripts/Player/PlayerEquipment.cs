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

    public bool Equip(ItemData item, out ItemData previousItem)
    {
        previousItem = null;

        if (item == null || !item.IsEquippable || item.EquipmentSlot == EquipmentSlotType.None)
        {
            return false;
        }

        if (!CanEquipForCurrentClass(item))
        {
            return false;
        }

        switch (item.EquipmentSlot)
        {
            case EquipmentSlotType.Head:
                previousItem = head;
                head = item;
                break;

            case EquipmentSlotType.Chest:
                previousItem = chest;
                chest = item;
                break;

            case EquipmentSlotType.Legs:
                previousItem = legs;
                legs = item;
                break;

            case EquipmentSlotType.Feet:
                previousItem = feet;
                feet = item;
                break;

            case EquipmentSlotType.Weapon:
                previousItem = weapon;
                weapon = item;
                break;

            case EquipmentSlotType.Offhand:
                previousItem = offhand;
                offhand = item;
                break;

            default:
                return false;
        }

        ApplyStatBonuses();
        OnEquipmentChanged?.Invoke();
        return true;
    }

    public bool Unequip(EquipmentSlotType slotType, out ItemData removedItem)
    {
        removedItem = GetEquippedItem(slotType);

        if (removedItem == null)
        {
            return false;
        }

        switch (slotType)
        {
            case EquipmentSlotType.Head:
                head = null;
                break;

            case EquipmentSlotType.Chest:
                chest = null;
                break;

            case EquipmentSlotType.Legs:
                legs = null;
                break;

            case EquipmentSlotType.Feet:
                feet = null;
                break;

            case EquipmentSlotType.Weapon:
                weapon = null;
                break;

            case EquipmentSlotType.Offhand:
                offhand = null;
                break;

            default:
                return false;
        }

        ApplyStatBonuses();
        OnEquipmentChanged?.Invoke();
        return true;
    }

    private bool CanEquipForCurrentClass(ItemData item)
    {
        if (item == null)
        {
            return false;
        }

        if (item.WeaponType == WeaponType.None)
        {
            return true;
        }

        CharacterClassData selectedClass = classController != null
            ? classController.SelectedClass
            : null;

        if (selectedClass == null)
        {
            return true;
        }

        if (selectedClass.CanUseWeaponType(item.WeaponType))
        {
            return true;
        }

        PostSystem($"{selectedClass.ClassName} cannot equip {item.WeaponType} weapons.");
        return false;
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

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}