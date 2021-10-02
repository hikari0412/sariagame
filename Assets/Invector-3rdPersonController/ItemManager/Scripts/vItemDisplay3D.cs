using Invector.vItemManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vItemDisplay3D : MonoBehaviour
{
    //旋转最大角度
    public int yMinLimit = -20;
    public int yMaxLimit = 80;
    //旋转速度
    public float xSpeed = 250.0f;
    public float ySpeed = 120.0f;
    //旋转角度
    private float x = 0.0f;
    private float y = 0.0f;

    public RectTransform itemCanvas;
	
    [System.Serializable]
    public class vDisplay
    {
        public int itemId;
        public GameObject itemModel;
    }
    public GameObject currentItemModel;
    public float currentItemRotateSpeed = 45.0f;
    public List<vDisplay> displays;
    public void Display(vItemSlot slot)
    {
       if(slot) Display(slot.item);
    }

    public void Display(int id)
    {
        vDisplay display = displays.Find(d => d.itemId.Equals(id));
        if(display!=null)
        {
            if (currentItemModel) currentItemModel.SetActive(false);
            display.itemModel.SetActive(true);
            currentItemModel = display.itemModel;
        }
    }

    public void Display(vItem item)
    {
        if(item)Display(item.id);
    }

    private void Update()
    {
        if (currentItemModel)
        {
			if (Input.GetMouseButton(0))
			{
                if (RectTransformUtility.RectangleContainsScreenPoint(itemCanvas, Input.mousePosition))
                {
                    //Input.GetAxis("MouseX")获取鼠标移动的X轴的距离
                    x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                    y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                    y = ClampAngle(y, yMinLimit, yMaxLimit);
                    //欧拉角转化为四元数
                    Quaternion rotation = Quaternion.Euler(-y, -x, 0);
                    currentItemModel.transform.rotation = rotation;
                }
			}
        }
    }
    //角度范围值限定
    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
