using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace LuanYu_ArtisanBeer
{
    public class ArtisanBeerVM :ViewModel
    {
        private readonly Mission _mission;
        private bool _isVisible;
        private int _artisanBeerCount;

        public ArtisanBeerVM(Mission mission)
        {
            this._mission = mission;
            OnMissionChanged();
            TaleWorlds.CampaignSystem.Roster.ItemRoster itemRoster = MobileParty.MainParty.ItemRoster;
            ItemObject artisanBeerObject = MBObjectManager.Instance.GetObject<ItemObject>("artisan_beer");
            ArtisanBeerCount = itemRoster.GetItemNumber(artisanBeerObject);
        }

        private void OnMissionChanged()
        {
            if (_mission.Mode != MissionMode.Conversation && _mission.Mode != MissionMode.CutScene)
            {
                IsVisible = true;
            }
            else
            {
                IsVisible = false;
            }
        }

        [DataSourceProperty]
        public bool IsVisible
        {
            get
            {
                return this._isVisible;
            }
            set
            {
                if (value != this._isVisible)
                {
                    this._isVisible = value;
                    base.OnPropertyChangedWithValue(value, "IsVisible");
                }
            }
        }

        [DataSourceProperty]
        public int ArtisanBeerCount
        {
            get
            {
                return this._artisanBeerCount;
            }
            set
            {
                if (value != this._artisanBeerCount)
                {
                    this._artisanBeerCount = value;
                    base.OnPropertyChangedWithValue(value, "ArtisanBeerCount");
                }
            }
        }

        public void DrinkArtisanBeer()
        {
            if (TaleWorlds.InputSystem.Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.Q))
            {
                if (!(this._mission.Mode is MissionMode.Battle or MissionMode.Stealth))
                    return;

                //在战斗过程中 按Q,消耗一个 artisan beer,回血20
                TaleWorlds.CampaignSystem.Roster.ItemRoster itemRoster = MobileParty.MainParty.ItemRoster;
                ItemObject artisanBeerObject = MBObjectManager.Instance.GetObject<ItemObject>("artisan_beer");
                if (itemRoster.GetItemNumber(artisanBeerObject) <= 0)
                {
                    InformationManager.DisplayMessage(new InformationMessage("Artisan Beer 库存不足"));
                    return;
                }

                if (this._mission.MainAgent.Health >= this._mission.MainAgent.HealthLimit)
                {
                    InformationManager.DisplayMessage(new InformationMessage("血量满,不可以使用"));
                    return;
                }

                itemRoster.AddToCounts(artisanBeerObject, -1);
                ArtisanBeerCount--;
                this._mission.MainAgent.Health += 20;

                InformationManager.DisplayMessage(new InformationMessage($"加了20滴血"));

            }
        }
    }
}