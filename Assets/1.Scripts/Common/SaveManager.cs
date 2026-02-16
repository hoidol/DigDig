using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
public class SaveManager 
{
    private static readonly string EncryptionKey = "4W8vrR4P2jwdEIoQlIuwcnzCLUk28Jvj"; // 16, 24, 32 문자 길이로 설정

    //SaveFile() fName로 저장하기 + T 타입의 데이터를 파일 이름
    public static void SaveData<T>(string fName, T data)
    {

#if UNITY_EDITOR
        string path = Path.Combine(Application.dataPath, fName);
#else
        string path = Path.Combine(Application.persistentDataPath, fName);
#endif
        //Debug.Log($"데이터 로드 경로 : {path}");
        if (!File.Exists(path))
        {
            File.Create(path).Close();
        }

        string jsonData = JsonUtility.ToJson(data, true);
        string encryptedData = Encrypt(jsonData, EncryptionKey); // 데이터 암호화
        File.WriteAllText(path, encryptedData);

    }

    // LoadFile 데이터 로드하기
    public static T LoadData<T>(string fName)
    {
        T data = default;
#if UNITY_EDITOR //유니티 에디터에서만 동작되는 코드
        string path = Path.Combine(Application.dataPath, fName);
#else
				//에디터가 아닌 경우 
        string path = Path.Combine(Application.persistentDataPath, fName);
#endif
        //Debug.Log($"데이터 세이브 경로 : {path}");

        if (File.Exists(path))
        {

            string fileContent = File.ReadAllText(path);
            string decryptedData;

            try
            {
                // 데이터 복호화 시도
                decryptedData = Decrypt(fileContent, EncryptionKey);
            }
            catch
            {
                // 복호화 실패: 암호화되지 않은 데이터로 간주
                Debug.LogWarning("파일이 암호화되지 않았습니다. 암호화된 상태로 다시 저장합니다.");

                // JSON 데이터를 암호화하고 다시 저장
                decryptedData = fileContent; // 원본 데이터를 그대로 사용
                string encryptedData = Encrypt(decryptedData, EncryptionKey);
                File.WriteAllText(path, encryptedData);
            }

            // 복호화된 데이터를 JSON으로 변환
            data = JsonUtility.FromJson<T>(decryptedData);
        }

        return data;
    }
    // 데이터 암호화
    private static string Encrypt(string plainText, string key)
    {
#if UNITY_EDITOR
        return plainText;
#else
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = new byte[16]; // 초기화 벡터 (IV)는 기본적으로 0으로 설정
            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                return Convert.ToBase64String(encryptedBytes);
            }
        }
#endif
    }

    // 데이터 복호화
    private static string Decrypt(string encryptedText, string key)
    {
#if UNITY_EDITOR
        return encryptedText;
#else

        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        using (Aes aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = new byte[16]; // 초기화 벡터 (IV)는 암호화 시와 동일하게 설정
            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
#endif
    }
}
