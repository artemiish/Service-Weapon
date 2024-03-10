using RimWorld;
using Verse;
using UnityEngine;
using Verse.Sound;
using System.Collections.Generic;


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
                ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(Props.itemDef);
                pawn.equipment.DropAllEquipment(pawn.Position);
                pawn.equipment.AddEquipment(thingWithComps);
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

    public static class HediffDefOf
    {
        public static HediffDef Artemiish_ServiceWeaponHediff_Jesse;

        static HediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
        }
    }

    public class CompUseEffect_InstallServiceWeapon : CompUseEffect_InstallImplant
    {
        public override bool CanBeUsedBy(Pawn pawn, out string failReason)
        {
            HediffSet hediff = pawn.health.hediffSet;
            if (hediff.HasHediff(HediffDef.Named("Artemiish_ServiceWeaponHediff_Jesse")))
            {
                failReason = "already bound";
                return false;
            }
            else
            {
                failReason = null;
                return true;
            }
        }
        public override TaggedString ConfirmMessage(Pawn pawn)
        {
            if (pawn.WorkTagIsDisabled(WorkTags.Violent))
            {
                return "The Service Weapon will be unusable when bound by a non-violent pawn. Are you sure you want to continue?";
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

        public override void Notify_PawnDied()
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
                reason = "Not enough psychic sensitivity";
                return true;
            }
            reason = null;
            return false;
        }
    }

    public class CompProperties_AbilityReq : AbilityCompProperties
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

            /*bool apparelWear = HasApparel(ThingDef.Named("VWE_Apparel_Exoframe"));
            if (!ModLister.HasActiveModWithName("Vanilla Factions Expanded - Pirates") && !parent.pawn.story.traits.HasTrait(TraitDef.Named("Tough")) && !apparelWear)
            {
                reason = "Weapon is too heavy for this pawn";
                return true;
            }
            if (ModLister.HasActiveModWithName("Vanilla Factions Expanded - Pirates") && !parent.pawn.story.traits.HasTrait(TraitDef.Named("Tough")) && !parent.pawn.story.traits.HasTrait(TraitDef.Named("VFEP_WarcasketTrait")) && !apparelWear)
            {
                reason = "Weapon is too heavy for this pawn";
                return true;
            }
            reason = null;
            return false;*/
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
    }
}
