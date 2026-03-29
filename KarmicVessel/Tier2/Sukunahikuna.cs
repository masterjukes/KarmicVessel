using System;
using System.Collections.Generic;
using System.Linq;
using KarmicVessel.Other;
using KarmicVessel.Tier1;
using ThunderRoad;
using ThunderRoad.Skill;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;

namespace KarmicVessel.Tier2
{
    public class Sukunahikuna : SpellSkillData
    {
        private float radius = 3f;         
        private float maxDistance = 30f;
        private List<Renderer> oldRend;
        private LayerMask sphereMask = (1 << 25);
        private LayerMask losMask = (1 << 0) | (1 << 5) | (1 << 7) | (1 << 25);

        private RaycastHit[] hits = new RaycastHit[20];
        private List<(Handle handle, float score)> scored = new List<(Handle, float)>();
        public static GameObject go;
        public static Handle currentTarget;
        public Material Outline;
        public Material Mask;
        
        private bool lastAlternateUsePressed;
        private Vector3 lastHandPos = Vector3.zero;
        
        
        
        public override void OnSpellLoad(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellLoad(spell, caster);
            if (!(spell is  KarmaBase karmaSpell))
                return;
            

            
            karmaSpell.OnSpellThrowEvent += KarmaSpellOnOnSpellThrowEvent;
            karmaSpell.OnSpellUpdateEvent += KarmaSpellOnOnSpellUpdateEvent;
            KarmaBase.OnFireEvent += KarmaBaseOnOnFireEvent;
            if(!KarmaBase.SpellAbilityCycle.Contains(ModOptions.SpellHands.Shrink))
                KarmaBase.SpellAbilityCycle.Add(ModOptions.SpellHands.Shrink);
            
            
            Catalog.LoadAssetAsync<Material>("ShrinkSelectMaterial", material =>
            {
                Outline = material;
                dbg.Log("Loaded Shrink Select Material");
            }, "ShrinkSelectMaterialLoad");
            Catalog.LoadAssetAsync<Material>("ShrinkSelectMaterialMask", material =>
            {
                Mask = material;
                dbg.Log("Loaded Shrink Select Material Mask");
            }, "ShrinkSelectMaterialMaskLoad");
            
            Catalog.LoadAssetAsync<GameObject>("Karma.ShrinkFlash", goo =>
            {
                go = goo;
            }, "ShrinkFlash");

        }

        private void SpellTelekinesisOnonTelekinesisRepelEvent(SpellCaster spellCaster, SpellTelekinesis spellTelekinesis, Side side, Handle handle, EventTime eventTime)
        {
            var karma = spellCaster.spellInstance as KarmaBase;
            if(eventTime == EventTime.OnEnd || handle.item == null || !ModOptions.ShrinkUseTelekinesis || karma.ability != ModOptions.SpellHands.Shrink || handle.item == null) return;
            
            handle.item.transform.localScale = handle.item.transform.localScale * 2 * Time.deltaTime;
        }

        private void SpellTelekinesisOnonTelekinesisPullEvent(SpellCaster spellCaster, SpellTelekinesis spellTelekinesis, Side side, Handle handle, EventTime eventTime)
        {
            var karma = spellCaster.spellInstance as KarmaBase;
            if(eventTime == EventTime.OnEnd || handle.item == null || !ModOptions.ShrinkUseTelekinesis || karma.ability != ModOptions.SpellHands.Shrink || handle.item == null) return;
            handle.item.transform.localScale = handle.item.transform.localScale / 2 * Time.deltaTime;

        }


        private void KarmaBaseOnOnFireEvent(SpellCaster caster)
        {
            if(caster.side == Side.Right && ModOptions.RightHandAbilities != ModOptions.SpellHands.Shrink)
                return;
            if(caster.side == Side.Left && ModOptions.LeftHandAbilities != ModOptions.SpellHands.Shrink)
                return;
            
            lastHandPos = caster.ragdollHand.playerHand.transform.position;
        }

        private void KarmaSpellOnOnSpellUpdateEvent(SpellCastCharge spell)
        {
            if(spell.spellCaster.side == Side.Right && ModOptions.RightHandAbilities != ModOptions.SpellHands.Shrink)
                return;
            if(spell.spellCaster.side == Side.Left && ModOptions.LeftHandAbilities != ModOptions.SpellHands.Shrink)
                return;
            
            bool pressed = Player.local.GetHand(spell.spellCaster.side).controlHand.castPressed;
            
            
            if (pressed && !lastAlternateUsePressed)
            {
                dbg.Log("New Hand Pos");
                lastHandPos = spell.spellCaster.ragdollHand.playerHand.transform.position;
            }
            if (!pressed && lastAlternateUsePressed)
                lastHandPos = Vector3.zero;
            
            lastAlternateUsePressed = pressed;
            if (currentTarget == null)
                return;
            if (lastHandPos == Vector3.zero)
                return;
            
            float deltaY = spell.spellCaster.transform.position.y - lastHandPos.y;
            float targetScale = deltaY + 1;

            if (targetScale > 1)
            {
                float scaleChange = Mathf.Clamp((targetScale - 1) * ModOptions.GrowShrinkSpeed, 0.01f, 10f);
                float newScale = Mathf.Clamp(currentTarget.item.transform.localScale.y + scaleChange, 0.1f, 10f);
                currentTarget.item.transform.localScale = Vector3.one * newScale;
            }
            else
            {
                float scaleChange = Mathf.Clamp((targetScale - 1) / ModOptions.GrowShrinkSpeed, -0.9f, 0f);
                float newScale = Mathf.Clamp(currentTarget.item.transform.localScale.y + scaleChange, 0.1f, 10f);
                currentTarget.item.transform.localScale = Vector3.one * newScale;
            }






        }

        


