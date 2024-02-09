using System;
using UnityEngine.LowLevel;

namespace PLUME.Core.Utils
{
    public static class PlayerLoopUtils
    {
        public static bool InjectUpdateInCurrentLoop(Type updateType, PlayerLoopSystem.UpdateFunction updateFunction,
            Type playerLoopSystemType)
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var success = InjectUpdateInLoop(updateType, updateFunction, ref playerLoop, playerLoopSystemType);
            PlayerLoop.SetPlayerLoop(playerLoop);
            return success;
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