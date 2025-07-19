using System;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD;
using TaleWorlds.ObjectSystem;

namespace LuanYu_ArtisanBeer
{

    [OverrideView(typeof(MissionAgentLockVisualizerView))]
    public class ArtisanBeerMissonView :  MissionGauntletBattleUIBase
    {
        private ArtisanBeerVM _dataSource;
        private GauntletLayer _layer;
        private IGauntletMovie _movie;

        public override void OnMissionTick(float dt)
        {

            _dataSource?.DrinkArtisanBeer();
            

        }

        

        protected override void OnCreateView()
        {
            this._dataSource = new ArtisanBeerVM(Mission);
            this._layer = new GauntletLayer(1, "GauntletLayer", false);
            this._movie = this._layer.LoadMovie("ArtisanBeerCount_GUI", this._dataSource);
            if (base.Mission.Mode != MissionMode.Conversation && base.Mission.Mode != MissionMode.CutScene)
            {
                base.MissionScreen.AddLayer(this._layer);
            }
        }

        protected override void OnDestroyView()
        {
            if (base.Mission.Mode != MissionMode.Conversation && base.Mission.Mode != MissionMode.CutScene)
            {
                base.MissionScreen.RemoveLayer(this._layer);
            }
            this._dataSource = null;
            this._movie = null;
            this._layer = null;
        }
    }
}