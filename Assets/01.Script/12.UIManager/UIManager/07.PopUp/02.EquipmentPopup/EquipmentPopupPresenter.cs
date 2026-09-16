using System;
using System.Collections.Generic;
using UnityEngine;


//안타깝게도 현재 아이템 구현량이 현저히 적어 아이템을 6개로 줄였습니다ㅣ
//투구, 갑옷, 장갑, 신발, 목걸이, 링 입니다.
//구현량이 또 너무 많으실까봐 기능도 줄였습니다.
//2. 인벤토리에서 누르면 해당. 슬롯칸으로 장착됨. 인벤에 있던 거 그냥 파괴
//3. 슬롯창 누르면 인벤에 이동.
// 비합리적인 구조이지만 그냥 구현 때문에 그렇게 한거니 신경쓰지 마세요.
// 이 부분은 타당성 검토를 합니다. 이건 반드시 있을 수 밖에 없는 멤버나 프로퍼티가 아니라 콜렉션의 데이터를 가져오는 거라 그렇습니다.


/*
IReadOnlyList<CharacterInventoryEquipment> GetEquipmentInventory();

bool TryGetItemData(int itemId, out ItemData data);

void EquipItem(Guid instanceId);

아이템 휙득 이벤트(인벤토리 변경 이벤트)
장비 장착 이벤트

 */


public class EquipmentPopupPresenter
{
    EquipmentPopupView view;
    CharacterFacade model;

    //참조 : 아이템 휙득 이벤트
    //참조 : 장비 장착 이벤트

    private IReadOnlyList<CharacterInventoryEquipment> currentItems; //스냅샷.

    public readonly CharacterEquipmentSlot[] displayedEquipmentSlots =
    {
        CharacterEquipmentSlot.Hat,
        CharacterEquipmentSlot.Top,       // Armor
        CharacterEquipmentSlot.Shoes,
        CharacterEquipmentSlot.Necklace,
        CharacterEquipmentSlot.Gloves,
        CharacterEquipmentSlot.Ring1
    };

    public EquipmentPopupPresenter(EquipmentPopupView view, CharacterFacade model)
    {
        this.view = view;
        this.model = model;
        view.OnItemClicked += HandleItemClicked;


        // model.아이템휙득이벤트 += HandleInventoryChanged;
        // model.장비 장착 이벤트 +=HandleInventoryChanged;
    }
    public void Refresh()
    {
        RefreshInventory();
        RefreshEquipment();
    }
   
    public void RefreshInventory()
    {

        //스냅샷인 IReadOnlyList<CharacterInventoryEquipment> currentItems; 에 대입할 정보.
        //currentItems = model.GetEquipmentInventory(); 비슷한 메서드가 있던데, 파사드에서 접근해야 합니다.


        List<Sprite> icons = new List<Sprite>(currentItems.Count);

        for (int i = 0; i < currentItems.Count; i++)
        {
            CharacterInventoryEquipment item = currentItems[i];
            // item에는 Sprite가 없으므로 ItemId로 원본 데이터 조회할 메서드. 필요. 이 형태로 맞춰주세요.
            if (/*model.TryGetItemData(item.ItemId, out ItemData itemData)*/ true) //true는 오류때문에 적은거에요.
            {
                //icons.Add(itemData.Icon); //리스트에 추가하는 겁니다 신경쓸 필요 없음.
            }
            else
            {
                //icons.Add(null); 사소한 오류라 잠시 주석처리
            }
        }

        view.SetInventory(icons);
    }      

    private void HandleItemClicked(int itemIndex)
    {
        if (currentItems == null) return;

        if (itemIndex < 0 || itemIndex >= currentItems.Count) return;

        CharacterInventoryEquipment selectedItem = currentItems[itemIndex];
              
        // model.EquipItem(selectedItem.InstanceId); //아이템 장착 메서드. 인자는 맞춰주세요.

        RefreshEquipment();
    }
    private void RefreshEquipment()
    {
        for (int i = 0; i < displayedEquipmentSlots.Length; i++)
        {
            CharacterEquipmentSlot slot = displayedEquipmentSlots[i];

            RefreshEquipmentSlot(slot);
        }
    }
    private void RefreshEquipmentSlot(CharacterEquipmentSlot slot)
    {
        // 현재 해당 부위에 장비가 없으면 비움
        if (!model.TryGetEquippedItem(slot, out CharacterInventoryEquipment equipment)) //이부분은 있는 거 같아서 가져다 넣었습니다.
        {
            view.ClearEquippedItem(slot);
            return;
        }

        // 장비 인스턴스의 ItemId로 원본 아이템 데이터 조회
        if (/*!model.TryGetItemData(equipment.ItemId, out ItemData itemData)*/ true) //맨 위에거랑 같은거
        {
            view.ClearEquippedItem(slot);
            return;
        }

        //view.SetEquippedItem(slot, itemData.Icon);
    }

    private void HandleEquipmentChanged()
    {
        RefreshEquipment();
    }
    private void HandleInventoryChanged()
    {
        RefreshInventory();
    }


}

