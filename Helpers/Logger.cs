using System.Collections.Generic;
using TMPro;

public class Logger
{
    #region Singleton
    private static Logger _instance;
    public static Logger Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Logger();
            }
            return _instance;
        }
    }
    #endregion

    private TextMeshProUGUI _logInfo;
    private string _logName;

    private UnityMainThreadDispatcher _unityMainThreadDispatcher;

    // Initialize the logger with a reference to the TextMeshProUGUI component
    public void Initialize(UnityMainThreadDispatcher unityMainThreadDispatcher, TextMeshProUGUI logInfo = null)
    {
        _unityMainThreadDispatcher = unityMainThreadDispatcher;
        if(logInfo != null) _logInfo = logInfo;
    }

    public void Info(string infoToPrint)
    {
        _logName += infoToPrint + "\n";

        // Schedule the update of the log text on the main thread
        _unityMainThreadDispatcher.AddJob(() =>
        {
            if (_logInfo != null)
            {
                _logInfo.text = _logName;
            }
            UnityEngine.Debug.Log(infoToPrint);
        });
    }
}