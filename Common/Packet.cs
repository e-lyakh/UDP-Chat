using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public enum DataId
    {
        Message,
        LogIn,
        LogOut,
        Null
    }

    public class Packet
    {
        private DataId dataId;
        private string name;
        private string message;


        public DataId ChatDataId
        {
            get { return dataId; }
            set { dataId = value; }
        }

        public string ChatName
        {
            get { return name; }
            set { name = value; }
        }

        public string ChatMessage
        {
            get { return message; }
            set { message = value; }
        }

        public Packet()
        {
            dataId = DataId.Null;
            message = null;
            name = null;
        }

        public Packet(byte[] dataStream)
        {
            dataId = (DataId)BitConverter.ToInt32(dataStream, 0);

            int nameLength = BitConverter.ToInt32(dataStream, 4);

            int msgLength = BitConverter.ToInt32(dataStream, 8);

            if (nameLength > 0)
                name = Encoding.UTF8.GetString(dataStream, 12, nameLength);
            else
                name = null;

            if (msgLength > 0)
                message = Encoding.UTF8.GetString(dataStream, 12 + nameLength, msgLength);
            else
                message = null;
        }

        public byte[] GetDataStream()
        {
            List<byte> dataStream = new List<byte>();

            dataStream.AddRange(BitConverter.GetBytes((int)dataId));

            if (name != null)
                dataStream.AddRange(BitConverter.GetBytes(name.Length));
            else
                dataStream.AddRange(BitConverter.GetBytes(0));

            if (message != null)
                dataStream.AddRange(BitConverter.GetBytes(message.Length));
            else
                dataStream.AddRange(BitConverter.GetBytes(0));

            if (name != null)
                dataStream.AddRange(Encoding.UTF8.GetBytes(name));

            if (message != null)
                dataStream.AddRange(Encoding.UTF8.GetBytes(message));

            return dataStream.ToArray();
        }
    }
}
