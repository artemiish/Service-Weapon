using RimWorld;
using Verse;
using UnityEngine;
using Verse.Sound;
using System.Collections.Generic;
using System.Linq;
using RimWorld.QuestGen;


namespace ServiceWeapon
{
    public class CompProperties_WeaponMaker : CompProperties_AbilityEffect
    {
        public ThingDef itemDef;

        public ThingDef destroyDef;

        public FleckDef fleck;

        public FloatRange fleckOffsetRange = new FloatRange(0.2f, 0.4f);

        public new SoundDef sound;

        public CompProperties_WeaponMaker()
        {
            compClass = typeof(CompWeaponMaker);
        }
    }

    public class CompWeaponMaker : CompAbilityEffect
    {
        public new CompProperties_WeaponMaker Props => (CompProperties_WeaponMaker)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (target.Thing is Pawn pawn)
            {
                ThingWithComps item = (ThingWithComps)ThingMaker.MakeThing(Props.itemDef);
                pawn.equipment.DropAllEquipment(pawn.Position);
                pawn.equipment.AddEquipment(item);
                if (Props.fleck != null)
                {
                    Vector3 drawPos = pawn.DrawPos;
                    Vector2 vector = Rand.InsideUnitCircle * Props.fleckOffsetRange.RandomInRange * Rand.Sign;
                    Vector3 loc = new Vector3(drawPos.x + vector.x, drawPos.y, drawPos.z + vector.y);
                    FleckMaker.Static(loc, pawn.Map, Props.fleck);
                }
                if (Props.sound != null)
                {
                    Props.sound.PlayOneShot(SoundInfo.InMap(pawn));
                }
            }
        }
    }

    public class CompUseEffect_InstallServiceWeapon : CompUseEffect_InstallImplant
    {
        public override AcceptanceReport CanBeUsedBy(Pawn pawn)
        {
            HediffSet hediff = pawn.health.hediffSet;
            if (hediff.HasHediff(HediffDef.Named("Artemiish_ServiceWeaponHediff")))
            {
                return false;
            }
            if (pawn.WorkTagIsDisabled(WorkTags.Violent))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public override TaggedString ConfirmMessage(Pawn pawn)
        {
            if (pawn.GetStatValueForPawn(StatDef.Named("PsychicSensitivity"), pawn) <= 0)
            {
                return "ServiceWeaponInstallWarning".Translate();
            }
            return null;
        }
    }

    public class HediffCompProperties_DoOnDeath : HediffCompProperties
    {
        public FleckDef fleck;

        public FloatRange fleckOffsetRange = new FloatRange(0.2f, 0.4f);

        public SoundDef sound;

        public ThingDef drop;

        public ThingDef filth;

        public int filthCount = 4;

        public HediffCompProperties_DoOnDeath()
        {
            compClass = typeof(HediffComp_DoOnDeath);
        }
    }

    public class HediffComp_DoOnDeath : HediffComp
    {
        public HediffCompProperties_DoOnDeath Props => (HediffCompProperties_DoOnDeath)props;

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
            if (!base.Pawn.Spawned)
            {
                return;
            }
            if (Props.fleck != null)
            {
                Vector3 drawPos = base.Pawn.DrawPos;
                Vector2 vector = Rand.InsideUnitCircle * Props.fleckOffsetRange.RandomInRange * Rand.Sign;
                Vector3 loc = new Vector3(drawPos.x + vector.x, drawPos.y, drawPos.z + vector.y);
                FleckMaker.Static(loc, base.Pawn.Map, Props.fleck);
            }
            if (Props.sound != null)
            {
                Props.sound.PlayOneShot(SoundInfo.InMap(base.Pawn));
            }
            if (Props.filth != null)
            {
                FilthMaker.TryMakeFilth(base.Pawn.Position, base.Pawn.Map, Props.filth, Props.filthCount);
            }
            if (Props.drop != null)
            {
                Thing thing = ThingMaker.MakeThing(Props.drop);
                GenPlace.TryPlaceThing(thing, base.Pawn.Position, base.Pawn.Map, ThingPlaceMode.Near);
            }
        }

        public override void Notify_PawnKilled()
        {
            if (!base.Pawn.Spawned)
            {
                return;
            }
            if (Props.fleck != null)
            {
                Vector3 drawPos = base.Pawn.DrawPos;
                Vector2 vector = Rand.InsideUnitCircle * Props.fleckOffsetRange.RandomInRange * Rand.Sign;
                Vector3 loc = new Vector3(drawPos.x + vector.x, drawPos.y, drawPos.z + vector.y);
                FleckMaker.Static(loc, base.Pawn.Map, Props.fleck);
            }
            if (Props.sound != null)
            {
                Props.sound.PlayOneShot(SoundInfo.InMap(base.Pawn));
            }
            if (Props.filth != null)
            {
                FilthMaker.TryMakeFilth(base.Pawn.Position, base.Pawn.Map, Props.filth, Props.filthCount);
            }
            if (Props.drop != null)
            {
                Thing thing = ThingMaker.MakeThing(Props.drop);
                GenPlace.TryPlaceThing(thing, base.Pawn.Position, base.Pawn.Map, ThingPlaceMode.Near);
            }
        }
    }

    public class CompProperties_DestroyOnUnequip : CompProperties
    {
        public DestroyMode destroyMode = DestroyMode.Vanish;

        public CompProperties_DestroyOnUnequip()
        {
            compClass = typeof(CompDestroyOnUnequip);
        }
    }
    public class CompDestroyOnUnequip : ThingComp
    {
        public CompProperties_DestroyOnUnequip Props => (CompProperties_DestroyOnUnequip)props;

        public override void Notify_Unequipped(Pawn pawn)
        {
            if (!base.parent.DestroyedOrNull())
            {
                parent.Destroy(Props.destroyMode);
            }
        }
    }

    public class CompProperties_AbilityRequirement : AbilityCompProperties
    {
        public StatDef statDef;

        public float statValue;

        public CompProperties_AbilityRequirement()
        {
            compClass = typeof(CompAbility_Requirement);
        }
    }

    public class CompAbility_Requirement : AbilityComp
    {
        public CompProperties_AbilityRequirement Props => (CompProperties_AbilityRequirement)props;

        public override bool GizmoDisabled(out string reason)
        {
            if (parent.pawn.GetStatValueForPawn (Props.statDef, parent.pawn) < Props.statValue)
            {
                reason = "ServiceWeaponPsyReq".Translate();
                return true;
            }
            reason = null;
            return false;
        }
    }

    /*public class CompProperties_AbilityReq : AbilityCompProperties
    {
        public List<TraitDef> traitDefs;

        public List<ThingDef> apparelDefs;
        public CompProperties_AbilityReq()
        {
            compClass = typeof(CompAbility_AbilityReq);
        }
    }
    public class CompAbility_AbilityReq : AbilityComp
    {
        public CompProperties_AbilityReq Props => (CompProperties_AbilityReq)props;

        public override bool GizmoDisabled(out string reason)
        {
            for (int i = 0; i < Props.traitDefs.Count; i++)
            {
                for (int l = 0; l < Props.apparelDefs.Count; l++)
                {
                    if (parent.pawn.story.traits.HasTrait(Props.traitDefs[i]) || HasApparel(Props.apparelDefs[l]))
                    {
                        reason = null;
                        return false;
                    }
                }
            }
            reason = "Weapon is too heavy for this pawn";
            return true;
        }
        private bool HasApparel(ThingDef thingDef)
        {
            foreach (Apparel item in parent.pawn.apparel.WornApparel)
            {
                if (item.def == thingDef)
                {
                    return true;
                }
            }
            return false;
        }
    }*/

    public class IncidentWorker_ServiceWeaponArrival : IncidentWorker_MeteoriteImpact
    {
        protected override List<Thing> GenerateMeteorContents(IncidentParms parms)
        {
            Thing item1 = ThingMaker.MakeThing(ThingDef.Named("Artemiish_ArchoPlinth"));
            Thing item2 = ThingMaker.MakeThing(ThingDef.Named("Artemiish_ServiceWeaponImplant"));
            return new List<Thing> { item1, item2 };
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            List<Thing> contents;
            Skyfaller skyfaller = SpawnArchoStructIncoming(parms, out contents);
            if (skyfaller == null)
            {
                return false;
            }
            skyfaller.impactLetter = MakeLetter(skyfaller, contents);
            return true;
        }
        protected Skyfaller SpawnArchoStructIncoming(IncidentParms parms, out List<Thing> contents)
        {
            Map map = (Map)parms.target;
            if (!TryFindCell(out var cell, map))
            {
                contents = null;
                return null;
            }
            contents = GenerateMeteorContents(parms);
            return SkyfallerMaker.SpawnSkyfaller(ThingDef.Named("ArchoPlinthIncoming"), contents, cell, map);
        }
        protected override Letter MakeLetter(Skyfaller meteorite, List<Thing> contents)
        {
            return LetterMaker.MakeLetter("ServiceWeaponArrivalLabel".Translate(), "ServiceWeaponArrivalText".Translate(), LetterDefOf.ThreatSmall, new TargetInfo(meteorite.Position, meteorite.Map));
        }
        private static bool TryFindCell(out IntVec3 cell, Map map)
        {
            int maxMineables = ThingSetMaker_Meteorite.MineablesCountRange.max;
            return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteIncoming, map, out cell, 10, default(IntVec3), -1, allowRoofedCells: true, allowCellsWithItems: false, allowCellsWithBuildings: false, colonyReachable: false, avoidColonistsIfExplosive: true, alwaysAvoidColonists: true, delegate (IntVec3 x)
            {
                int num = Mathf.CeilToInt(Mathf.Sqrt(maxMineables)) + 2;
                CellRect other = CellRect.CenteredOn(x, num, num);
                int num2 = 0;
                foreach (IntVec3 item in other)
                {
                    if (item.InBounds(map) && item.Standable(map))
                    {
                        num2++;
                    }
                }
                if (ModsConfig.RoyaltyActive)
                {
                    foreach (Thing item2 in map.listerThings.ThingsOfDef(ThingDefOf.MonumentMarker))
                    {
                        MonumentMarker monumentMarker = item2 as MonumentMarker;
                        if (monumentMarker.AllDone && monumentMarker.sketch.OccupiedRect.ExpandedBy(3).MovedBy(monumentMarker.Position).Overlaps(other))
                        {
                            return false;
                        }
                    }
                }
                return num2 >= maxMineables;
            });
        }
    }
}
