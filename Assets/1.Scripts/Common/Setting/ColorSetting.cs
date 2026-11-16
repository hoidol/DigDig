using UnityEngine;

public class ColorSetting
{
   public static Color healColor = new Color32(0xA4, 0xFF, 0x7E, 0xFF); //A4FF7E
   public static Color characterDamageColor = new Color32(0xFF, 0xA7, 0x01, 0xFF); //FFA701
   public static Color characterCrtDamageColor = new Color32(0xFF, 0x00, 0x00, 0xFF);//FF0000

   public static Color weakDamageColor = new Color32(0xFF, 0x5A, 0x00, 0xFF); //FF5A00 약점 피해 (방어 타입 배율 > 1)
   public static Color resistDamageColor = new Color32(0x9A, 0x9A, 0x9A, 0xFF); //9A9A9A 내성 피해 (배율 < 1)

   public static Color enemyDamageColor = new Color32(0xFF, 0x00, 0x00, 0xFF);//FF0000

   public static Color activeColor = new Color32(0x25, 0x9C, 0xFF, 0xFF); //259CFF
   public static Color inactiveColor = new Color32(0x52, 0x4B, 0x4A, 0xFF); //524B4A


}
