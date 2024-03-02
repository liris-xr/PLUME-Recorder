using System.Collections.Generic;
using PLUME.Base.Events;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;
using UnityEngine.Scripting;
using TextSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<UnityEngine.UI.Text>;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.Text
{
    [Preserve]
    public class TextRecorderModule : ComponentRecorderModule<UnityEngine.UI.Text, TextFrameData>
    {
        private readonly Dictionary<TextSafeRef, TextCreate> _createSamples = new();
        private readonly Dictionary<TextSafeRef, TextDestroy> _destroySamples = new();
        private readonly Dictionary<TextSafeRef, TextUpdate> _updateSamples = new();

        protected override void OnCreate(RecorderContext ctx)
        {
            base.OnCreate(ctx);
            TextEvents.OnTextChanged += (text, value) => OnTextChanged(ctx, text, value);
        }
        
        protected override void OnObjectMarkedCreated(TextSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);
            
            var text = objSafeRef.Component;
            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Text = text.text;
            updateSample.FontId = GetAssetIdentifierPayload(text.font);
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
            _createSamples[objSafeRef] = new TextCreate { Id = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnObjectMarkedDestroyed(TextSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new TextDestroy { Id = GetComponentIdentifierPayload(objSafeRef) };
        }
        
        private void OnTextChanged(RecorderContext ctx, UnityEngine.UI.Text text, string value)
        {
            if (!ctx.IsRecording)
                return;

            var objSafeRef = ctx.ObjectSafeRefProvider.GetOrCreateComponentSafeRef(text);

            if (!IsRecordingObject(objSafeRef))
                return;

            var updateSample = GetOrCreateUpdateSample(objSafeRef);
            updateSample.Text = value;
        }

        private TextUpdate GetOrCreateUpdateSample(TextSafeRef objSafeRef)
        {
            if (_updateSamples.TryGetValue(objSafeRef, out var sample))
                return sample;
            sample = new TextUpdate { Id = GetComponentIdentifierPayload(objSafeRef) };
            _updateSamples[objSafeRef] = sample;
            return sample;
        }

        protected override TextFrameData CollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            var frameData = TextFrameData.Pool.Get();
            frameData.AddCreateSamples(_createSamples.Values);
            frameData.AddDestroySamples(_destroySamples.Values);
            frameData.AddUpdateSamples(_updateSamples.Values);
            return frameData;
        }

        protected override void OnAfterCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.OnAfterCollectFrameData(frameInfo, ctx);
            _createSamples.Clear();
            _destroySamples.Clear();
            _updateSamples.Clear();
        }
    }
}