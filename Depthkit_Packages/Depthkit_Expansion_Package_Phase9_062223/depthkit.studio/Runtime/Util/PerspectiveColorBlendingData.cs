using UnityEngine;
using System;

namespace Depthkit
{
    [Serializable]
    public struct PerspectiveColorBlending
    {
#pragma warning disable CS0414
        public static PerspectiveColorBlending[] Create(int count)
        {
            PerspectiveColorBlending[] data = new PerspectiveColorBlending[count];
            for (int i = 0; i < count; ++i)
            {
                data[i].enabled = 1;
                data[i].viewWeightPowerContribution = 1.0f;
            }
            return data;
        }

        public int enabled;
        public float viewWeightPowerContribution;
    };

#pragma warning restore CS0414

    [Serializable]
    public class PerspectiveColorBlendingData : SyncedStructuredBuffer<PerspectiveColorBlending>
    {
        public PerspectiveColorBlendingData(string name, int count) : base(name, count, PerspectiveColorBlending.Create(count))
        { }

        public float GetViewDependentColorBlendContribution(int perspective)
        {
            return m_data[perspective].viewWeightPowerContribution;
        }

        public void SetViewDependentColorBlendContribution(int perspective, float contribution)
        {
            contribution = Mathf.Clamp01(contribution);
            if (!Mathf.Approximately(contribution, m_data[perspective].viewWeightPowerContribution))
            {
                m_data[perspective].viewWeightPowerContribution = contribution;
                MarkDirty();
            }
        }

        public bool GetPerspectiveEnabled(int perspective)
        {
            return m_data[perspective].enabled == 1;
        }

        public void SetPerspectiveEnabled(int perspective, bool enabled)
        {
            if ((m_data[perspective].enabled == 1) != enabled)
            {
                m_data[perspective].enabled = enabled ? 1 : 0;
                MarkDirty();
            }
        }
    };
}