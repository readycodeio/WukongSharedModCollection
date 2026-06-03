using System;
using System.Collections.Generic;
using System.Text;
using WukongMp.Sdk;

namespace WukongMp.PropHunt;

public class PropHuntSystem : ModSystemBase
{
    protected override void OnUpdate(UpdateTick tick)
    {
        Core.Update(tick.deltaTime);
    }
}

