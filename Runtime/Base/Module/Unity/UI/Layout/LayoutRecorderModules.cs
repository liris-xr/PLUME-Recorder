using PLUME.Base.Module.Unity.UI.Common;
using PLUME.Sample.Unity;
using PLUME.Sample.Unity.UI;
using UnityEngine;
using UnityEngine.Scripting;
using Empty = Google.Protobuf.WellKnownTypes.Empty;
using static PLUME.Core.Utils.SampleUtils;

// Layout components are recorded for analysis only: the RectTransform module already records the sizes and positions
// they produce, so a player must not re-run them on top of the recorded RectTransforms.
namespace PLUME.Base.Module.Unity.UI.Layout
{
    /// <summary>Records <see cref="UnityEngine.UI.HorizontalLayoutGroup"/> and
    /// <see cref="UnityEngine.UI.VerticalLayoutGroup"/>.</summary>
    [Preserve]
    public class HorizontalOrVerticalLayoutGroupRecorderModule : PolledUIComponentRecorderModule<
        UnityEngine.UI.HorizontalOrVerticalLayoutGroup, HorizontalOrVerticalLayoutGroupRecorderModule.State,
        HorizontalOrVerticalLayoutGroupCreate, HorizontalOrVerticalLayoutGroupDestroy,
        HorizontalOrVerticalLayoutGroupUpdate, Empty>
    {
        public struct State
        {
            public bool Enabled;
            public bool Vertical;
            public Vector4 Padding;
            public UnityEngine.TextAnchor ChildAlignment;
            public float Spacing;
            public bool ChildControlWidth;
            public bool ChildControlHeight;
            public bool ChildForceExpandWidth;
            public bool ChildForceExpandHeight;
            public bool ChildScaleWidth;
            public bool ChildScaleHeight;
            public bool ReverseArrangement;
        }

        protected override HorizontalOrVerticalLayoutGroupCreate CreateCreateSample(ComponentIdentifier id) =>
            new() { Component = id };

        protected override HorizontalOrVerticalLayoutGroupDestroy CreateDestroySample(ComponentIdentifier id) =>
            new() { Component = id };

        protected override HorizontalOrVerticalLayoutGroupUpdate CreateUpdateSample(ComponentIdentifier id) =>
            new() { Component = id };

        protected override State ReadState(UnityEngine.UI.HorizontalOrVerticalLayoutGroup group) => new()
        {
            Enabled = group.enabled,
            Vertical = group is UnityEngine.UI.VerticalLayoutGroup,
            Padding = UIDiff.ToVector4(group.padding),
            ChildAlignment = group.childAlignment,
            Spacing = group.spacing,
            ChildControlWidth = group.childControlWidth,
            ChildControlHeight = group.childControlHeight,
            ChildForceExpandWidth = group.childForceExpandWidth,
            ChildForceExpandHeight = group.childForceExpandHeight,
            ChildScaleWidth = group.childScaleWidth,
            ChildScaleHeight = group.childScaleHeight,
            ReverseArrangement = group.reverseArrangement
        };

        protected override void WriteChanges(bool has, in State p, in State c, UpdateSampleBuilder sample)
        {
            if (UIDiff.Changed(has, p.Enabled, c.Enabled))
                sample.Get().Enabled = c.Enabled;
            if (UIDiff.Changed(has, p.Vertical, c.Vertical))
                sample.Get().Vertical = c.Vertical;
            if (UIDiff.Changed(has, p.Padding, c.Padding))
                sample.Get().Padding = c.Padding.ToPayload();
            if (UIDiff.Changed(has, (int)p.ChildAlignment, (int)c.ChildAlignment))
                sample.Get().ChildAlignment = c.ChildAlignment.ToPayload();
            if (UIDiff.Changed(has, p.Spacing, c.Spacing))
                sample.Get().Spacing = c.Spacing;
            if (UIDiff.Changed(has, p.ChildControlWidth, c.ChildControlWidth))
                sample.Get().ChildControlWidth = c.ChildControlWidth;
            if (UIDiff.Changed(has, p.ChildControlHeight, c.ChildControlHeight))
                sample.Get().ChildControlHeight = c.ChildControlHeight;
            if (UIDiff.Changed(has, p.ChildForceExpandWidth, c.ChildForceExpandWidth))
                sample.Get().ChildForceExpandWidth = c.ChildForceExpandWidth;
            if (UIDiff.Changed(has, p.ChildForceExpandHeight, c.ChildForceExpandHeight))
                sample.Get().ChildForceExpandHeight = c.ChildForceExpandHeight;
            if (UIDiff.Changed(has, p.ChildScaleWidth, c.ChildScaleWidth))
                sample.Get().ChildScaleWidth = c.ChildScaleWidth;
            if (UIDiff.Changed(has, p.ChildScaleHeight, c.ChildScaleHeight))
                sample.Get().ChildScaleHeight = c.ChildScaleHeight;
            if (UIDiff.Changed(has, p.ReverseArrangement, c.ReverseArrangement))
                sample.Get().ReverseArrangement = c.ReverseArrangement;
        }
    }

