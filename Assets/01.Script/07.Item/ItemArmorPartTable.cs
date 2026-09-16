using System.Collections.Generic;

/// <summary>
/// 장비 아이템 id → 착용 부위 매핑.
/// CharacterEquipment.armorPartRules 대신 ItemFacade에서 조회할 때 사용한다.
/// </summary>
public static class ItemArmorPartTable
{
    static readonly Dictionary<int, CharacterArmorPart> PartByItemId = new Dictionary<int, CharacterArmorPart>
    {
        // 필수 6종 (투구·갑옷·신발·장갑·반지·목걸이)
        { 10010, CharacterArmorPart.Hat },       // 투구
        { 10011, CharacterArmorPart.Top },       // 갑옷(상의)
        { 10012, CharacterArmorPart.Shoes },     // 신발
        { 10013, CharacterArmorPart.Gloves },    // 장갑
        { 10014, CharacterArmorPart.Ring },      // 반지
        { 10015, CharacterArmorPart.Necklace },  // 목걸이

        // 나머지 5슬롯 — 고가 장비(아까운 슬롯)
        { 10016, CharacterArmorPart.Bottom },    // 하의
        { 10017, CharacterArmorPart.Cape },      // 망토
        { 10018, CharacterArmorPart.Shoulder },  // 견갑
        { 10019, CharacterArmorPart.Belt },      // 벨트
        { 10020, CharacterArmorPart.Ring },      // 반지2
    };

    public static bool TryGetPart(int itemId, out CharacterArmorPart part)
    {
        return PartByItemId.TryGetValue(itemId, out part);
    }

    public static bool IsEquipment(int itemId)
    {
        return PartByItemId.ContainsKey(itemId);
    }
}
