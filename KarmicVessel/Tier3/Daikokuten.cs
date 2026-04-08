using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KarmicVessel.ItemModules;
using KarmicVessel.Other;
using KarmicVessel.Tier1;
using Newtonsoft.Json;
using ThunderRoad;
using ThunderRoad.Skill;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;

namespace KarmicVessel.Tier3
{
    public class Daikokuten : SpellSkillData
    {
        public static Daikokuten _instance;
        public float velocityThreshold = 1.5f; 
        public float angleThreshold = 30f;
        private bool justStopped = false;

        private string prevDaikokutenItem;

        
        bool summoning = false;
        
        public DaikokutenData data = new DaikokutenData();
        

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);
            _instance = this;
            data.Load();
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);
            data.SaveToJSON();
        }


        public override void OnSpellLoad(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellLoad(spell, caster);
            if(!(spell is KarmaBase karmaSpell))
                return;
            
            if(!KarmaBase.SpellAbilityCycle.Contains(ModOptions.SpellHands.Daikokuten))
                KarmaBase.SpellAbilityCycle.Add(ModOptions.SpellHands.Daikokuten);
            
            
            
            caster.telekinesis.OnGrabEvent += TelekinesisOnOnGrabEvent;
            karmaSpell.OnSpellUpdateEvent += KarmaSpellOnOnSpellUpdateEvent;
            karmaSpell.OnSpellStopEvent += KarmaSpellOnOnSpellStopEvent;
            karmaSpell.OnSpellThrowEvent += KarmaSpellOnOnSpellThrowEvent;
            
        }



        private void KarmaSpellOnOnSpellThrowEvent(SpellCastCharge _spell, Vector3 velocity)
        {
            var spell = _spell as KarmaBase;
            if(spell.ability != ModOptions.SpellHands.Daikokuten || spell.DaikokutenItem == null) return;
            
            var item = spell.DaikokutenItem;
            spell.DaikokutenItem = null;
            Object.Destroy(item.GetComponent<DkJoint>());
            item.physicBody.velocity = Vector3.zero;
            item.AddForce(spell.spellCaster.ragdollHand.PalmDir * 25, ForceMode.VelocityChange);
            item.Throw();
            item.GetOrAddComponent<CustomLoop>().code = () =>
            {
                if(item.transform.localScale.x < 1.0f)
                    item.transform.localScale += new Vector3(0.05f, 0.05f, 0.05f);
                else
                {
                    item.ResetRagdollCollision();
                    item.GetOrAddComponent<CustomLoop>().enabled = false;
                }
            };
            dbg.Log("DaikokutenItem Thrown");

        }

        private void KarmaSpellOnOnSpellStopEvent(SpellCastCharge _spell)
        {
            var spell = _spell as KarmaBase;
            if (spell.DaikokutenItem != null)
            {
                GameObject.Destroy(spell.DaikokutenItem.GetComponent<SmoothFollow>());
                data.items.Push(spell.DaikokutenItem.data.id);
                spell.DaikokutenItem.physicBody.isKinematic = false;
                spell.DaikokutenItem.Despawn(0.4f);
                spell.DaikokutenItem = null;
                spell.DaikokutenItem.GetMainHandle(_spell.spellCaster.ragdollHand.side).Grabbed += OnGrabbed;
                
                
                dbg.Log("DaikokutenItem Stopped and despawned");

            }


        }

        private void OnGrabbed(RagdollHand ragdollHand, Handle handle, EventTime eventTime)
        {
            if(eventTime == EventTime.OnEnd)
                return;
            
            handle.Release();
            handle.ReleaseAllTkHandlers();
            var prevItemId = handle.item.data.id;
            handle.item.Despawn();
            
            var dkItem = prevItemId;
            Catalog.GetData<ItemData>(dkItem).SpawnAsync(item =>
            {
                var _handle = item.GetMainHandle(ragdollHand.side);
                ragdollHand.Grab(_handle, true);
                dbg.Log("Grabbed DaikokutenItem");
            });
        }
        
        
        private void KarmaSpellOnOnSpellUpdateEvent(SpellCastCharge _spell)
        {
            var spell = _spell as KarmaBase;
            if(spell.ability != ModOptions.SpellHands.Daikokuten) return;

            if(_spell.spellCaster.ragdollHand.playerHand.controlHand.gripPressed)
                Debug.Log("Gripping");
            
            if (spell.DaikokutenItem == null)
            {
                var hand = spell.spellCaster.ragdollHand; 
                var velocity = hand.Velocity();
                var speed = velocity.magnitude;
                var angle = Vector3.Angle(Vector3.up, velocity.normalized);
                
                var canGesture = (speed > velocityThreshold && angle < angleThreshold && Vector3.Dot(hand.PalmDir.normalized, Vector3.up) > 0.5f);
                
                if (canGesture && !summoning) SummonItem(spell);
            }
            
        }

        private void TelekinesisOnOnGrabEvent(SpellCaster spellCaster, SpellTelekinesis spellTelekinesis, Side side, Handle handle)
        {
            var spell = spellCaster.spellInstance as KarmaBase;
            if(spell.ability != ModOptions.SpellHands.Daikokuten) return;

            if (handle.item != null)
            {
                data.items.Push(handle.item.data.id);
                handle.item.Despawn();
            }
        }


        public void SummonItem(KarmaBase spell)
        {
            Debug.Log("Summoning DaikokutenItem");
            var spellCaster = spell.spellCaster;
            string dkItem = null;

            if (data.items.Count > 0)
            {
                dkItem = data.items.Pop();
                var ChakraSound = AssetStorage.GetAssetSafe<EffectData>("KokuganChakraSFX").Spawn(Player.currentCreature.ragdoll.targetPart.transform);
                ChakraSound.Play();
            }

            summoning = true;


            if(dkItem != null)
                Catalog.GetData<ItemData>(dkItem).SpawnAsync(item =>
                {
                    try
                    {
                        spell.DaikokutenItem = item;
                        item.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
                        item.ScaleToGlobalSize(0.15f);
                        item.transform.position = spellCaster.Orb.transform.position;

                        item.GetOrAddComponent<DkJoint>().Init(item, spellCaster.Orb, spellCaster);

                        var homing = item.GetOrAddComponent<ItemHomingBehavior>();
                        homing.RandomTarget = true;
                        
                        item.IgnoreRagdollCollision(spellCaster.ragdollHand.ragdoll);
                        item.transform.rotation = Quaternion.LookRotation(spellCaster.ragdollHand.PalmDir);
                        summoning = false;
                    }
                    finally
                    {
                        
                        summoning = false;
                    }

                });
            else
            {
                summoning = false;
            }
            
        }
        


        public override void OnSpellUnload(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellUnload(spell, caster);
            if (!(spell is  KarmaBase karmaSpell))
                return;
            
            caster.telekinesis.OnGrabEvent -= TelekinesisOnOnGrabEvent;
            karmaSpell.OnSpellUpdateEvent -= KarmaSpellOnOnSpellUpdateEvent;
            karmaSpell.OnSpellStopEvent -= KarmaSpellOnOnSpellStopEvent;
            karmaSpell.OnSpellThrowEvent -= KarmaSpellOnOnSpellThrowEvent;
        }
    }
}