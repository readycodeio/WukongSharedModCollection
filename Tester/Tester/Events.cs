using System;
using System.Collections.Generic;
using System.Text;
using WukongMp.Sdk.Api;

namespace Tester;

public static class Events
{
    public static void foo ()
    {
        if (WukongApi.Sync.LocalMainCharacter is not { } player) return;
    }
}
