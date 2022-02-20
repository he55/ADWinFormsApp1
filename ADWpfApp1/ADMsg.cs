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

        public static bool IsMSG(byte[] buf)
        {
            uint v = BitConverter.ToUInt32(buf, 0);
            return v == HEADER;
        }

        public static ADMsg ToMSG(byte[] buf)
        {
            uint v = BitConverter.ToUInt32(buf, 0);
            if (v != HEADER)
            {
                throw new Exception();
            }

            ADMsg msg;
            msg.header = HEADER;
            msg.msgType = BitConverter.ToInt32(buf, 4);
            msg.len = BitConverter.ToInt32(buf, 8);
            msg.data = new byte[0];

            if (msg.len != 0)
            {
                msg.data = new byte[msg.len];
                Array.Copy(buf, 12, msg.data, 0, msg.len);
            }

            return msg;
        }

        public static ADMsg helloData()
        {
            ADMsg adMsg = new ADMsg(ADMsgType.hello);
            return adMsg;
        }

        public static ADMsg helloOKData(string name)
        {
            ADMsg adMsg = sendStringData(name);
            adMsg.msgType = (int)ADMsgType.helloOK;
            return adMsg;
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
            ADMsg adMsg = new ADMsg(ADMsgType.sendFileCancel);
            return adMsg;
        }

        public static ADMsg sendStringData(string str)
        {
            ADMsg adMsg = new ADMsg(ADMsgType.sendString);
            adMsg.data = Encoding.UTF8.GetBytes(str);
            adMsg.len = adMsg.data.Length;
            return adMsg;
        }

        public static ADMsg sendUrlData(string url)
        {
            ADMsg adMsg = sendStringData(url);
            adMsg.msgType = (int)ADMsgType.sendUrl;
            return adMsg;
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
    }
}
