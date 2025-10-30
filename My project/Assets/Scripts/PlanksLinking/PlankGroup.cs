using System.Collections.Generic;
using UnityEngine;

public class PlankGroup : MonoBehaviour
{
    internal static Dictionary<Rigidbody, PlankGroup> plankToGroup = new();
    internal List<Rigidbody> planks = new();

    public static PlankGroup GetOrCreateGroup(Rigidbody basePlank)
    {
        if (plankToGroup.TryGetValue(basePlank, out var g)) return g;

        var obj = new GameObject("PlankGroup");
        g = obj.AddComponent<PlankGroup>();
        g.AddPlank(basePlank);
        return g;
    }

    public void AddPlank(Rigidbody rb)
    {
        if (planks.Contains(rb)) return;
        planks.Add(rb);
        plankToGroup[rb] = this;
    }

    // NEW: Merge another group into this one
    public void MergeGroup(PlankGroup other)
    {
        if (other == this) return;

        // Add all planks from the other group to this one
        foreach (var rb in other.planks)
        {
            if (!planks.Contains(rb))
            {
                planks.Add(rb);
                plankToGroup[rb] = this;
            }
        }

        // Destroy the other group's GameObject
        Destroy(other.gameObject);
    }

    public void WeldAll()
    {
        if (planks.Count < 2) return;

        for (int i = 0; i < planks.Count; i++)
        {
            for (int j = i + 1; j < planks.Count; j++)
            {
                if (planks[i] == planks[j]) continue;

                bool already = false;
                foreach (var joint in planks[i].GetComponents<ConfigurableJoint>())
                {
                    if (joint.connectedBody == planks[j])
                    {
                        already = true;
                        break;
                    }
                }

                if (!already)
                {
                    var joint = planks[i].gameObject.AddComponent<ConfigurableJoint>();
                    joint.connectedBody = planks[j];
                    joint.autoConfigureConnectedAnchor = false;
                    joint.anchor = planks[i].transform.InverseTransformPoint(planks[j].worldCenterOfMass);
                    joint.connectedAnchor = planks[j].transform.InverseTransformPoint(planks[j].worldCenterOfMass);

                    joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Locked;
                    joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Locked;

                    joint.enablePreprocessing = false;
                    joint.projectionMode = JointProjectionMode.PositionAndRotation;
                    joint.projectionDistance = 0.001f;
                    joint.projectionAngle = 1f;
                    joint.breakForce = joint.breakTorque = Mathf.Infinity;

                    planks[i].solverIterations = planks[j].solverIterations = 12;
                    planks[i].solverVelocityIterations = planks[j].solverVelocityIterations = 12;
                    planks[i].interpolation = planks[j].interpolation = RigidbodyInterpolation.Interpolate;
                    planks[i].angularDamping = planks[j].angularDamping = 0.2f;
                }
            }
        }
    }

    public void SetKinematicAndFollow(PlankGroupGrabSync lead)
    {
        Rigidbody leadRb = lead.GetComponent<Rigidbody>();

        foreach (var rb in planks)
        {
            // Destroy all joints first
            foreach (var j in rb.GetComponents<ConfigurableJoint>())
                DestroyImmediate(j);

            // Reset all velocities immediately
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (rb == leadRb)
            {
                // Leader stays dynamic but with frozen rotation
                rb.isKinematic = false;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.useGravity = false;
                rb.linearDamping = 50f;
                rb.angularDamping = 100f;
            }
            else
            {
                // Followers become kinematic and track the leader
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.GetComponent<PlankGroupGrabSync>()?.MarkFollow(lead);
            }
        }
    }

    public void ReleaseKinematic()
    {
        foreach (var rb in planks)
        {
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
            rb.useGravity = true;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.05f;
            rb.GetComponent<PlankGroupGrabSync>()?.MarkFollow(null);
        }

        WeldAll();
    }

    public void RotateGroupOnAxis(Vector3 axis, float angleDegrees)
    {
        if (planks.Count == 0) return;

        // Find the leader
        Rigidbody leader = null;
        foreach (var rb in planks)
        {
            if (!rb.isKinematic)
            {
                leader = rb;
                break;
            }
        }

        if (leader == null) return;

        Vector3 pivot = leader.position;

        // Rotate all planks around the leader's position on the specified axis
        foreach (var rb in planks)
        {
            rb.transform.RotateAround(pivot, axis, angleDegrees);
        }
    }

// Keep the old method for backwards compatibility
    public void RotateGroup(float angleDegrees)
    {
        RotateGroupOnAxis(Vector3.up, angleDegrees);
    }
}