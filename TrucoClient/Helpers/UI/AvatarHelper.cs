using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TrucoClient.Helpers.Paths;
using TrucoClient.Properties.Langs;
using TrucoClient.Views;

namespace TrucoClient.Helpers.UI
{
    public static class AvatarHelper
    {
        private const string DEFAULT_AVATAR_ID = "avatar_aaa_default";
        private const string MESSAGE_ERROR = "Error";   
        private static readonly string defaultAvatarPackUri = GetPackUri(DEFAULT_AVATAR_ID);

        private static readonly List<string> internalAvatars = new List<string>
        {
            "avatar_aaa_default", "avatar_c_hr_rallycarback",
            "avatar_c_hr_rallycarfront", "avatar_c_hr_redcarback", "avatar_c_hr_redcarfront",
            "avatar_c_nr_jetski", "avatar_fc_boxer", "avatar_fc_boxing", "avatar_fc_bw_bigman",
            "avatar_fc_bw_hippie", "avatar_fc_bw_skull", "avatar_fc_cod_soldierone",
            "avatar_fc_cod_soldierthree", "avatar_fc_cod_soldiertwo", "avatar_fc_gv_chop",
            "avatar_fc_gv_franklin", "avatar_fc_gv_michael", "avatar_fc_gv_trevor",
            "avatar_fc_h_warriorblue", "avatar_fc_h_warriorgreen", "avatar_fc_hr_a_husky",
            "avatar_fc_hr_a_marathonpig", "avatar_fc_hr_a_monkey", "avatar_fc_hr_a_teddybear",
            "avatar_fc_hr_ap_robberbandana", "avatar_fc_hr_ap_robbergasmask", "avatar_fc_hr_ap_vampire",
            "avatar_fc_hr_ap_zombie", "avatar_fc_hr_bigchin", "avatar_fc_hr_p_blondegirl",
            "avatar_fc_hr_p_bullskull", "avatar_fc_hr_p_demonicninja", "avatar_fc_hr_p_emorocker",
            "avatar_fc_hr_p_hobbit", "avatar_fc_hr_p_winterqueen", "avatar_fc_twod_c_angryman",
            "avatar_fc_twod_c_aviator", "avatar_fc_twod_c_biker", "avatar_fc_twod_c_cowboyone",
            "avatar_fc_twod_c_demongirl", "avatar_fc_twod_c_gamerskull", "avatar_fc_twod_c_hitman",
            "avatar_fc_twod_c_jesus", "avatar_fc_twod_c_pirate", "avatar_fc_twod_c_plug",
            "avatar_fc_twod_t_alien", "avatar_fc_twod_t_banjo", "avatar_fc_twod_t_bubblegum",
            "avatar_fc_twod_t_cowboytwo", "avatar_fc_twod_t_cyberpanda", "avatar_fc_twod_t_dog",
            "avatar_fc_twod_t_girl", "avatar_fc_twod_t_ninja", "avatar_fc_twod_t_singingman",
            "avatar_fl_argentina", "avatar_fl_bolivia", "avatar_fl_brasil", "avatar_fl_canada",
            "avatar_fl_chile", "avatar_fl_colombia", "avatar_fl_costarica", "avatar_fl_cuba",
            "avatar_fl_dominican", "avatar_fl_ecuador", "avatar_fl_elsalvador", "avatar_fl_guatemala",
            "avatar_fl_honduras", "avatar_fl_mexico", "avatar_fl_nicaragua", "avatar_fl_panama",
            "avatar_fl_paraguay", "avatar_fl_peru", "avatar_fl_puertorico", "avatar_fl_spain",
            "avatar_fl_uk", "avatar_fl_uruguay", "avatar_fl_usa", "avatar_fl_venezuela",
            "avatar_ft_america", "avatar_ft_atlas", "avatar_ft_atleticosanluis", "avatar_ft_chivas",
            "avatar_ft_cruzazul", "avatar_ft_fcjuarez", "avatar_ft_leon", "avatar_ft_mazatlan",
            "avatar_ft_monterrey", "avatar_ft_necaxa", "avatar_ft_pachuca", "avatar_ft_pueblafc",
            "avatar_ft_pumas", "avatar_ft_queretaro", "avatar_ft_santos", "avatar_ft_tiburones",
            "avatar_ft_tigres", "avatar_ft_tijuana", "avatar_ft_toluca", "avatar_t_hr_bigdaddy",
            "avatar_t_hr_eagle", "avatar_t_hr_football", "avatar_t_hr_goldensun", "avatar_t_hr_golf",
            "avatar_t_hr_lime", "avatar_t_hr_smiley", "avatar_t_hr_wheel", "avatar_t_logo_skate",
            "avatar_t_mc_cookie", "avatar_t_mc_goldenapple", "avatar_t_mc_porkchop",
            "avatar_t_txt_bigbomb", "avatar_t_txt_bigreds", "avatar_t_txt_ggheart", "avatar_t_txt_hammer",
            "avatar_t_txt_lisgreen", "avatar_t_txt_lisred", "avatar_t_txt_moneybag", "avatar_t_txt_pumpkin",
            "avatar_t_txt_saucy", "avatar_t_txt_soap", "avatar_t_txt_softwaresquad", "avatar_t_txt_sq",
            "avatar_tt_dragon", "avatar_tt_lilbird", "avatar_tt_revolver", "avatar_tt_twentydollars", 
            "avatar_tt_ztv_wire", "avatar_tt_ztv_bb", "avatar_tt_ztv_lost",
        };

        public static readonly ReadOnlyCollection<string> availableAvatars = internalAvatars.AsReadOnly();

        public static void LoadAvatarImage(Image imageControl, string avatarId)
        {
            if (imageControl == null)
            {
                return;
            }

            string idToLoad = string.IsNullOrWhiteSpace(avatarId) ? DEFAULT_AVATAR_ID : avatarId;

            string packUri = GetPackUri(idToLoad);

            try
            {
                imageControl.Source = new BitmapImage(new Uri(packUri, UriKind.Absolute));
            }
            catch (UriFormatException)
            {
                imageControl.Source = new BitmapImage(new Uri(defaultAvatarPackUri, UriKind.Absolute));
            }
            catch (IOException)
            {
                LoadDefaultAvatar(imageControl);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(Lang.ExceptionTextErrorLoadingAvatar, 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
                imageControl.Source = new BitmapImage(new Uri(defaultAvatarPackUri, UriKind.Absolute));
            }
        }

        public static void LoadDefaultAvatar(Image imageControl)
        {
            try
            {
                imageControl.Source = new BitmapImage(new Uri(defaultAvatarPackUri, UriKind.Absolute));
            }
            catch (FileNotFoundException)
            {
                CustomMessageBox.Show(string.Format(Lang.ExceptionTextAvatarIdFailedToLoadDefault, defaultAvatarPackUri), 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (UriFormatException)
            {
                CustomMessageBox.Show(string.Format(Lang.ExceptionTextAvatarIdFailedToLoadDefault, defaultAvatarPackUri), 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception)
            {
                CustomMessageBox.Show(string.Format(Lang.ExceptionTextAvatarIdFailedToLoadDefault, defaultAvatarPackUri), 
                    MESSAGE_ERROR, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static string GetPackUri(string avatarId)
        {
            return $"pack://application:,,,/TrucoClient;component{ResourcePaths.RESOURCE_BASE_PATH}{avatarId}.png";
        }
    }
}
