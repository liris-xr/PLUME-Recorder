using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;
using UnityEngine;
using UnityEngine.Scripting;
using Empty = Google.Protobuf.WellKnownTypes.Empty;
using static PLUME.Core.Utils.SampleUtils;

namespace PLUME.Base.Module.Unity.UI.Interaction
{
    /// <summary>
    /// Records scroll views. The scrolled content's movement is already captured by its RectTransform; this module
    /// adds the normalized scroll position and configuration for analysis.
    /// </summary>
    [Preserve]
    public class ScrollRectRecorderModule : PolledUIComponentRecorderModule<UnityEngine.UI.ScrollRect,
        ScrollRectRecorderModule.State, ScrollRectCreate, ScrollRectDestroy, ScrollRectUpdate, Empty>
    {
        public struct State
        {
            public bool Enabled;
            public Vector2 NormalizedPosition;
            public bool Horizontal;
            public bool Vertical;
            public UnityEngine.RectTransform Content;
            public UnityEngine.RectTransform Viewport;
        }

        protected override ScrollRectCreate CreateCreateSample(ComponentIdentifier id) => new() { Component = id };

        protected override ScrollRectDestroy CreateDestroySample(ComponentIdentifier id) => new() { Component = id };

        protected override ScrollRectUpdate CreateUpdateSample(ComponentIdentifier id) => new() { Component = id };

        protected override State ReadState(UnityEngine.UI.ScrollRect scrollRect) => new()
        {
            Enabled = scrollRect.enabled,
            NormalizedPosition = scrollRect.normalizedPosition,
            Horizontal = scrollRect.horizontal,
            Vertical = scrollRect.vertical,
            Content = scrollRect.content,
            Viewport = scrollRect.viewport
        };

        protected override void WriteChanges(bool has, in State p, in State c, UpdateSampleBuilder sample)
        {
            if (UIDiff.Changed(has, p.Enabled, c.Enabled))
                sample.Get().Enabled = c.Enabled;
            if (UIDiff.Changed(has, p.NormalizedPosition, c.NormalizedPosition))
                sample.Get().NormalizedPosition = c.NormalizedPosition.ToPayload();
            if (UIDiff.Changed(has, p.Horizontal, c.Horizontal))
                sample.Get().Horizontal = c.Horizontal;
            if (UIDiff.Changed(has, p.Vertical, c.Vertical))
                sample.Get().Vertical = c.Vertical;
            if (UIDiff.Changed(has, p.Content, c.Content))
                sample.Get().Content = GetComponentIdentifierPayload(c.Content);
            if (UIDiff.Changed(has, p.Viewport, c.Viewport))
                sample.Get().Viewport = GetComponentIdentifierPayload(c.Viewport);
        }
    }
}
