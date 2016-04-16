using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StationSoftware
{
    public interface IStringConverter<T>
    {
        string ConvertToString();
        void ConvertFrom(string s);
    }

    public class Channel : IStringConverter<Channel>
    {
        uint channel;

        public uint ChannelValue
        {
            get { return channel; }
            private set { channel = value; }
        }

        public Channel()
        {
        }

        public Channel(uint channel)
        {
            ChannelValue = channel;
        }

        public string ConvertToString()
        {
            return ChannelValue.ToString();
        }

        public void ConvertFrom(string s)
        {
            uint value = Convert.ToUInt32(s);
            ChannelValue = value;
        }
    }
}
