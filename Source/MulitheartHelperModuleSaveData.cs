using System.Collections.Generic;

namespace Celeste.Mod.MultiheartHelper;

public class MultiheartHelperModuleSaveData : EverestModuleSaveData {
    public List<AreaMultiheartData> areaData { get; set; } = [];

    public AreaMultiheartData GetData(int areaName) => areaData.Find(x => x.areaID == areaName);
    public bool TryGetData(int areaID, out AreaMultiheartData data) {
        data = GetData(areaID);
        return data != null;
    }

    public void RegisterHeartCollect(int area, string heartName) {
        if(TryGetData(area, out var data)) {
            data.unlockedHearts.Add(heartName);
            return;
        }
        areaData.Add(new() { areaID = area, unlockedHearts = [heartName] });
    }

    public bool WasCollected(int area, string heartName) {
        if(TryGetData(area, out var data)) {
            return data.unlockedHearts.Contains(heartName);
        }
        return false;
    }
}


public class AreaMultiheartData {
    public int areaID { get; set; }
    public HashSet<string> unlockedHearts { get; set; } = []; 
}

public class HeartInfo {
    public string Name { get; set; }
    public string Texture { get; set; }
}