    [Preserve]
    public class GridLayoutGroupRecorderModule : PolledUIComponentRecorderModule<UnityEngine.UI.GridLayoutGroup,
        GridLayoutGroupRecorderModule.State, GridLayoutGroupCreate, GridLayoutGroupDestroy, GridLayoutGroupUpdate,
        Empty>
    {
        public struct State
        {
            public bool Enabled;
            public Vector4 Padding;
            public UnityEngine.TextAnchor ChildAlignment;
            public Vector2 CellSize;
            public Vector2 Spacing;
            public int StartCorner;
            public int StartAxis;
            public int Constraint;
            public int ConstraintCount;
        }

        protected override GridLayoutGroupCreate CreateCreateSample(ComponentIdentifier id) =>
            new() { Component = id };

        protected override GridLayoutGroupDestroy CreateDestroySample(ComponentIdentifier id) =>
            new() { Component = id };

        protected override GridLayoutGroupUpdate CreateUpdateSample(ComponentIdentifier id) =>
            new() { Component = id };

        protected override State ReadState(UnityEngine.UI.GridLayoutGroup group) => new()
        {
            Enabled = group.enabled,
            Padding = UIDiff.ToVector4(group.padding),
            ChildAlignment = group.childAlignment,
            CellSize = group.cellSize,
            Spacing = group.spacing,
            StartCorner = (int)group.startCorner,
            StartAxis = (int)group.startAxis,
            Constraint = (int)group.constraint,
            ConstraintCount = group.constraintCount
        };

        protected override void WriteChanges(bool has, in State p, in State c, UpdateSampleBuilder sample)
        {
            if (UIDiff.Changed(has, p.Enabled, c.Enabled))
                sample.Get().Enabled = c.Enabled;
            if (UIDiff.Changed(has, p.Padding, c.Padding))
                sample.Get().Padding = c.Padding.ToPayload();
            if (UIDiff.Changed(has, (int)p.ChildAlignment, (int)c.ChildAlignment))
                sample.Get().ChildAlignment = c.ChildAlignment.ToPayload();
            if (UIDiff.Changed(has, p.CellSize, c.CellSize))
                sample.Get().CellSize = c.CellSize.ToPayload();
            if (UIDiff.Changed(has, p.Spacing, c.Spacing))
                sample.Get().Spacing = c.Spacing.ToPayload();
            if (UIDiff.Changed(has, p.StartCorner, c.StartCorner))
                sample.Get().StartCorner = (GridCorner)c.StartCorner;
            if (UIDiff.Changed(has, p.StartAxis, c.StartAxis))
                sample.Get().StartAxis = (GridAxis)c.StartAxis;
            if (UIDiff.Changed(has, p.Constraint, c.Constraint))
                sample.Get().Constraint = (GridConstraint)c.Constraint;
            if (UIDiff.Changed(has, p.ConstraintCount, c.ConstraintCount))
                sample.Get().ConstraintCount = c.ConstraintCount;
        }
    }

