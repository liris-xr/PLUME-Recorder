using System.Collections.Generic;
using PLUME.Core.Hooks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Hooks
{
    [Preserve]
    public class LineRendererHooks : IRegisterHooksCallback
    {
        public delegate void OnPositionsChangedDelegate(LineRenderer lineRenderer, IEnumerable<Vector3> positions);

        public delegate void OnColorChangedDelegate(LineRenderer lineRenderer, Gradient color);

        public delegate void OnWidthCurveChangedDelegate(LineRenderer lineRenderer, AnimationCurve widthCurve);

        public delegate void OnWidthMultiplierChangedDelegate(LineRenderer lineRenderer, float widthMultiplier);

        public static event OnPositionsChangedDelegate OnPositionsChanged = delegate { };

        public static event OnColorChangedDelegate OnColorChanged = delegate { };

        public static event OnWidthCurveChangedDelegate OnWidthCurveChanged = delegate { };

        public static event OnWidthMultiplierChangedDelegate OnWidthMultiplierChanged = delegate { };

        public void RegisterHooks(HooksRegistry hooksRegistry)
        {
            hooksRegistry.RegisterHook(typeof(LineRendererHooks).GetMethod(nameof(SetWidthMultiplierAndNotify)),
                typeof(LineRenderer).GetProperty(nameof(LineRenderer.widthMultiplier))!.GetSetMethod());

            hooksRegistry.RegisterHook(typeof(LineRendererHooks).GetMethod(nameof(SetStartWidthAndNotify)),
                typeof(LineRenderer).GetProperty(nameof(LineRenderer.startWidth))!.GetSetMethod());

            hooksRegistry.RegisterHook(typeof(LineRendererHooks).GetMethod(nameof(SetEndWidthAndNotify)),
                typeof(LineRenderer).GetProperty(nameof(LineRenderer.endWidth))!.GetSetMethod());

            hooksRegistry.RegisterHook(typeof(LineRendererHooks).GetMethod(nameof(SetWidthCurveAndNotify)),
                typeof(LineRenderer).GetProperty(nameof(LineRenderer.widthCurve))!.GetSetMethod());

            hooksRegistry.RegisterHook(typeof(LineRendererHooks).GetMethod(nameof(SetStartColorAndNotify)),
                typeof(LineRenderer).GetProperty(nameof(LineRenderer.startColor))!.GetSetMethod());

            hooksRegistry.RegisterHook(typeof(LineRendererHooks).GetMethod(nameof(SetEndColorAndNotify)),
                typeof(LineRenderer).GetProperty(nameof(LineRenderer.endColor))!.GetSetMethod());

            hooksRegistry.RegisterHook(typeof(LineRendererHooks).GetMethod(nameof(SetColorAndNotify)),
                typeof(LineRenderer).GetProperty(nameof(LineRenderer.colorGradient))!.GetSetMethod());

            hooksRegistry.RegisterHook(typeof(LineRendererHooks).GetMethod(nameof(SetPositionCountPropertyAndNotify)),
                typeof(LineRenderer).GetProperty(nameof(LineRenderer.positionCount))!.GetSetMethod());

            hooksRegistry.RegisterHook(typeof(LineRendererHooks).GetMethod(nameof(SetPositionAndNotify)),
                typeof(LineRenderer).GetMethod(nameof(LineRenderer.SetPosition),
                    new[] { typeof(int), typeof(Vector3) }));

            hooksRegistry.RegisterHook(
                typeof(LineRendererHooks).GetMethod(nameof(SetPositionsAndNotify),
                    new[] { typeof(LineRenderer), typeof(Vector3[]) }),
                typeof(LineRenderer).GetMethod(nameof(LineRenderer.SetPositions), new[] { typeof(Vector3[]) }));

            hooksRegistry.RegisterHook(
                typeof(LineRendererHooks).GetMethod(nameof(SetPositionsAndNotify),
                    new[] { typeof(LineRenderer), typeof(NativeArray<Vector3>) }),
                typeof(LineRenderer).GetMethod(nameof(LineRenderer.SetPositions),
                    new[] { typeof(NativeArray<Vector3>) }));

            hooksRegistry.RegisterHook(
                typeof(LineRendererHooks).GetMethod(nameof(SetPositionsAndNotify),
                    new[] { typeof(LineRenderer), typeof(NativeSlice<Vector3>) }),
                typeof(LineRenderer).GetMethod(nameof(LineRenderer.SetPositions),
                    new[] { typeof(NativeSlice<Vector3>) }));
        }

        public static void SetWidthMultiplierAndNotify(LineRenderer lineRenderer, float widthMultiplier)
        {
            lineRenderer.widthMultiplier = widthMultiplier;
            OnWidthMultiplierChanged(lineRenderer, widthMultiplier);
        }

        public static void SetStartWidthAndNotify(LineRenderer lineRenderer, float startWidth)
        {
            lineRenderer.startWidth = startWidth;
            OnWidthCurveChanged(lineRenderer, lineRenderer.widthCurve);
        }

        public static void SetEndWidthAndNotify(LineRenderer lineRenderer, float endWidth)
        {
            lineRenderer.endWidth = endWidth;
            OnWidthCurveChanged(lineRenderer, lineRenderer.widthCurve);
        }

        public static void SetWidthCurveAndNotify(LineRenderer lineRenderer, AnimationCurve widthCurve)
        {
            lineRenderer.widthCurve = widthCurve;
            OnWidthCurveChanged(lineRenderer, widthCurve);
        }

        public static void SetStartColorAndNotify(LineRenderer lineRenderer, Color startColor)
        {
            lineRenderer.startColor = startColor;
            OnColorChanged(lineRenderer, lineRenderer.colorGradient);
        }

        public static void SetEndColorAndNotify(LineRenderer lineRenderer, Color endColor)
        {
            lineRenderer.endColor = endColor;
            OnColorChanged(lineRenderer, lineRenderer.colorGradient);
        }

        public static void SetColorAndNotify(LineRenderer lineRenderer, Gradient color)
        {
            lineRenderer.colorGradient = color;
            OnColorChanged(lineRenderer, color);
        }

        public static void SetPositionCountPropertyAndNotify(LineRenderer lineRenderer, int positionCount)
        {
            lineRenderer.positionCount = positionCount;
            var positions = new Vector3[positionCount];
            var nPositions = lineRenderer.GetPositions(positions);
            OnPositionsChanged(lineRenderer, positions[..nPositions]);
        }

        public static void SetPositionAndNotify(LineRenderer lineRenderer, int index, Vector3 position)
        {
            lineRenderer.SetPosition(index, position);
            var positions = new Vector3[lineRenderer.positionCount];
            var nPositions = lineRenderer.GetPositions(positions);
            OnPositionsChanged(lineRenderer, positions[..nPositions]);
        }

        public static void SetPositionsAndNotify(LineRenderer lineRenderer, Vector3[] positions)
        {
            lineRenderer.SetPositions(positions);
            OnPositionsChanged(lineRenderer, positions[..lineRenderer.positionCount]);
        }

        public static void SetPositionsAndNotify(LineRenderer lineRenderer, NativeArray<Vector3> positions)
        {
            lineRenderer.SetPositions(positions);
            OnPositionsChanged(lineRenderer, positions.Slice(0, lineRenderer.positionCount));
        }

        public static void SetPositionsAndNotify(LineRenderer lineRenderer, NativeSlice<Vector3> positions)
        {
            lineRenderer.SetPositions(positions);
            OnPositionsChanged(lineRenderer, positions.Slice(0, lineRenderer.positionCount));
        }
    }
}