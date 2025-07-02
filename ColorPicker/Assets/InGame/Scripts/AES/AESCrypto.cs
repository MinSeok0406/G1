using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ColorPicker.InGame
{
    public static class AESCrypto
    {
        /// <summary>
        /// 문자열을 AES 키로 암호화하여 Base64로 반환하는 함수
        /// </summary>
        /// <param name="plainText">평문 문자열</param>
        /// <param name="aesKey">AES 256비트 키 (32바이트)</param>
        /// <returns>암호화된 Base64 문자열</returns>
        public static string EncryptString(string plainText, byte[] aesKey)
        {
            using (Aes aes = Aes.Create()) //AES의 라이프 타임을 지정해 객체 생성
            {
                aes.Key = aesKey; // 대칭키 설정
                aes.GenerateIV(); // iV 생성

                ICryptoTransform encryptor = aes.CreateEncryptor(); // 변환기 객체 생성
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText); // 평문을 UTF8바이트 배열로 변환
                byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length); // AES로 변환한 후 암호문 바이트 배열 반환

                byte[] result = new byte[aes.IV.Length + cipherBytes.Length]; 
                Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length); // IV를 결과를 앞에서 부터 저장
                Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length); // IV 이후 암호문을 저장 

                return Convert.ToBase64String(result); // 문자열 인코딩 후 반환
            }
        }

        /// <summary>
        /// AES 키로 암호화된 Base64 문자열을 복호화하는 함수
        /// </summary>
        /// <param name="base64CipherText">암호화된 Base64 문자열</param>
        /// <param name="aesKey">AES 256비트 키 (32바이트)</param>
        /// <returns>복호화된 평문 문자열</returns>
        public static string DecryptString(string base64CipherText, byte[] aesKey)
        {
            byte[] fullCipher = Convert.FromBase64String(base64CipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = aesKey;

                byte[] iv = new byte[16];
                byte[] cipherBytes = new byte[fullCipher.Length - 16];

                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);

                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor();

                byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                return Encoding.UTF8.GetString(plainBytes);
            }
        }

        /// <summary>
        /// 바이트 배열을 AES 키로 암호화하는 함수
        /// </summary>
        /// <param name="plainBytes"> 평문 바이트 배열</param>
        /// <param name="aesKey">AES 256비트 키 (32바이트)</param>
        /// <returns>암호화된 바이트 배열 (IV + 데이터)</returns>
        public static byte[] EncryptBytes(byte[] plainBytes, byte[] aesKey)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = aesKey;
                aes.GenerateIV();

                ICryptoTransform encryptor = aes.CreateEncryptor();
                byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                byte[] result = new byte[aes.IV.Length + cipherBytes.Length]; // IV : 동일한 평문을 암호화해도 매번 다른 결과를 만들기 위해 사용하는 값
                Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
                Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

                return result;
            }
        }

        /// <summary>
        /// 암호화된 바이트 배열을 AES 키로 복호화하는 함수
        /// </summary>
        /// <param name="fullCipher"> IV + 암호문 바이트 배열</param>
        /// <param name="aesKey">AES 256비트 키 (32바이트)</param>
        /// <returns>복호화된 평문 바이트 배열</returns>
        public static byte[] DecryptBytes(byte[] fullCipher, byte[] aesKey)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = aesKey;

                byte[] iv = new byte[16];
                byte[] cipherBytes = new byte[fullCipher.Length - 16];

                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);

                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor();

                return decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            }
        }
    }
}
