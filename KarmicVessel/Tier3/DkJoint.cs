using System;
using ThunderRoad;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KarmicVessel.Tier3
{
    public class DkJoint : MonoBehaviour
    {
        private Item item;
        public Transform target;
        SpellCaster caster;

        [Header("Settings")]
        public float strength = 10f;
        public float damping = 2f;
        public float maxForce = 50f;
        public float rotationSpeed = 10f;

        public void Init(Item item, Transform target, SpellCaster caster)
        {
            this.target = target;
            this.item = item;
            this.caster = caster;
        }

        private void FixedUpdate()
        {
            if (!item || !target) return;

            Vector3 toTarget = target.position - transform.position;
            float distance = toTarget.magnitude;

            if (distance < 0.001f) return;

            Vector3 dir = toTarget / distance;

            // Spring force with damping
            Vector3 velocity = item.physicBody.velocity;
            Vector3 force = (toTarget * strength) - (velocity * damping);

            // Clamp force to avoid instability
            force = Vector3.ClampMagnitude(force, maxForce);

            item.AddForce(force, ForceMode.Acceleration);

            // Smooth rotation
            Quaternion targetRot = Quaternion.LookRotation(caster.ragdollHand.PalmDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }
}