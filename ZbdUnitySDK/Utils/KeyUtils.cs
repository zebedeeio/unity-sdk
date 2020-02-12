namespace ZbdUnitySDK.UnityUtils
{
    /*
        Copyright (c) 2013-2019 BitPay, Inc. Licensed under The MIT License (MIT)
        See license text at https://github.com/bitpay/csharp-bitpay-client#license
    */

    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using Multiformats.Base;
    using NBitcoin;
    using Org.BouncyCastle.Crypto.Digests;
    using Org.BouncyCastle.Math;
    using ZbdUnitySDK.Utils;

    public class KeyUtils
    {
        private static char[] hexArray = "0123456789abcdef".ToCharArray();
        private KeyUtils() { }

        public static Key CreateNBEcKey()
        {
            // Default constructor uses SecureRandom numbers.
            return new Key();
        }

        public static Key LoadNBEcKey(IResourceDataAccess resourceDataAccess)
        {
            string base58 = resourceDataAccess.Load();

            return LoadNBEcKey(base58);
        }

        public static Key LoadNBEcKey(String base58)
        {
            byte[] b = Multibase.Decode(base58, out MultibaseEncoding encoding);
            EcKey key = EcKey.FromAsn1(b);
            return new Key(key.GetPrivKeyBytes());
        }

        public static void SaveEcKey(Key nbEcKey,IResourceDataAccess resourceDataAccess)
        {
            byte[] bytes = EcKey.ToAsn1(nbEcKey.ToBytes(), nbEcKey.PubKey.ToBytes());
            string base58 = Multibase.Encode(MultibaseEncoding.Base58Btc, bytes);
            resourceDataAccess.Save(base58);
        }

        public static String DeriveSIN(EcKey ecKey)
        {
            byte[] pubKey = ecKey.PubKey;
            return DeriveSIN(pubKey);
        }

        public static String DeriveSIN(Key nbEcKey)
        {
            byte[] pubKey = nbEcKey.PubKey.Decompress().ToBytes();
            return DeriveSIN(pubKey);
        }

        private static String DeriveSIN(byte[] pubKeyBytes)
        {
            // Get sha256 hash and then the RIPEMD-160 hash of the public key (this call gets the result in one step).
            byte[] hash = new SHA256Managed().ComputeHash(pubKeyBytes);
            RipeMD160Digest ripeMd160Digest = new RipeMD160Digest();
            ripeMd160Digest.BlockUpdate(hash, 0, hash.Length);
            byte[] output = new byte[20];
            ripeMd160Digest.DoFinal(output, 0);

            byte[] pubKeyHash = output;

            // Convert binary pubKeyHash, SINtype and version to Hex
            String version = "0F";
            String SINtype = "02";
            String pubKeyHashHex = bytesToHex(pubKeyHash);

            // Concatenate all three elements
            String preSIN = version + SINtype + pubKeyHashHex;

            // Convert the hex string back to binary and double sha256 hash it leaving in binary both times
            byte[] preSINbyte = hexToBytes(preSIN);
            byte[] hash2Bytes = DoubleDigest(preSINbyte);

            // Convert back to hex and take first four bytes
            String hashString = bytesToHex(hash2Bytes);
            String first4Bytes = hashString.Substring(0, 8);

            // Append first four bytes to fully appended SIN string
            String unencoded = preSIN + first4Bytes;
            byte[] unencodedBytes = new BigInteger(unencoded,
                                                   16).ToByteArray();
            String encoded = Encode(unencodedBytes);

            return encoded;
        }

        private const string _alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        private static readonly BigInteger _base = BigInteger.ValueOf(58);

        public static string Encode(byte[] input)
        {
            // TODO: This could be a lot more efficient.
            var bi = new BigInteger(1, input);
            var s = new StringBuilder();
            while (bi.CompareTo(_base) >= 0)
            {
                var mod = bi.Mod(_base);
                s.Insert(0, new[] { _alphabet[mod.IntValue] });
                bi = bi.Subtract(mod).Divide(_base);
            }
            s.Insert(0, new[] { _alphabet[bi.IntValue] });
            // Convert leading zeros too.
            foreach (var anInput in input)
            {
                if (anInput == 0)
                    s.Insert(0, new[] { _alphabet[0] });
                else
                    break;
            }
            return s.ToString();
        }

        /// <summary>
        /// See <see cref="DoubleDigest(byte[], int, int)"/>.
        /// </summary>
        public static byte[] DoubleDigest(byte[] input)
        {
            return DoubleDigest(input, 0, input.Length);
        }

        /// <summary>
        /// Calculates the SHA-256 hash of the given byte range, and then hashes the resulting hash again. This is
        /// standard procedure in BitCoin. The resulting hash is in big endian form.
        /// </summary>
        public static byte[] DoubleDigest(byte[] input, int offset, int length)
        {
            var algorithm = new SHA256Managed();
            var first = algorithm.ComputeHash(input, offset, length);
            return algorithm.ComputeHash(first);
        }

        public static byte[] ConvertDerToP1393(byte[] data)
        {
            byte[] b = new byte[132];
            int totalLength = data[1];
            int n = 0;
            int offset = 4;
            int thisLength = data[offset++];
            if (data[offset] == 0)
            {
                // Negative number!
                ++offset;
                --thisLength;
            }
            for (int i = thisLength; i < 66; ++i)
            {
                b[n++] = 0;
            }
            if (thisLength > 66)
            {
                System.Console.WriteLine("BAD, first number is too big! " + thisLength);
            }
            else
            {
                for (int i = 0; i < thisLength; ++i)
                {
                    b[n++] = data[offset++];
                }
            }
            ++offset;
            thisLength = data[offset++];

            for (int i = thisLength; i < 66; ++i)
            {
                b[n++] = 0;
            }
            if (thisLength > 66)
            {
                System.Console.WriteLine("BAD, second number is too big! " + thisLength);
            }
            else
            {
                for (int i = 0; i < thisLength; ++i)
                {
                    b[n++] = data[offset++];
                }
            }
            return b;
        }

        public static string sign(EcKey ecKey, string input)
        {
            // return ecKey.Sign(input);
            String hash = Sha256Hash(input);
            var hashBytes = hexToBytes(hash);
            var signature = ecKey.Sign(hashBytes);
            var bytesHex = bytesToHex(signature);
            return bytesHex;
            //return bytesToHex(ecKey.Sign(hexToBytes(hash)));
        }

        private static byte[] Sha256HashBytes(string value)
        {
            using (var hash = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                var result = hash.ComputeHash(bytes);
                return result;
            }
        }

        public static String Sha256Hash(String value)
        {
            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }

        private static int getHexVal(char hex)
        {
            int val = (int)hex;
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
        private static bool isValidHexDigit(char chr)
        {
            return ('0' <= chr && chr <= '9') || ('a' <= chr && chr <= 'f') || ('A' <= chr && chr <= 'F');
        }

        public static byte[] hexToBytes(string hex)
        {
            if (hex == null)
                throw new ArgumentNullException("hex");
            if (hex.Length % 2 == 1)
                throw new FormatException("The binary key cannot have an odd number of digits");

            if (hex == string.Empty)
                return new byte[0];

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                char highNibble = hex[i << 1];
                char lowNibble = hex[(i << 1) + 1];

                if (!isValidHexDigit(highNibble) || !isValidHexDigit(lowNibble))
                    throw new FormatException("The binary key contains invalid chars.");

                arr[i] = (byte)((getHexVal(highNibble) << 4) + (getHexVal(lowNibble)));
            }
            return arr;
        }

        public static String bytesToHex(byte[] bytes)
        {
            char[] hexChars = new char[bytes.Length * 2];
            for (int j = 0; j < bytes.Length; j++)
            {
                int v = bytes[j] & 0xFF;
                hexChars[j * 2] = hexArray[(int)((uint)v >> 4)];
                hexChars[j * 2 + 1] = hexArray[v & 0x0F];
            }
            return new String(hexChars);
        }

        static byte[] getBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
