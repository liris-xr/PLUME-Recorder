using System.Collections.Generic;
using PLUME.Core;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting;

namespace PLUME.Base.Events
{
    [Preserve]
    public static class LineRendererEvents
    {
        public delegate void OnPositionsChangedDelegate(LineRenderer lineRenderer, IEnumerable<Vector3> positions);

        public delegate void OnColorChangedDelegate(LineRenderer lineRenderer, Gradient color);

        public delegate void OnWidthCurveChangedDelegate(LineRenderer lineRenderer, AnimationCurve widthCurve);
        
        public delegate void OnWidthMultiplierChangedDelegate(LineRenderer lineRenderer, float widthMultiplier);

        public static event OnPositionsChangedDelegate OnPositionsChanged = delegate { };

        public static event OnColorChangedDelegate OnColorChanged = delegate { };

        public static event OnWidthCurveChangedDelegate OnWidthCurveChanged = delegate { };
        
        public static event OnWidthMultiplierChangedDelegate OnWidthMultiplierChanged = delegate { };

        [RegisterPropertySetterDetour(typeof(LineRenderer), nameof(LineRenderer.widthMultiplier))]
        public static void SetWidthMultiplierAndNotify(LineRenderer lineRenderer, float widthMultiplier)
        {
            lineRenderer.widthMultiplier = widthMultiplier;
            OnWidthMultiplierChanged(lineRenderer, widthMultiplier);
        }
        
        [RegisterPropertySetterDetour(typeof(LineRenderer), nameof(LineRenderer.startWidth))]
        public static void SetStartWidthAndNotify(LineRenderer lineRenderer, float startWidth)
        {
            lineRenderer.startWidth = startWidth;
            OnWidthCurveChanged(lineRenderer, lineRenderer.widthCurve);
        }

        [RegisterPropertySetterDetour(typeof(LineRenderer), nameof(LineRenderer.startWidth))]
        public static void SetEndWidthAndNotify(LineRenderer lineRenderer, float endWidth)
        {
            lineRenderer.endWidth = endWidth;
            OnWidthCurveChanged(lineRenderer, lineRenderer.widthCurve);
        }

        [RegisterPropertySetterDetour(typeof(LineRenderer), nameof(LineRenderer.widthCurve))]
        public static void SetWidthCurveAndNotify(LineRenderer lineRenderer, AnimationCurve widthCurve)
        {
            lineRenderer.widthCurve = widthCurve;
            OnWidthCurveChanged(lineRenderer, widthCurve);
        }
        
        [RegisterPropertySetterDetour(typeof(LineRenderer), nameof(LineRenderer.startColor))]
        public static void SetStartColorAndNotify(LineRenderer lineRenderer, Color startColor)
        {
            lineRenderer.startColor = startColor;
            OnColorChanged(lineRenderer, lineRenderer.colorGradient);
        }
        
        [RegisterPropertySetterDetour(typeof(LineRenderer), nameof(LineRenderer.endColor))]
        public static void SetEndColorAndNotify(LineRenderer lineRenderer, Color endColor)
        {
            lineRenderer.endColor = endColor;
            OnColorChanged(lineRenderer, lineRenderer.colorGradient);
        }
        
        [RegisterPropertySetterDetour(typeof(LineRenderer), nameof(LineRenderer.colorGradient))]
        public static void SetColorAndNotify(LineRenderer lineRenderer, Gradient color)
        {
            lineRenderer.colorGradient = color;
            OnColorChanged(lineRenderer, color);
        }

        [RegisterMethodDetour(typeof(LineRenderer), nameof(LineRenderer.SetPosition), typeof(int), typeof(Vector3))]
        public static void SetPositionAndNotify(LineRenderer lineRenderer, int index, Vector3 position)
        {
            lineRenderer.SetPosition(index, position);
            var positions = new Vector3[lineRenderer.positionCount];
            var nPositions = lineRenderer.GetPositions(positions);
            OnPositionsChanged(lineRenderer, positions[..nPositions]);
        }

        [RegisterMethodDetour(typeof(LineRenderer), nameof(LineRenderer.SetPositions), typeof(Vector3[]))]
        public static void SetPositionsAndNotify(LineRenderer lineRenderer, Vector3[] positions)
        {
            lineRenderer.SetPositions(positions);
            OnPositionsChanged(lineRenderer, positions);
        }

        [RegisterMethodDetour(typeof(LineRenderer), nameof(LineRenderer.SetPositions), typeof(NativeArray<Vector3>))]
        public static void SetPositionsAndNotify(LineRenderer lineRenderer, NativeArray<Vector3> positions)
        {
            lineRenderer.SetPositions(positions);
            OnPositionsChanged(lineRenderer, positions);
        }

        [RegisterMethodDetour(typeof(LineRenderer), nameof(LineRenderer.SetPositions), typeof(NativeSlice<Vector3>))]
        public static void SetPositionsAndNotify(LineRenderer lineRenderer, NativeSlice<Vector3> positions)
        {
            lineRenderer.SetPositions(positions);
            OnPositionsChanged(lineRenderer, positions);
        }
    }
}