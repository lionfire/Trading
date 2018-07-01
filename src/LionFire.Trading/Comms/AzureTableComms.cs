using LionFire.Trading.Bots;
using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Trading.Comms
{
    public class AzureTableComms
    {

        #region Bot

        public IBot Bot
        {
            get { return bot; }
            set
            {
                if (bot == value) return;
                if (bot != null)
                {

                }
                bot = value;
                if (bot != null)
                {
                    //bot.Settings
                }
            }
        }
        private IBot bot;

        #endregion


    }
}
