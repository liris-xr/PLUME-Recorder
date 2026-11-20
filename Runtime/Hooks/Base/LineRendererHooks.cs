using System;
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

        internal static void NotifyWidthMultiplierChanged(LineRenderer lineRenderer, float widthMultiplier)
        {
            try
            {
                OnWidthMultiplierChanged(lineRenderer, widthMultiplier);
            }
            catch (Exception e)
            {
                HookErrors.Report($"{nameof(LineRendererHooks)}.{nameof(OnWidthMultiplierChanged)}",
                    "Failed to record a line renderer width multiplier change. The application is not affected.", e);
            }
        }

        private static void NotifyWidthCurveChanged(LineRenderer lineRenderer, AnimationCurve widthCurve)
        {
            try
            {
                OnWidthCurveChanged(lineRenderer, widthCurve);
            }
            catch (Exception e)
            {
                HookErrors.Report($"{nameof(LineRendererHooks)}.{nameof(OnWidthCurveChanged)}",
                    "Failed to record a line renderer width change. The application is not affected.", e);
            }
        }

        private static void NotifyColorChanged(LineRenderer lineRenderer, Gradient color)
        {
            try
            {
                OnColorChanged(lineRenderer, color);
            }
            catch (Exception e)
            {
                HookErrors.Report($"{nameof(LineRendererHooks)}.{nameof(OnColorChanged)}",
                    "Failed to record a line renderer color change. The application is not affected.", e);
            }
        }

        private static void NotifyPositionsChanged(LineRenderer lineRenderer)
        {
            // Positions are read back inside the guard: slicing the user's input by positionCount throws when the
            // input is shorter, which must not reach the user.
            try
            {
                var positions = new Vector3[lineRenderer.positionCount];
                var nPositions = lineRenderer.GetPositions(positions);
                OnPositionsChanged(lineRenderer, positions[..nPositions]);
            }
            catch (Exception e)
            {
                HookErrors.Report($"{nameof(LineRendererHooks)}.{nameof(OnPositionsChanged)}",
                    "Failed to record a line renderer positions change. The application is not affected.", e);
            }
        }

        public static void SetWidthMultiplierAndNotify(LineRenderer lineRenderer, float widthMultiplier)
        {
            lineRenderer.widthMultiplier = widthMultiplier;
            NotifyWidthMultiplierChanged(lineRenderer, widthMultiplier);
        }

        public static void SetStartWidthAndNotify(LineRenderer lineRenderer, float startWidth)
        {
            lineRenderer.startWidth = startWidth;
            NotifyWidthCurveChanged(lineRenderer, lineRenderer.widthCurve);
        }

        public static void SetEndWidthAndNotify(LineRenderer lineRenderer, float endWidth)
        {
            lineRenderer.endWidth = endWidth;
            NotifyWidthCurveChanged(lineRenderer, lineRenderer.widthCurve);
        }

        public static void SetWidthCurveAndNotify(LineRenderer lineRenderer, AnimationCurve widthCurve)
        {
            lineRenderer.widthCurve = widthCurve;
            NotifyWidthCurveChanged(lineRenderer, widthCurve);
        }

        public static void SetStartColorAndNotify(LineRenderer lineRenderer, Color startColor)
        {
            lineRenderer.startColor = startColor;
            NotifyColorChanged(lineRenderer, lineRenderer.colorGradient);
        }

        public static void SetEndColorAndNotify(LineRenderer lineRenderer, Color endColor)
        {
            lineRenderer.endColor = endColor;
            NotifyColorChanged(lineRenderer, lineRenderer.colorGradient);
        }

        public static void SetColorAndNotify(LineRenderer lineRenderer, Gradient color)
        {
            lineRenderer.colorGradient = color;
            NotifyColorChanged(lineRenderer, color);
        }

        public static void SetPositionCountPropertyAndNotify(LineRenderer lineRenderer, int positionCount)
        {
            lineRenderer.positionCount = positionCount;
            NotifyPositionsChanged(lineRenderer);
        }

        public static void SetPositionAndNotify(LineRenderer lineRenderer, int index, Vector3 position)
        {
            lineRenderer.SetPosition(index, position);
            NotifyPositionsChanged(lineRenderer);
        }

        public static void SetPositionsAndNotify(LineRenderer lineRenderer, Vector3[] positions)
        {
            lineRenderer.SetPositions(positions);
            NotifyPositionsChanged(lineRenderer);
        }

        public static void SetPositionsAndNotify(LineRenderer lineRenderer, NativeArray<Vector3> positions)
        {
            lineRenderer.SetPositions(positions);
            NotifyPositionsChanged(lineRenderer);
        }

        public static void SetPositionsAndNotify(LineRenderer lineRenderer, NativeSlice<Vector3> positions)
        {
            lineRenderer.SetPositions(positions);
            NotifyPositionsChanged(lineRenderer);
        }
    }
}