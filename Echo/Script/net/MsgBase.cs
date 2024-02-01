using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;


namespace Echo.Script.net
{
    class MsgBase
    {
        public string protoName = "null";

        //编码器
        static JavaScriptSerializer js = new JavaScriptSerializer();
        
        //编码
        public static byte[] Encode(MsgBase msgBase)
        {
            string s = js.Serialize(msgBase);
            return System.Text.Encoding.UTF8.GetBytes(s);

        }
        //解码
        public static MsgBase Decode(string protoName,byte[] bytes,int offset,int count)
        {
            string s = System.Text.Encoding.UTF8.GetString(bytes,offset,count);
            MsgBase msgBase = (MsgBase)js.Deserialize(s, Type.GetType(protoName));
            return msgBase;
        }

        //编码协议名
        public static byte[] EncodeName(MsgBase msgBase)
        {

            return new byte[3];
        }

    }
}
