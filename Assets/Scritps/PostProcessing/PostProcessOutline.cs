using System;
using UnityEditor;
using UnityEngine.Rendering.PostProcessing;
using PostProcessAttribute = UnityEngine.Rendering.PostProcessing.PostProcessAttribute;

[Serializable]
[PostProcess(typeof(PostProcessOutlineRenderer), PostProcessEvent.AfterStack, "Outline")]
public sealed class PostProcessOutline : PostProcessEffectSettings
{
    public FloatParameter thinkness = new FloatParameter() { value = 1f };
    public FloatParameter depthMin = new FloatParameter() { value = 0f };
    public FloatParameter depthMax = new FloatParameter() { value = 1f };


}
