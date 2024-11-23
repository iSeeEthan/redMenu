using System;
using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

namespace vMenuShared
{
    public static class PermissionsManager
    {
        public enum Permission
        {
            // Global
            #region global
            Everything,
            DontKickMe,
            DontBanMe,
            NoClip,
            Staff,
            #endregion

            // Online Players
            #region online players
            OPMenu,
            OPAll,
            OPTeleport,
            OPWaypoint,
            OPSpectate,
            OPIdentifiers,
            OPSummon,
            OPKick,
            OPPermBan,
            OPTempBan,
            OPUnban,
            OPViewBannedPlayers,
            OPSeePrivateMessages,
            #endregion

            // Player Options
            #region player options
            POMenu,
            POAll,
            POGod,
            POInvisible,
            POFastRun,
            POFastSwim,
            POSuperjump,
            PONoRagdoll,
            PONeverWanted,
            POSetWanted,
            POClearBlood,
            POSetBlood,
            POIgnored,
            POMaxHealth,
            POMaxArmor,
            POCleanPlayer,
            PODryPlayer,
            POWetPlayer,
            POFreeze,
            POScenarios,
            POUnlimitedStamina,
            #endregion
                
            // Player Appearance
            #region player appearance
            PAMenu,
            PAAll,
            PACustomize,
            PASpawnSaved,
            PASpawnNew,
            PAAddonPeds,
            #endregion

            // Time Options
            #region time options
            TOMenu,
            TOAll,
            TOFreezeTime,
            TOSetTime,
            #endregion

            // Weather Options
            #region weather options
            WOMenu,
            WOAll,
            WODynamic,
            WOSetWeather,
            WORemoveClouds,
            WORandomizeClouds,
            #endregion

            // Misc Settings
            #region misc settings
            MSAll,
            MSClearArea,
            MSTeleportToWp,
            MSTeleportToCoord,
            MSShowCoordinates,
            MSShowLocation,
            MSJoinQuitNotifs,
            MSDeathNotifs,
            MSLocationBlips,
            MSPlayerBlips,
            MSOverheadNames,
            MSTeleportLocations,
            MSTeleportSaveLocation,
            MSConnectionMenu,
            MSRestoreAppearance,
            MSDriftMode,
            MSEntitySpawner,
            MSDevTools,
            #endregion

            // Voice Chat
            #region voice chat
            VCMenu,
            VCAll,
            VCEnable,
            VCShowSpeaker,
            VCStaffChannel,
            #endregion
        };

        public static Dictionary<Permission, bool> Permissions { get; private set; } = new Dictionary<Permission, bool>();
        public static bool ArePermissionsSetup { get; set; } = false;

        #if SERVER
        public static bool IsAllowed(Permission permission, Player source) => IsAllowedServer(permission, source);
        #endif

        #if CLIENT
        public static bool IsAllowed(Permission permission, bool checkAnyway = false) => IsAllowedClient(permission, checkAnyway);
        private static bool IsAllowedClient(Permission permission, bool checkAnyway)
        {
            // Implementation remains the same
            return false; // Just a placeholder; this will require actual logic to check permissions
        }
        #endif

        #if SERVER
        private static bool IsAllowedServer(Permission permission, Player source)
        {
            if (source == null)
            {
                return false;
            }

            if (IsPlayerAceAllowed(source.Handle, GetAceName(permission)))
            {
                return true;
            }
            return false;
        }
        #endif

        private static readonly Dictionary<Permission, List<Permission>> parentPermissions = new();

        public static List<Permission> GetPermissionAndParentPermissions(Permission permission)
        {
            if (parentPermissions.ContainsKey(permission))
            {
                return parentPermissions[permission];
            }
            else
            {
                var list = new List<Permission>() { Permission.Everything, permission };
                parentPermissions[permission] = list;
                return list;
            }
        }

        #if SERVER
        public static void SetPermissionsForPlayer([FromSource] Player player)
        {
            if (player == null)
            {
                return;
            }

            var perms = new Dictionary<Permission, bool>();

            if (!ConfigManager.GetSettingsBool(ConfigManager.Setting.vmenu_use_permissions))
            {
                foreach (var p in Enum.GetValues(typeof(Permission)))
                {
                    var permission = (Permission)p;
                    switch (permission)
                    {
                        case Permission.Everything:
                            break; // Skip permissions like Everything
                        default:
                            perms.Add(permission, true);
                            break;
                    }
                }
            }
            else
            {
                foreach (var p in Enum.GetValues(typeof(Permission)))
                {
                    var permission = (Permission)p;
                    if (!perms.ContainsKey(permission))
                    {
                        perms.Add(permission, IsAllowed(permission, player)); // triggers IsAllowedServer
                    }
                }
            }

            player.TriggerEvent("vMenu:SetPermissions", Newtonsoft.Json.JsonConvert.SerializeObject(perms));
            player.TriggerEvent("vMenu:SetAddons");
        }
        #endif
    }
}
