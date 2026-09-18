using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[System.Serializable]
public abstract class UserBaseManager
{
    private bool isDirty;

    public abstract void LoadData();
    public abstract void SaveData();

    //즉시 저장 대신 dirty 표시만 하고 UserManager의 LateUpdate에서 한 프레임당 한 번만 실제 저장되도록 요청
    protected void RequestSave()
    {
        if (isDirty)
            return;

        isDirty = true;
        UserManager.Instance.RegisterDirtySave(this);
    }

    public void FlushSave()
    {
        if (!isDirty)
            return;

        isDirty = false;
        SaveData();
    }
}
