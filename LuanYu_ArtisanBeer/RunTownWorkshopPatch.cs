using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;

namespace LuanYu_ArtisanBeer
{

    [HarmonyPatch(typeof(WorkshopsCampaignBehavior))]
    [HarmonyPatch("RunTownWorkshop")] 
    public class RunTownWorkshopPatch
    {
        static bool Prefix(Town townComponent, Workshop workshop, WorkshopsCampaignBehavior __instance)
        {
            WorkshopType workshopType = workshop.WorkshopType;
            bool flag = false;
            for (int i = 0; i < workshopType.Productions.Count; i++)
            {
                float num = workshop.GetProductionProgress(i);
                if (num > 1f)
                {
                    num = 1f;
                }
                //修改这个地方可以影响到生产

                num += Campaign.Current.Models.WorkshopModel.
                    GetEffectiveConversionSpeedOfProduction(workshop, workshopType.Productions[i].ConversionSpeed, false).
                    ResultNumber * ArtisanBeerCampaignBehavior.GetNormalBeerEff(workshop);
                if (num >= 1f)
                {
                    bool flag2 = true;
                    while (flag2 && num >= 1f)
                    {
                        System.Reflection.MethodInfo methodInfo1 = AccessTools.Method(typeof(WorkshopsCampaignBehavior), "TickOneProductionCycleForPlayerWorkshop");
                        System.Reflection.MethodInfo methodInfo2 = AccessTools.Method(typeof(WorkshopsCampaignBehavior), "TickOneProductionCycleForNotableWorkshop");
                        flag2 = ((workshop.Owner == Hero.MainHero) ? (bool)methodInfo1.Invoke(__instance,new object[] { workshopType.Productions[i], workshop } ): (bool)methodInfo2.Invoke(__instance, new object[] { workshopType.Productions[i], workshop }));
                        if (flag2)
                        {
                            flag = true;
                        }
                        num -= 1f;
                    }
                }
             
                workshop.SetProgress(i, num);
            }
            if (flag)
            {
                workshop.UpdateLastRunTime();
            }

            return false;
        }
    }
}
