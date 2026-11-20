#if TMP_ENABLED
using System.Collections.Generic;
using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Core.Recorder;
using PLUME.Core.Recorder.Module.Frame;
using PLUME.Sample.Unity.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using TMPTextSafeRef = PLUME.Core.Object.SafeRef.IComponentSafeRef<TMPro.TextMeshProUGUI>;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.Graphics.Text
{
    [Preserve]
    public class TMPTextRecorderModule : GraphicRecorderModule<TextMeshProUGUI, TMPTextFrameData>
    {
        private struct TMPTextState
        {
            public string Text;
            public Color Color;
            public TMP_FontAsset Font;
            public FontStyles FontStyle;
            public float FontSize;
            public bool AutoSize;
            public float FontSizeMin;
            public float FontSizeMax;
            public float CharacterSpacing;
            public float WordSpacing;
            public float LineSpacing;
            public float ParagraphSpacing;
            public TextAlignmentOptions Alignment;
            public bool WrappingEnabled;
            public TextOverflowModes Overflow;
            public TextureMappingOptions HorizontalMapping;
            public TextureMappingOptions VerticalMapping;
            public Vector4 Margin;
        }

        private readonly Dictionary<TMPTextSafeRef, TMPTextCreate> _createSamples = new();
        private readonly Dictionary<TMPTextSafeRef, TMPTextDestroy> _destroySamples = new();
        private readonly Dictionary<TMPTextSafeRef, TMPTextUpdate> _updateSamples = new();

        // Last recorded state per text, compared every frame. Text set by scripts after creation (questionnaire
        // labels, scores, timers...) is the common case, and TMP exposes no hookable setter for it from user code.
        private readonly Dictionary<TMPTextSafeRef, TMPTextState> _states = new();

        // The TMP material getter instantiates a material on first access; the shared material is the asset.
        protected override Material GetRecordedMaterial(TextMeshProUGUI graphic) => graphic.fontSharedMaterial;

        protected override void OnObjectMarkedCreated(TMPTextSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedCreated(objSafeRef, ctx);
            _createSamples[objSafeRef] = new TMPTextCreate { Component = GetComponentIdentifierPayload(objSafeRef) };
            _states.Remove(objSafeRef);
            PollText(objSafeRef);
        }

        protected override void OnObjectMarkedDestroyed(TMPTextSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnObjectMarkedDestroyed(objSafeRef, ctx);
            _destroySamples[objSafeRef] = new TMPTextDestroy { Component = GetComponentIdentifierPayload(objSafeRef) };
        }

        protected override void OnStopRecordingObject(TMPTextSafeRef objSafeRef, RecorderContext ctx)
        {
            base.OnStopRecordingObject(objSafeRef, ctx);
            _states.Remove(objSafeRef);
        }

        protected override void OnStopRecording(RecorderContext ctx)
        {
            base.OnStopRecording(ctx);
            _states.Clear();
        }

        protected override void BeforeCollectFrameData(FrameInfo frameInfo, RecorderContext ctx)
        {
            base.BeforeCollectFrameData(frameInfo, ctx);

            var recorded = RecordedComponents;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < recorded.Count; i++)
                PollText(recorded[i]);
        }

        private void PollText(TMPTextSafeRef objSafeRef)
        {
            var text = objSafeRef.Component;

            if (text == null)
                return;

            var current = new TMPTextState
            {
                Text = text.text,
                Color = text.color,
                Font = text.font,
                FontStyle = text.fontStyle,
                FontSize = text.fontSize,
                AutoSize = text.enableAutoSizing,
                FontSizeMin = text.fontSizeMin,
                FontSizeMax = text.fontSizeMax,
                CharacterSpacing = text.characterSpacing,
                WordSpacing = text.wordSpacing,
                LineSpacing = text.lineSpacing,
                ParagraphSpacing = text.paragraphSpacing,
                Alignment = text.alignment,
#pragma warning disable CS0618 // enableWordWrapping is obsolete in recent TMP versions but still reflects wrapping
                WrappingEnabled = text.enableWordWrapping,
#pragma warning restore CS0618
                Overflow = text.overflowMode,
                HorizontalMapping = text.horizontalMapping,
                VerticalMapping = text.verticalMapping,
                Margin = text.margin
            };

            var has = _states.TryGetValue(objSafeRef, out var p);

            if (UIDiff.Changed(has, p.Text, current.Text))
                GetOrCreateUpdateSample(objSafeRef).Text = current.Text ?? "";
            if (UIDiff.Changed(has, p.Color, current.Color))
                GetOrCreateUpdateSample(objSafeRef).Color = current.Color.ToPayload();
            if (UIDiff.Changed(has, p.Font, current.Font))
                GetOrCreateUpdateSample(objSafeRef).Font = GetAssetIdentifierPayload(current.Font);
            if (UIDiff.Changed(has, (int)p.FontStyle, (int)current.FontStyle))
                GetOrCreateUpdateSample(objSafeRef).FontStyle = (int)current.FontStyle;
            if (UIDiff.Changed(has, p.FontSize, current.FontSize))
                GetOrCreateUpdateSample(objSafeRef).FontSize = current.FontSize;
            if (UIDiff.Changed(has, p.AutoSize, current.AutoSize))
                GetOrCreateUpdateSample(objSafeRef).AutoSize = current.AutoSize;
            if (UIDiff.Changed(has, p.FontSizeMin, current.FontSizeMin))
                GetOrCreateUpdateSample(objSafeRef).FontSizeMin = current.FontSizeMin;
            if (UIDiff.Changed(has, p.FontSizeMax, current.FontSizeMax))
                GetOrCreateUpdateSample(objSafeRef).FontSizeMax = current.FontSizeMax;
            if (UIDiff.Changed(has, p.CharacterSpacing, current.CharacterSpacing))
                GetOrCreateUpdateSample(objSafeRef).CharacterSpacing = current.CharacterSpacing;
            if (UIDiff.Changed(has, p.WordSpacing, current.WordSpacing))
                GetOrCreateUpdateSample(objSafeRef).WordSpacing = current.WordSpacing;
            if (UIDiff.Changed(has, p.LineSpacing, current.LineSpacing))
                GetOrCreateUpdateSample(objSafeRef).LineSpacing = current.LineSpacing;
            if (UIDiff.Changed(has, p.ParagraphSpacing, current.ParagraphSpacing))
                GetOrCreateUpdateSample(objSafeRef).ParagraphSpacing = current.ParagraphSpacing;
            if (UIDiff.Changed(has, (int)p.Alignment, (int)current.Alignment))
                GetOrCreateUpdateSample(objSafeRef).Alignment = (int)current.Alignment;
            if (UIDiff.Changed(has, p.WrappingEnabled, current.WrappingEnabled))
                GetOrCreateUpdateSample(objSafeRef).WrappingEnabled = current.WrappingEnabled;
            if (UIDiff.Changed(has, (int)p.Overflow, (int)current.Overflow))
                GetOrCreateUpdateSample(objSafeRef).Overflow = (int)current.Overflow;
            if (UIDiff.Changed(has, (int)p.HorizontalMapping, (int)current.HorizontalMapping))
                GetOrCreateUpdateSample(objSafeRef).HorizontalMapping = (int)current.HorizontalMapping;
            if (UIDiff.Changed(has, (int)p.VerticalMapping, (int)current.VerticalMapping))
                GetOrCreateUpdateSample(objSafeRef).VerticalMapping = (int)current.VerticalMapping;
            if (UIDiff.Changed(has, p.Margin, current.Margin))
                GetOrCreateUpdateSample(objSafeRef).Margin = current.Margin.ToPayload();

            _states[objSafeRef] = current;
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
