// // 약점 공략 - 디버프 걸린 적에게 30% 추가 피해
// using UnityEngine;

// public class DaggerItem : Item, IBulletForce
// {
//     float[] BONUSValues = {1.2f,1.3f,1.4f};

//     public override string GetDescription(int lv = 1,bool detail = false)
//     {
//         return $"뒤로 공격 시 {BONUSValues[lv-1] * 100}% 추가 데미지";
//     }


//     public float GetMultiDamage(BulletObject bullet, IHittable hit, RaycastHit2D hit2D, Vector2 shootDir)
//     {      
//         if( hit.Transform.TryGetComponent<Enemy>(out Enemy e))
//         {
//             //백어택 확인
//             //hit2D.point
            
//             float x = hit2D.point.x - e.transform.position.x ; //x 0보면 크면 오론쪽, 작으면 왼쪽 
//             if ((e.face > 0 && x <0) || (e.face < 0 && x > 0)) //오른쪽 봄 
//             {
//                 return BONUSValues[count-1];
//             }

//             if(InGameUtil.CheckBackAttack(e.transform, e.face, hit2D.point))
//             {
//                 return BONUSValues[count-1];
//             }

//             // Vector2 dir = (Player.Instance.transform.position - hit.Transform.position).normalized;
//             // if(Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
//             // {
//             //     if(dir.x > 0)
//             //     {
//             //         dir.x = -1;
//             //         dir.y = 0;
//             //     }else 
//             //     {
                    
//             //         dir.x = 1;
//             //         dir.y = 0;
//             //     }
//             // }
//             // else
//             // {
//             //     if(dir.y > 0)
//             //     {
//             //         dir.x = 0;
//             //         dir.y = -1;
//             //     }else 
//             //     {
                    
//             //         dir.x = 0;
//             //         dir.y = 1;
//             //     }
//             // }
            
//             // float angle = Vector2.Angle(dir,hit2D.normal);
//             // if(angle < 10)
//             // {
//             //     return BONUSValues[GetLevel()-1];
//             // }

//         }   
//         return 1;
//         //throw new System.NotImplementedException();
//     }
// }
