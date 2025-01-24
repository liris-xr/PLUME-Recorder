#if TMP_ENABLED
using System.Collections.Generic;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;
using TMPro;
using UnityEngine.Scripting;
using TMPTextSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<TMPro.TextMeshProUGUI>;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.Graphics.Text
{
    [Preserve]
    public class TMPTextRecorderModule : GraphicRecorderModule<TextMeshProUGUI, TMPTextFrameData>
    {
        private readonly Dictionary<TMPTextSafeRef, TMPTextCreate> _createSamples = new();
        private readonly Dictionary<TMPTextSafeRef, TMPTextDestroy> _destroySamples = new();
        private readonly Dictionary<TMPTextSafeRef, TMPTextUpdate> _updateSamples = new();
        
        protected override void OnObjectMarkedCreated(TMPTextSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);
            
            var text = objSafeRef.Component;
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Text = text.text;
            updateSample.Color = text.color.ToPayload(); 
            updateSample.Font = GetAssetIdentifierPayload(text.font);
            updateSample.FontStyle = (int) text.fontStyle;
            updateSample.FontSize = text.fontSize;
            updateSample.AutoSize = text.enableAutoSizing;
            updateSample.FontSizeMin = text.fontSizeMin;
            updateSample.FontSizeMax = text.fontSizeMax;
            updateSample.CharacterSpacing = text.characterSpacing;
            updateSample.WordSpacing = text.wordSpacing;
            updateSample.LineSpacing = text.lineSpacing;
            updateSample.ParagraphSpacing = text.paragraphSpacing;
            updateSample.Alignment = (int) text.alignment;
            updateSample.WrappingEnabled = text.enableWordWrapping;
            updateSample.Overflow = (int) text.overflowMode;
            updateSample.HorizontalMapping = (int) text.horizontalMapping;
            updateSample.VerticalMapping = (int) text.verticalMapping;
            updateSample.Margin = text.margin.ToPayload();
            _createSamples[objSafeRef] = new TMPTextCreate { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(TMPTextSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new TMPTextDestroy { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        private TMPTextUpdate GetOrCreateUpdateSample(TMPTextSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new TMPTextUpdate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override TMPTextFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = TMPTextFrameData.Pool.Get();
            frameData.AddCreateSamples(_createSamples.Values);
            frameData.AddDestroySamples(_destroySamples.Values);
            frameData.AddUpdateSamples(_updateSamples.Values);
            frameData.AddGraphicUpdateSamples(GetGraphicUpdateSamples());
            return frameData;
        }

        protected override void AfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.AfterCollectFrameData(frameInfo, ctx);
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
        }
    }
}
#endif