        private void GetTarget(SpellCastCharge spell)
        {
            dbg.Log("Attempted Get Target");
            var caster = spell.spellCaster;
            Vector3 origin = caster.rayDir.position;
            Vector3 handDir = caster.rayDir.forward;
            Transform head = Player.local.head.transform;
            
            int count = Physics.SphereCastNonAlloc(
                origin,
                radius,
                handDir,
                hits,
                maxDistance,
                sphereMask
            );
            
            

            if (count == 0)
            {
                ClearTarget();
                return;
            }

            scored.Clear();
            
            
            for (int i = 0; i < count; i++)
            {
                dbg.DrawLine(origin, hits[i].point, 1, Color.yellow);
                
                if (!hits[i].collider.TryGetComponent(out Handle handle))
                    continue;

                Vector3 dirTo = (handle.transform.position - origin).normalized;

                float handAngle = Vector3.Angle(handDir, dirTo);
                float headAngle = Vector3.Angle(head.forward, dirTo);

                float score = handAngle + headAngle;
                if(handle.item != null && handle.handlers.Count == 0)
                    scored.Add((handle, score));
            }

            if (scored.Count == 0)
            {
                ClearTarget();
                return;
            }

            scored.Sort((a, b) => a.score.CompareTo(b.score));
            Handle best = scored[0].handle;
            Vector3 losDir = best.transform.position - head.position;

            if (Physics.Raycast(head.position, losDir, out var hit, maxDistance + 1f, losMask))
            {
                
                var bestCol = best.GetComponent<Collider>();
                
                if (hit.collider != bestCol)
                {
                    ClearTarget();
                    return;
                }
            }
            
            // ReSharper disable once RedundantCheckBeforeAssignment
            if (best != currentTarget)
            {
                // Clear previous target first
                if (currentTarget != null)
                    ClearTarget();
                
                SpellOrbSwitcher.UpdateOrb(caster, ModOptions.SpellHands.Shrink);
                currentTarget = best;
                ApplyHighlight(currentTarget);
            }
            
        }
        
        private Dictionary<Renderer, Material[]> originalMats = new Dictionary<Renderer, Material[]>();

        private void ApplyHighlight(Handle handle)
        {
            originalMats.Clear();

            foreach (var renderer in handle.item.renderers)
            {
                if (renderer == null) continue;
                
                if (!originalMats.ContainsKey(renderer))
                    originalMats[renderer] = renderer.materials.ToArray();
                
                if (!renderer.materials.Contains(Outline))
                {
                    var mats = renderer.materials;
                    Array.Resize(ref mats, mats.Length + 2);
                    mats[mats.Length - 2] = Outline;
                    mats[mats.Length - 1] = Mask;
                    renderer.materials = mats;
                }
            }
        }

        private void ClearTarget()
        {
            if (currentTarget == null) return;

            // Restore original materials
            foreach (var kv in originalMats)
            {
                if (kv.Key != null)
                    kv.Key.materials = kv.Value;
            }

            originalMats.Clear();
            currentTarget = null;
        }
        




        private void KarmaSpellOnOnSpellThrowEvent(SpellCastCharge spell, Vector3 velocity)
        {
            if(spell.spellCaster.side == Side.Right && ModOptions.RightHandAbilities != ModOptions.SpellHands.Shrink)
                return;
            if(spell.spellCaster.side == Side.Left && ModOptions.LeftHandAbilities != ModOptions.SpellHands.Shrink)
                return;

            dbg.Log($"Throw Event with Velocity: {velocity.magnitude}");
            if(velocity.magnitude < 2.3f)
                return;
            
            GetTarget(spell);
            if (currentTarget != null)
            {
                var ChakraSound = AssetStorage.GetAssetSafe<EffectData>("KokuganShrinkSFX")
                    .Spawn(Player.currentCreature.ragdoll.targetPart.transform);
                ChakraSound.Play();
                SpellOrbSwitcher.UpdateOrb(spell.spellCaster, ModOptions.SpellHands.Shrink);
            }
            
            dbg.Log("Target: " + currentTarget?.name);
            
                
            

            
        }


        public override void OnSpellUnload(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellUnload(spell, caster);
            if (!(spell is  KarmaBase karmaSpell))
                return;
            
            KarmaBase.SpellAbilityCycle.Remove(ModOptions.SpellHands.Shrink);
            karmaSpell.OnSpellThrowEvent -= KarmaSpellOnOnSpellThrowEvent;
            
            SpellTelekinesis.onTelekinesisPullEvent -= SpellTelekinesisOnonTelekinesisPullEvent;
            SpellTelekinesis.onTelekinesisRepelEvent -= SpellTelekinesisOnonTelekinesisRepelEvent;
            
            Catalog.ReleaseAsset("ShrinkSelectMaterialMaskLoad");
            Catalog.ReleaseAsset("ShrinkSelectMaterialLoad");

        }
        
        
        
        
    }
    
    
}