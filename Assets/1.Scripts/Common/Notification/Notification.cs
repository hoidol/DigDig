// using UnityEngine;
// #if UNITY_ANDROID
// using Unity.Notifications.Android;
// using UnityEngine.Android;
// #endif
// #if UNITY_IOS
// using Unity.Notifications.iOS;
// #endif
// public class Notification : MonoSingleton<Notification>
// {
//     void Start()
//     {


// #if UNITY_ANDROID

//         var androidChannel = new AndroidNotificationChannel()
//         {
//             Id = "LuckyMine_channel",
//             Name = "LuckyMine Notification",
//             Importance = Importance.High,
//             Description = "Description Test", //TranslateManager.GetText("noti_desc")
//             EnableVibration = true,
//         };
//         AndroidNotificationCenter.RegisterNotificationChannel(androidChannel);
//         // Android 알림 수신 콜백 등록
//         AndroidNotificationCenter.OnNotificationReceived += OnAndroidNotificationReceived;
// #endif

// #if UNITY_IOS
//         // iOS 알림 수신 콜백 등록
//         iOSNotificationCenter.OnNotificationReceived += OnIOSNotificationReceived;
// #endif
//     }

//     public void SendNotification(string id, string title, string body, int sec)
//     {
//         // Android 설정

//         string notiId = PlayerPrefs.GetString(id.ToString());
//         if (CheckRegisteredNoti(notiId))
//         {
//             CancelNotification(notiId);
//         }

// #if UNITY_ANDROID
//         if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
//         {
//             // 권한 요청
//             Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
//         }

//         var notification = new AndroidNotification()
//         {
//             Title = title,//
//             Text = body, // 
//             SmallIcon = "small_icon",
//             ShowInForeground = false,
//             LargeIcon = "large_icon",
//             FireTime = System.DateTime.Now.AddSeconds(sec)
//         };
//         Debug.Log($" id {id} 시간 : {System.DateTime.Now.AddSeconds(sec)} 에 알람 ");

//         int notiIdInt= AndroidNotificationCenter.SendNotification(notification, "LuckyMine_channel");
//         PlayerPrefs.SetString(id, notiIdInt.ToString());
// #elif UNITY_IOS
//  var timeTrigger = new iOSNotificationTimeIntervalTrigger()
//         {
//             TimeInterval = new System.TimeSpan(0, 0, sec),
//             Repeats = false
//         };

//         var notification = new iOSNotification()
//         {
//             Identifier = id,
//             Title =title,
//             Body = body,
//             ShowInForeground = true,
//             ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
//             CategoryIdentifier = "category_a",
//             ThreadIdentifier = "thread1",
//             Trigger = timeTrigger,
//         };

//         iOSNotificationCenter.ScheduleNotification(notification);

// #endif


//     }

//     public void CancelNotification(string notificationId)
//     {
//         if (!CheckRegisteredNoti(notificationId))
//             return;

// #if UNITY_ANDROID
//         string idInt = PlayerPrefs.GetString(notificationId);
//         if (!string.IsNullOrEmpty(idInt))
//         {
//             if(int.TryParse(idInt,out int v)){
//                 AndroidNotificationCenter.CancelNotification(v);
//             }
//         }

// #elif UNITY_IOS
//         iOSNotificationCenter.RemoveScheduledNotification(notificationId);
//         iOSNotificationCenter.RemoveDeliveredNotification(notificationId);
// #endif
//     }



//     bool CheckRegisteredNoti(string notificationId)
//     {
// #if UNITY_ANDROID// 알림이 등록되어 있는지 확인
//         string idInt = PlayerPrefs.GetString(notificationId);

//         if (!string.IsNullOrEmpty(idInt))
//         {
//             if(int.TryParse(idInt,out int v)){
//                 if (AndroidNotificationCenter.CheckScheduledNotificationStatus(v)
//            != NotificationStatus.Unknown)
//                 {
//                     Debug.Log($"알림 ID {notificationId}가 등록되어 있습니다.");
//                     return true;
//                 }
//             }

//             return false;
//         }
//         else
//             Debug.Log($"알림 ID {notificationId}가 등록되어 있지 않습니다.");
//         {
//             return false;
//         }
// #elif UNITY_IOS
// // 예약된 알림 확인
//         var scheduledNotifications = iOSNotificationCenter.GetScheduledNotifications();
//         bool isScheduled = false;

//         foreach (var notification in scheduledNotifications)
//         {
//             if (notification.Identifier == notificationId)
//             {
//                 isScheduled = true;
//                 break;
//             }
//         }

//         if (isScheduled)
//         {
//             Debug.Log($"알림 ID {notificationId}가 예약되어 있습니다.");
//             return true;
//         }
//         else
//         {
//             Debug.Log($"알림 ID {notificationId}가 등록되어 있지 않습니다.");
//             return false;
//         }
// #endif

//     }

// #if UNITY_ANDROID
//     // Android 알림 수신 콜백
//     private void OnAndroidNotificationReceived(AndroidNotificationIntentData intentData)
//     {
//         AndroidNotificationCenter.CancelNotification(intentData.Id);
//         Debug.Log("알림 수신(Android): " + intentData.Notification.Title);
//         // 알림을 처리하는 로직을 추가할 수 있습니다.
//     }
// #endif

// #if UNITY_IOS
//     // iOS 알림 수신 콜백
//     private void OnIOSNotificationReceived(iOSNotification notification)
//     {

//          iOSNotificationCenter.RemoveScheduledNotification(notification.Identifier);
//         iOSNotificationCenter.RemoveDeliveredNotification(notification.Identifier);
//         Debug.Log("알림 수신(iOS): " + notification.Title);
//         // 알림을 처리하는 로직을 추가할 수 있습니다.
//     }
// #endif

// }
