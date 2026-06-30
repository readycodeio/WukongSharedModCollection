using BtlB1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WukongMp.Toolkit.Actors;

public static class Assets
{
    public static List<string> ToList(Type type, bool includeAll = false)
    {
        List<string> list;

        if (!includeAll)
        {
            list = type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.FieldType == typeof(string))
                .Select(f => (string)f.GetValue(null))
                .ToList();
        }
        else
        {
            list = type
                .GetNestedTypes()
                .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                .Where(f => f.FieldType == typeof(string))
                .Select(f => (string)f.GetValue(null))
                .ToList();
        }
        return list;
    }

    public static List<string> ToList<T>(bool includeAll = false) where T : class
    {
        return ToList(typeof(T), includeAll);
    }

    public static class Static
    {
        public const string BPO_GYCY_Build_Obj_huopen_02 =
            "/Game/00MainHZ/Environment/BPO/HFS/Non-interactive/BPO_GYCY_Build_Obj_huopen_02.BPO_GYCY_Build_Obj_huopen_02_C";

        public const string SM_GYCY_Split_01_Object029 =
            "/Game/00MainHZ/Environment/BPO/HFS/Non-interactive/PA/SM_GYCY_Split_01_Object029.SM_GYCY_Split_01_Object029_C";

        public const string SM_GYCY_Split_01_Object028 =
            "/Game/00MainHZ/Environment/BPO/HFS/Non-interactive/PA/SM_GYCY_Split_01_Object028.SM_GYCY_Split_01_Object028_C";

        public const string SM_GYCY_Split_01_Object027 =
            "/Game/00MainHZ/Environment/BPO/HFS/Non-interactive/PA/SM_GYCY_Split_01_Object027.SM_GYCY_Split_01_Object027_C";

        public const string SM_GYCY_Build_zhenmushou_02_Deng =
            "/Game/00MainHZ/Environment/BPO/HFS/Non-interactive/PA/SM_GYCY_Build_zhenmushou_02_Deng.SM_GYCY_Build_zhenmushou_02_Deng_C";

        public const string SM_GYCY_Build_Obj_zhulian_01 =
            "/Game/00MainHZ/Environment/BPO/HFS/Non-interactive/PA/SM_GYCY_Build_Obj_zhulian_01.SM_GYCY_Build_Obj_zhulian_01_C";

        public const string SM_GYCY_Build_Fbx_denglong_E =
            "/Game/00MainHZ/Environment/BPO/HFS/Non-interactive/PA/SM_GYCY_Build_Fbx_denglong_E.SM_GYCY_Build_Fbx_denglong_E_C";

        public const string SM_GYCY_Build_Fbx_denglong =
            "/Game/00MainHZ/Environment/BPO/HFS/Non-interactive/PA/SM_GYCY_Build_Fbx_denglong.SM_GYCY_Build_Fbx_denglong_C";

        public const string SM_HFM_LaZhu_02 =
            "/Game/00MainHZ/Environment/BPO/HFM/Non-interactive/SM_HFM_LaZhu_02.SM_HFM_LaZhu_02_C";

        public const string SM_HFM_LaZhu_01 =
            "/Game/00MainHZ/Environment/BPO/HFM/Non-interactive/SM_HFM_LaZhu_01.SM_HFM_LaZhu_01_C";

        public const string BPO_HFM_LaZhu_02 =
            "/Game/00MainHZ/Environment/BPO/HFM/Non-interactive/BPO_HFM_LaZhu_02.BPO_HFM_LaZhu_02_C";

        public const string BPO_HFM_LaZhu_01 =
            "/Game/00MainHZ/Environment/BPO/HFM/Non-interactive/BPO_HFM_LaZhu_01.BPO_HFM_LaZhu_01_C";

        public const string SM_HFM_GanShi_02A =
            "/Game/00MainHZ/Environment/BPO/HFM/Non-interactive/PA/SM_HFM_GanShi_02A.SM_HFM_GanShi_02A_C";

        public const string SM_HFM_Build_Bungalow_Spilt_11_chuanglian_PhysicsAsset =
            "/Game/00MainHZ/Environment/BPO/HFM/Non-interactive/PA/SM_HFM_Build_Bungalow_Spilt_11_chuanglian_PhysicsAsset.SM_HFM_Build_Bungalow_Spilt_11_chuanglian_PhysicsAsset_C";

        public const string SM_GYCY_kulou_wutou =
            "/Game/00MainHZ/Environment/BPO/HFM/Non-interactive/PA/SM_GYCY_kulou_wutou.SM_GYCY_kulou_wutou_C";

        public const string SM_GYCY_kulou =
            "/Game/00MainHZ/Environment/BPO/HFM/Non-interactive/PA/SM_GYCY_kulou.SM_GYCY_kulou_C";

        public const string SK_HFM_Ganshi_03B =
            "/Game/00MainHZ/Environment/BPO/HFM/Non-interactive/PA/SK_HFM_Ganshi_03B.SK_HFM_Ganshi_03B_C";

        public const string SK_HFM_build_wugong_2 =
            "/Game/00MainHZ/Environment/BPO/HFM/Non-interactive/PA/SK_HFM_build_wugong_2.SK_HFM_build_wugong_2_C";

        public const string SK_HFM_build_wugong_01 =
            "/Game/00MainHZ/Environment/BPO/HFM/Non-interactive/PA/SK_HFM_build_wugong_01.SK_HFM_build_wugong_01_C";

        public const string HFM_ChanZhang_01a_Spilt_2 =
            "/Game/00MainHZ/Environment/BPO/HFM/Non-interactive/PA/HFM_ChanZhang_01a_Spilt_2.HFM_ChanZhang_01a_Spilt_2_C";
    }

    public static class Destructible
    {
        public const string HFS_Destructible_Zhong_Droppable_Test2 =
            "/Game/00MainHZ/Environment/BPO/HFS_Destructible_Zhong_Droppable_Test2.HFS_Destructible_Zhong_Droppable_Test2_C";

        public const string HFS_Destructible_ShiGu_Droppable_Test1 =
            "/Game/00MainHZ/Environment/BPO/HFS_Destructible_ShiGu_Droppable_Test1.HFS_Destructible_ShiGu_Droppable_Test1_C";

        public const string BP_DroppableDestructionBase =
            "/Game/00MainHZ/Environment/BPO/BP_DroppableDestructionBase.BP_DroppableDestructionBase_C";

        public const string BP_DestructibleBase =
            "/Game/00MainHZ/Environment/BPO/BP_DestructibleBase.BP_DestructibleBase_C";

        public const string HFS_Destructible_Zhong =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/HFS_Destructible_Zhong.HFS_Destructible_Zhong_C";

        public const string HFS_Destructible_ShuiGang =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/HFS_Destructible_ShuiGang.HFS_Destructible_ShuiGang_C";

        public const string HFS_Destructible_ShuanMaZhu_B =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/HFS_Destructible_ShuanMaZhu_B.HFS_Destructible_ShuanMaZhu_B_C";

        public const string HFS_Destructible_ShuanMaZhu_A =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/HFS_Destructible_ShuanMaZhu_A.HFS_Destructible_ShuanMaZhu_A_C";

        public const string HFS_Destructible_ShiZhu =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/HFS_Destructible_ShiZhu.HFS_Destructible_ShiZhu_C";

        public const string HFS_Destructible_ShiGu =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/HFS_Destructible_ShiGu.HFS_Destructible_ShiGu_C";

        public const string HFS_Destructible_ShiBei =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/HFS_Destructible_ShiBei.HFS_Destructible_ShiBei_C";

        public const string HFS_Destructible_MuJia6 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/HFS_Destructible_MuJia6.HFS_Destructible_MuJia6_C";

        public const string HFS_Destructible_MuJia5 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/HFS_Destructible_MuJia5.HFS_Destructible_MuJia5_C";

        public const string HFS_Destructible_MuJia4 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/HFS_Destructible_MuJia4.HFS_Destructible_MuJia4_C";

        public const string HFS_Destructible_MuJia3 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/HFS_Destructible_MuJia3.HFS_Destructible_MuJia3_C";

        public const string HFS_Destructible_MuJia2 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/HFS_Destructible_MuJia2.HFS_Destructible_MuJia2_C";

        public const string HFS_Destructible_MuJia1 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/HFS_Destructible_MuJia1.HFS_Destructible_MuJia1_C";

        public const string HFS_Destructible_MuJia01 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/HFS_Destructible_MuJia01.HFS_Destructible_MuJia01_C";

        public const string HFS_Destructible_HuoPen =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/HFS_Destructible_HuoPen.HFS_Destructible_HuoPen_C";

        public const string HFS_Destructible_DengKan =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/HFS_Destructible_DengKan.HFS_Destructible_DengKan_C";

        public const string BPO_HFS_Destructible_HuoBaHuangPaoLang =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/BPO_HFS_Destructible_HuoBaHuangPaoLang.BPO_HFS_Destructible_HuoBaHuangPaoLang_C";

        public const string SM_BSD_Destructible_Ice_04 =
            "/Game/00MainHZ/Environment/BPO/BSD/Interactable/SM_BSD_Destructible_Ice_04.SM_BSD_Destructible_Ice_04_C";

        public const string SM_BSD_Destructible_Ice_03 =
            "/Game/00MainHZ/Environment/BPO/BSD/Interactable/SM_BSD_Destructible_Ice_03.SM_BSD_Destructible_Ice_03_C";

        public const string SM_BSD_Destructible_Ice_01 =
            "/Game/00MainHZ/Environment/BPO/BSD/Interactable/SM_BSD_Destructible_Ice_01.SM_BSD_Destructible_Ice_01_C";
    }

    public static class Interactable
    {
        public const string SM_build_mb_02 =
            "/Game/00MainHZ/Environment/BPO/SM/Interactable/SM_build_mb_02.SM_build_mb_02_C";

        public const string SM_psd_kuloudui_03 =
            "/Game/00MainHZ/Environment/BPO/PSD/Interactable/SM_psd_kuloudui_03.SM_psd_kuloudui_03_C";

        public const string SM_JiaSi_10 =
            "/Game/00MainHZ/Environment/BPO/PSD/Interactable/SM_JiaSi_10.SM_JiaSi_10_C";

        public const string SM_JiaSi_09 =
            "/Game/00MainHZ/Environment/BPO/PSD/Interactable/SM_JiaSi_09.SM_JiaSi_09_C";

        public const string SM_JiaSi_08 =
            "/Game/00MainHZ/Environment/BPO/PSD/Interactable/SM_JiaSi_08.SM_JiaSi_08_C";

        public const string SM_JiaSi_05 =
            "/Game/00MainHZ/Environment/BPO/PSD/Interactable/SM_JiaSi_05.SM_JiaSi_05_C";

        public const string SM_JiaSi_04 =
            "/Game/00MainHZ/Environment/BPO/PSD/Interactable/SM_JiaSi_04.SM_JiaSi_04_C";

        public const string SM_JiaSi_02 =
            "/Game/00MainHZ/Environment/BPO/PSD/Interactable/SM_JiaSi_02.SM_JiaSi_02_C";

        public const string S_Cracked_Boulder_ujomfhrfa_high =
            "/Game/00MainHZ/Environment/BPO/LYS/Interactable/S_Cracked_Boulder_ujomfhrfa_high.S_Cracked_Boulder_ujomfhrfa_high_C";

        public const string SM_LYS_PianDian_JiaGou_F10 =
            "/Game/00MainHZ/Environment/BPO/LYS/Interactable/SM_LYS_PianDian_JiaGou_F10.SM_LYS_PianDian_JiaGou_F10_C";

        public const string SM_LYS_build_shita_03 =
            "/Game/00MainHZ/Environment/BPO/LYS/Interactable/SM_LYS_build_shita_03.SM_LYS_build_shita_03_C";

        public const string SM_LYS_build_shita_02 =
            "/Game/00MainHZ/Environment/BPO/LYS/Interactable/SM_LYS_build_shita_02.SM_LYS_build_shita_02_C";

        public const string SM_hys_qiaolangan_02 =
            "/Game/00MainHZ/Environment/BPO/LYS/Interactable/SM_hys_qiaolangan_02.SM_hys_qiaolangan_02_C";

        public const string SM_GYCY_Build_DaDian_01 =
            "/Game/00MainHZ/Environment/BPO/LYS/Interactable/SM_GYCY_Build_DaDian_01.SM_GYCY_Build_DaDian_01_C";

        public const string SM_bingdong_03 =
            "/Game/00MainHZ/Environment/BPO/LYS/Interactable/SM_bingdong_03.SM_bingdong_03_C";

        public const string BPO_lys_mo4_ganshi =
            "/Game/00MainHZ/Environment/BPO/LYS/Interactable/BPO_lys_mo4_ganshi.BPO_lys_mo4_ganshi_C";

        public const string bingdongnew_02 =
            "/Game/00MainHZ/Environment/BPO/LYS/Interactable/bingdongxiaoguai_new/bingdongnew_02.bingdongnew_02_C";

        public const string S_Icelandic_rock_assembly_tebtbhfda_high =
            "/Game/00MainHZ/Environment/BPO/HYS/S_Icelandic_rock_assembly_tebtbhfda_high.S_Icelandic_rock_assembly_tebtbhfda_high_C";

        public const string S_Icelandic_rocks_skgtz_high_var1 =
            "/Game/00MainHZ/Environment/BPO/HYS/S_Icelandic_rocks_skgtz_high_var1.S_Icelandic_rocks_skgtz_high_var1_C";

        public const string S_Icelandic_boulder_tceueavda_high =
            "/Game/00MainHZ/Environment/BPO/HYS/S_Icelandic_boulder_tceueavda_high.S_Icelandic_boulder_tceueavda_high_C";

        public const string SM_LYS_GongZhou_01 =
            "/Game/00MainHZ/Environment/BPO/HYS/SM_LYS_GongZhou_01.SM_LYS_GongZhou_01_C";

        public const string SM_hys_tandaoyong_05 =
            "/Game/00MainHZ/Environment/BPO/HYS/SM_hys_tandaoyong_05.SM_hys_tandaoyong_05_C";

        public const string SM_hys_tandaoyong_04 =
            "/Game/00MainHZ/Environment/BPO/HYS/SM_hys_tandaoyong_04.SM_hys_tandaoyong_04_C";

        public const string SM_hys_tandaoyong_02 =
            "/Game/00MainHZ/Environment/BPO/HYS/SM_hys_tandaoyong_02.SM_hys_tandaoyong_02_C";

        public const string SM_hys_tandaoyong_01 =
            "/Game/00MainHZ/Environment/BPO/HYS/SM_hys_tandaoyong_01.SM_hys_tandaoyong_01_C";

        public const string SM_hys_qiaowuding_05 =
            "/Game/00MainHZ/Environment/BPO/HYS/SM_hys_qiaowuding_05.SM_hys_qiaowuding_05_C";

        public const string SM_hys_qiaolangan_04 =
            "/Game/00MainHZ/Environment/BPO/HYS/SM_hys_qiaolangan_04.SM_hys_qiaolangan_04_C";

        public const string SM_hys_dx_04 =
            "/Game/00MainHZ/Environment/BPO/HYS/SM_hys_dx_04.SM_hys_dx_04_C";

        public const string SM_hys_dx_03 =
            "/Game/00MainHZ/Environment/BPO/HYS/SM_hys_dx_03.SM_hys_dx_03_C";

        public const string SM_hys_dx_02 =
            "/Game/00MainHZ/Environment/BPO/HYS/SM_hys_dx_02.SM_hys_dx_02_C";

        public const string SM_hys_dx_01 =
            "/Game/00MainHZ/Environment/BPO/HYS/SM_hys_dx_01.SM_hys_dx_01_C";

        public const string SM_hyd_dengzuo_01 =
            "/Game/00MainHZ/Environment/BPO/HYS/SM_hyd_dengzuo_01.SM_hyd_dengzuo_01_C";

        public const string SM_hfm_rock_ue5_05 =
            "/Game/00MainHZ/Environment/BPO/HYS/SM_hfm_rock_ue5_05.SM_hfm_rock_ue5_05_C";

        public const string SM_HFM_Build_Bungalow_01_a01_b =
            "/Game/00MainHZ/Environment/BPO/HYS/SM_HFM_Build_Bungalow_01_a01_b.SM_HFM_Build_Bungalow_01_a01_b_C";

        public const string SM_luoshaqiaolian_pa =
            "/Game/00MainHZ/Environment/BPO/HYS/PA/SM_luoshaqiaolian_pa.SM_luoshaqiaolian_pa_C";

        public const string SM_HFM_Build_Bungalow_03_diaoxiang08_lys =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_HFM_Build_Bungalow_03_diaoxiang08_lys.SM_HFM_Build_Bungalow_03_diaoxiang08_lys_C";

        public const string SM_HFM_Build_Bungalow_03_diaoxiang08 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_HFM_Build_Bungalow_03_diaoxiang08.SM_HFM_Build_Bungalow_03_diaoxiang08_C";

        public const string SM_GYCY_Zhenmushou_posun_01 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_GYCY_Zhenmushou_posun_01.SM_GYCY_Zhenmushou_posun_01_C";

        public const string SM_GYCY_Obj_ZhuoZi_03 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_GYCY_Obj_ZhuoZi_03.SM_GYCY_Obj_ZhuoZi_03_C";

        public const string SM_GYCY_Obj_ZhuoZi_02 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_GYCY_Obj_ZhuoZi_02.SM_GYCY_Obj_ZhuoZi_02_C";

        public const string SM_GYCY_Hill_Fbx_DiaoXiang_01 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_GYCY_Hill_Fbx_DiaoXiang_01.SM_GYCY_Hill_Fbx_DiaoXiang_01_C";

        public const string SM_GYCY_Bulid_MuJia_01 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_GYCY_Bulid_MuJia_01.SM_GYCY_Bulid_MuJia_01_C";

        public const string SM_GYCY_Build_ShiPen_01 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_GYCY_Build_ShiPen_01.SM_GYCY_Build_ShiPen_01_C";

        public const string SM_GYCY_Build_Obj_zhalan_03 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_GYCY_Build_Obj_zhalan_03.SM_GYCY_Build_Obj_zhalan_03_C";

        public const string SM_GYCY_Build_Obj_zhalan_02 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_GYCY_Build_Obj_zhalan_02.SM_GYCY_Build_Obj_zhalan_02_C";

        public const string SM_GYCY_Build_Obj_zhalan_01 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_GYCY_Build_Obj_zhalan_01.SM_GYCY_Build_Obj_zhalan_01_C";

        public const string SM_GYCY_Build_Obj_ShuiTong_01 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_GYCY_Build_Obj_ShuiTong_01.SM_GYCY_Build_Obj_ShuiTong_01_C";

        public const string SM_GYCY_Build_Obj_ShuiGang_02 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_GYCY_Build_Obj_ShuiGang_02.SM_GYCY_Build_Obj_ShuiGang_02_C";

        public const string SM_GYCY_Build_obj_ShiTa_TJ_01 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_GYCY_Build_obj_ShiTa_TJ_01.SM_GYCY_Build_obj_ShiTa_TJ_01_C";

        public const string SM_GYCY_Build_Obj_ShiGu_02 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_GYCY_Build_Obj_ShiGu_02.SM_GYCY_Build_Obj_ShiGu_02_C";

        public const string SM_GYCY_Build_Obj_ShiGu_01 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_GYCY_Build_Obj_ShiGu_01.SM_GYCY_Build_Obj_ShiGu_01_C";

        public const string SM_GYCY_Build_Obj_Mujia_07_C =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_GYCY_Build_Obj_Mujia_07_C.SM_GYCY_Build_Obj_Mujia_07_C_C";

        public const string SM_GYCY_Build_obj_DengKan_01 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_GYCY_Build_obj_DengKan_01.SM_GYCY_Build_obj_DengKan_01_C";

        public const string SM_GYCY_Build_LiShiXiang_03 =
            "/Game/00MainHZ/Environment/BPO/HFS/Interactable/SM_GYCY_Build_LiShiXiang_03.SM_GYCY_Build_LiShiXiang_03_C";

        public const string SM_HFS_TreeStump_PY_01_Object002_GeometryCollection =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFS_TreeStump_PY_01_Object002_GeometryCollection.SM_HFS_TreeStump_PY_01_Object002_GeometryCollection_C";

        public const string SM_HFS_TreeStump_PY_01_Aset_rlthg_1_LOD6_GeometryCollection =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFS_TreeStump_PY_01_Aset_rlthg_1_LOD6_GeometryCollection.SM_HFS_TreeStump_PY_01_Aset_rlthg_1_LOD6_GeometryCollection_C";

        public const string SM_HFM_Tree_01_GeometryCollection =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Tree_01_GeometryCollection.SM_HFM_Tree_01_GeometryCollection_C";

        public const string SM_HFM_SM_initialShadingGroup =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_SM_initialShadingGroup.SM_HFM_SM_initialShadingGroup_C";

        public const string SM_HFM_GuanCai_02 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_GuanCai_02.SM_HFM_GuanCai_02_C";

        public const string SM_HFM_GuanCai_01 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_GuanCai_01.SM_HFM_GuanCai_01_C";

        public const string SM_HFM_GanShi_01A_Body_Posed1 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_GanShi_01A_Body_Posed1.SM_HFM_GanShi_01A_Body_Posed1_C";

        public const string SM_HFM_GanShi_01A_Body_Posed =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_GanShi_01A_Body_Posed.SM_HFM_GanShi_01A_Body_Posed_C";

        public const string SM_HFM_Canyon_Sandstone_Rock_uk4cdch_high1 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Canyon_Sandstone_Rock_uk4cdch_high1.SM_HFM_Canyon_Sandstone_Rock_uk4cdch_high1_C";

        public const string SM_HFM_Build_ShiBei_Spilt_008 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Build_ShiBei_Spilt_008.SM_HFM_Build_ShiBei_Spilt_008_C";

        public const string SM_HFM_Build_ShiBei_Spilt_001 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Build_ShiBei_Spilt_001.SM_HFM_Build_ShiBei_Spilt_001_C";

        public const string SM_HFM_Build_ShiBei_FuShu_2 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Build_ShiBei_FuShu_2.SM_HFM_Build_ShiBei_FuShu_2_C";

        public const string SM_HFM_Build_ShiBei_FuShu =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Build_ShiBei_FuShu.SM_HFM_Build_ShiBei_FuShu_C";

        public const string SM_HFM_Build_ShiBei_4_GeometryCollection =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Build_ShiBei_4_GeometryCollection.SM_HFM_Build_ShiBei_4_GeometryCollection_C";

        public const string SM_HFM_Build_ShiBei_27 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Build_ShiBei_27.SM_HFM_Build_ShiBei_27_C";

        public const string SM_HFM_Build_ShiBei_07_Spilt_2 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Build_ShiBei_07_Spilt_2.SM_HFM_Build_ShiBei_07_Spilt_2_C";

        public const string SM_HFM_Build_ShiBei_07_Spilt =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Build_ShiBei_07_Spilt.SM_HFM_Build_ShiBei_07_Spilt_C";

        public const string SM_HFM_Build_ShiBei_04 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Build_ShiBei_04.SM_HFM_Build_ShiBei_04_C";

        public const string SM_HFM_build_laobing_01 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_build_laobing_01.SM_HFM_build_laobing_01_C";

        public const string SM_HFM_build_lajiao_01 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_build_lajiao_01.SM_HFM_build_lajiao_01_C";

        public const string SM_HFM_Build_JiuGang_03_A =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Build_JiuGang_03_A.SM_HFM_Build_JiuGang_03_A_C";

        public const string SM_HFM_Build_JiuGang_03 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Build_JiuGang_03.SM_HFM_Build_JiuGang_03_C";

        public const string SM_HFM_build_guo_01 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_build_guo_01.SM_HFM_build_guo_01_C";

        public const string SM_HFM_build_Fbx_kaohongshu_01 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_build_Fbx_kaohongshu_01.SM_HFM_build_Fbx_kaohongshu_01_C";

        public const string SM_HFM_Build_Bungalow_Spilt_02 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Build_Bungalow_Spilt_02.SM_HFM_Build_Bungalow_Spilt_02_C";

        public const string SM_HFM_Build_Bungalow_09_a01_gang02 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Build_Bungalow_09_a01_gang02.SM_HFM_Build_Bungalow_09_a01_gang02_C";

        public const string SM_HFM_Build_Bungalow_09_a01_gang01 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Build_Bungalow_09_a01_gang01.SM_HFM_Build_Bungalow_09_a01_gang01_C";

        public const string SM_HFM_Build_Bungalow_01_a21 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Build_Bungalow_01_a21.SM_HFM_Build_Bungalow_01_a21_C";

        public const string SM_HFM_Build_Bungalow_01_a20 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Build_Bungalow_01_a20.SM_HFM_Build_Bungalow_01_a20_C";

        public const string SM_HFM_Build_Bungalow_01_a18 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/SM_HFM_Build_Bungalow_01_a18.SM_HFM_Build_Bungalow_01_a18_C";

        public const string HFM_ChanZhang_01a_Spilt_03_GeometryCollection =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/HFM_ChanZhang_01a_Spilt_03_GeometryCollection.HFM_ChanZhang_01a_Spilt_03_GeometryCollection_C";

        public const string HFM_Build_Door_02_damen02_posui =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/HFM_Build_Door_02_damen02_posui.HFM_Build_Door_02_damen02_posui_C";

        public const string HFM_Build_Door_02_damen01_posui =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/HFM_Build_Door_02_damen01_posui.HFM_Build_Door_02_damen01_posui_C";

        public const string BP_hfm_chuilong_stone =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/BP_hfm_chuilong_stone.BP_hfm_chuilong_stone_C";

        public const string BPO_HFM_ShanZhen_03 =
            "/Game/00MainHZ/Environment/BPO/HFM/Interactable/BPO_HFM_ShanZhen_03.BPO_HFM_ShanZhen_03_C";
    }
    public static class Droppable
    {
        public const string BP_zys_jiugang_droppable_xiezijing =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_zys_jiugang_droppable_xiezijing.BP_zys_jiugang_droppable_xiezijing_C";

        public const string BP_psd_droppable_chongluan_short_low =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_psd_droppable_chongluan_short_low.BP_psd_droppable_chongluan_short_low_C";

        public const string BP_psd_droppable_chongluan_short =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_psd_droppable_chongluan_short.BP_psd_droppable_chongluan_short_C";

        public const string BP_psd_droppable_chongluan_round_low =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_psd_droppable_chongluan_round_low.BP_psd_droppable_chongluan_round_low_C";

        public const string BP_psd_droppable_chongluan_round =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_psd_droppable_chongluan_round.BP_psd_droppable_chongluan_round_C";

        public const string BP_psd_droppable_chongluan_low =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_psd_droppable_chongluan_low.BP_psd_droppable_chongluan_low_C";

        public const string BP_psd_droppable_chongluan =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_psd_droppable_chongluan.BP_psd_droppable_chongluan_C";

        public const string BP_hfm_shuitong_01_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_hfm_shuitong_01_droppable.BP_hfm_shuitong_01_droppable_C";

        public const string BP_hfm_shuigang_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_hfm_shuigang_droppable.BP_hfm_shuigang_droppable_C";

        public const string BP_hfm_shuigang_02_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_hfm_shuigang_02_droppable.BP_hfm_shuigang_02_droppable_C";

        public const string BP_hfm_shipen_01_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_hfm_shipen_01_droppable.BP_hfm_shipen_01_droppable_C";

        public const string BP_hfm_jiugang_04_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_hfm_jiugang_04_droppable.BP_hfm_jiugang_04_droppable_C";

        public const string BP_hfm_jiugang_03_droppable_xingjiushi =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_hfm_jiugang_03_droppable_xingjiushi.BP_hfm_jiugang_03_droppable_xingjiushi_C";

        public const string BP_hfm_jiugang_03_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_hfm_jiugang_03_droppable.BP_hfm_jiugang_03_droppable_C";

        public const string BP_hfm_guo_01_droppable_prefab_woodtable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_hfm_guo_01_droppable_prefab_woodtable.BP_hfm_guo_01_droppable_prefab_woodtable_C";

        public const string BP_hfm_guo_01_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_hfm_guo_01_droppable.BP_hfm_guo_01_droppable_C";

        public const string BP_hfm_ganshi_01_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_hfm_ganshi_01_droppable.BP_hfm_ganshi_01_droppable_C";

        public const string BP_hfm_gang_02_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_hfm_gang_02_droppable.BP_hfm_gang_02_droppable_C";

        public const string BP_hfm_gang_01_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/BP_hfm_gang_01_droppable.BP_hfm_gang_01_droppable_C";

        public const string SM_HFM_Build_JiuGang_04_c_gang_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/ReplaceDroppable/SM_HFM_Build_JiuGang_04_c_gang_droppable.SM_HFM_Build_JiuGang_04_c_gang_droppable_C";

        public const string SM_HFM_Build_JiuGang_04_b_gang_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/ReplaceDroppable/SM_HFM_Build_JiuGang_04_b_gang_droppable.SM_HFM_Build_JiuGang_04_b_gang_droppable_C";

        public const string SM_HFM_Build_JiuGang_04_a_gang_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/ReplaceDroppable/SM_HFM_Build_JiuGang_04_a_gang_droppable.SM_HFM_Build_JiuGang_04_a_gang_droppable_C";

        public const string SM_HFM_Build_JiuGang_03_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/ReplaceDroppable/SM_HFM_Build_JiuGang_03_droppable.SM_HFM_Build_JiuGang_03_droppable_C";

        public const string SM_HFM_Build_JiuGang_03_A =
            "/Game/00MainHZ/Environment/BPO/ADroppable/ReplaceDroppable/SM_HFM_Build_JiuGang_03_A.SM_HFM_Build_JiuGang_03_A_C";

        public const string SM_HFM_Build_Bungalow_09_a01_gang02 =
            "/Game/00MainHZ/Environment/BPO/ADroppable/ReplaceDroppable/SM_HFM_Build_Bungalow_09_a01_gang02.SM_HFM_Build_Bungalow_09_a01_gang02_C";

        public const string SM_HFM_Build_Bungalow_09_a01_gang01_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/ReplaceDroppable/SM_HFM_Build_Bungalow_09_a01_gang01_droppable.SM_HFM_Build_Bungalow_09_a01_gang01_droppable_C";

        public const string SM_GYCY_Build_Obj_ShuiGang_02_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/ReplaceDroppable/SM_GYCY_Build_Obj_ShuiGang_02_droppable.SM_GYCY_Build_Obj_ShuiGang_02_droppable_C";

        public const string HFS_Destructible_ShuiGang_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/ReplaceDroppable/HFS_Destructible_ShuiGang_droppable.HFS_Destructible_ShuiGang_droppable_C";

        public const string BP_Destructible_ShuiGang_jiuchong =
            "/Game/00MainHZ/Environment/BPO/ADroppable/ReplaceDroppable/BP_Destructible_ShuiGang_jiuchong.BP_Destructible_ShuiGang_jiuchong_C";

        public const string bingdongnew_08_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/ReplaceDroppable/bingdongnew_08_droppable.bingdongnew_08_droppable_C";

        public const string bingdongnew_07_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/ReplaceDroppable/bingdongnew_07_droppable.bingdongnew_07_droppable_C";

        public const string bingdongnew_06_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/ReplaceDroppable/bingdongnew_06_droppable.bingdongnew_06_droppable_C";

        public const string bingdongnew_05_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/ReplaceDroppable/bingdongnew_05_droppable.bingdongnew_05_droppable_C";

        public const string bingdongnew_04_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/ReplaceDroppable/bingdongnew_04_droppable.bingdongnew_04_droppable_C";

        public const string bingdongnew_03_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/ReplaceDroppable/bingdongnew_03_droppable.bingdongnew_03_droppable_C";

        public const string bingdongnew_02_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/ReplaceDroppable/bingdongnew_02_droppable.bingdongnew_02_droppable_C";

        public const string bingdongnew_01_droppable =
            "/Game/00MainHZ/Environment/BPO/ADroppable/ReplaceDroppable/bingdongnew_01_droppable.bingdongnew_01_droppable_C";
    }
    public static class Chests
    {
        public const string BPO_TreasureBox_01 =
            "/Game/00Main/Design/InteractiveObjUnits/General/BPO_TreasureBox_01.BPO_TreasureBox_01_C";

        public const string BPO_TreasureBox_02 =
            "/Game/00Main/Design/InteractiveObjUnits/General/BPO_TreasureBox_02.BPO_TreasureBox_02_C";

        public const string BPO_TreasureBox_03a =
            "/Game/00Main/Design/InteractiveObjUnits/General/BPO_TreasureBox_03a.BPO_TreasureBox_03a_C";

        public const string BPO_TreasureBox_04a =
            "/Game/00Main/Design/InteractiveObjUnits/General/BPO_TreasureBox_04a.BPO_TreasureBox_04a_C";

        public const string BPO_TreasureBox_05a =
            "/Game/00Main/Design/InteractiveObjUnits/General/BPO_TreasureBox_05a.BPO_TreasureBox_05a_C";

        public const string BPO_TreasureBox_06 =
            "/Game/00Main/Design/InteractiveObjUnits/General/BPO_TreasureBox_06.BPO_TreasureBox_06_C";

        public const string BPO_TreasureBox_07a =
            "/Game/00Main/Design/InteractiveObjUnits/General/BPO_TreasureBox_07a.BPO_TreasureBox_07a_C";

        public const string BPO_TreasureBox_08 =
            "/Game/00Main/Design/InteractiveObjUnits/General/BPO_TreasureBox_08.BPO_TreasureBox_08_C";

        public const string BPO_TreasureBox_Coffin_01 =
            "/Game/00Main/Design/InteractiveObjUnits/General/BPO_TreasureBox_Coffin_01.BPO_TreasureBox_Coffin_01_C";

        public const string BPO_TreasureBox_Coffin_02 =
            "/Game/00Main/Design/InteractiveObjUnits/General/BPO_TreasureBox_Coffin_02.BPO_TreasureBox_Coffin_02_C";

        public const string BPO_TreasureBox_JiaSi_07a =
            "/Game/00Main/Design/InteractiveObjUnits/General/BPO_TreasureBox_JiaSi_07a.BPO_TreasureBox_JiaSi_07a_C";
    }
}