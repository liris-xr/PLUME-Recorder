using System;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace PLUME.Core.Utils
{
    public static class PlayerLoopUtils
    {
        public static void InjectFixedUpdate<T>(PlayerLoopSystem.UpdateFunction fixedUpdate)
        {
            InjectAfterUpdateInCurrentLoop<T, FixedUpdate>(fixedUpdate);
        }

        public static void InjectPreUpdate<T>(PlayerLoopSystem.UpdateFunction preUpdate)
        {
            InjectAfterUpdateInCurrentLoop<T, PreUpdate>(preUpdate);
        }

        public static void InjectUpdate<T>(PlayerLoopSystem.UpdateFunction update)
        {
            InjectAfterUpdateInCurrentLoop<T, Update>(update);
        }

        public static void InjectEarlyUpdate<T>(PlayerLoopSystem.UpdateFunction earlyUpdate)
        {
            InjectAfterUpdateInCurrentLoop<T, EarlyUpdate>(earlyUpdate);
        }

        public static void InjectPreLateUpdate<T>(PlayerLoopSystem.UpdateFunction preLateUpdate)
        {
            InjectAfterUpdateInCurrentLoop<T, PreLateUpdate>(preLateUpdate);
        }

        public static void InjectPostLateUpdate<T>(PlayerLoopSystem.UpdateFunction postLateUpdate)
        {
            InjectAfterUpdateInCurrentLoop<T, PostLateUpdate>(postLateUpdate);
        }

        public static bool InjectAfterUpdateInCurrentLoop(Type updateType, PlayerLoopSystem.UpdateFunction updateFunction,
            Type playerLoopSystemType)
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var success = InjectAfterUpdateInLoop(updateType, updateFunction, ref playerLoop, playerLoopSystemType);
            PlayerLoop.SetPlayerLoop(playerLoop);
            return success;
        }

        public static bool InjectAfterUpdateInCurrentLoop<TU, TV>(PlayerLoopSystem.UpdateFunction updateFunction)
        {
            return InjectAfterUpdateInCurrentLoop(typeof(TU), updateFunction, typeof(TV));
        }

        public static bool InjectAfterUpdateInLoop(Type updateType, PlayerLoopSystem.UpdateFunction updateFunction,
            ref PlayerLoopSystem playerLoop, Type playerLoopSystemType)
        {
            if (updateType == null || updateFunction == null || playerLoopSystemType == null)
                return false;

            if (playerLoop.type == playerLoopSystemType)
            {
                var oldListLength = playerLoop.subSystemList?.Length ?? 0;
                var newSubsystemList = new PlayerLoopSystem[oldListLength + 1];

                if (playerLoop.subSystemList != null)
                {
                    for (var i = 0; i < oldListLength; ++i)
                        newSubsystemList[i] = playerLoop.subSystemList[i];
                }

                newSubsystemList[oldListLength] = new PlayerLoopSystem
                {
                    type = updateType,
                    updateDelegate = updateFunction
                };
                playerLoop.subSystemList = newSubsystemList;
                return true;
            }

            if (playerLoop.subSystemList == null)
                return false;

            for (var i = 0; i < playerLoop.subSystemList.Length; ++i)
            {
                if (InjectAfterUpdateInLoop(updateType, updateFunction, ref playerLoop.subSystemList[i],
                        playerLoopSystemType))
                    return true;
            }

            return false;
        }
    }
}