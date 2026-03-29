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
            karmaSpell.spellCaster.ragdollHand.playerHand.OnFistEvent += PlayerHandOnOnFistEvent;
            
        }

        private void PlayerHandOnOnFistEvent(PlayerHand hand, bool gripping)
        {
            var spell = Player.currentCreature.mana.GetCaster(hand.side).spellInstance as KarmaBase;
            if(spell.ability != ModOptions.SpellHands.Daikokuten || !gripping) return;
            if(data.items.Count == 0) return;
            
            var dkItem = data.items.Pop();
            dkItem.SpawnAsync(item =>
            {
                var handle = item.GetMainHandle(hand.side);
                hand.ragdollHand.Grab(handle, true);;
                dbg.Log("Grabbed DaikokutenItem");
            });
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
                data.items.Push(spell.DaikokutenItem.data);
                spell.DaikokutenItem.physicBody.isKinematic = false;
                spell.DaikokutenItem.Despawn(0.4f);
                spell.DaikokutenItem = null;
                dbg.Log("DaikokutenItem Stopped and despawned");
            }


        }
        
        private void KarmaSpellOnOnSpellUpdateEvent(SpellCastCharge _spell)
        {
            var spell = _spell as KarmaBase;
            if(spell.ability != ModOptions.SpellHands.Daikokuten) return;

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
                data.items.Push(handle.item.data);
                handle.item.Despawn();
            }
        }


        public void SummonItem(KarmaBase spell)
        {
            Debug.Log("Summoning DaikokutenItem");
            var spellCaster = spell.spellCaster;
            ItemData dkItem = null;

            if (data.items.Count > 0)
            {
                dkItem = data.items.Pop();
                var ChakraSound = AssetStorage.GetAssetSafe<EffectData>("KokuganChakraSFX").Spawn(Player.currentCreature.ragdoll.targetPart.transform);
                ChakraSound.Play();
            }

            summoning = true;


            if(dkItem != null)
                dkItem.SpawnAsync(item =>
                {
                    try
                    {
                        spell.DaikokutenItem = item;
                        item.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
                        item.ScaleToGlobalSize(0.15f);
                        item.transform.position = spellCaster.Orb.transform.position;

                        item.GetOrAddComponent<DkJoint>().Init(item, spellCaster.Orb);

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
             //MTQ4Nzg0NDY3MDU2OTcxMzcwNA.Ggz7kz.Ck1Nnhcn_EKYAN28NBhP3bEzzqXH6tDhmAn_OY
            
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
            karmaSpell.spellCaster.ragdollHand.playerHand.OnFistEvent -= PlayerHandOnOnFistEvent;
        }
    }
}