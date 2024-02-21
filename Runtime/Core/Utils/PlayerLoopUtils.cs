using System;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace PLUME.Core.Utils
{
    public static class PlayerLoopUtils
    {
        public static void InjectFixedUpdate<T>(PlayerLoopSystem.UpdateFunction fixedUpdate)
        {
            InjectUpdateInCurrentLoop<T, FixedUpdate>(fixedUpdate);
        }

        public static void InjectPreUpdate<T>(PlayerLoopSystem.UpdateFunction preUpdate)
        {
            InjectUpdateInCurrentLoop<T, PreUpdate>(preUpdate);
        }

        public static void InjectUpdate<T>(PlayerLoopSystem.UpdateFunction update)
        {
            InjectUpdateInCurrentLoop<T, Update>(update);
        }

        public static void InjectEarlyUpdate<T>(PlayerLoopSystem.UpdateFunction earlyUpdate)
        {
            InjectUpdateInCurrentLoop<T, EarlyUpdate>(earlyUpdate);
        }

        public static void InjectPreLateUpdate<T>(PlayerLoopSystem.UpdateFunction preLateUpdate)
        {
            InjectUpdateInCurrentLoop<T, PreLateUpdate>(preLateUpdate);
        }

        public static void InjectPostLateUpdate<T>(PlayerLoopSystem.UpdateFunction postLateUpdate)
        {
            InjectUpdateInCurrentLoop<T, PostLateUpdate>(postLateUpdate);
        }

        public static bool InjectUpdateInCurrentLoop(Type updateType, PlayerLoopSystem.UpdateFunction updateFunction,
            Type playerLoopSystemType)
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var success = InjectUpdateInLoop(updateType, updateFunction, ref playerLoop, playerLoopSystemType);
            PlayerLoop.SetPlayerLoop(playerLoop);
            return success;
        }

        public static bool InjectUpdateInCurrentLoop<TU, TV>(PlayerLoopSystem.UpdateFunction updateFunction)
        {
            return InjectUpdateInCurrentLoop(typeof(TU), updateFunction, typeof(TV));
        }

        public static bool InjectUpdateInLoop(Type updateType, PlayerLoopSystem.UpdateFunction updateFunction,
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
                if (InjectUpdateInLoop(updateType, updateFunction, ref playerLoop.subSystemList[i],
                        playerLoopSystemType))
                    return true;
            }

            return false;
        }
    }
}