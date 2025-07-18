using SandBox.Conversation;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace LuanYu_ArtisanBeer
{
    internal class ArtisanBeerCampaignBehavior : CampaignBehaviorBase
    {

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, OnDailyTickTown);
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, new Action<Dictionary<string, int>>(this.LocationCharactersAreReadyToSpawn));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            this.AddArtisanWokerDialogs(starter);
        }

        private void AddArtisanWokerDialogs(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddDialogLine("artisanbeer_owner_begin", "start", "artisanbeer_npc_player", "嘿，朋友！想来点特别的吗？我们这有上等的精酿啤酒，200第纳尔一杯。要尝尝吗？", 
            ()=> CharacterObject.OneToOneConversationCharacter == PlayerEncounter.LocationEncounter.Settlement.Culture.CaravanMaster, null, 100, null);
            campaignGameStarter.AddPlayerLine("artisanbeer_buy_one", "artisanbeer_npc_player", "artisanbeer_player_buy", "当然，给我来一杯", null, 
            () =>
            {
                TaleWorlds.CampaignSystem.Roster.ItemRoster itemRoster = MobileParty.MainParty.ItemRoster;
                ItemObject artisanBeerObject = MBObjectManager.Instance.GetObject<ItemObject>("artisan_beer");
                itemRoster.AddToCounts(artisanBeerObject, 1);
                Hero.MainHero.ChangeHeroGold(-200);
                
            }
            , 100, new ConversationSentence.OnClickableConditionDelegate(this.BuyArtisanBeer_clickable_condition), null);
            campaignGameStarter.AddDialogLine("artisanbeer_buy_one_2", "artisanbeer_player_buy", "end", "明智的选择！这可是用高地麦芽酿的，包你回味无穷！\"", 
                null, null, 100, null);
            campaignGameStarter.AddPlayerLine("artisanbeer_no_buy", "artisanbeer_npc_player", "artisanbeer_nobuy", "不用了，我对啤酒没兴趣", null, null, 100, null, null);
            campaignGameStarter.AddDialogLine("artisanbeer_no_buy_2", "artisanbeer_nobuy", "close_window", "哼，不识货的家伙...下次可别后悔。", null, null, 100, null);
        }

        private bool BuyArtisanBeer_clickable_condition(out TextObject explanation)
        {
            
            if (Hero.MainHero.Gold < 200)
            {
                explanation = new TextObject("金币不足200，无法购买", null);
                return false;
            }
            explanation = TextObject.Empty;
            return true;
        }

        private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
        {
            Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("center");
            if (CampaignMission.Current.Location == locationWithId && CampaignTime.Now.IsDayTime)
            {
                this.AddShopWorkersToTownCenter(unusedUsablePointCount);
            }
        }

        private void AddShopWorkersToTownCenter(Dictionary<string, int> unusedUsablePointCount)
        {
            Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
            CharacterObject shopWorker = settlement.Culture.CaravanMaster;

            Monster monsterWithSuffix = TaleWorlds.Core.FaceGen.GetMonsterWithSuffix(shopWorker.Race, "_settlement");
            Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("center");
            int minValue;
            int maxValue;
            Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(shopWorker, out minValue, out maxValue, "");
            foreach (Workshop workshop in settlement.Town.Workshops)
            {
                if (workshop.WorkshopType.StringId == "brewery")
                {
                    LocationCharacter locationCharacter =
                           new LocationCharacter(new AgentData(
                               new SimpleAgentOrigin(shopWorker, -1, null, default(UniqueTroopDescriptor)))
                           .Monster(monsterWithSuffix).Age(MBRandom.RandomInt(minValue, maxValue)),
                           new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                           workshop.Tag, true, LocationCharacter.CharacterRelations.Neutral, null, true, false, null, false, false, true);
                    locationWithId.AddCharacter(locationCharacter);
                }
            }
        }

        private void OnDailyTickTown(Town town)
        {
            //啤酒厂收益平衡性调整
            foreach (var workshop in town.Workshops)
            {
              
                if (workshop.WorkshopType.StringId == "brewery")
                {
                    //如果是啤酒厂,则调整收益 减少百分之15的收益
                    workshop.ChangeGold(-MathF.Floor(workshop.Expense*0.15));
                    InformationManager.DisplayMessage(new InformationMessage($"{town.Name} 啤酒厂 收益减少了百分之15"));
                }

            }
        }

        public override void SyncData(IDataStore dataStore)
        {
           
        }
    }
}