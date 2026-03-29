using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KarmicVessel.Other;
using ThunderRoad;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Random = System.Random;

namespace KarmicVessel.Tier1
{
    public class KarmaBase : SpellCastCharge
    {
        public ModOptions.SpellHands ability = ModOptions.SpellHands.ChakraRods;
        public Vector3 centerpos;
        public Vector3 direction;
        public Vector3 up;
        public static EffectInstance ChakraSound;
        public bool StunningRods;
        public bool HomingRods;
        public static float LaunchSpeed = 30f;
        public static int NumOfRods = 1;
        public Quaternion rotation;
        public bool isCasting;
        public bool canDisableSpellWheel = false;
        bool lastAlternateUsePressed;
        public static Item lastItem;
        public static SpellCaster lastCaster;

        public delegate void FireEvent(SpellCaster caster);

        public static event FireEvent OnFireEvent;

        public delegate void GripCast(SpellCaster caster, HandleRagdoll handle);

        public event GripCast OnGripCastEvent;
        
        public Coroutine EnableCoroutine;
        public Coroutine DisableCoroutine;

        public Item DaikokutenItem;
        
        public Material oldBodyMat;
        public Material oldHandMat;
        public Material oldHeadMat;
        public Material oldEyesMat;
        
        public Renderer bodyRenderer;
        public Renderer handRenderer;
        public Renderer headRenderer;
        public Renderer eyeRenderer;

        
        public static bool hasKarmaMarkings;
        public static bool removingKarmaMarkings;
        public static bool applyingKarmaMarkings;


        public enum RodLaunchLocation
        {
            Eyes,
            Hand
        }

        public static List<ModOptions.SpellHands> SpellAbilityCycle = new List<ModOptions.SpellHands>
        {
            ModOptions.SpellHands.ChakraRods,
        };

        public override void Load(SpellCaster spellCaster)
        {
            base.Load(spellCaster);
            HomingRods = Player.local.creature.HasSkill("KarmaHomingRods");
            StunningRods = Player.local.creature.HasSkill("KarmaStunningRods");

            AssetStorage.LoadAll();
            spellCaster.StartCoroutine(UpdateOrb(spellCaster, ability));

            if (!hasKarmaMarkings && !applyingKarmaMarkings)
            {
                hasKarmaMarkings = true;
                GameManager.local.StartCoroutine(ApplyKarmaEffect());
            }

        }

