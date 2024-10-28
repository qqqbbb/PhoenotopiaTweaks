﻿using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhoenotopiaTweaks
{
    internal class Config
    {
        public static ConfigEntry<bool> quickStart;
        public static ConfigEntry<bool> healthHeartAnimation;
        public static ConfigEntry<bool> quickSelect;
        public static ConfigEntry<bool> stupidFlashes;
        public static ConfigEntry<float> playerDamageMultiplier;
        public static ConfigEntry<float> enemyDamageMultiplier;


        public static void Bind()
        {
            quickStart = Main.config.Bind("", "Skip developer logo when starting the game", false);
            healthHeartAnimation = Main.config.Bind("", "Health heart animation", true);
            quickSelect = Main.config.Bind("", "You dont have to press right stick to equip items", false);
            playerDamageMultiplier = Main.config.Bind("", "Damage dealt to player will be multiplied by this", 1f);
            enemyDamageMultiplier = Main.config.Bind("", "Damage dealt to anything but player will be multiplied by this", 1f);
            stupidFlashes = Main.config.Bind("", "White color flashes when saving game and playing spheralis", true);

        }
    }
}
