using System;
using System.IO;
using System.Net;
using System.Text;

namespace ADWpfApp1
{
    public enum ADMsgType : int
    {
        hello = 0x1,
        helloOK,
        sendFile,
        sendFileOK,
        sendFileCancel,
        sendString,
        sendUrl
    }

    public struct ADMsg
    {
        const uint HEADER = 0xad00ad00;

        uint header;
        int msgType;
        int len;
        byte[] data;

        public ADMsg(ADMsgType msgType)
        {
            this.header = HEADER;
            this.msgType = (int)msgType;
            this.len = 0;
            this.data = new byte[0];
        }

        public ADMsg(ADMsgType msgType, string str)
        {
            this.header = HEADER;
            this.msgType = (int)msgType;
            this.data = Encoding.UTF8.GetBytes(str);
            this.len = this.data.Length;
        }

        public int GetMsgType()
        {
            return this.msgType;
        }

        public IPEndPoint ToIPData()
        {
            long addr = BitConverter.ToInt64(this.data, 0);
            int port = BitConverter.ToInt32(this.data, 8);
            return new IPEndPoint(addr, port);
        }

        public string ToStringData()
        {
            return Encoding.UTF8.GetString(this.data);
        }

        public MyDownloadFileInfo ToFileData()
        {
            MyDownloadFileInfo myDownloadFileInfo = new MyDownloadFileInfo();
            myDownloadFileInfo.Len = BitConverter.ToInt64(this.data, 0);
            myDownloadFileInfo.FileName = Encoding.UTF8.GetString(this.data, 8, this.len - 8);
            return myDownloadFileInfo;
        }

        public byte[] ToArr()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write(this.header);
                    binaryWriter.Write(this.msgType);
                    binaryWriter.Write(this.len);
                    binaryWriter.Write(this.data);
                }
                return memoryStream.ToArray();
            }
        }


        public static bool IsMSG(byte[] buf)
        {
            uint v = BitConverter.ToUInt32(buf, 0);
            return v == HEADER;
        }

        public static ADMsg ToMSG(byte[] buf)
        {
            if (!IsMSG(buf))
                throw new Exception();

            ADMsg msg;
            msg.header = HEADER;
            msg.msgType = BitConverter.ToInt32(buf, 4);
            msg.len = BitConverter.ToInt32(buf, 8);
            msg.data = new byte[msg.len];

            if (msg.len != 0)
                Array.Copy(buf, 12, msg.data, 0, msg.len);

            return msg;
        }

        public static ADMsg helloData()
        {
            return new ADMsg(ADMsgType.hello);
        }

        public static ADMsg helloOKData(string name)
        {
            return new ADMsg(ADMsgType.helloOK, name);
        }

        public static ADMsg sendFileData(string filePath)
        {
            ADMsg adMsg = new ADMsg(ADMsgType.sendFile);

            FileInfo fileInfo = new FileInfo(filePath);
            long length = fileInfo.Length;

            byte[] vs1 = BitConverter.GetBytes(length);
            byte[] vs = Encoding.UTF8.GetBytes(fileInfo.Name);

            adMsg.len = 8 + vs.Length;
            adMsg.data = new byte[adMsg.len];

            vs1.CopyTo(adMsg.data, 0);
            vs.CopyTo(adMsg.data, 8);

            return adMsg;
        }

        public static ADMsg sendFileOKData(IPEndPoint ep)
        {
            ADMsg adMsg = new ADMsg(ADMsgType.sendFileOK);

            byte[] addr = ep.Address.GetAddressBytes();
            adMsg.len = 12;
            adMsg.data = new byte[12] {
                addr[0],
                addr[1],
                addr[2],
                addr[3],
                0,
                0,
                0,
                0,
                (byte)ep.Port,
                (byte)(ep.Port>>8),
                (byte)(ep.Port>>16),
                (byte)(ep.Port>>24),
            };

            return adMsg;
        }

        public static ADMsg sendFileCancelData()
        {
            return new ADMsg(ADMsgType.sendFileCancel);
        }

        public static ADMsg sendStringData(string str)
        {
            return new ADMsg(ADMsgType.sendString, str);
        }

        public static ADMsg sendUrlData(string url)
        {
            return new ADMsg(ADMsgType.sendUrl, url);
        }
    }
}