        public IEnumerator ApplyKarmaEffect()
        {
            var creature = Player.currentCreature;
            
            var bodyid = "masterjukes.KarmaBody";
            var handid = "masterjukes.KarmaHands";
            var headid = "masterjukes.KarmaHead";
            var eyesid = "masterjukes.EyeT3mat";

            var bodyDone = false;
            var handDone = false;
            var headDone = false;
            float emissionValue = Player.currentCreature.HasSkill("KarmaShrinking") ? 1 : 0;
            
            applyingKarmaMarkings = true;

            if (removingKarmaMarkings)
            {
                yield return new WaitUntil(() => !removingKarmaMarkings);
            }

            foreach (var renderer in creature.renderers)
            {
                Debug.Log(renderer.renderer.name);
            }
            Debug.Log(creature.data.ethnicityId);
            
            if (emissionValue.IsApproximately(1f))
            {
                Catalog.LoadAssetAsync<Material>(eyesid, eyes =>
                {
                    var renderer = creature.renderers.First(r => r.renderer.name == "Eyes_LOD0").renderer;
                    if(oldEyesMat == null)
                        oldEyesMat = new Material(renderer.material);
                    
                    eyeRenderer = renderer;
                    renderer.material = eyes;
                }, this.GetName());
            }
            
            if(!ModOptions.ShowKarmaMarks)
                yield break;
            
            Catalog.LoadAssetAsync<Material>(bodyid, body =>
            {
                var renderer = creature.renderers.First(r => r.renderer.name == "Chest_LOD0").renderer;
                
                if(oldBodyMat == null)
                    oldBodyMat = new Material(renderer.material);
                
                
                bodyRenderer = renderer;
                renderer.material = body;
                var bodyMat = renderer.material;
                
                bodyMat.SetFloat("_Float", 0f);
                bodyMat.SetFloat("_emiss", emissionValue);
                bodyMat.SetColor("_emissColor", new Color(0.5f, 0, 0, 0));
                bodyMat.SetFloat("_CutoffAlpha", 0.5f);
                
                bodyMat.SetTexture("_Texture2D", oldBodyMat.GetTexture("_BaseMap"));
                bodyMat.SetColor("_baseColor", oldBodyMat.GetColor("_BaseColor"));
                //bodyMat.SetTexture("_norm", oldBodyMat.GetTexture("_NORMALMAP"));

                
                //bodyMat.SetTexture("_moes", oldBodyMat.GetTexture("_MetallicMap"));

                GameManager.local.StartCoroutine(LerpFloat(bodyMat));

                bodyDone = true;



            }, this.GetName());
            Catalog.LoadAssetAsync<Material>(handid, hand =>
            {
                var renderer = creature.renderers.First(r => r.renderer.name == "HandLeft_LOD0").renderer;
                
                
                if(oldHandMat == null)
                    oldHandMat = new Material(renderer.material);
                handRenderer = renderer;
                
                renderer.material = hand;
                var handMat = renderer.material;
                
                handMat.SetFloat("_Float", 0f);
                handMat.SetFloat("_emiss", emissionValue);
                handMat.SetColor("_emissColor", new Color(0.5f, 0, 0, 0));
                handMat.SetFloat("_CutoffAlpha", 0.5f);

                
                handMat.SetColor("_baseColor", oldHandMat.GetColor("_BaseColor"));
                handMat.SetTexture("_Texture2D", oldHandMat.GetTexture("_BaseMap"));
                //handMat.SetTexture("_norm", oldHandMat.GetTexture("_NORMALMAP"));
                //handMat.SetTexture("_moes", oldHandMat.GetTexture("_MetallicMap"));
                
                GameManager.local.StartCoroutine(LerpFloat(handMat));
                handDone = true;
                
            }, this.GetName());
            if(creature.data.ethnicityId == "Eradian")
                Catalog.LoadAssetAsync<Material>(headid, head =>
                {
                    var renderer = creature.renderers.First(r => r.renderer.name == "Head_LOD0").renderer;
                    
                    DumpShader(renderer.material);
                    
                    if(oldHeadMat == null)
                        oldHeadMat = new Material(renderer.material);
                    
                    
                    headRenderer = renderer;
                    
                    renderer.material = head;
                    var headMat = renderer.material;
                    
                    headMat.SetFloat("_Float", 0f);
                    headMat.SetFloat("_emiss", emissionValue);
                    headMat.SetColor("_emissColor", new Color(0.5f, 0, 0, 0));
                    headMat.SetFloat("_CutoffAlpha", 0.5f);

                    
                    headMat.SetColor("_baseColor", oldHeadMat.GetColor("_BaseColor"));
                    headMat.SetTexture("_Texture2D", oldHeadMat.GetTexture("_BaseMap"));
                    //headMat.SetTexture("_norm", oldHeadMat.GetTexture("_NORMALMAP"));

                    
                    //headMat.SetTexture("_moes", oldHeadMat.GetTexture("_MetallicMap"));

                    GameManager.local.StartCoroutine(LerpFloat(headMat));
                    headDone = true;
                }, this.GetName());
            else
                headDone = true;
            
            yield return new WaitUntil(() => bodyDone && handDone && headDone);
            
            applyingKarmaMarkings = false;
            creature.RefreshRenderers();
            creature.UpdateRenderers();
            
        }
        
