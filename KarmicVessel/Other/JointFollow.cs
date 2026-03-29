using System;
using ThunderRoad;
using UnityEngine;

namespace KarmicVessel.Other
{
    public class FloatPID {
    public float pFactor, iFactor, dFactor;

    float integral;
    float lastError;

    public FloatPID(float pFactor, float iFactor, float dFactor) {
        this.pFactor = pFactor;
        this.iFactor = iFactor;
        this.dFactor = dFactor;
    }

    public float Update(float present, float timeFrame) {
        integral += present * timeFrame;
        float derivation = (present - lastError) / timeFrame;
        lastError = present;
        return present * pFactor + integral * iFactor + derivation * dFactor;
    }
    public void Reset() {
        lastError = 0;
        integral = 0;
    }
}

public class PID {
	public float pFactor, iFactor, dFactor;
		
	Vector3 integral;
	Vector3 lastError;
	
	public PID(float pFactor, float iFactor, float dFactor) {
		this.pFactor = pFactor;
		this.iFactor = iFactor;
		this.dFactor = dFactor;
	}

	public Vector3 Update(Vector3 present, float timeFrame) {
		integral += present * timeFrame;
		Vector3 deriv = (present - lastError) / timeFrame;
		lastError = present;
		return present * pFactor + integral * iFactor + deriv * dFactor;
	}

    public void Reset() {
        lastError = Vector3.zero;
        integral = Vector3.zero;
    }
}

public class RBPID {
    public PID velocityPID;
    public PID headingPID;
    public Rigidbody rb;
    public Vector3 anchor;

    private ForceMode forceMode;

    public bool isActive;
    private float maxForce;

    public RBPID(Rigidbody rigidbody, float p = 1, float i = 0, float d = 0.3f, ForceMode forceMode = ForceMode.Force, float maxForce = 100, Vector3 anchor = default) {
        rb = rigidbody;
        isActive = true;
        this.maxForce = maxForce;
        this.forceMode = forceMode;
        this.anchor = anchor;
        velocityPID = new PID(p, i, d);
        headingPID = new PID(p, i, d);
    }

    public RBPID Position(float p, float i, float d) {
        velocityPID = new PID(p, i, d);
        return this;
    }

    public RBPID Rotation(float p, float i, float d) {
        headingPID = new PID(p, i, d);
        return this;
    }

    public void SetAnchor(Vector3 anchor) {
        this.anchor = anchor;
    }

    public void Update(Vector3 targetPos, Quaternion targetRot, float forceMult = 1) {
        if (!isActive)
            return;
        UpdateVelocity(targetPos, forceMult);
        UpdateTorque(targetRot, forceMult);
    }

    public void UpdateVelocity(Vector3 targetPos, float forceMult = 1f) {
        if (!isActive)
            return;
        if (Time.deltaTime == 0 || Time.deltaTime == float.NaN) return;
        var force = velocityPID.Update(targetPos - rb.transform.TransformPoint(anchor), Time.deltaTime) * forceMult;
        rb.AddForce(force.ClampMagnitude(0, maxForce), forceMode);
    }

    public void UpdateTorque(Quaternion targetRot, float forceMult = 1f) {
        if (!isActive)
            return;
        if (Time.deltaTime == 0 || Time.deltaTime == float.NaN) return;
        var rotation = (Vector3.Cross(rb.transform.rotation * Vector3.forward, targetRot * Vector3.forward)
                        + Vector3.Cross(
                            rb.transform.rotation
                            * Vector3.up,
                            targetRot * Vector3.up)).normalized
                       * Quaternion.Angle(rb.transform.rotation, targetRot)
                       / 360;

        var torque = headingPID.Update(rotation, Time.deltaTime) * forceMult;
        rb.AddTorque(torque.ClampMagnitude(0, maxForce), forceMode);
    }

    public void Reset() {
        velocityPID.Reset();
        headingPID.Reset();
    }
}

    public class SmoothFollow : MonoBehaviour
    {
        public Transform target;
        public RagdollHand hand;
        public Vector3 offset;

        public RBPID pid;
        
        public Quaternion ForwardRotation {
            get {
                var item = pid.rb.GetComponentInParent<Item>();
                var localBounds = item.GetLocalBounds();
                var smallestAxis = localBounds.size.x < localBounds.size.y ? ExtensionMethods.Axis.X : ExtensionMethods.Axis.Y;
                smallestAxis = localBounds.size.GetAxis(smallestAxis) < localBounds.size.z ? smallestAxis : ExtensionMethods.Axis.Z;
                if (item.flyDirRef) {
                    var lookDir = Quaternion.LookRotation(item.flyDirRef.forward, item.transform.GetUnitAxis(smallestAxis));
                    return lookDir;
                }

                if (item.holderPoint) {
                    return item.transform.rotation
                           * Quaternion.Inverse(item.transform.InverseTransformRotation(item.holderPoint.rotation
                               * Quaternion.AngleAxis(180, Vector3.up)));
                }

                return Quaternion.LookRotation(item.transform.up, item.transform.forward);
            }
        }
        void FixedUpdate()
        {
            if (target == null) return;
            
            pid.SetAnchor(pid.rb.GetComponentInParent<Item>().GetLocalBounds().center);
            pid.Update(target.position + offset, Quaternion.Inverse(pid.rb.GetComponentInParent<Item>().transform.InverseTransformRotation(ForwardRotation)), forceMult: 1f);
        }

        void OnDisable()
        {
            pid?.Reset();
        }
    

    }
}