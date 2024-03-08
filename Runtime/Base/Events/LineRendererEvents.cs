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

        public static event OnPositionsChangedDelegate OnPositionsChanged = delegate { };

        [RegisterMethodDetour(typeof(LineRenderer), nameof(LineRenderer.SetPosition), typeof(int), typeof(Vector3))]
        public static void SetPositionAndNotify(LineRenderer lineRenderer, int index, Vector3 position)
        {
            lineRenderer.SetPosition(index, position);
            var positions = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(positions);
            OnPositionsChanged(lineRenderer, positions);
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