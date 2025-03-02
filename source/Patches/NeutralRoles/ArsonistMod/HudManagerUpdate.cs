﻿using System.Linq;
using HarmonyLib;
using TownOfUs.Extensions;
using TownOfUs.Roles;
using UnityEngine;

namespace TownOfUs.NeutralRoles.ArsonistMod
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudManagerUpdate
    {
        public static Sprite IgniteSprite => TownOfUs.IgniteSprite;
        
        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.AllPlayerControls.Count <= 1) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (PlayerControl.LocalPlayer.Data == null) return;
            if (!PlayerControl.LocalPlayer.Is(RoleEnum.Arsonist)) return;
            var role = Role.GetRole<Arsonist>(PlayerControl.LocalPlayer);

            foreach (var playerId in role.DousedPlayers)
            {
                var player = Utils.PlayerById(playerId);
                var data = player?.Data;
                if (data == null || data.Disconnected || data.IsDead || PlayerControl.LocalPlayer.Data.IsDead)
                    continue;

                player.myRend().material.SetColor("_VisorColor", role.Color);
                player.nameText().color = Color.black;
            }

            if (role.IgniteButton == null)
            {
                role.IgniteButton = Object.Instantiate(__instance.KillButton, __instance.KillButton.transform.parent);
                role.IgniteButton.graphic.enabled = true;
                role.IgniteButton.GetComponent<AspectPosition>().DistanceFromEdge = TownOfUs.ButtonPosition;
                role.IgniteButton.gameObject.SetActive(false);
            }
            role.IgniteButton.GetComponent<AspectPosition>().Update();
            role.IgniteButton.graphic.sprite = IgniteSprite;

            role.IgniteButton.gameObject.SetActive(!PlayerControl.LocalPlayer.Data.IsDead && !MeetingHud.Instance);
            __instance.KillButton.gameObject.SetActive(!PlayerControl.LocalPlayer.Data.IsDead && !MeetingHud.Instance);
            if (!role.LastKiller || !CustomGameOptions.IgniteCdRemoved) role.IgniteButton.SetCoolDown(role.DouseTimer(), CustomGameOptions.DouseCd);
            else role.IgniteButton.SetCoolDown(0f, CustomGameOptions.DouseCd);
            if (role.DousedAlive < CustomGameOptions.MaxDoused)
            {
                __instance.KillButton.SetCoolDown(role.DouseTimer(), CustomGameOptions.DouseCd);
            }

            var notDoused = PlayerControl.AllPlayerControls.ToArray().Where(
                player => !role.DousedPlayers.Contains(player.PlayerId)
            ).ToList();
            var doused = PlayerControl.AllPlayerControls.ToArray().Where(
                player => role.DousedPlayers.Contains(player.PlayerId)
            ).ToList();

            if (role.DousedAlive < CustomGameOptions.MaxDoused)
            {
                Utils.SetTarget(ref role.ClosestPlayerDouse, __instance.KillButton, float.NaN, notDoused);
            }

            if (role.DousedAlive > 0)
            {
                Utils.SetTarget(ref role.ClosestPlayerIgnite, role.IgniteButton, float.NaN, doused);
            }

            return;
        }
    }
}
