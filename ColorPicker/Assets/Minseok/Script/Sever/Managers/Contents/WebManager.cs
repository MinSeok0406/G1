using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

namespace Minseok
{
    public class WebManager
    {
        // 기존 ConfigureBase/BuildUrl 시그니처 유지 → 내부적으로 ServerConfig 사용
        public void ConfigureBase(string scheme, string host, int port, string basePath = "/api/")
        {
            ServerConfig.Configure(scheme, host, port, basePath);
        }

        public string BuildUrl(string path)
        {
            return ServerConfig.Combine(path);
        }

        public void SendPostRequest<T>(string url, object obj, Action<T> res)
        {
            Managers.Instance.StartCoroutine(CoSendWebRequest(url, UnityWebRequest.kHttpVerbPOST, obj, res));
        }

        IEnumerator CoSendWebRequest<T>(string url, string method, object obj, Action<T> res)
        {
            var sendUrl = BuildUrl(url);

            byte[] jsonBytes = null;
            if (obj != null)
            {
                string jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
            }

            using (var uwr = new UnityWebRequest(sendUrl, method))
            {
                uwr.uploadHandler = (jsonBytes != null) ? new UploadHandlerRaw(jsonBytes) : null;
                uwr.downloadHandler = new DownloadHandlerBuffer();
                uwr.SetRequestHeader("Content-Type", "application/json");
                uwr.SetRequestHeader("Accept", "application/json");
                uwr.timeout = 15; // ★ 타임아웃 추가

                Debug.Log($"[HTTP] {method} {sendUrl}");
                if (jsonBytes != null) Debug.Log($"[HTTP] payload {jsonBytes.Length} bytes");

                yield return uwr.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
                bool hasError = uwr.result == UnityWebRequest.Result.ConnectionError ||
                                uwr.result == UnityWebRequest.Result.ProtocolError;
#else
    bool hasError = uwr.isNetworkError || uwr.isHttpError;
#endif

                if (hasError)
                {
                    var msg = $"[HTTP][ERR] code={uwr.responseCode}, error={uwr.error}, text={uwr.downloadHandler?.text}";
                    Debug.LogError(msg);

                    var ui = GameObject.FindObjectOfType<Minseok.UI_LoginScene>();
                    if (ui != null) ui.SendMessage("OnHttpErrorText", msg, SendMessageOptions.DontRequireReceiver);
                    yield break;
                }

                var responseText = uwr.downloadHandler?.text;
                Debug.Log($"[HTTP][OK] len={(responseText?.Length ?? 0)}");

                try
                {
                    var resObj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseText);
                    res?.Invoke(resObj);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[HTTP][PARSE] {e.Message}\ntext: {responseText}");
                }
            }
        }
    }
}
