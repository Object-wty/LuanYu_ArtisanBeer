using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace LuanYu_ArtisanBeer
{
    internal class ArtisanBeerCampaignBehavior : CampaignBehaviorBase
    {

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, OnDailyTickTown);
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, new Action<Dictionary<string, int>>(this.LocationCharactersAreReadyToSpawn));
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