    [Preserve]
    public class LayoutElementRecorderModule : PolledUIComponentRecorderModule<UnityEngine.UI.LayoutElement,
        LayoutElementRecorderModule.State, LayoutElementCreate, LayoutElementDestroy, LayoutElementUpdate, Empty>
    {
        public struct State
        {
            public bool Enabled;
            public bool IgnoreLayout;
            public float MinWidth;
            public float MinHeight;
            public float PreferredWidth;
            public float PreferredHeight;
            public float FlexibleWidth;
            public float FlexibleHeight;
            public int LayoutPriority;
        }

        protected override LayoutElementCreate CreateCreateSample(ComponentIdentifier id) => new() { Component = id };

        protected override LayoutElementDestroy CreateDestroySample(ComponentIdentifier id) =>
            new() { Component = id };

        protected override LayoutElementUpdate CreateUpdateSample(ComponentIdentifier id) => new() { Component = id };

        protected override State ReadState(UnityEngine.UI.LayoutElement element) => new()
        {
            Enabled = element.enabled,
            IgnoreLayout = element.ignoreLayout,
            MinWidth = element.minWidth,
            MinHeight = element.minHeight,
            PreferredWidth = element.preferredWidth,
            PreferredHeight = element.preferredHeight,
            FlexibleWidth = element.flexibleWidth,
            FlexibleHeight = element.flexibleHeight,
            LayoutPriority = element.layoutPriority
        };

        protected override void WriteChanges(bool has, in State p, in State c, UpdateSampleBuilder sample)
        {
            if (UIDiff.Changed(has, p.Enabled, c.Enabled))
                sample.Get().Enabled = c.Enabled;
            if (UIDiff.Changed(has, p.IgnoreLayout, c.IgnoreLayout))
                sample.Get().IgnoreLayout = c.IgnoreLayout;
            if (UIDiff.Changed(has, p.MinWidth, c.MinWidth))
                sample.Get().MinWidth = c.MinWidth;
            if (UIDiff.Changed(has, p.MinHeight, c.MinHeight))
                sample.Get().MinHeight = c.MinHeight;
            if (UIDiff.Changed(has, p.PreferredWidth, c.PreferredWidth))
                sample.Get().PreferredWidth = c.PreferredWidth;
            if (UIDiff.Changed(has, p.PreferredHeight, c.PreferredHeight))
                sample.Get().PreferredHeight = c.PreferredHeight;
            if (UIDiff.Changed(has, p.FlexibleWidth, c.FlexibleWidth))
                sample.Get().FlexibleWidth = c.FlexibleWidth;
            if (UIDiff.Changed(has, p.FlexibleHeight, c.FlexibleHeight))
                sample.Get().FlexibleHeight = c.FlexibleHeight;
            if (UIDiff.Changed(has, p.LayoutPriority, c.LayoutPriority))
                sample.Get().LayoutPriority = c.LayoutPriority;
        }
    }

    [Preserve]
    public class AspectRatioFitterRecorderModule : PolledUIComponentRecorderModule<
        UnityEngine.UI.AspectRatioFitter, AspectRatioFitterRecorderModule.State, AspectRatioFitterCreate,
        AspectRatioFitterDestroy, AspectRatioFitterUpdate, Empty>
    {
        public struct State
        {
            public bool Enabled;
            public int AspectMode;
            public float AspectRatio;
        }

        protected override AspectRatioFitterCreate CreateCreateSample(ComponentIdentifier id) =>
            new() { Component = id };

        protected override AspectRatioFitterDestroy CreateDestroySample(ComponentIdentifier id) =>
            new() { Component = id };

        protected override AspectRatioFitterUpdate CreateUpdateSample(ComponentIdentifier id) =>
            new() { Component = id };

        protected override State ReadState(UnityEngine.UI.AspectRatioFitter fitter) => new()
        {
            Enabled = fitter.enabled,
            AspectMode = (int)fitter.aspectMode,
            AspectRatio = fitter.aspectRatio
        };

        protected override void WriteChanges(bool has, in State p, in State c, UpdateSampleBuilder sample)
        {
            if (UIDiff.Changed(has, p.Enabled, c.Enabled))
                sample.Get().Enabled = c.Enabled;
            if (UIDiff.Changed(has, p.AspectMode, c.AspectMode))
                sample.Get().AspectMode = (AspectMode)c.AspectMode;
            if (UIDiff.Changed(has, p.AspectRatio, c.AspectRatio))
                sample.Get().AspectRatio = c.AspectRatio;
        }
    }
}
