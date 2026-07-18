using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PLUME.Core.Settings
{
    /// <summary>
    /// Which render pipeline assets to include when building the asset bundle. Bundling a render
    /// pipeline asset drags in its shader variant set, the dominant asset-bundle build cost.
    /// </summary>
    public enum RenderPipelineExportMode
    {
        // The render pipeline asset currently in use (active quality level override, or the
        // graphics default). Stateless, resolved at build time. Cheapest bundle. Default.
        CurrentActive,

        // Every quality level's render pipeline asset plus the graphics default.
        All,

        // A user-selected set of render pipeline assets (see CustomRenderPipelineAssets).
        Custom
    }

    [Serializable]
    public sealed class RecorderSettings : Settings
    {
        internal const string SettingsWindowPath = "Project/PLUME Recorder";

        public bool StartOnPlay => startOnPlay;

        public string DefaultRecordPrefix => defaultRecordPrefix;

        public string DefaultRecordExtraMetadata => defaultRecordExtraMetadata;

        public RenderPipelineExportMode RenderPipelineExport => renderPipelineExport;

        public IReadOnlyList<RenderPipelineAsset> CustomRenderPipelineAssets => customRenderPipelineAssets;

        [SerializeField] [Tooltip("If true, the recorder will start recording as soon as the game starts.")]
        private bool startOnPlay = true;

        [SerializeField] [Tooltip("Default name of the records. Will be suffixed with the date.")]
        private string defaultRecordPrefix = "record";

        [SerializeField] [Tooltip("Extra information that might be relevant to integrate in every record.")]
        private string defaultRecordExtraMetadata = "";

        [SerializeField]
        [Tooltip("Which render pipeline assets to include in the asset bundle. Fewer pipelines = fewer shader variants to compile = faster builds.")]
        private RenderPipelineExportMode renderPipelineExport = RenderPipelineExportMode.CurrentActive;

        [SerializeField]
        [Tooltip("Render pipeline assets to include when Render Pipeline Export is set to Custom.")]
        private List<RenderPipelineAsset> customRenderPipelineAssets = new();

        internal override string GetSettingsFileName()
        {
            return "RecorderSettings";
        }

        internal override string GetSettingsWindowPath()
        {
            return SettingsWindowPath;
        }
    }
}