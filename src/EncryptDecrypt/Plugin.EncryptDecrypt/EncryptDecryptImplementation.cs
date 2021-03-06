﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

 using Plugin.EncryptDecrypt.Abstractions;

namespace Plugin.EncryptDecrypt
{

#if __NETSTANDARD__
    public class EncryptDecryptImplementation : EncryptDecryptBase, IEncryptDecrypt
    {

        protected override byte[] Decrypt(byte[] keys, byte[] data)
        {
            throw new NotImplementedException();
        }

        protected override byte[] Encrypt(byte[] keys, byte[] data)
        {
            throw new NotImplementedException();
        }

        protected override byte[] GetKey(string password)
        {
            throw new NotImplementedException();
        }
    }
#endif

#if WINDOWS_UWP
    using Windows.Security.Cryptography;
    using Windows.Security.Cryptography.Core;
    using Windows.Storage.Streams;

    public class EncryptDecryptImplementation : EncryptDecryptBase, IEncryptDecrypt
    {
        protected override byte[] Decrypt(byte[] keys, byte[] data)
        {
            try
            {
                IBuffer data2Decrypt = CryptographicBuffer.CreateFromByteArray(data);
                IBuffer keyMaterial = CryptographicBuffer.CreateFromByteArray(keys);

                SymmetricKeyAlgorithmProvider objAlg = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcbPkcs7);
                var key = objAlg.CreateSymmetricKey(keyMaterial);

                var ret = CryptographicEngine.Decrypt(key, data2Decrypt, null);

                CryptographicBuffer.CopyToByteArray(ret, out byte[] byteRet);
                return byteRet;
            }
            catch (Exception ex)
            {
                 throw new EncryptDecriptException("Error Decrypting data. WrongPassword or DataCorruption.", ex);
            }        
        }

        protected override byte[] Encrypt(byte[] keys, byte[] data)
        {
            IBuffer data2Encrypt = CryptographicBuffer.CreateFromByteArray(data);
            IBuffer keyMaterial = CryptographicBuffer.CreateFromByteArray(keys);
            //IBuffer keyMaterial = CryptographicBuffer.GenerateRandom(16);

           CryptographicBuffer.CopyToByteArray(keyMaterial, out byte[] temp);

            SymmetricKeyAlgorithmProvider objAlg = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcbPkcs7);
            
            var key = objAlg.CreateSymmetricKey(keyMaterial);

            var ret = CryptographicEngine.Encrypt(key, data2Encrypt, null);

            CryptographicBuffer.CopyToByteArray(ret, out byte[] byteRet);
            return byteRet;
        }

        protected override byte[] GetKey(string password)
        {
            IBuffer buffUtf8Msg = CryptographicBuffer.ConvertStringToBinary(password, BinaryStringEncoding.Utf8);

            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);

            IBuffer buffHash = objAlgProv.HashData(buffUtf8Msg);

            CryptographicBuffer.CopyToByteArray(buffHash, out byte[] byteRet);

            return byteRet;
        }

        //protected override byte[] GetKey(string password)
        //{
        //    // Create a string that contains the algorithm name.
        //    string strAlgName = KeyDerivationAlgorithmNames.Pbkdf2Sha1;
        //    // Open the specified algorithm.
        //    KeyDerivationAlgorithmProvider objKdfProv = KeyDerivationAlgorithmProvider.OpenAlgorithm(strAlgName);
        //    // Specify the requested size, in bytes, of the derived key. 
        //    UInt32 targetSize = 24;
        //    // Create a buffer that contains the secret used during derivation.
        //    String strSecret = "SomeSecret";   //Change and move somewhere else
        //    IBuffer buffSecret = CryptographicBuffer.ConvertStringToBinary(strSecret, BinaryStringEncoding.Utf8);
        //    // Create a random salt value.
        //    String strSalt = "SaltValue"; //change and move somewhere else
        //    IBuffer buffSalt = CryptographicBuffer.ConvertStringToBinary(strSalt, BinaryStringEncoding.Utf8);
        //    // Specify the number of iterations to be used during derivation.
        //    UInt32 iterationCountIn = 5000;
        //    // Create the derivation parameters.
        //    KeyDerivationParameters pbkdf2Params = KeyDerivationParameters.BuildForPbkdf2(buffSalt, iterationCountIn);
        //    // Create a key from the secret value.
        //    CryptographicKey keyOriginal = objKdfProv.CreateKey(buffSecret);
        //    // Derive a key based on the original key and the derivation parameters.
        //    IBuffer keyDerived = CryptographicEngine.DeriveKeyMaterial(keyOriginal, pbkdf2Params, targetSize);

        //    return keyDerived;
        //}
    }
#endif

#if __ANDROID__ || __IOS__ || __NET__ 
    using System.Security.Cryptography;
    public class EncryptDecryptImplementation : EncryptDecryptBase, IEncryptDecrypt
    {
        protected override byte[] Encrypt(byte[] keys, byte[] data)
        {
            using (var prov = new AesCryptoServiceProvider() { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
            {
                ICryptoTransform transform = prov.CreateEncryptor();
                byte[] results = transform.TransformFinalBlock(data, 0, data.Length);
                return results;
            }
        }
        protected override byte[] Decrypt(byte[] keys, byte[] data)
        {
            try
            {
                using (var prov = new AesCryptoServiceProvider() { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                {
                    ICryptoTransform transform = prov.CreateDecryptor();
                    byte[] results = transform.TransformFinalBlock(data, 0, data.Length);

                    return results;
                }
            }
            catch (Exception ex)
            {
                throw new EncryptDecriptException("Error Decrypting data. WrongPassword or DataCorruption.", ex);
            }
        }
        protected override byte[] GetKey(string password)
        {
            using(var hashProv = new SHA256CryptoServiceProvider())
            {
                return hashProv.ComputeHash(UTF8Encoding.UTF8.GetBytes(password));
            }
        }
    }
#endif

    public class EncryptDecriptException : Exception
    {
        public EncryptDecriptException(string message) : base(message)
        {
        }
        public EncryptDecriptException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class EncryptDecryptExceptionDataCorruption : Exception
    {
        public EncryptDecryptExceptionDataCorruption(string message) : base(message)
        {
        }
        public EncryptDecryptExceptionDataCorruption(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    //public class EncryptDecryptExceptionWrongPassword : Exception
    //{
    //    public EncryptDecryptExceptionWrongPassword(string message) : base(message)
    //    {
    //    }
    //    public EncryptDecryptExceptionWrongPassword(string message, Exception innerException) : base(message, innerException)
    //    {
    //    }
    //}  


}
