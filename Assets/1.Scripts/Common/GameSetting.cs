using UnityEngine;

public class GameSetting
{
   public const int MAX_EQUIPT_BULLET_COUNT = 5;
   public const int MAX_MF_GROUP_POINT = 5; //MemoryFragment
   public static string FIRST_STAGE_KEY = "Gateway1";
   public static CharacterName INIT_CHARACTER_NAME= CharacterName.Lucky ;
   public static string[] INIT_BULLE_KEYS = new string[]
   {
      "Pierce","Flame","Giant","Thunder","Iron"
   };
   public static Color healColor = new Color32(0xA4, 0xFF, 0x7E, 0xFF); //A4FF7E
   public static Color characterDamageColor = new Color32(0xFF, 0xA7, 0x01, 0xFF); //FFA701
   public static Color characterCrtDamageColor = new Color32(0xFF, 0x00, 0x00, 0xFF);//FF0000

   public static Color enemyDamageColor = new Color32(0xFF, 0x00, 0x00, 0xFF);//FF0000


}
