using System;
using System.Collections;
using System.Linq;
using KarmicVessel.Other;
using KarmicVessel.Tier1;
using ThunderRoad;
using ThunderRoad.Skill;
using UnityEngine;
namespace KarmicVessel.Tier3

{
    public class PortalCannon : SpellSkillData
    {
        
        public float threshold = 0.05f;
        public float cooldown = 0.4f; 
        private Vector3 lastPos;
        private float lastTriggerTime;
        
        public override void OnSpellLoad(SpellData spell, SpellCaster caster = null)
        {
            base.OnSpellLoad(spell, caster);
            if(!(spell is KarmaBase karmaSpell))
                return;
            
            if(!KarmaBase.SpellAbilityCycle.Contains(ModOptions.SpellHands.Daikokuten))
                KarmaBase.SpellAbilityCycle.Add(ModOptions.SpellHands.Daikokuten);
            
            
            
            karmaSpell.OnSpellUpdateEvent += KarmaSpellOnOnSpellUpdateEvent;

            
        }

        private void KarmaSpellOnOnSpellUpdateEvent(SpellCastCharge spell)
        {
            var caster = spell.spellCaster;
            var _spell = spell as KarmaBase;
            if(_spell.ability != ModOptions.SpellHands.Daikokuten || _spell.DaikokutenItem != null) return;
            DetectGesture(caster.Orb);
        }
        
        void DetectGesture(Transform target)
        {
            Vector3 delta = target.position - lastPos;

            if (Time.time - lastTriggerTime > cooldown)
            {
                GestureUtils.Direction dir =
                    GestureUtils.GetCameraRelativeDirection(target, lastPos, Player.local.head.cam.transform, threshold);

                if (dir != GestureUtils.Direction.None)
                {
                    OnGesture(dir);
                    lastTriggerTime = Time.time;
                }
            }

            lastPos = target.position;
        }

        void OnGesture(GestureUtils.Direction dir)
        {
            Creature creature = Creature.allActive.First();
            Vector3 creaturePortalOffset = Vector3.zero;
            
            
            switch (dir)
            {
                case GestureUtils.Direction.Forward:
                    creaturePortalOffset = creature.ragdoll.targetPart.transform.forward * -0.5f;
                    break;

                case GestureUtils.Direction.Backward:
                    creaturePortalOffset = creature.ragdoll.targetPart.transform.forward * 0.5f;
                    break;

                case GestureUtils.Direction.Left:
                    creaturePortalOffset = creature.ragdoll.targetPart.transform.right * -0.5f;
                    break;

                case GestureUtils.Direction.Right:
                    creaturePortalOffset = creature.ragdoll.targetPart.transform.right * 0.5f;
                    break;
                
                case GestureUtils.Direction.Down:
                    creaturePortalOffset = creature.ragdoll.targetPart.transform.up * 0.5f;
                    break;
            }
            
            var portalPos = creature.ragdoll.targetPart.transform.position + creaturePortalOffset;

            Player.local.StartCoroutine(SpawnPortalCoroutine(portalPos, creature.ragdoll.targetPart.transform, () =>
            {
                var spawn = portalPos;
                var target = creature.ragdoll.targetPart.transform;
                var item = Daikokuten._instance.data.items.Pop();
                if(item != null)
                    item.SpawnAsync(_i =>
                    {
                        _i.transform.position = spawn;
                        _i.transform.LookAt(target);
                        _i.Throw();
                        _i.AddForce(_i.transform.forward * 10 * item.mass, ForceMode.Impulse);
                    });
            }));
            
        }
        
        private static GameObject SpawnPortal(Vector3 pos, Transform target)
        {
            var portal = GameObject.Instantiate(AssetStorage.AssetKarmaPortal);
            portal.transform.position = pos;
            portal.transform.LookAt(target);

            return portal;
        }



        public static IEnumerator SpawnPortalCoroutine(Vector3 pos, Transform target, Action afterSpawn = null)
        {
            var portal = SpawnPortal(pos, target);
            portal.transform.localScale = Vector3.zero;
            while (portal.transform.localScale.x < 1)
            {
                portal.transform.localScale += new Vector3(0.05f, 0.05f, 0.05f);
                yield return null;
            }
            portal.transform.localScale = Vector3.one;
            
            if (afterSpawn != null)
                yield return afterSpawn;
        }
    }
}