        void DumpShader(Material mat)
        {
            var shader = mat.shader;

            Debug.Log($"Shader: {shader.name}");
            Debug.Log($"Property count: {shader.GetPropertyCount()}");

            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                var name = shader.GetPropertyName(i);
                var type = shader.GetPropertyType(i);

                Debug.Log($"[{i}] {name} ({type})");

                switch (type)
                {
                    case UnityEngine.Rendering.ShaderPropertyType.Color:
                        Debug.Log($"   Value: {mat.GetColor(name)}");
                        break;

                    case UnityEngine.Rendering.ShaderPropertyType.Vector:
                        Debug.Log($"   Value: {mat.GetVector(name)}");
                        break;

                    case UnityEngine.Rendering.ShaderPropertyType.Float:
                    case UnityEngine.Rendering.ShaderPropertyType.Range:
                        Debug.Log($"   Value: {mat.GetFloat(name)}");
                        break;

                    case UnityEngine.Rendering.ShaderPropertyType.Texture:
                        Debug.Log($"   Texture: {mat.GetTexture(name)}");
                        break;
                }
            }
        }
        
        private IEnumerator LerpFloat(Material mat)
        {
            float elapsedTime = 0;

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime;
                mat.SetFloat("_Float", Mathf.Lerp(0, 1, elapsedTime));
                yield return null;
            }

