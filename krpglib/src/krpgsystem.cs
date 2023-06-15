using System;
using ProtoBuf;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace krpglib
{

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class KRPGCmdResponse
    {
        public string response;
    }

    /// <summary>
    /// Core library for the Kronos RPG System
    /// </summary>
    public class KRPGSystem : ModSystem
    {
        #region Core
        public ICoreAPI Api { get; private set; }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            Api = api;
        }
        #endregion

        #region Client
        IClientNetworkChannel clientChannel;
        ICoreClientAPI clientApi;

        public override void StartClientSide(ICoreClientAPI api)
        {
            clientApi = api;

            clientChannel =
                api.Network.RegisterChannel("krpgcmd")
                .RegisterMessageType(typeof(KRPGCmdResponse))
                .SetMessageHandler<KRPGCmdResponse>(OnServerKRPGCmdResponse);
        }

        private void OnServerKRPGCmdResponse(KRPGCmdResponse networkMessage)
        {
            clientApi.ShowChatMessage(networkMessage.response);
        }
        #endregion

        #region Server
        IServerNetworkChannel serverChannel;
        ICoreServerAPI serverApi;

        public override void StartServerSide(ICoreServerAPI api)
        {
            serverApi = api;

            serverChannel =
                api.Network.RegisterChannel("krpgcmd")
                .RegisterMessageType(typeof(KRPGCmdResponse));

            api.RegisterCommand("krpg", "Manage all Kronos RPG mods.", "", OnKRPGCmd, Privilege.controlserver);
        }

        public void OnKRPGCmd(IServerPlayer player, int groupID, CmdArgs args)
        {
            // We shouldn't have more args than 5.
            if (args != null && args.Length >= 1 && args.Length <= 5)
            {
                // If 1 Arg is supplied
                if (args.Length == 1)
                {
                    // Manual
                    if (args[0].ToLower() == "help")
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = KRPGCmd.helpMessage }, player);
                    else if (args[0].ToLower() == "player")
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = KRPGCmd.helpPlayerMessage }, player);
                    // Config reload for all mods
                    else if (args[0].ToLower() == "reload")
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = "All KRPG configs reloaded." }, player);
                    // Version info for all mods
                    else if (args[0].ToLower() == "version")
                    {
                        string ver = null;

                        for (int i = 0; i < KRPGCmd.allKRPGMods.Length; i++)
                        {
                            // Set our Version
                            if (serverApi.ModLoader.GetMod(KRPGCmd.allKRPGMods[i]) != null)
                                ver = serverApi.ModLoader.GetMod(KRPGCmd.allKRPGMods[i]).Info.Version.ToString();

                            if (ver != null)
                                serverChannel.SendPacket(new KRPGCmdResponse() { response = KRPGCmd.allKRPGMods[i] + " version: " + ver }, player);

                            ver = null;
                        }
                    }
                    else
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = KRPGCmd.helpMessage }, player);
                }
                // If 2 Args are supplied for Help
                else if (args.Length == 2 && args[0].ToLower() == "help")
                {
                    if (args[1].ToLower() == "player")
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = KRPGCmd.helpPlayerMessage }, player);
                    else if (args[1].ToLower() == "reload")
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = KRPGCmd.helpReloadMessage }, player);
                    else if (args[1].ToLower() == "version")
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = KRPGCmd.helpVersionMessage }, player);
                    else
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = KRPGCmd.helpMessage }, player);
                }
                // If 2 Args are supplied for Player
                else if (args.Length == 2 && args[0].ToLower() == "player")
                {
                    if (args[1].ToLower() == "list")
                    {
                        string playerList = "Kronos RPG Players:";

                        for (int i = 0; i < serverApi.World.AllPlayers.Length; i++)
                        {
                            playerList = playerList + "\n" + serverApi.World.AllPlayers[i].PlayerName + " : Level 0";
                        }
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = playerList }, player);
                    }
                    else
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = KRPGCmd.helpPlayerMessage }, player);
                }
                // If 2 Args are supplied for Reload
                else if (args.Length == 2 && args[0].ToLower() == "reload")
                {
                    string mID = serverApi.ModLoader.GetModSystem(args[1].ToLower()).Mod.Info.ModID.ToLower();
                    if (mID != null)
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = mID + " config reloaded." }, player);
                    else
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = args[1] + " not found." }, player);
                }
                // If 2 Args are supplied for Version
                else if (args.Length == 2 && args[0].ToLower() == "version")
                {
                    string ver = null;
                    // Set our Version
                    if (serverApi.ModLoader.GetMod(args[1]) != null)
                        ver = serverApi.ModLoader.GetMod(args[1]).Info.Version.ToString();

                    if (ver != null)
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = args[1] + " version: " + ver }, player);
                    else
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = args[1] + " not found." }, player);
                }
                // If 3 Args are supplied for Player
                else if (args.Length == 3 && args[0].ToLower() == "player")
                {
                    if (args[2].ToLower() == "delete")
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = args[1] + " has been deleted from Kronos RPG server." }, player);
                    else if (args[2].ToLower() == "stats")
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = args[1] + " has no stats on Kronos RPG server." }, player);
                    else
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = KRPGCmd.helpPlayerMessage }, player);
                }
                // If 4 Args are supplied for Player
                else if (args.Length == 4 && args[0].ToLower() == "player")
                {
                    // Verify the stat we want exists
                    bool statExists = false;
                    float statValue = 0f;
                    if (args[3].ToLower() == "get" && statExists == true)
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = args[3] + " is " + statValue }, player);
                    else
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = KRPGCmd.helpPlayerMessage }, player);
                }
                // If 5 Args are supplied for Player
                else if (args.Length == 5 && args[0].ToLower() == "player")
                {
                    // Verify the stat we want exists
                    bool statExists = false;
                    float statValue = 0f;
                    if (args[3].ToLower() == "set" && statExists == true)
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = args[3] + " set to " + statValue }, player);
                    else
                        serverChannel.SendPacket(new KRPGCmdResponse() { response = KRPGCmd.helpPlayerMessage }, player);
                }
            }
            // Display general help if no match.
            else
                serverChannel.SendPacket(new KRPGCmdResponse() { response = KRPGCmd.helpMessage }, player);
        }
        #endregion
    }
}