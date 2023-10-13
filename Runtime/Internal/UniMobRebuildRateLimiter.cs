using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UniMob.UI.Internal
{
    public struct UniMobRebuildRateLimiter
    {
        private static readonly HashSet<Type> IgnoredOwnerTypes = new HashSet<Type>();

        private const int MaxContinuousRebuildCount = 15;

        private readonly BuildContext _buildContext;

        private int _continuousRebuildCount;
        private int _lastRebuildFrame;

        public UniMobRebuildRateLimiter(BuildContext buildContext)
        {
            _buildContext = buildContext;
            _continuousRebuildCount = 0;
            _lastRebuildFrame = 0;
        }

        public void TrackRebuild(Widget widget)
        {
            var frame = Time.frameCount;

            if (frame == _lastRebuildFrame + 1)
            {
                _continuousRebuildCount += 1;
            }

            _lastRebuildFrame = frame;

            if (_continuousRebuildCount >= MaxContinuousRebuildCount)
            {
                _continuousRebuildCount = 0;

                var ownerCtx = _buildContext;

                while (ownerCtx?.Parent != null &&
                       ownerCtx?.State?.GetType().Namespace is var ns &&
                       ns != null && ns.StartsWith("UniMob"))
                {
                    ownerCtx = ownerCtx.Parent;
                }

                var ownerType = ownerCtx?.State?.GetType();

                if (ownerType == null || !IgnoredOwnerTypes.Contains(ownerType))
                {
                    var widgetTypeName = widget?.GetType().Name ?? "Unknown";

                    Debug.LogError(
                        $"{widgetTypeName} at {ownerType} was rebuilt {MaxContinuousRebuildCount} frames in a row");
                }
            }
        }

        [PublicAPI]
        public static void Ignore<T>() where T : IState
        {
            IgnoredOwnerTypes.Add(typeof(T));
        }
    }
}