            mat.SetFloat("_Float", 1f);
        }

    public IEnumerator UpdateOrb(SpellCaster caster, ModOptions.SpellHands abilities)
        {
            yield return Yielders.ForSeconds(Time.deltaTime * 2);
            
            SpellOrbSwitcher.UpdateOrb(caster, abilities);
        }

        public override void Fire(bool active)
        {
            base.Fire(active);
            isCasting = active;
            
            if (isCasting)
            {
                OnFireEvent.Invoke(spellCaster);
                if(EnableCoroutine != null)
                    GameManager.local.StopCoroutine(EnableCoroutine);
                if(DisableCoroutine != null)
                    GameManager.local.StopCoroutine(DisableCoroutine);
                EnableCoroutine = GameManager.local.StartCoroutine(SpellOrbSwitcher.OrbJustEnabled(spellCaster));
                if(spellCaster.allowSpellWheel)
                    spellCaster.DisableSpellWheel(this);
                //Player.currentCreature.mana.GetPowerSlowTime().allowSkill = false;
                
            }
            else
            {
                if(DisableCoroutine != null)
                    GameManager.local.StopCoroutine(DisableCoroutine);
                if(EnableCoroutine != null)
                    GameManager.local.StopCoroutine(EnableCoroutine);
                DisableCoroutine = GameManager.local.StartCoroutine(SpellOrbSwitcher.OrbJustDisabled(spellCaster));
                if(!spellCaster.allowSpellWheel)
                    spellCaster.AllowSpellWheel(this);
                //Player.currentCreature.mana.GetPowerSlowTime().allowSkill = true;
                
            }
            
                
        }

        public override void UpdateGripCast(HandleRagdoll handle)
        {
            base.UpdateGripCast(handle);
            dbg.Log($"Grop Cast Event:{handle.ragdollPart.name}");
            OnGripCastEvent.Invoke(spellCaster, handle);
            
        }

        public override void UpdateCaster()
        {
            base.UpdateCaster();
            bool pressed = spellCaster.ragdollHand.playerHand.controlHand.alternateUsePressed;
            if (isCasting)
            {
                if(pressed && !lastAlternateUsePressed)
                    AbilitySwitcher(spellCaster);
            }
            lastAlternateUsePressed = pressed;
        }

        public override void Throw(Vector3 velocity)
        {
            base.Throw(velocity);
            if(spellCaster.side == Side.Right && ModOptions.RightHandAbilities != ModOptions.SpellHands.ChakraRods)
                return;
            if(spellCaster.side == Side.Left && ModOptions.LeftHandAbilities != ModOptions.SpellHands.ChakraRods)
                return;
            
            switch (ModOptions.ProjectileLaunchesFromEyes)
            {
                case RodLaunchLocation.Eyes:
                    centerpos = Player.local.head.cam.transform.position + Player.local.head.cam.transform.forward * 0.6f;
                    direction = Player.local.head.cam.transform.forward;
                    up = Player.local.head.cam.transform.up;
                    rotation = Player.local.head.cam.transform.rotation;
                    break;
                case RodLaunchLocation.Hand:
                    centerpos = spellCaster.transform.position + spellCaster.ragdollHand.PalmDir * 0.2f;
                    direction = spellCaster.ragdollHand.PalmDir;
                    up = Vector3.Cross(spellCaster.ragdollHand.transform.right, direction).normalized;
                    rotation = Quaternion.Euler(spellCaster.Orb.transform.rotation.eulerAngles - new Vector3(90, 90, 0)) ;
                    break;
            }
            
            ChakraSound = AssetStorage.GetAssetSafe<EffectData>("KokuganChakraSFX").Spawn(Player.currentCreature.ragdoll.targetPart.transform);
            ChakraSound.Play();


            AssetStorage.GetAssetSafe<ItemData>("SmallChakraRod").SpawnAsync(item =>
            {
                lastItem = item;
                lastCaster = spellCaster;
                
                item.IgnoreRagdollCollision(Player.currentCreature.ragdoll);
                
                item.Despawn(13f);
                item.renderers.ForEach(r => r.enabled = false);
                item.transform.rotation = rotation;
                item.transform.Rotate(90, 180, 0);
                /*
                item.GetOrAddComponent<DefaultModule>();
                if (StunningRods)
                    item.GetOrAddComponent<ItemStunBehaviour>();
                if (HomingRods)
                    item.GetOrAddComponent<ItemHomingBehavior>();
                */
                item.transform.position = centerpos;
                item.Throw();
                item.renderers.ForEach(r => r.enabled = true);
                item.AddForce(direction * LaunchSpeed, ForceMode.Impulse);
            });
                
                
                
            
        }

        void AbilitySwitcher(SpellCaster caster)
        {
            ref ModOptions.SpellHands abilities = ref ModOptions.RightHandAbilities;
            if(caster.side == Side.Left)
                abilities = ref ModOptions.LeftHandAbilities;
            
            
            abilities = SpellAbilityCycle[(SpellAbilityCycle.IndexOf(abilities) + 1) % SpellAbilityCycle.Count];
            ability = abilities;
            dbg.Log("Ability Switched to: " + abilities + "");
            
            ModOptions.AbilitySwitched?.Invoke();
            SpellOrbSwitcher.UpdateOrb(caster, abilities);

        }

        IEnumerator RemoveKarmaEffect()
        {
            if(applyingKarmaMarkings)
                yield return new WaitUntil(() => !applyingKarmaMarkings);
            
            eyeRenderer.material = oldEyesMat;
            
            removingKarmaMarkings = true;
            var elapsedTime = 0f;
            while (elapsedTime < 1)
            {
                elapsedTime += Time.deltaTime;
                if(Player.currentCreature.data.ethnicityId == "Eradian")
                    headRenderer.material.SetFloat("_Float", Mathf.Lerp(1, 0, elapsedTime));
                bodyRenderer.material.SetFloat("_Float", Mathf.Lerp(1, 0, elapsedTime));
                handRenderer.material.SetFloat("_Float", Mathf.Lerp(1, 0, elapsedTime));
                yield return null;
            }
            if(Player.currentCreature.data.ethnicityId == "Eraden")
                headRenderer.material = oldHeadMat;
            bodyRenderer.material = oldBodyMat;
            handRenderer.material = oldHandMat;
            
            hasKarmaMarkings = false;
            removingKarmaMarkings = false;
            
        }

        public override void Unload()
        {
            base.Unload();

            if (ModOptions.ShowKarmaMarks)
            {

                if ((hasKarmaMarkings || (!hasKarmaMarkings && applyingKarmaMarkings)) && !removingKarmaMarkings)
                    GameManager.local.StartCoroutine(RemoveKarmaEffect());
            }
            else
                eyeRenderer.material = oldEyesMat;


            if(!spellCaster.allowSpellWheel)
                spellCaster.AllowSpellWheel(this);

        }
    }
    
    
}