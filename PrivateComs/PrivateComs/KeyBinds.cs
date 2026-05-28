using CSharpModBase.Input;
using System;
using System.Collections.Generic;
using System.Text;
using WukongMp.Sdk.Api;

namespace WukongMp.PrivateComs;

public static class KeyBinds
{
    public static void RegisterKeyBinds()
    {
        //WukongApi.Input.RegisterKeyBind(Key.F2, UpdateMsgAutocomplete);
        //WukongApi.Input.RegisterKeyBind(Key.F3, () =>
        //{
        //    WukongApi.Chat.ShowLocalMessage("Zwykły <span color=\"#FF0000\">Czerwony</> Zwykły", FLinearColor.White);
        //    WukongApi.Chat.ShowLocalMessage("Zwykły <color=#FF0000>Czerwony</color> Zwykły", FLinearColor.White);
        //    WukongApi.Chat.ShowLocalMessage("Zwykły [color=red]Czerwony[/color] Zwykły", FLinearColor.White);

        //    WukongApi.Chat.SendPlayerMessage("Zwykły <span color=\"#FF0000\">Czerwony</> Zwykły");
        //    WukongApi.Chat.SendPlayerMessage("Zwykły <color=#FF0000>Czerwony</color> Zwykły");
        //    WukongApi.Chat.SendPlayerMessage("Zwykły [color=red]Czerwony[/color] Zwykły");

        //    WukongApi.Chat.SendServerMessage("Zwykły <span color=\"#FF0000\">Czerwony</> Zwykły");
        //    WukongApi.Chat.SendServerMessage("Zwykły <color=#FF0000>Czerwony</color> Zwykły");
        //    WukongApi.Chat.SendServerMessage("Zwykły [color=red]Czerwony[/color] Zwykły");
        //});
        WukongApi.Input.RegisterKeyBind(Key.L, () =>
        {
            WukongApi.Sync.TryGetPlayerInfoById(Mod.PlayerId.Value, out _, out Mod.PlayerTeam);
        });
    }
}
