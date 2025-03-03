using System.Collections.Generic;
using PLUME.Base.Hooks;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;
using UnityEngine.Scripting;
using TextSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.UI.Text>;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.Graphics.Text
{
    [Preserve]
    public class TextRecorderModule : GraphicRecorderModule<UnityEngine.UI.Text, TextFrameData>
    {
        private readonly Dictionary<TextSafeRef, TextCreate> _createSamples = new();
        private readonly Dictionary<TextSafeRef, TextDestroy> _destroySamples = new();
        private readonly Dictionary<TextSafeRef, TextUpdate> _updateSamples = new();

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
            TextHooks.OnTextChanged += (text, value) => OnTextChanged(ctx, text, value);
        }
        
        protected override void OnObjectMarkedCreated(TextSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);
            
            var text = objSafeRef.Component;
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Text = text.text;
            updateSample.Font = GetAssetIdentifierPayload(text.font);
            updateSample.FontStyle = text.fontStyle.ToPayload();
            updateSample.FontSize = text.fontSize;
            updateSample.Color = text.color.ToPayload();
            updateSample.LineSpacing = text.lineSpacing;
            updateSample.SupportRichText = text.supportRichText;
            updateSample.Alignment = text.alignment.ToPayload();
            updateSample.AlignByGeometry = text.alignByGeometry;
            updateSample.HorizontalOverflow = text.horizontalOverflow.ToPayload();
            updateSample.VerticalOverflow = text.verticalOverflow.ToPayload();
            updateSample.ResizeTextForBestFit = text.resizeTextForBestFit;
            updateSample.ResizeTextMinSize = text.resizeTextMinSize;
            updateSample.ResizeTextMaxSize = text.resizeTextMaxSize;
            _createSamples[objSafeRef] = new TextCreate { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(TextSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new TextDestroy { Component = GetComponentIdentifierPayload(objSafeRef) };
        }
        
        private void OnTextChanged(RecorderContext ctx, UnityEngine.UI.Text text, string value)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.SafeRefProvider.GetOrCreateComponentSafeRef(text);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Text = value;
        }

        private TextUpdate GetOrCreateUpdateSample(TextSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new TextUpdate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override TextFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = TextFrameData.Pool.Get();
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