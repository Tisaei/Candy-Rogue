using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class LogPanelController : MonoBehaviour
{
    [SerializeField]
    private int MaxNumberOfLogs = 32;
    private Queue<GameObject> logQueue;
    [SerializeField]
    private GameObject ButtonManager, Log;
    [SerializeField]
    private Vector2Int initLogPos;
    private PlayerController playerController;
    private Dictionary<string, string> logDict;

    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform child in gameObject.transform) { Destroy(child.gameObject); }
        logQueue = new Queue<GameObject>();
        logDict = new Dictionary<string, string>();
        LoadAllLogText();
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        ButtonManager.SetActive(true);

        GameObject log = Instantiate(Log);
        log.transform.SetParent(transform);
        log.GetComponent<RectTransform>().anchoredPosition = initLogPos; // なぜか整数に四捨五入されてしまう(たとえVector2をつかっても). 画面が荒すぎるのが原因?
        logQueue.Enqueue(log);
        if (logDict.ContainsKey("gameStart")) { log.GetComponent<Text>().text = logDict["gameStart"]; }
    }

    private void LoadAllLogText()
    {
        var loadTextAsset = Resources.Load<TextAsset>("Log/Candy-Rogue_text");
        if(loadTextAsset == null){ CannotLoadError("ファイルがない"); return; }
        string loadText = loadTextAsset.text;
        loadText = Regex.Replace(loadText, "//.*", string.Empty, RegexOptions.Multiline); // コメントの削除.
        loadText = Regex.Replace(loadText, "^[\r\n]+", string.Empty, RegexOptions.Multiline); // 空白行の削除.
        loadText += "@";
        string oneLog;

        oneLog = Regex.Match(loadText, @"(?<=@gameStart)(.*?)(?=@)", RegexOptions.IgnoreCase | RegexOptions.Singleline).Value.Trim();
        if(oneLog == string.Empty) { CannotLoadError("gameStart"); return; }
        logDict.Add("gameStart", oneLog);
        Debug.Log(oneLog);

        oneLog = Regex.Match(loadText, @"(?<=@attack)(.*?)(?=@)", RegexOptions.IgnoreCase|RegexOptions.Singleline).Value.Trim(); // 「@Attack」の直後から「@」の直前までを大文字小文字関係なく検索し，前後の改行文字を削除.
        if (!ContainsStrings(oneLog, "<playerName>", "<enemyName>", "<damage>")) { CannotLoadError("attack"); return; }
        logDict.Add("attack", oneLog);
        Debug.Log(oneLog);

        oneLog = Regex.Match(loadText, @"(?<=@damaged)(.*?)(?=@)", RegexOptions.IgnoreCase | RegexOptions.Singleline).Value.Trim();
        if (!ContainsStrings(oneLog, "<playerName>", "<enemyName>", "<damage>")) { CannotLoadError("damaged"); return; }
        logDict.Add("damaged", oneLog);
        Debug.Log(oneLog);

        oneLog = Regex.Match(loadText, @"(?<=@defeat)(.*?)(?=@)", RegexOptions.IgnoreCase | RegexOptions.Singleline).Value.Trim();
        if (!ContainsStrings(oneLog, "<enemyName>")) { CannotLoadError("defeat"); return; }
        logDict.Add("defeat", oneLog);
        Debug.Log(oneLog);

        oneLog = Regex.Match(loadText, @"(?<=@defeated)(.*?)(?=@)", RegexOptions.IgnoreCase | RegexOptions.Singleline).Value.Trim();
        if (!ContainsStrings(oneLog, "<enemyName>")) { CannotLoadError("defeated"); return; }
        logDict.Add("defeated", oneLog);
        Debug.Log(oneLog);

        Debug.Log("ログテキストデータを正しく読み込みました");
    }

    private bool ContainsStrings(string target, params string[] searches)
    {
        foreach(string search in searches)
        {
            if (!target.Contains(search)) return false;
        }
        return true;
    }

    private void CannotLoadError(string complement = "")
    {
        Debug.LogError("ログテキストデータが読み込めませんでした("+complement+")");
        gameObject.SetActive(false);
    }

    public void AttackLog(EnemyController enemy, int damage)
    {
        string logString;
        if (logDict.ContainsKey("attack")) { logString = logDict["attack"]; } else { Debug.LogError("attack logの取得に失敗しました"); return; }
        logString.Replace("<playerName>", playerController.actorData.actorName);
        logString.Replace("<enemyName>", enemy.actorData.actorName);
        logString.Replace("<damage>", damage.ToString());
        SetLogAndAddList(logString);
    }

    public void DamagedLog(EnemyController enemy, int damage)
    {
        string logString;
        if (logDict.ContainsKey("damaged")) { logString = logDict["damaged"]; } else { Debug.LogError("damaged logの取得に失敗しました"); return; }
        logString.Replace("<playerName>", playerController.actorData.actorName);
        logString.Replace("<enemyName>", enemy.actorData.actorName);
        logString.Replace("<damage>", damage.ToString());
        SetLogAndAddList(logString);
    }

    public void DefeatLog(EnemyController enemy)
    {
        string logString;
        if (logDict.ContainsKey("defeat")) { logString = logDict["defeat"]; } else { Debug.LogError("defeat logの取得に失敗しました"); return; }
        logString.Replace("<enemyName>", enemy.actorData.actorName);
        SetLogAndAddList(logString);
    }

    public void DefeatedLog(EnemyController enemy)
    {
        string logString;
        if (logDict.ContainsKey("defeated")) { logString = logDict["defeated"]; } else { Debug.LogError("defeated logの取得に失敗しました"); return; }
        logString.Replace("<enemyName>", enemy.actorData.actorName);
        SetLogAndAddList(logString);
    }

    private void SetLogAndAddList(string logString)
    {
        // これまでの記録を上にあげ，これから追加するやつを含めてmaxNumberOfLogs以上ならば最初を消す.
        GameObject log = Instantiate(Log);
        log.transform.SetParent(transform);
        log.GetComponent<RectTransform>().anchoredPosition = initLogPos;
        logQueue.Enqueue(log);
        log.GetComponent<Text>().text = logString;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
