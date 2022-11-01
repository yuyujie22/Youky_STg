/* 
 *  Author : Jk_Chen
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 通用帮助类
/// </summary>
public class Tools
{
    // ———————————————————— 纯C#

    /// <summary>
    /// 随机打乱List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="L"></param>
    public static void RandomShuffle<T>(List<T> L)
    {
        System.Random rd = new System.Random();
        for (int i = 0; i < L.Count; i++)
        {
            int j = rd.Next(0, L.Count - 1);
            T tmp = L[i];
            L[i] = L[j];
            L[j] = tmp;
        }
    }

    /// <summary>
    /// 获取当前时间的字符串
    /// </summary>
    /// <returns></returns>
    public static string NowTime()
    {
        int hour = System.DateTime.Now.Hour;
        int minute = System.DateTime.Now.Minute;
        int second = System.DateTime.Now.Second;
        int year = System.DateTime.Now.Year;
        int month = System.DateTime.Now.Month;
        int day = System.DateTime.Now.Day;
        return string.Format("{0}/{1:D2}/{2:D2} {3:D2}:{4:D2}:{5:D2}", year, month, day, hour, minute, second);
    }

    /// <summary>
    /// 通过类名创建一个实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T CreateObjectInstanceByClassName<T>(string name)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        object o = assembly.CreateInstance(name);
        return (T)o;
    }

    /// <summary>
    /// 浅拷贝
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    public static void CopyValue(object origin, object target)
    {
        System.Reflection.PropertyInfo[] properties = (target.GetType()).GetProperties();
        System.Reflection.FieldInfo[] fields = (origin.GetType()).GetFields();
        for (int i = 0; i < fields.Length; i++)
        {
            for (int j = 0; j < properties.Length; j++)
            {
                if (fields[i].Name == properties[j].Name && properties[j].CanWrite)
                {
                    properties[j].SetValue(target, fields[i].GetValue(origin), null);
                }
            }
        }
    }

    /// <summary>
    /// 深拷贝
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T DeepCopy<T>(T obj)
    {
        object retval;
        using (MemoryStream ms = new MemoryStream())
        {
            BinaryFormatter bf = new BinaryFormatter();
            //序列化成流
            bf.Serialize(ms, obj);
            ms.Seek(0, SeekOrigin.Begin);
            //反序列化成对象
            retval = bf.Deserialize(ms);
            ms.Close();
        }
        return (T)retval;
    }

    /// <summary>
    /// 弧度转角度
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static float GetAngle(float angle)
    {
        return angle / Mathf.PI * 180;
    }


    // ———————————————————— Unity系统

    /// <summary>
    /// 退出游戏
    /// </summary>
    void _Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 获取流文件夹路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetStreamingAssetsPath(string path)
    {
        string streamingAssetsPath =
#if UNITY_ANDROID && !UNITY_EDITOR
		Application.streamingAssetsPath; //安卓的Application.streamingAssetsPath已默认有"file://"
#elif UNITY_IOS && !UNITY_EDITOR
		"file://" + Application.streamingAssetsPath;
#elif UNITY_STANDLONE_WIN || UNITY_EDITOR
        "file://" + Application.streamingAssetsPath;
#else
		string.Empty;
#endif
        streamingAssetsPath = System.IO.Path.Combine(streamingAssetsPath, path);
        return streamingAssetsPath;
    }

    /// <summary>
    /// 每隔second秒时间执行1次（大致意思、不精确）：返回true
    /// 需要精确的需要用协程
    /// </summary>
    /// <param name="second"></param>
    /// <returns></returns>
    public static bool DealInterval(float second)
    {
        int frame = Mathf.RoundToInt(Application.targetFrameRate * second);
        if (frame < 1) frame = 1;
        return Time.frameCount % frame == 0;
    }

    /// <summary>
    /// 获取持久化文件夹路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetPersistentDataPath(string path)
    {
        return System.IO.Path.Combine(Application.persistentDataPath, path);
    }

    /// <summary>
    /// 随机颜色
    /// </summary>
    /// <returns></returns>
    public static Color RandomColor()
    {
        float red = UnityEngine.Random.Range(0.0f, 1.0f);
        float green = UnityEngine.Random.Range(0.0f, 1.0f);
        float blue = UnityEngine.Random.Range(0.0f, 1.0f);

        return new Color(red, green, blue);
    }

    /// <summary>
    /// 随机方向向量
    /// </summary>
    /// <returns></returns>
    public static Vector2 RandomDirection()
    {
        float x = Random.Range(-1f, 1f);
        float y = Mathf.Sqrt(1 - x * x);
        if (Random.Range(0f, 1f) > 0.5f)
        {
            y *= -1;
        }
        return new Vector2(x, y);
    }

    /// <summary>
    /// 判断Animator是否结束
    /// </summary>
    /// <param name="animator"></param>
    /// <returns></returns>
    public static bool JudgeAnimatorIsOver(Animator animator)
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 1.0f) return true;
        else return false;
    }

    /// <summary>
    /// PlayerPrefs设置参数
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void SetInt(string key, int value)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetInt(key, value);
        }
    }
    public static void SetFloat(string key, float value)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetFloat(key, value);
        }
    }
    public static void SetString(string key, string value)
    {
        if (!PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetString(key, value);
        }
    }

    /// <summary>
    /// 返回多边形碰撞器的外接矩形的大小
    /// </summary>
    /// <param name="pc"></param>
    /// <returns></returns>
    public static Vector2 GetSizeOfPolygon(PolygonCollider2D pc)
    {
        float mxy = -1e9f, mxx = -1e9f, miy = 1e9f, mix = 1e9f;
        foreach (var p in pc.points)
        {
            if (mxy < p.y) { mxy = p.y; }
            if (mxx < p.x) { mxx = p.x; }
            if (miy > p.y) { miy = p.y; }
            if (mix > p.x) { mix = p.x; }
        }
        return new Vector2(mxx - mix, mxy - miy);
    }

    // ———————————————————— Unity协程相关

    /// <summary>
    /// 存已经加载的资源
    /// </summary>
    public static Dictionary<string, Object> LoadedResources = new Dictionary<string, Object>();
    /// <summary>
    /// 读取资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public static T GetResource<T>(string path) where T : Object
    {
        if (LoadedResources.ContainsKey(path))
        {
            return (T)LoadedResources[path];
        }
        else
        {
            T rs = Resources.Load<T>(path);
            LoadedResources[path] = rs;
            return rs;
        }
    }

    /// <summary>
    /// 开始协程
    /// </summary>
    /// <param name="action"></param>
    public static void _StartCoroutine(IEnumerator action)
    {
        GameManager.Instance.StartCoroutine(action);
    }

    /// <summary>
    /// 延时启动
    /// </summary>
    /// <param name="action">函数</param>
    /// <param name="second">延时秒数</param>
    /// <param name="affectedByTimeScale">是否需要受函数影响</param>
    /// <returns></returns>
    public static void Timer(System.Action action, float second, bool affectedByTimeScale = true)
    {
        _StartCoroutine(Timer_(action, second, affectedByTimeScale));
    }
    public static IEnumerator Timer_(System.Action action, float second, bool affectedByTimeScale = true)
    {
        if (affectedByTimeScale)
        {
            yield return new WaitForSeconds(second);
        }
        else
        {
            yield return new WaitForSecondsRealtime(second);
        }
        action();
        yield break;
    }

    /// <summary>
    /// 存已经读入的包文件
    /// </summary>
    public static Dictionary<string, AssetBundle> asset = new Dictionary<string, AssetBundle>();
    /// <summary>
    /// 存已经加载的Prefab
    /// </summary>
    public static Dictionary<string, GameObject> Asset_Prefab = new Dictionary<string, GameObject>();
    /// <summary>
    /// 载入AssetBundle
    /// </summary>
    /// <param name="package">包名（后缀）</param>
    /// <param name="name">资源名数组</param>
    //public static void LoadAssetBundle_Memory_Perfabs(string package, string name)
    //{
    //    _StartCoroutine(LoadAssetBundle_Memory_Perfabs_(package, name, () => { }));
    //}
    //public static void LoadAssetBundle_Memory_Perfabs(string package, string name, System.Action callback)
    //{
    //    _StartCoroutine(LoadAssetBundle_Memory_Perfabs_(package, name, callback));
    //}
    //static IEnumerator LoadAssetBundle_Memory_Perfabs_(string package, string name, System.Action callback)
    //{
    //    string path = Path.Combine(GameConfig.AssetBundlePath, package);
    //    //byte[] bytes = File.ReadAllBytes(path);
    //    //AssetBundle asset = AssetBundle.LoadFromMemory(bytes);
    //    string pname = package + "." + name;
    //    if (!Asset_Prefab.ContainsKey(pname))
    //    {
    //        if (!asset.ContainsKey(package))
    //        {
    //            asset[package] = AssetBundle.LoadFromFile(path);
    //        }
    //        Asset_Prefab[pname] = asset[package].LoadAsset(name) as GameObject;
    //        if (Asset_Prefab[pname] != null)
    //        {
    //            Debug.Log("Loaded " + pname);
    //        }
    //        else
    //        {
    //            Debug.Log("想要加载的资源不存在：" + pname);
    //        }
    //    }
    //    callback();
    //    yield break;
    //}

    /// <summary>
    /// 准备载入的场景
    /// </summary>
    static string sceneToLoad;
    /// <summary>
    /// 场景载入进度
    /// </summary>
    static float LoadingProcessValue;
    /// <summary>
    /// 载入场景（先跳转至Loading界面）
    /// </summary>
    /// <param name="scene"></param>
    public static void LoadScene(string scene)
    {
        sceneToLoad = scene;
        SceneManager.LoadScene("Loading");
        _StartCoroutine(LoadingScene_());
    }
    /// <summary>
    /// 载入场景协程
    /// </summary>
    /// <returns></returns>
    static IEnumerator LoadingScene_()
    {
        LoadingProcessValue = 0;
        float maxSpeed = 0.03f;
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);

        // 禁止载入后自己切换
        op.allowSceneActivation = false;
        // isDone后再加载最后的10%
        while (op.progress < 0.9f)
        {
            // 连续加载
            while (LoadingProcessValue < op.progress)
            {
                LoadingProcessValue += maxSpeed;
                yield return new WaitForEndOfFrame();
            }
        }
        while (LoadingProcessValue < 1)
        {
            LoadingProcessValue += maxSpeed;
            yield return new WaitForEndOfFrame();
        }
        op.allowSceneActivation = true;
    }

    // ———————————————————— Unity游戏坐标、对象


    /// <summary>
    /// 调整GameObject obj的不透明度为a
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="a"></param>
    public static void SetOpaquenessOfGameObject(GameObject obj, float a)
    {
        Color c = obj.GetComponent<SpriteRenderer>().material.color;
        c.a = a;
        obj.GetComponent<SpriteRenderer>().material.color = c;
    }

    /// <summary>
    /// 实例化UI对象
    /// 注意实例化后接上的代码在Awake和Start之间（Awake后Start前）
    /// </summary>
    /// <param name="gameObject">预置物</param>
    /// <param name="parent">父亲的transform</param>
    /// <param name="position">位置</param>
    /// <param name="name">对象名称</param>
    /// <returns>生成的对象</returns>
    public static GameObject InstantiateUIObject(GameObject gameObject, Transform parent, Vector2 position, string name)
    {
        GameObject obj = Object.Instantiate(gameObject, new Vector2(0, 0), Quaternion.identity);
        obj.transform.SetParent(parent);
        obj.GetComponent<RectTransform>().anchoredPosition = position;
        obj.GetComponent<RectTransform>().transform.localScale = Vector2.one;
        obj.name = name;
        return obj;
    }

    /// <summary>
    /// 获取以center为中心radius范围内的所有标签为tag的对象
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    public static GameObject[] GetAllObjectsInRangeByTag(Vector2 center, float radius, string tag)
    {
        Collider2D[] objs = Physics2D.OverlapCircleAll(center, radius);
        List<GameObject> res = new List<GameObject>();
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].gameObject.tag == tag)
            {
                res.Add(objs[i].gameObject);
            }
        }
        return res.ToArray();
    }

    /// <summary>
    /// 检测canvas内鼠标停留位置的UI对象
    /// </summary>
    /// <param name="canvas"></param>
    /// <returns></returns>
    public static GameObject MouseUIObject(GameObject canvas)
    {
        if (canvas.GetComponent<GraphicRaycaster>() == null) return null;
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        GraphicRaycaster gr = canvas.GetComponent<GraphicRaycaster>();
        List<RaycastResult> results = new List<RaycastResult>();
        gr.Raycast(pointerEventData, results);
        if (results.Count != 0)
        {
            return results[0].gameObject;
        }
        return null;
    }

    /// <summary>
    /// 获取3D鼠标下的游戏对象
    /// </summary>
    /// <returns></returns>
    public static GameObject MouseGameObject()
    {
        GameObject res = null;
        Ray rays = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(rays.origin, rays.direction * 100, Color.yellow);
        RaycastHit hit;
        if (Physics.Raycast(rays, out hit)) { res = hit.transform.gameObject; }
        return res;
    }

    /// <summary>
    /// 获取鼠标下的3D游戏对象，若鼠标落在UI对象上返回null
    /// </summary>
    /// <returns></returns>
    public static GameObject MouseGameObjectUp(GameObject canvas)
    {
        GameObject ui = MouseUIObject(canvas);
        if (ui != null) return null;
        return MouseGameObject();
    }

    /// <summary>
    /// 获取鼠标下的2D游戏对象
    /// </summary>
    /// <returns></returns>
    public static GameObject MouseGameObject2D(int layer = ~0)
    {
        float len = 0.001f;
        GameObject res = null;
        //Ray2D rays = new Ray2D(MousePosInWorld(), Vector2.up);
        //Debug.DrawRay(rays.origin, rays.direction, Color.yellow);
        RaycastHit2D hit;
        hit = Physics2D.Raycast(MousePosInWorld(), Vector2.up, len, layer);
        if (hit.collider != null)
        {
            res = hit.transform.gameObject;
        }
        return res;
    }

    /// <summary>
    /// 获取鼠标下的2D游戏对象，若鼠标落在UI对象上返回null
    /// </summary>
    /// <returns></returns>
    public static GameObject MouseGameObjectUp2D(GameObject canvas, int layer = ~0)
    {
        GameObject ui = MouseUIObject(canvas);
        if (ui != null) return null;
        return MouseGameObject2D(layer);
    }

    /// <summary>
    /// 检测是否可以感知(2维空间的两个对象是否直线连接无障碍) 需要2维Collider
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="maxDis"></param>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static bool PerceiveObject(Vector2 a, GameObject b, float maxDis, int layer = ~0)
    {
        Ray2D ray = new Ray2D(a, ((Vector2)b.transform.position - a).normalized);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, maxDis, layer);
        if (hit == false)
        {
            return false;
        }
        Debug.Log(hit.collider.gameObject + " : " + b);
        return hit.collider.gameObject == b;
    }

    /// <summary>
    /// 判断鼠标下是否存在UI对象或者游戏物体
    /// </summary>
    /// <param name="canvas"></param>
    /// <returns></returns>
    public static bool MouseEmpty(GameObject canvas)
    {
        return MouseGameObject2D() == null && MouseUIObject(canvas) == null;
    }

    /// <summary>
    /// 获取鼠标在当前canvas上的位置
    /// 相对于canvas中心，可与UI的RectTransform.anchoredPosition联动
    /// </summary>
    /// <param name="canvas"></param>
    /// <returns></returns>
    public static Vector2 MousePosInCanvas(GameObject canvas)
    {
        Vector2 uisize = canvas.GetComponent<RectTransform>().sizeDelta;//得到画布的尺寸
        Vector2 screenpos = Input.mousePosition;
        Vector2 screenpos2;
        screenpos2.x = screenpos.x - (Screen.width / 2);//转换为以屏幕中心为原点的屏幕坐标
        screenpos2.y = screenpos.y - (Screen.height / 2);
        Vector2 uipos;//UI坐标
        uipos.x = screenpos2.x * (uisize.x / Screen.width);//转换后的屏幕坐标*画布与屏幕宽高比
        uipos.y = screenpos2.y * (uisize.y / Screen.height);
        return uipos;
    }

    /// <summary>
    /// 鼠标的世界坐标
    /// </summary>
    /// <returns></returns>
    public static Vector2 MousePosInWorld()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    ///// <summary>
    ///// 返回WorldPos的屏幕坐标
    ///// 用于Anchor在屏幕中心的RectTransform.anchoredPosition
    ///// </summary>
    ///// <param name="WorldPos"></param>
    ///// <returns></returns>
    //public static Vector2 WorldToRectPosition(Vector2 WorldPos)
    //{
    //    Vector3 screenPos = Camera.main.WorldToScreenPoint(WorldPos);

    //    Vector2 uiPos = Vector2.zero;
    //    RectTransformUtility.ScreenPointToLocalPointInRectangle(ObjectLibrary.gameUI.transform as RectTransform, screenPos, null, out uiPos);
    //    return uiPos;
    //}
}


