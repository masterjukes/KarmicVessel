using System;
using System.Collections;
using System.Linq;
using IngameDebugConsole;
using KarmicVessel.Other;
using KarmicVessel.Tier1;
using ThunderRoad;
using ThunderRoad.Skill;
using UnityEngine;
using Object = System.Object;

namespace KarmicVessel.Tier3

{
    public class PortalCannon : SpellSkillData
    {
        
        public float threshold = 0.05f;
        public float cooldown = 0.4f; 
        private Vector3 lastPos;
        private float lastTriggerTime;

        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);
            Action<GestureUtils.Direction> OGesture = OnGesture;
            DebugLogConsole.AddCommand("PortalShoot", "does a portal thing", OGesture, "direction");

        }


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
            if(_spell.isCasting)
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
                    Debug.Log("Detected gesture: " + dir);
                    OnGesture(dir);
                    lastTriggerTime = Time.time;
                }
            }
            

            lastPos = target.position;
        }
        
        

        
        [ConsoleMethod( "PortalShoot", "does a portal thing", "direction")]
        void OnGesture(GestureUtils.Direction dir)
        {
            var te = Creature.AimAssist(Player.currentCreature.transform.position, Player.local.head.cam.transform.forward, 50, 90, ignoredEntity: Player.currentCreature );
            if(!(te is Creature creature)) return;
            Vector3 creaturePortalOffset = Vector3.zero;
            
            
            switch (dir)
            {
                case GestureUtils.Direction.Forward:
                    creaturePortalOffset = creature.ragdoll.targetPart.transform.right * 1f;
                    break;

                case GestureUtils.Direction.Backward:
                    creaturePortalOffset = creature.ragdoll.targetPart.transform.right * -1f;
                    break;

                case GestureUtils.Direction.Left:
                    creaturePortalOffset = creature.ragdoll.targetPart.transform.forward * 1f;
                    break;

                case GestureUtils.Direction.Right:
                    creaturePortalOffset = creature.ragdoll.targetPart.transform.forward * -1f;
                    break;
                
                case GestureUtils.Direction.Down:
                    creaturePortalOffset = creature.ragdoll.targetPart.transform.up * 1f;
                    break;
            }
            
            var portalPos = creature.ragdoll.targetPart.transform.position + creaturePortalOffset;

            Player.local.StartCoroutine(SpawnPortalCoroutine(portalPos, creature.ragdoll.targetPart.transform, true, 1f, () =>
            {
                Debug.Log("running afterSpawn");
                var spawn = portalPos;
                var target = creature.ragdoll.targetPart.transform;
                var item = Daikokuten._instance.data.items.Pop();
                if(item != null)
                    item.SpawnAsync(_i =>
                    {
                        Debug.Log("Spawned portal item");
                        _i.transform.position = spawn;
                        _i.transform.LookAt(target);
                        _i.Throw();
                        _i.AddForce(_i.transform.forward * 10 * item.mass, ForceMode.Impulse);
                    });
                else
                    Debug.Log("No portal item");
            }));
            
        }
        
        private static GameObject SpawnPortal(Vector3 pos, Transform target)
        {
            var portal = GameObject.Instantiate(AssetStorage.AssetKarmaPortal);
            portal.transform.position = pos;
            portal.transform.LookAt(target);

            return portal;
        }



        public static IEnumerator SpawnPortalCoroutine(Vector3 pos, Transform target, bool fade, float fadeAfter, Action afterSpawn = null)
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
            
            if (fade)
                yield return DestroyPortalFade(portal, fadeAfter);
        }

        public static IEnumerator DestroyPortalFade(GameObject portal, float seconds)
        {
            yield return Yielders.ForSeconds(seconds);
            portal.transform.localScale = Vector3.one;
            while (portal.transform.localScale.x > 0)
            {
                portal.transform.localScale -= new Vector3(0.05f, 0.05f, 0.05f);
                yield return null;
            }

            portal.transform.localScale = Vector3.zero;

            GameObject.Destroy(portal);
        }
    }
}