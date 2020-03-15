using RimWorld;
using System;
using System.Linq;
using Verse;

namespace TraderWhereAreYou
{
    public class CompUseEffect_CallTrader : CompUseEffect
    {
        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {
            Map map = p.Map;
            if (map.passingShipManager.passingShips.Count >= 5)
            {
                failReason = "MaxShipsInOrbit".Translate();
                return false;
            }

            failReason = null;
            return true;
        }
        // this is a weird amalgamation between CompDoEffect and IncidentWorker_OrbitalTraderArrival
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            Map map = usedBy.Map;
            if (map.passingShipManager.passingShips.Count >= 5)
            {
                return;
            }

            if (DefDatabase<TraderKindDef>.AllDefs
                .Where(x => x.orbital)
                .TryRandomElementByWeight(traderDef => traderDef.commonality, out TraderKindDef def))
            {
                TradeShip tradeShip = new TradeShip(def);
                if (map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole && b.GetComp<CompPowerTrader>().PowerOn))
                {
                    Find.LetterStack.ReceiveLetter(tradeShip.def.LabelCap, "TraderArrival".Translate(
                        tradeShip.name,
                        tradeShip.def.label,
                        "TraderArrivalNoFaction".Translate()
                    ), LetterDefOf.PositiveEvent, null);
                }
                map.passingShipManager.AddShip(tradeShip);
                tradeShip.GenerateThings();
            }
            else throw new InvalidOperationException();

        }
    }
}