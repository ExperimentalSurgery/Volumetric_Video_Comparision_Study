#ifndef _DEPTHKIT_STUDIO_UNIFORMS_CGINC
#define _DEPTHKIT_STUDIO_UNIFORMS_CGINC

//TODO per perspective edge blending is unnecessary if the feature is disabled
struct PerspectiveColorBlending
{
    int enablePerspective;
    float viewWeightPowerContribution;
};

float _PerViewDisparityThreshold = 0.05f;
float _PerViewDisparityBlendWidth = 0.05f;
float _SurfaceNormalColorBlendingPower = 1.0f;
float _GlobalViewDependentColorBlendWeight = 1.0f;
float3 _UntexturedFragDefaultColor;
float _InvalidateEdgeWidth = 0.5f;


StructuredBuffer<PerspectiveColorBlending> _PerspectiveColorBlending;

#endif //_DEPTHKIT_CORE_UNIFORMS_CGINC