using System;
using System.Collections.Generic;
using OHUndeadKnight;
using HarmonyLib;

namespace AllManagersModTemplate
{
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.OnNewConnection))]
    public static class RegisterAndCheckVersion
    {
        private static void Prefix(ZNetPeer peer, ref ZNet __instance)
        {
            // Register version check call
            OHUndeadKnightPlugin.OHUndeadKnightLogger.LogDebug("Registering version RPC handler");
            peer.m_rpc.Register($"{OHUndeadKnightPlugin.ModName}_VersionCheck",
                new Action<ZRpc, ZPackage>(RpcHandlers.RPC_AllManagersModTemplate_Version));

            // Make calls to check versions
            OHUndeadKnightPlugin.OHUndeadKnightLogger.LogDebug("Invoking version check");
            ZPackage zpackage = new();
            zpackage.Write(OHUndeadKnightPlugin.ModVersion);
            peer.m_rpc.Invoke($"{OHUndeadKnightPlugin.ModName}_VersionCheck", zpackage);
        }
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
    public static class VerifyClient
    {
        private static bool Prefix(ZRpc rpc, ZPackage pkg, ref ZNet __instance)
        {
            if (!__instance.IsServer() || RpcHandlers.ValidatedPeers.Contains(rpc)) return true;
            // Disconnect peer if they didn't send mod version at all
            OHUndeadKnightPlugin.OHUndeadKnightLogger.LogWarning(
                $"Peer ({rpc.m_socket.GetHostName()}) never sent version or couldn't due to previous disconnect, disconnecting");
            rpc.Invoke("Error", 3);
            return false; // Prevent calling underlying method
        }

        private static void Postfix(ZNet __instance)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "RequestAdminSync",
                new ZPackage());
        }
    }



    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Disconnect))]
    public static class RemoveDisconnectedPeerFromVerified
    {
        private static void Prefix(ZNetPeer peer, ref ZNet __instance)
        {
            if (!__instance.IsServer()) return;
            // Remove peer from validated list
            OHUndeadKnightPlugin.OHUndeadKnightLogger.LogInfo(
                $"Peer ({peer.m_rpc.m_socket.GetHostName()}) disconnected, removing from validated list");
            _ = RpcHandlers.ValidatedPeers.Remove(peer.m_rpc);
        }
    }

    public static class RpcHandlers
    {
        public static readonly List<ZRpc> ValidatedPeers = new();

        public static void RPC_AllManagersModTemplate_Version(ZRpc rpc, ZPackage pkg)
        {
            string? version = pkg.ReadString();
            OHUndeadKnightPlugin.OHUndeadKnightLogger.LogInfo("Version check, local: " +
                                                                              OHUndeadKnightPlugin.ModVersion +
                                                                              ",  remote: " + version);
            if (version != OHUndeadKnightPlugin.ModVersion)
            {
                OHUndeadKnightPlugin.ConnectionError =
                    $"{OHUndeadKnightPlugin.ModName} Installed: {OHUndeadKnightPlugin.ModVersion}\n Needed: {version}";
                if (!ZNet.instance.IsServer()) return;
                // Different versions - force disconnect client from server
                OHUndeadKnightPlugin.OHUndeadKnightLogger.LogWarning(
                    $"Peer ({rpc.m_socket.GetHostName()}) has incompatible version, disconnecting");
                rpc.Invoke("Error", 3);
            }
            else
            {
                if (!ZNet.instance.IsServer())
                {
                    // Enable mod on client if versions match
                    OHUndeadKnightPlugin.OHUndeadKnightLogger.LogInfo(
                        "Received same version from server!");
                }
                else
                {
                    // Add client to validated list
                    OHUndeadKnightPlugin.OHUndeadKnightLogger.LogInfo(
                        $"Adding peer ({rpc.m_socket.GetHostName()}) to validated list");
                    ValidatedPeers.Add(rpc);
                }
            }
        }
    }
}