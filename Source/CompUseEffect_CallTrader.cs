using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;

namespace TraderWhereAreYou
{
    public class CompUseEffect_CallTrader : CompUseEffect
    {
        /// <summary>
        /// By default props returns the base CompProperties class.
        /// You can get props and cast it everywhere you use it, 
        /// or you create a Getter like this, which casts once and returns it.
        /// Careful of case sensitivity!
        /// </summary>
        public OrbitalCompUseEffectProperties Props => (OrbitalCompUseEffectProperties)this.props;

        public string TradeBeaconType => Props.TradeBeaconType;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Log.Message("CompUseEffect_CallTrader::Initialize");
        }
        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            Map map = p.Map;
            if (map.passingShipManager.passingShips.Count >= 5)
            {
                return "MaxShipsInOrbit".Translate();
            }

            return AcceptanceReport.WasAccepted;
        }

        // this is a weird amalgamation between CompDoEffect and IncidentWorker_OrbitalTraderArrival
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            Log.Message($"[OrbitalTradeBeacon] Effect {TradeBeaconType}");
            Map map = usedBy.Map;
            if (map.passingShipManager.passingShips.Count >= 5)
            {
                return;
            }

            TraderKindDef def = null;
            var iDef = DefDatabase<TraderKindDef>.AllDefs.Where(x => x.orbital && x.defName == TradeBeaconType);
            if(iDef.EnumerableNullOrEmpty())
            {
                if(DefDatabase<TraderKindDef>.AllDefs.Where(x => x.orbital).TryRandomElementByWeight(traderDef => traderDef.commonality, out TraderKindDef weightedDef))
                {
                    def = weightedDef;
                }
            }
            else
            {
                def = iDef.First();
            }
            if (def != null)
            {
                TradeShip tradeShip = new TradeShip(def);
                if (map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole && b.GetComp<CompPowerTrader>().PowerOn))
                {
                    Find.LetterStack.ReceiveLetter(tradeShip.def.LabelCap, "TraderArrival".Translate(
                        tradeShip.name,
                        tradeShip.def.label,
                        "TraderArrivalNoFaction".Translate()
                    ), LetterDefOf.PositiveEvent);
                }
                map.passingShipManager.AddShip(tradeShip);
                tradeShip.GenerateThings();
            }
            else throw new InvalidOperationException();

        }
    }

    public class OrbitalCompUseEffectProperties : CompProperties_UseEffect
    {
        public string TradeBeaconType;
        /// <summary>
        /// These constructors aren't strictly required if the compClass is set in the XML.
        /// </summary>
        public OrbitalCompUseEffectProperties()
        {
            this.compClass = typeof(CompUseEffect_CallTrader);
        }
    }
}
