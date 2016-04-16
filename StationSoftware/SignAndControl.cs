using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StationSoftware
{
    public class SignAndControl
    {
        KellSCM.Controller controler;

        public SignAndControl()
        {
            controler = new KellSCM.Controller();
            controler.Readed += new KellSCM.ReadedHandler(controler_Readed);
        }

        public int GetChannelStatus(int channel)
        {
            return controler.GetChannelStatus(channel);
        }
        /// <summary>
        /// 采集信号，以便进行相应的处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void controler_Readed(object sender, KellSCM.ReadDataArgs e)
        {
            if (e.ComName == "")
            {
                int status;
                switch (e.ChannelNum)
                {
                    case 0:
                        status = controler.GetChannelStatus(0);

                        break;
                    case 1:
                        status = controler.GetChannelStatus(1);

                        break;
                    case 2:
                        status = controler.GetChannelStatus(2);

                        break;
                    case 3:
                        status = controler.GetChannelStatus(3);

                        break;
                    case 4:
                        status = controler.GetChannelStatus(4);

                        break;
                    case 5:
                        status = controler.GetChannelStatus(5);

                        break;
                    case 6:
                        status = controler.GetChannelStatus(6);

                        break;
                    case 7:
                        status = controler.GetChannelStatus(7);

                        break;
                }
            }
        }

        public bool Control(int channel, bool set)
        {
            return controler.Send(channel, set);
        }

        public bool Read(int channel)
        {
            return controler.Send(channel, false, true);
        }
    }
}
