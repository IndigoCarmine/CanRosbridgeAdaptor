
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CanRosbridgeAdaptor
{
    public static class COBS
    {
        //raw to encoded bytes
        public static byte[] Encode(byte[] bytes)
        {
            byte[] result = new byte[bytes.Length + 2];
            result[bytes.Length] = 0;
            byte zero_counter = 1;
            for (int i = bytes.Length - 1; i >=
                0; i--)
            {
                if (bytes[i] == 0)
                {
                    result[i] = zero_counter;
                    zero_counter = 0;
                }
                else
                {
                    result[i] = zero_counter;
                }
                zero_counter++;
            }
            return result;
        }
        public static byte[] Decode(byte[] bytes)
        {
            if (bytes.Length <= 2) throw new Exception("Decode size is too small");

            byte[] result = new byte[bytes.Length-2];
            byte zero_counter = bytes[0];
            for (int i = 0; i<result.Length; i++)
            {
                zero_counter--;

                if (zero_counter==0)
                {
                    result[i] = 0;
                    zero_counter = bytes[i];
                }
                else
                {
                    result[i] = bytes[i];
                }
            }

            return result;
        }
    }


    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var random = new Random();
            
            byte[] bytes = new byte[20];
            random.NextBytes(bytes);
            byte[] bytes2 = COBS.Decode(COBS.Encode(bytes));
            for(int j = 0; j < bytes.Length; j++)
            {
                Assert.AreEqual(bytes[j], bytes2[j]);
            }
        }
    }
}