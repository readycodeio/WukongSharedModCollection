using System;
using System.Collections.Generic;
using System.Text;
using WukongMp.Sdk;
using WukongMp.Toolkit.Levels;

namespace WukongMp.Toolkit.Levels
{
    public class LevelsSystem : ModSystemBase
    {
        protected override void OnUpdate(UpdateTick tick)
        {
            LevelsCore.Update(tick.deltaTime);
        }
    }
}
