using System.Collections.Generic;

/// <summary>
/// 장비 아이템 id → CharacterArmorPart 매핑 (CharacterEquipmentSlot 11종과 대응).
/// </summary>
public static class ItemArmorPartTable
{
    static readonly Dictionary<int, CharacterArmorPart> PartByItemId = new Dictionary<int, CharacterArmorPart>
    {
        { 10010, CharacterArmorPart.Hat },
        { 10011, CharacterArmorPart.Top },
        { 10012, CharacterArmorPart.Bottom },
        { 10013, CharacterArmorPart.Gloves },
        { 10014, CharacterArmorPart.Cape },
        { 10015, CharacterArmorPart.Shoulder },
        { 10016, CharacterArmorPart.Belt },
        { 10017, CharacterArmorPart.Shoes },
        { 10018, CharacterArmorPart.Ring },
        { 10019, CharacterArmorPart.Ring },
        { 10020, CharacterArmorPart.Necklace },
    };

    public static bool TryGetPart(int itemId, out CharacterArmorPart part)
    {
        return PartByItemId.TryGetValue(itemId, out part);
    }
}
