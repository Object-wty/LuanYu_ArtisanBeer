using System;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ObjectSystem;

namespace LuanYu_ArtisanBeer
{
    public class ArtisanBeerMissonView : MissionView
    {
        

        public override void OnMissionTick(float dt)
        {

            if (TaleWorlds.InputSystem.Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.Q))
            {
                if (!(Mission.Mode is MissionMode.Battle or MissionMode.Stealth))
                    return;

                //在战斗过程中 按Q,消耗一个 artisan beer,回血20
                TaleWorlds.CampaignSystem.Roster.ItemRoster itemRoster = MobileParty.MainParty.ItemRoster;
                ItemObject artisanBeerObject = MBObjectManager.Instance.GetObject<ItemObject>("artisan_beer");
                if (itemRoster.GetItemNumber(artisanBeerObject) <= 0)
                {
                    InformationManager.DisplayMessage(new InformationMessage("Artisan Beer 库存不足"));
                    return;
                }

                if (Mission.MainAgent.Health >= Mission.MainAgent.HealthLimit)
                {
                    InformationManager.DisplayMessage(new InformationMessage("血量满,不可以使用"));
                    return;
                }

                itemRoster.AddToCounts(artisanBeerObject, -1);

                Mission.MainAgent.Health += 20;

                InformationManager.DisplayMessage(new InformationMessage($"加了20滴血"));

            }

        }

    }
}