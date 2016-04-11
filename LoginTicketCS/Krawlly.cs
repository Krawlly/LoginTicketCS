using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Krawlly
{
    public sealed class TicketFactory
    {
        private readonly RSACryptoServiceProvider rsaEncryptor;

        public TicketFactory(string certificateFileName)
        {
            rsaEncryptor = getProvider(new X509Certificate2(certificateFileName));
        }

        public TicketFactory(byte[] certificate)
        {
            rsaEncryptor = getProvider(new X509Certificate2(certificate));
        }

        private RSACryptoServiceProvider getProvider(X509Certificate2 cert)
        {
            return (RSACryptoServiceProvider)cert.PublicKey.Key;
        }

        public string CreateTicket(Object data)
        {
            if (data == null) return "";
            DataContractJsonSerializer s = new DataContractJsonSerializer(data.GetType());
            MemoryStream ms = new MemoryStream();
            s.WriteObject(ms, data);
            ms.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(ms);
            string result = sr.ReadToEnd();
            sr.Close();
            ms.Close();
            return CreateTicket(result);
        }

        public string CreateTicket(String data)
        {
            AsnBuffer paramAsn = new AsnBuffer();
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            paramAsn.AddString("AES");
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            paramAsn.AddString("AES/CBC/PKCS5Padding");
            aes.KeySize = 128;
            aes.BlockSize = aes.KeySize;
            aes.GenerateKey();
            aes.GenerateIV();
            paramAsn.AddOctetString(aes.IV);

            ICryptoTransform enc = aes.CreateEncryptor(aes.Key, aes.IV);

            MemoryStream ms = new MemoryStream();

            byte[] paramEncoded = rsaEncryptor.Encrypt(paramAsn.ToArray(), false);
            ms.Write(paramEncoded, 0, paramEncoded.Length);

            byte[] wrappedKey = rsaEncryptor.Encrypt(aes.Key, false);
            ms.Write(wrappedKey, 0, wrappedKey.Length);

            CryptoStream cs = new CryptoStream(ms, enc, CryptoStreamMode.Write);
            StreamWriter sw = new StreamWriter(cs);
            sw.Write(data);
            sw.Flush();
            sw.Close();
            cs.Close();
            ms.Close();

            return Convert.ToBase64String(ms.ToArray());
        }
    }

    sealed class AsnBuffer
    {
        private byte[] buffer;

        public AsnBuffer()
        {
            buffer = new byte[2];
            buffer[0] = 48;
            buffer[1] = 0;
        }

        public void AddString(string ps)
        {
            byte[] b = Encoding.UTF8.GetBytes(ps);
            int i = buffer.Length;
            Array.Resize<byte>(ref buffer, buffer.Length + 2 + b.Length);
            buffer[1] = (byte)(buffer[1] + b.Length + 2);
            buffer[i] = 22;
            buffer[i + 1] = (byte)b.Length;
            Array.Copy(b, 0, buffer, i + 2, b.Length);
        }

        public void AddOctetString(byte[] b)
        {
            int i = buffer.Length;
            Array.Resize<byte>(ref buffer, buffer.Length + 2 + b.Length);
            buffer[1] = (byte)(buffer[1] + b.Length + 2);
            buffer[i] = 4;
            buffer[i + 1] = (byte)b.Length;
            Array.Copy(b, 0, buffer, i + 2, b.Length);
        }

        public byte[] ToArray()
        {
            byte[] snapshot = new byte[buffer.Length];
            buffer.CopyTo(snapshot, 0);
            return snapshot;
        }
    }
}
