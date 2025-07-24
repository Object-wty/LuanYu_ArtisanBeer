using SandBox;
using SandBox.Conversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace LuanYu_ArtisanBeer
{
    public class ArtisanBeerCampaignBehavior : CampaignBehaviorBase
    {
        private CharacterObject _artisanBrewer;
        private ItemObject _artisanBeerObject;


        public ArtisanBeerCampaignBehavior()
        {
            _instance = this;
        }
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, OnDailyTickTown);
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, new Action<Dictionary<string, int>>(this.LocationCharactersAreReadyToSpawn));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
            CampaignEvents.OnItemProducedEvent.AddNonSerializedListener(this, OnItemProduced);
        }

        private void OnItemProduced(ItemObject item, Settlement settlement, int arg3)
        {
            if (item.StringId =="beer")
            {
                InformationManager.DisplayMessage(new InformationMessage(settlement.Town.Name.ToString() + "生产了 beer"));
            }

            
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            _artisanBrewer = MBObjectManager.Instance.GetObject<CharacterObject>("artisan_brewer");
            _artisanBeerObject = MBObjectManager.Instance.GetObject<ItemObject>("artisan_beer");
            this.AddArtisanWokerDialogs(starter);
            this.AddGameMenus(starter);
        }
        
        private void AddGameMenus(CampaignGameStarter starter)
        {
            for (int i=0;i<5;i++)
            {
                AddBrewery(starter, i);

            }
            starter.AddGameMenu("brewer_workshop", "精酿啤酒厂", (MenuCallbackArgs args) => { }, GameOverlays.MenuOverlayType.SettlementWithBoth, GameMenu.MenuFlags.None, null);

            starter.AddGameMenuOption("brewer_workshop", "brewer_workshop_take_artisan_beer", "进入仓库", (MenuCallbackArgs args) =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                return true;
            }, delegate (MenuCallbackArgs x)
            {
                //实现快速拿取仓库里的精酿啤酒,到我们的背包里
                List<InquiryElement> list = new List<InquiryElement>();
                ArtisanBeerWorkShopState artisanBeerWorkShopState = GetArtisanBeerWorkShopState(_selectWorkshop);
                for(int i = 0; i < artisanBeerWorkShopState._currentStock; i++)
                {
                    list.Add(new InquiryElement(_artisanBeerObject, _artisanBeerObject.Name.ToString(), new ImageIdentifier(_artisanBeerObject)));

                }
                
                MBInformationManager.ShowMultiSelectionInquiry(
                    new MultiSelectionInquiryData(new TextObject("精酿啤酒仓库", null).ToString(), 
                    string.Empty, list, true, 1, 0, new TextObject("拿取", null).ToString(), 
                    new TextObject("{=3CpNUnVl}Cancel", null).ToString(), new Action<List<InquiryElement>>(this.QuickTakeArtisanBeer),
                    new Action<List<InquiryElement>>(this.QuickTakeArtisanBeer), "", false), false, false);
            }, true, -1, false, null);

            starter.AddGameMenuOption("brewer_workshop", "brewer_workshop_go_to_managment", "生产管理", (MenuCallbackArgs args) =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.ManageGarrison;
                return true;
            }, delegate (MenuCallbackArgs x)
            {
                ArtisanBeerManagmentState gameState = Game.Current.GameStateManager.CreateState<ArtisanBeerManagmentState>(_selectWorkshop,GetArtisanBeerWorkShopState(_selectWorkshop));
                Game.Current.GameStateManager.PushState(gameState, 0);
                
            }, true, -1, false, null);

            starter.AddGameMenuOption("brewer_workshop", "brewer_workshop_back_to_center", "{=qWAmxyYz}Back to town center", (MenuCallbackArgs args) =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }, delegate (MenuCallbackArgs x)
            {
                GameMenu.SwitchToMenu("town");
            }, true, -1, false, null);
        }

        private void QuickTakeArtisanBeer(List<InquiryElement> list)
        {
            ArtisanBeerWorkShopState artisanBeerWorkShopState = GetArtisanBeerWorkShopState(_selectWorkshop);
            artisanBeerWorkShopState.AddStock(-list.Count);
            TaleWorlds.CampaignSystem.Roster.ItemRoster itemRoster = MobileParty.MainParty.ItemRoster;

            itemRoster.AddToCounts(_artisanBeerObject, list.Count);
        }

        private void AddBrewery(CampaignGameStarter starter, int i)
        {
            starter.AddGameMenuOption("town", "brewer_workshop_go_to_brewery", "去啤酒厂", (MenuCallbackArgs args) =>
            {
                if (i >= Settlement.CurrentSettlement.Town.Workshops.Length)
                    return false;
                //城镇中有啤酒厂,且啤酒厂属于主角
                if (Settlement.CurrentSettlement.Town.Workshops[i].Owner == Hero.MainHero
                && Settlement.CurrentSettlement.Town.Workshops[i].WorkshopType.StringId == "brewery")
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Manage;
                    return true;
                }
                    return false;
            }, delegate (MenuCallbackArgs x)
            {
                _selectWorkshop = Settlement.CurrentSettlement.Town.Workshops[i];
                GameMenu.SwitchToMenu("brewer_workshop");
            }, true, -1, false, null);
        }

        private void AddArtisanWokerDialogs(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddDialogLine("artisanbeer_owner_begin", "start", "end", "嘿，兄弟,没货了,过会再来!",
          () => CharacterObject.OneToOneConversationCharacter == _artisanBrewer && GetCurrentArtisanBeerWorkshopStateStock()<=0, null, 100, null);

            campaignGameStarter.AddDialogLine("artisanbeer_owner_begin", "start", "artisanbeer_npc_player", "嘿，朋友！想来点特别的吗？我们这有上等的精酿啤酒，200第纳尔一杯。要尝尝吗？",
            () => CharacterObject.OneToOneConversationCharacter == _artisanBrewer, null, 100, null);
            campaignGameStarter.AddPlayerLine("artisanbeer_buy_one", "artisanbeer_npc_player", "artisanbeer_player_buy", "当然，给我来一杯", null,
            () =>
            {
                TaleWorlds.CampaignSystem.Roster.ItemRoster itemRoster = MobileParty.MainParty.ItemRoster;

                itemRoster.AddToCounts(_artisanBeerObject, 1);
                Hero.MainHero.ChangeHeroGold(-200);
                ArtisanBeerWorkShopState artisanBeerWorkShopState = FindCurrentArtisanBeerWorkshopState();
                artisanBeerWorkShopState.AddStock(-1);
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
            CharacterObject shopWorker = _artisanBrewer;

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
                    workshop.ChangeGold(-MathF.Floor(workshop.Expense * 0.15));
                    InformationManager.DisplayMessage(new InformationMessage($"{town.Name} 啤酒厂 收益减少了百分之15"));
                    
                    ArtisanBeerWorkShopState artisanBeerWorkShopState = GetArtisanBeerWorkShopState(workshop);
                    artisanBeerWorkShopState.AddStock(artisanBeerWorkShopState._dailyProctionAmount);

                }

            }
        }

        static ArtisanBeerCampaignBehavior  _instance;
        public static float GetNormalBeerEff(Workshop workshop)
        {
            if (workshop.WorkshopType.StringId == "brewery")
            {

                ArtisanBeerWorkShopState artisanBeerWorkShopState = _instance.GetArtisanBeerWorkShopState(workshop);
                float value = (5- artisanBeerWorkShopState._dailyProctionAmount)/5.0f;
                return value;
            }
            else
            {
                return 1;
            }
        }

        public ArtisanBeerWorkShopState GetArtisanBeerWorkShopState(Workshop workshop)
        {

            string id = workshop.Settlement.Town.StringId + "_" + workshop.Tag;
            ArtisanBeerWorkShopState artisanBeerWorkShopState;
            if (_artisanBeerWorkShopState.TryGetValue(id, out artisanBeerWorkShopState))
            {
               
                return artisanBeerWorkShopState;
            }
            else
            {
                artisanBeerWorkShopState = new ArtisanBeerWorkShopState() { _currentStock = 0, _dailyProctionAmount = 1, _maxStock = 10 };
                _artisanBeerWorkShopState.Add(id, artisanBeerWorkShopState);
                return artisanBeerWorkShopState;
            }

        }

        public Dictionary<string, ArtisanBeerWorkShopState> _artisanBeerWorkShopState = new Dictionary<string, ArtisanBeerWorkShopState>();
        private Workshop _selectWorkshop;

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_artisanBeerWorkShopState", ref _artisanBeerWorkShopState);
        }

        public ArtisanBeerWorkShopState FindCurrentArtisanBeerWorkshopState()
        {
            Workshop workshop = this.FindCurrentWorkshop(ConversationMission.OneToOneConversationAgent);
            ArtisanBeerWorkShopState artisanBeerWorkShopState = GetArtisanBeerWorkShopState(workshop);
            if(artisanBeerWorkShopState != null ) return artisanBeerWorkShopState;
            return null;
        }

        public int GetCurrentArtisanBeerWorkshopStateStock()
        {
            ArtisanBeerWorkShopState artisanBeerWorkShopState = FindCurrentArtisanBeerWorkshopState();
            int _currentStock = artisanBeerWorkShopState._currentStock;
            if(_currentStock >0) return _currentStock;
            return 0;
        }
        private Workshop FindCurrentWorkshop(Agent agent)
        {
            if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsTown)
            {
                CampaignAgentComponent component = agent.GetComponent<CampaignAgentComponent>();
                AgentNavigator agentNavigator = (component != null) ? component.AgentNavigator : null;
                if (agentNavigator != null)
                {
                    foreach (Workshop workshop in Settlement.CurrentSettlement.Town.Workshops)
                    {
                        if (workshop.Tag == agentNavigator.SpecialTargetTag)
                        {
                            return workshop;
                        }
                    }
                }
            }
            return null;
        }

    }

    public class ArtisanBeerSaveDefiner : SaveableTypeDefiner
    {
        // use a big number and ensure that no other mod is using a close range
        public ArtisanBeerSaveDefiner() : base(828_333_000) { }

        protected override void DefineClassTypes()
        {
            // The Id's here are local and will be related to the Id passed to the constructor
            AddClassDefinition(typeof(ArtisanBeerWorkShopState), 1);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(Dictionary<string, ArtisanBeerWorkShopState>));
        }
    }

    public class ArtisanBeerWorkShopState
    {
        [SaveableField(1)]
        public int _currentStock;
        [SaveableField(2)]
        public int _maxStock;
        [SaveableField(3)]
        public int _dailyProctionAmount;

        public void AddStock(int amount)
        {
            _currentStock += amount;
            if (_currentStock > _maxStock) _currentStock = _maxStock;

            if (_currentStock < 0)
                throw new Exception("ArtisanBeer _currentStock不可为负");

        }

    }
}