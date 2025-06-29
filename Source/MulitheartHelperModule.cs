using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.MultiheartHelper.Data;
using Celeste.Mod.MultiheartHelper.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.MultiheartHelper;

public class MultiheartHelperModule : EverestModule {
    public static MultiheartHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(MultiheartHelperModuleSettings);
    public static MultiheartHelperModuleSettings Settings => (MultiheartHelperModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(MultiheartHelperModuleSession);
    public static MultiheartHelperModuleSession Session => (MultiheartHelperModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(MultiheartHelperModuleSaveData);
    public static MultiheartHelperModuleSaveData SaveData => (MultiheartHelperModuleSaveData) Instance._SaveData;

    public static Dictionary<AreaData, MultiheartMetadata> multiheartData = [];

    public MultiheartHelperModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(MultiheartHelperModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(MultiheartHelperModule), LogLevel.Info);
#endif
    }

    private static ILHook hookOrigUpdateFileSelectSlot;
    public override void Load() {
        On.Celeste.AreaData.Load += PostAreaLoad;
        IL.Celeste.OuiJournalProgress.ctor += Hook_OuiJournalProgress_ctor;
        hookOrigUpdateFileSelectSlot = new ILHook(typeof(OuiFileSelectSlot).GetMethod("orig_Render", BindingFlags.Public|BindingFlags.Instance), 
            Hook_OuiFileSelectSlot_origRender);
        On.Celeste.OuiFileSelectSlot.Show += Hook_OuiFileSelectSlot_Show;
        On.Celeste.OuiFileSelectSlot.ctor_int_OuiFileSelect_SaveData += Hook_OuiFileSelectSlot_Show_wrap;
        MultiheartModifier.Hook();
        SemipermanentCrumbleBlock.Hook();
    }

    private static void Hook_OuiJournalProgress_ctor(ILContext il)
    {
        ILCursor c = new(il);
        c.GotoNext(i => i.MatchLdstr("heartgem"));
        c.GotoNext(i => i.MatchLdstr("dot"));
        c.Index -= 2;
        c.Emit(OpCodes.Ldloc_2);
        c.Emit(OpCodes.Ldloc, 4);
        c.EmitDelegate(addHearts);

        c.GotoNext(i => i.MatchLdcR4(-32));
        c.Index++;
        c.Emit(OpCodes.Ldloc_2);
        c.Emit(OpCodes.Ldloc, 8);
        c.EmitDelegate(AddHeartIconsHook);

    }

    private static void addHearts(AreaData data, List<string> list) {
        if(multiheartData.TryGetValue(data, out MultiheartMetadata meta) && SaveData.TryGetData(data.ID, out var savedData)) {
            list.Clear();
            foreach(var heart in meta.Hearts) {
                if (MTN.Journal.Has(heart.Texture))
                {
                    if (savedData.unlockedHearts.Contains(heart.Name))
                        list.Add(heart.Texture);
                }
            }
        }
    }

    private static float AddHeartIconsHook(float value, AreaData data, OuiJournalPage.Row row) {
        if(!multiheartData.TryGetValue(data, out var meta))
            return value;


        return meta.Spacing;
    }

    private static void Hook_OuiFileSelectSlot_origRender(ILContext ctx) {
        var cursor = new ILCursor(ctx);
        
        cursor.GotoNext(MoveType.After, instr => instr.MatchLdstr("heartgem"));
        cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt<Monocle.MTexture>("DrawCentered"));
        var endLabel = cursor.DefineLabel();
        endLabel.Target = cursor.Next;
        
        cursor.GotoPrev(MoveType.Before, instr => instr.MatchLdsfld(typeof(MTN).GetField("FileSelect")));
        
        cursor.EmitLdloc(16);//index3
        cursor.EmitLdloc(14);//position2
        cursor.EmitLdloc(17);//num5
        cursor.EmitLdloc(11);//index1
        cursor.EmitLdarg0();//this
        cursor.EmitDelegate(drawFileSelectHearts);
        cursor.EmitBrtrue(endLabel);
    }

    private static void Hook_OuiFileSelectSlot_Show(On.Celeste.OuiFileSelectSlot.orig_Show orig, OuiFileSelectSlot self) {
        orig(self);
        recalcAreas(self);
    }
    private static void Hook_OuiFileSelectSlot_Show_wrap(On.Celeste.OuiFileSelectSlot.orig_ctor_int_OuiFileSelect_SaveData orig, OuiFileSelectSlot self,
        int index, OuiFileSelect parent, SaveData saveData) {
        orig(self, index, parent, saveData);
        recalcAreas(self);
    }

    private static Dictionary<OuiFileSelectSlot, List<MultiheartMetadata>> fileSelectHeartMetadata = new();
    private static void recalcAreas(OuiFileSelectSlot slot) {
        if (slot.SaveData == null) return;
        var list = new List<MultiheartMetadata>();
        foreach (AreaStats areaStats in slot.SaveData.Areas_Safe) {
            if (areaStats.ID_Safe <= slot.SaveData.UnlockedAreas_Safe) {
                if (!AreaData.Areas[areaStats.ID_Safe].Interlude_Safe && AreaData.Areas[areaStats.ID_Safe].CanFullClear) {
                    if (multiheartData.TryGetValue(AreaData.Areas[areaStats.ID_Safe], out var data)) {
                        list.Add(data);
                    } else {
                        list.Add(null);
                    }
                }
            }
        }

        fileSelectHeartMetadata[slot] = list;
    }

    private static bool drawFileSelectHearts(int side, Vector2 positionStartX, int yLevel, int area, OuiFileSelectSlot self) {
        if (fileSelectHeartMetadata.TryGetValue(self, out var data) && data.Count>area) {
            if (SaveData==null) return false; 
            Logger.Info(nameof(MultiheartHelperModule), "savedata exists, "+area);
            foreach (var mrr in SaveData.areaData) {
                Logger.Info(nameof(MultiheartHelperModule), " - "+mrr.areaID);
            }
            if(SaveData.areaData[area] == null) return false;
            
            var count = SaveData.areaData[area].unlockedHearts.Count;
            for (var i=0; i<data[area].Hearts.Count;i++) {
                MTN.FileSelect[data[area].Hearts[i].Texture].DrawCentered(
                    positionStartX + 
                    new Vector2(0.0f, yLevel * 14) +
                    new Vector2((0.5f - (float)i / count) * (56-data[area].Spacing)));
            }
            return true;
        }

        return false;
    }

    private static void PostAreaLoad(On.Celeste.AreaData.orig_Load orig)
    {
        orig();
        foreach(var map in AreaData.Areas) {
            MultiheartMetadata meta;
            if(Everest.Content.TryGet("Maps/" + map.Mode[0].Path + ".multiheart.meta", out ModAsset metaFile) && metaFile.TryDeserialize(out meta)) {
                multiheartData[map] = meta;
                Logger.Warn("MultiheartHelper", meta.MaxHearts.ToString());
            }
        }
    }


    public override void Unload() {
        On.Celeste.AreaData.Load -= PostAreaLoad;
        IL.Celeste.OuiJournalProgress.ctor -= Hook_OuiJournalProgress_ctor;
        hookOrigUpdateFileSelectSlot?.Dispose();
        multiheartData.Clear();
        MultiheartModifier.Unhook();
        SemipermanentCrumbleBlock.Unhook();
    }
}