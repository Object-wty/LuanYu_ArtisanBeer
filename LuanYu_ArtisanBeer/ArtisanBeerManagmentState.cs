using System;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.CampaignSystem.ViewModelCollection.Quests;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace LuanYu_ArtisanBeer
{
    public class ArtisanBeerManagmentState : GameState
    {

        public ArtisanBeerManagmentState()
        {

        }
        public readonly Workshop _workshop;
        public readonly ArtisanBeerWorkShopState _artisanBeerWorkShopState;

        public ArtisanBeerManagmentState(Workshop workshop, ArtisanBeerWorkShopState artisanBeerWorkShopState)
        {
            this._workshop = workshop;
            this._artisanBeerWorkShopState = artisanBeerWorkShopState;
        }
    }


    [GameStateScreen(typeof(ArtisanBeerManagmentState))]
    public class GauntleArtisanBeerManagmentScreen : ScreenBase, IGameStateListener
    {
        private readonly ArtisanBeerManagmentState _artisanBeerManagmentState;
        private ArtisanBeerManagmentVM _dataSource;
        private GauntletLayer _gauntletLayer;

        public GauntleArtisanBeerManagmentScreen(ArtisanBeerManagmentState artisanBeerManagmentState)
        {
            this._artisanBeerManagmentState = artisanBeerManagmentState;
            

        }
        void IGameStateListener.OnActivate()
        {
            this._dataSource = new ArtisanBeerManagmentVM(_artisanBeerManagmentState);
            this._gauntletLayer = new GauntletLayer(1, "GauntletLayer", true);
            this._gauntletLayer.LoadMovie("ArtisanBeer_WorkshopManagment", _dataSource);
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            this._gauntletLayer.IsFocusLayer = true;
            base.AddLayer(this._gauntletLayer);
            ScreenManager.TrySetFocus(this._gauntletLayer);
            
        }

        void IGameStateListener.OnDeactivate()
        {
          
            this._gauntletLayer.IsFocusLayer = false;
            ScreenManager.TryLoseFocus(this._gauntletLayer);
            base.RemoveLayer(this._gauntletLayer);
        }

        void IGameStateListener.OnFinalize()
        {
            ArtisanBeerManagmentVM dataSource = this._dataSource;
            if (dataSource != null)
            {
                dataSource.OnFinalize();
            }
            this._dataSource = null;
            this._gauntletLayer = null;
        }

        void IGameStateListener.OnInitialize()
        {
           
        }
    }

    public class ArtisanBeerManagmentVM : ViewModel
    {
        private int _proctionAmount;
        private string _prodctionRatio;
        private readonly ArtisanBeerManagmentState _artisanBeerManagmentState;

        public ArtisanBeerManagmentVM(ArtisanBeerManagmentState artisanBeerManagmentState)
        {
            this._artisanBeerManagmentState = artisanBeerManagmentState;
            ProctionAmount = _artisanBeerManagmentState._artisanBeerWorkShopState._dailyProctionAmount;
            UpadateRatio();
        }


        public void UpadateRatio()
        {

            ProdctionRatio = (100 - ProctionAmount / 5.0*100)+"%";
        }

        [DataSourceProperty]
        public string ProdctionRatio
        {
            get
            {
                return this._prodctionRatio;
            }
            set
            {
                if (value != this._prodctionRatio)
                {
                    this._prodctionRatio = value;
                    base.OnPropertyChangedWithValue(value, "ProdctionRatio");
                }
            }
        }


        [DataSourceProperty]
        public int ProctionAmount
        {
            get
            {
                return this._proctionAmount;
            }
            set
            {
                if (value != this._proctionAmount)
                {
                    this._proctionAmount = value;
                    base.OnPropertyChangedWithValue(value, "ProctionAmount");
                    UpadateRatio();
                }
            }
        }

        public void ExecuteDone()
        {
            //在这里需要保存我们的设置
            this._artisanBeerManagmentState._artisanBeerWorkShopState._dailyProctionAmount = ProctionAmount;
            Game.Current.GameStateManager.PopState(0);
        }

        public void ExecuteCancel()
        {
            Game.Current.GameStateManager.PopState(0);
        }
    }
}