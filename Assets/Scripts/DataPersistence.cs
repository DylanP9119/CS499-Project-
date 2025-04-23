using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
public class DataPersistence : MonoBehaviour
{
    public double[] cargoGridPercentsD;
    public double[] patrolGridPercentsD;
    public double[] pirateGridPercentsD;

    public double[] cargoGridPercentsN;
    public double[] patrolGridPercentsN;
    public double[] pirateGridPercentsN;

    public int cargoDayPercent;
    public int cargoNightPercent;
    public int pirateDayPercent;
    public int pirateNightPercent;
    public int patrolDayPercent;
    public int patrolNightPercent;

    public int dayCount;
    public int hourCount;
    public int minuteCount;
    public bool nightCaptureEnabled = false;
    public string fileNameString; 
    public string path;
    public bool wasEnteredfromLoadScene;

    public List<ReplayEvent> replayEvents = new List <ReplayEvent>(); 

    public static DataPersistence Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        cargoGridPercentsD = new double[101];
        patrolGridPercentsD = new double[101];
        pirateGridPercentsD = new double[401];

        cargoGridPercentsN = new double[101];
        patrolGridPercentsN = new double[101];
        pirateGridPercentsN = new double[401];
    }
    
    void Awake()
    {
        
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

    }

    // Update is called once per frame
    void Update()
    {
        if (UIControllerScript.Instance)
        {
            for (int i = 0; i < 101; i++){
                cargoGridPercentsD[i] = UIControllerScript.Instance.cargoGridPercentsD[i];
                patrolGridPercentsD[i] = UIControllerScript.Instance.patrolGridPercentsD[i];
                cargoGridPercentsN[i] = UIControllerScript.Instance.cargoGridPercentsN[i];
                patrolGridPercentsN[i] = UIControllerScript.Instance.patrolGridPercentsN[i];
            }

            for (int i = 0; i < 401; i++){
                pirateGridPercentsD[i] = UIControllerScript.Instance.pirateGridPercentsD[i];
                pirateGridPercentsN[i] = UIControllerScript.Instance.pirateGridPercentsN[i];
            }

            cargoDayPercent = UIControllerScript.Instance.cargoDayPercent;
            cargoNightPercent = UIControllerScript.Instance.cargoNightPercent;
            pirateDayPercent = UIControllerScript.Instance.pirateDayPercent;
            pirateNightPercent = UIControllerScript.Instance.pirateNightPercent;
            patrolDayPercent = UIControllerScript.Instance.patrolDayPercent;
            patrolNightPercent = UIControllerScript.Instance.patrolNightPercent;

            dayCount = UIControllerScript.Instance.dayCount;
            hourCount = UIControllerScript.Instance.hourCount;
            minuteCount = UIControllerScript.Instance.minuteCount;

            nightCaptureEnabled = UIControllerScript.Instance.nightCaptureEnabled;
            fileNameString = UIControllerScript.Instance.fileNameString;

        }

    }
}
