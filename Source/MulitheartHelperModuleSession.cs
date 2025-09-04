using System;
using System.Collections.Generic;
using Celeste.Mod.MultiheartHelper.Data;

namespace Celeste.Mod.MultiheartHelper;

public class MultiheartHelperModuleSession : EverestModuleSession
{
    public List<string> collectedHearts = [];
    public List<Action> BeforeUpdateNextFrame = [];
    public List<string> collectedItems = [];
}