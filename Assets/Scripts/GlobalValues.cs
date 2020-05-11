using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalValues
{
    //unit mods
    public static float skirmish_on_skirmish_mod = 1f;
    public static float skirmish_on_battle_mod = 0.25f;
    public static float skirmish_on_chase_mod = 1f;

    public static float light_on_skirmish_mod = 0.75f;
    public static float light_on_battle_mod = 0.75f;
    public static float light_on_chase_mod = 0.75f;

    public static float heavy_on_skirmish_mod = 0.25f;
    public static float heavy_on_battle_mod = 1f;
    public static float heavy_on_chase_mod = 0.25f;

    public static float skirmish_unit_mod = 1;
    public static float light_unit_mod = 2;
    public static float heavy_unit_mod = 3;

    //art mods
    public static float art_on_skirmish = 0;
    public static float art_on_battle = 1f;
    public static float art_on_chase = 0.25f;

    //phase mods
    public static float battle_wound_rate = 0.3f;
    public static float battle_capture_rate = 0f;
    public static float skirmish_wound_rate = 0.5f;
    public static float skirmish_capture_rate = 0f;
    public static float route_wound_rate = 0.1f;
    public static float route_capture_rate = 0.6f;

    //battle mods
    public static float flank_dominance_treshold = 0.15f;
    public static float min_combat_mod = 0.05f;
    public static float route_check_treshold = 0.75f;
    public static float massive_casualities_treshold = 0.33f;
    public static float insufficient_manpower_treshold = 0.5f;
    public static float balance_infavourable = 0.05f;
    public static float balance_losing = 0.2f;

    //morale losses
    public static float morale_loss = 0.01f;
    public static float ally_wiped_morale_loss = 0.1f;
    public static float ally_routing_morale_loss = 0.03f;


    //battle penalties
    public static float flanked_penalty = 0.25f;
    public static float under_fire_penalty = 0.25f;

    //battle boosts
    public static float under_cover_boost = 0.5f;
    public static float flanks_secured_boost = 0.25f;

}
